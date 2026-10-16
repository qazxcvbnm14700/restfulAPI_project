using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;

public sealed record MenuSnapshot(
    string RestaurantName,
    string Tagline,
    string[] Categories,
    List<JsonElement> Items
);

public sealed class SqliteStore(string databasePath)
{
    private readonly string _connectionString = new SqliteConnectionStringBuilder
    {
        DataSource = databasePath
    }.ToString();

    public async Task InitializeAsync(JsonElement seedMenu, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var createTables = connection.CreateCommand();
        createTables.CommandText = """
            CREATE TABLE IF NOT EXISTS menu_metadata (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                restaurant_name TEXT NOT NULL,
                tagline TEXT NOT NULL,
                categories_json TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS menu_items (
                id TEXT PRIMARY KEY,
                data_json TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS orders (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                order_id TEXT UNIQUE,
                status TEXT NOT NULL,
                estimated_prep_minutes INTEGER NOT NULL DEFAULT 0,
                estimated_ready_at TEXT NOT NULL DEFAULT '',
                status_updated_at TEXT NOT NULL DEFAULT '',
                customer_name TEXT NOT NULL,
                phone TEXT NOT NULL,
                dining_type TEXT NOT NULL,
                table_no TEXT NOT NULL,
                address TEXT NOT NULL,
                note TEXT NOT NULL,
                total_amount INTEGER NOT NULL,
                items_json TEXT NOT NULL,
                created_at TEXT NOT NULL
            );
            """;
        await createTables.ExecuteNonQueryAsync(cancellationToken);
        await EnsureOrderEstimateColumnsAsync(connection, cancellationToken);

        var insertMetadata = connection.CreateCommand();
        insertMetadata.CommandText = """
            INSERT OR IGNORE INTO menu_metadata (id, restaurant_name, tagline, categories_json)
            VALUES (1, $restaurantName, $tagline, $categories);
            """;
        insertMetadata.Parameters.AddWithValue("$restaurantName", seedMenu.GetProperty("restaurantName").GetString()!);
        insertMetadata.Parameters.AddWithValue("$tagline", seedMenu.GetProperty("tagline").GetString()!);
        insertMetadata.Parameters.AddWithValue("$categories", seedMenu.GetProperty("categories").GetRawText());
        await insertMetadata.ExecuteNonQueryAsync(cancellationToken);

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM menu_items;";
        var itemCount = (long)(await countCommand.ExecuteScalarAsync(cancellationToken))!;
        if (itemCount == 0)
        {
            foreach (var item in seedMenu.GetProperty("items").EnumerateArray())
            {
                var insertItem = connection.CreateCommand();
                insertItem.CommandText = "INSERT INTO menu_items (id, data_json) VALUES ($id, $data);";
                insertItem.Parameters.AddWithValue("$id", item.GetProperty("id").GetString()!);
                insertItem.Parameters.AddWithValue("$data", item.GetRawText());
                await insertItem.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await EnsureMenuPrepTimesAsync(connection, seedMenu, cancellationToken);
        await BackfillOrderEstimatesAsync(connection, seedMenu, cancellationToken);
    }

    public async Task<MenuSnapshot> GetMenuAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var metadataCommand = connection.CreateCommand();
        metadataCommand.CommandText = "SELECT restaurant_name, tagline, categories_json FROM menu_metadata WHERE id = 1;";
        await using var metadataReader = await metadataCommand.ExecuteReaderAsync(cancellationToken);
        await metadataReader.ReadAsync(cancellationToken);
        var restaurantName = metadataReader.GetString(0);
        var tagline = metadataReader.GetString(1);
        var categories = JsonSerializer.Deserialize<string[]>(metadataReader.GetString(2)) ?? [];
        await metadataReader.CloseAsync();

        var itemsCommand = connection.CreateCommand();
        itemsCommand.CommandText = "SELECT data_json FROM menu_items ORDER BY rowid;";
        await using var itemsReader = await itemsCommand.ExecuteReaderAsync(cancellationToken);
        var items = new List<JsonElement>();
        while (await itemsReader.ReadAsync(cancellationToken))
        {
            using var document = JsonDocument.Parse(itemsReader.GetString(0));
            items.Add(document.RootElement.Clone());
        }

        return new MenuSnapshot(restaurantName, tagline, categories, items);
    }

    public async Task<JsonElement?> GetMenuItemAsync(string dishId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "SELECT data_json FROM menu_items WHERE id = $id COLLATE NOCASE;";
        command.Parameters.AddWithValue("$id", dishId);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is not string itemJson)
        {
            return null;
        }

        using var document = JsonDocument.Parse(itemJson);
        return document.RootElement.Clone();
    }

    public async Task<string> CreateOrderAsync(OrderDetailDto order, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var insertCommand = connection.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = """
            INSERT INTO orders (
                status, estimated_prep_minutes, estimated_ready_at, status_updated_at,
                customer_name, phone, dining_type, table_no, address,
                note, total_amount, items_json, created_at
            ) VALUES (
                $status, $estimatedPrepMinutes, $estimatedReadyAt, $statusUpdatedAt,
                $customerName, $phone, $diningType, $tableNo, $address,
                $note, $totalAmount, $items, $createdAt
            );
            """;
        insertCommand.Parameters.AddWithValue("$status", order.Status);
        insertCommand.Parameters.AddWithValue("$estimatedPrepMinutes", order.EstimatedPrepMinutes);
        insertCommand.Parameters.AddWithValue("$estimatedReadyAt", order.EstimatedReadyAt.ToString("O", CultureInfo.InvariantCulture));
        insertCommand.Parameters.AddWithValue("$statusUpdatedAt", order.StatusUpdatedAt.ToString("O", CultureInfo.InvariantCulture));
        insertCommand.Parameters.AddWithValue("$customerName", order.CustomerName);
        insertCommand.Parameters.AddWithValue("$phone", order.Phone);
        insertCommand.Parameters.AddWithValue("$diningType", order.DiningType);
        insertCommand.Parameters.AddWithValue("$tableNo", order.TableNo);
        insertCommand.Parameters.AddWithValue("$address", order.Address);
        insertCommand.Parameters.AddWithValue("$note", order.Note);
        insertCommand.Parameters.AddWithValue("$totalAmount", order.TotalAmount);
        insertCommand.Parameters.AddWithValue("$items", JsonSerializer.Serialize(order.Items));
        insertCommand.Parameters.AddWithValue("$createdAt", order.CreatedAt.ToString("O", CultureInfo.InvariantCulture));
        await insertCommand.ExecuteNonQueryAsync(cancellationToken);

        var idCommand = connection.CreateCommand();
        idCommand.Transaction = transaction;
        idCommand.CommandText = "SELECT last_insert_rowid();";
        var rowId = (long)(await idCommand.ExecuteScalarAsync(cancellationToken))!;
        var orderId = $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{rowId:D3}";
        var updateCommand = connection.CreateCommand();
        updateCommand.Transaction = transaction;
        updateCommand.CommandText = "UPDATE orders SET order_id = $orderId WHERE id = $id;";
        updateCommand.Parameters.AddWithValue("$orderId", orderId);
        updateCommand.Parameters.AddWithValue("$id", rowId);
        await updateCommand.ExecuteNonQueryAsync(cancellationToken);
        transaction.Commit();

        return orderId;
    }

    public async Task<OrderDetailDto?> SyncOrderStatusAsync(string orderId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        var updateCommand = connection.CreateCommand();
        updateCommand.CommandText = """
            UPDATE orders
            SET status = '餐點已完成', status_updated_at = $now
            WHERE order_id = $orderId
              AND status = '料理製作中'
              AND estimated_ready_at <= $now;
            """;
        updateCommand.Parameters.AddWithValue("$now", now);
        updateCommand.Parameters.AddWithValue("$orderId", orderId);
        await updateCommand.ExecuteNonQueryAsync(cancellationToken);

        return await GetOrderAsync(connection, orderId, cancellationToken);
    }

    public async Task CompleteDueOrdersAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE orders
            SET status = '餐點已完成', status_updated_at = $now
            WHERE status = '料理製作中'
              AND estimated_ready_at <= $now;
            """;
        command.Parameters.AddWithValue("$now", now.ToString("O", CultureInfo.InvariantCulture));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<OrderDetailDto?> GetOrderAsync(
        SqliteConnection connection,
        string orderId,
        CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT order_id, status, estimated_prep_minutes, estimated_ready_at, status_updated_at,
                   customer_name, phone, dining_type, table_no, address, note,
                   total_amount, items_json, created_at
            FROM orders WHERE order_id = $orderId;
            """;
        command.Parameters.AddWithValue("$orderId", orderId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new OrderDetailDto(
            OrderId: reader.GetString(0),
            Status: reader.GetString(1),
            EstimatedPrepMinutes: reader.GetInt32(2),
            EstimatedReadyAt: DateTimeOffset.Parse(reader.GetString(3), CultureInfo.InvariantCulture),
            StatusUpdatedAt: DateTimeOffset.Parse(reader.GetString(4), CultureInfo.InvariantCulture),
            CustomerName: reader.GetString(5),
            Phone: reader.GetString(6),
            DiningType: reader.GetString(7),
            TableNo: reader.GetString(8),
            Address: reader.GetString(9),
            Note: reader.GetString(10),
            TotalAmount: reader.GetInt32(11),
            Items: JsonSerializer.Deserialize<List<OrderDetailItem>>(reader.GetString(12)) ?? [],
            CreatedAt: DateTimeOffset.Parse(reader.GetString(13), CultureInfo.InvariantCulture)
        );
    }

    private static async Task EnsureOrderEstimateColumnsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(orders);";
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(reader.GetString(1));
            }
        }

        var migrations = new Dictionary<string, string>
        {
            ["estimated_prep_minutes"] = "INTEGER NOT NULL DEFAULT 0",
            ["estimated_ready_at"] = "TEXT NOT NULL DEFAULT ''",
            ["status_updated_at"] = "TEXT NOT NULL DEFAULT ''"
        };
        foreach (var (column, definition) in migrations)
        {
            if (columns.Contains(column))
            {
                continue;
            }

            var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE orders ADD COLUMN {column} {definition};";
            await alterCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureMenuPrepTimesAsync(
        SqliteConnection connection,
        JsonElement seedMenu,
        CancellationToken cancellationToken)
    {
        foreach (var seedItem in seedMenu.GetProperty("items").EnumerateArray())
        {
            var id = seedItem.GetProperty("id").GetString()!;
            var getCommand = connection.CreateCommand();
            getCommand.CommandText = "SELECT data_json FROM menu_items WHERE id = $id;";
            getCommand.Parameters.AddWithValue("$id", id);
            if (await getCommand.ExecuteScalarAsync(cancellationToken) is not string itemJson)
            {
                continue;
            }

            var menuItem = JsonNode.Parse(itemJson)
                ?? throw new InvalidOperationException($"Menu item '{id}' contains invalid JSON.");
            if (menuItem["details"]?["prepTimeMinutes"] is not null)
            {
                continue;
            }

            menuItem["details"]!["prepTimeMinutes"] = seedItem
                .GetProperty("details")
                .GetProperty("prepTimeMinutes")
                .GetInt32();

            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = "UPDATE menu_items SET data_json = $data WHERE id = $id;";
            updateCommand.Parameters.AddWithValue("$data", menuItem.ToJsonString());
            updateCommand.Parameters.AddWithValue("$id", id);
            await updateCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task BackfillOrderEstimatesAsync(
        SqliteConnection connection,
        JsonElement seedMenu,
        CancellationToken cancellationToken)
    {
        var prepTimesByName = seedMenu.GetProperty("items")
            .EnumerateArray()
            .ToDictionary(
                item => item.GetProperty("name").GetString()!,
                item => item.GetProperty("details").GetProperty("prepTimeMinutes").GetInt32(),
                StringComparer.Ordinal);

        var orders = new List<(long Id, string ItemsJson, string CreatedAt, string StatusUpdatedAt)>();
        var selectCommand = connection.CreateCommand();
        selectCommand.CommandText = """
            SELECT id, items_json, created_at, status_updated_at
            FROM orders
            WHERE estimated_ready_at = '' OR status_updated_at = '' OR estimated_prep_minutes = 0;
            """;
        await using (var reader = await selectCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                orders.Add((reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
            }
        }

        foreach (var order in orders)
        {
            var items = JsonSerializer.Deserialize<List<OrderDetailItem>>(order.ItemsJson) ?? [];
            var updatedItems = items.Select(item => item with
            {
                EstimatedPrepMinutes = item.EstimatedPrepMinutes > 0
                    ? item.EstimatedPrepMinutes
                    : prepTimesByName.GetValueOrDefault(item.DishName)
            }).ToList();
            var estimatedPrepMinutes = updatedItems.Count == 0 ? 0 : updatedItems.Max(item => item.EstimatedPrepMinutes);
            var createdAt = DateTimeOffset.Parse(
                order.CreatedAt,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            var statusUpdatedAt = string.IsNullOrWhiteSpace(order.StatusUpdatedAt)
                ? createdAt
                : DateTimeOffset.Parse(order.StatusUpdatedAt, CultureInfo.InvariantCulture);

            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = """
                UPDATE orders
                SET estimated_prep_minutes = $estimatedPrepMinutes,
                    estimated_ready_at = $estimatedReadyAt,
                    status_updated_at = $statusUpdatedAt,
                    items_json = $items
                WHERE id = $id;
                """;
            updateCommand.Parameters.AddWithValue("$estimatedPrepMinutes", estimatedPrepMinutes);
            updateCommand.Parameters.AddWithValue(
                "$estimatedReadyAt",
                createdAt.AddMinutes(estimatedPrepMinutes).ToString("O", CultureInfo.InvariantCulture));
            updateCommand.Parameters.AddWithValue("$statusUpdatedAt", statusUpdatedAt.ToString("O", CultureInfo.InvariantCulture));
            updateCommand.Parameters.AddWithValue("$items", JsonSerializer.Serialize(updatedItems));
            updateCommand.Parameters.AddWithValue("$id", order.Id);
            await updateCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}