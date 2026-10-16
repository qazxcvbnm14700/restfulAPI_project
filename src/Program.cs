using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// 開發與測試支援 CORS 與靜態檔案
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 菜單資料包含列表和詳細資訊，由不同 API 回傳各自需要的欄位
var menu = new
{
    restaurantName = "弦月・本格日式料理屋",
    tagline = "職人炭火炙烤・慢熬豚骨白湯・築地旬魚直送",
    categories = new[] { "全部料理", "人氣推薦", "經典拉麵", "和風定食" },
    items = new object[]
    {
        new
        {
            id = "dish-1",
            name = "特上蒲燒鰻魚飯",
            subName = "とくじょう うなぎどん (Unagi Don)",
            category = "人氣推薦",
            price = 380,
            image = "images/unagi.jpg",
            badge = "主廚招牌",
            description = "嚴選外銷等級頂級白鰻，經炭火慢烤三道蒲燒工法，刷上傳承百年甘醇秘醬，油脂豐腴入口即化，佐日本新潟越光米。",
            details = new
            {
                ingredients = new[] { "日本靜岡特選鰻魚", "新潟縣產越光米", "百年秘傳照燒汁", "有田山椒粉" },
                origin = "台灣在地優質外銷無毒鰻魚直送",
                prepTime = "約 12-15 分鐘",
                prepTimeMinutes = 15,
                flavorTags = new[] { "炭火香氣", "油脂豐美", "主廚手作", "人氣 No.1" }
            },
            customOptions = new
            {
                riceAmount = new[] { "標準飯量", "增量大盛 (免費)", "少飯 (-30g)" },
                flavor = new[] { "標準醬汁", "醬汁增量", "少醬清爽" }
            },
            sides = new[]
            {
                new { id = "side-unagi-1", name = "日式蒲燒鰻手卷", price = 60, description = "香酥海苔包覆蒲燒鰻與現切小黃瓜" },
                new { id = "side-unagi-2", name = "極品鰻魚肝清湯", price = 40, description = "古法鰹魚昆布高湯清甜甘潤" },
                new { id = "side-unagi-3", name = "和風干貝茶碗蒸", price = 35, description = "滑嫩高湯蒸蛋佐青森帆立貝" },
                new { id = "side-unagi-4", name = "柚子白蘿蔔漬物", price = 25, description = "高知縣產柚子醋清脆爽口解膩" }
            },
        },
        new
        {
            id = "dish-2",
            name = "招牌特濃豚骨叉燒拉麵",
            subName = "とんこつ チャーシューメン (Tonkotsu Ramen)",
            category = "經典拉麵",
            price = 240,
            image = "images/ramen.jpg",
            badge = "人氣熱銷",
            description = "熬煮18小時以上的大骨與豚脂乳化出純白濃郁湯頭，佐特製炙燒五花厚切叉燒、黃金流心溏心蛋與日本博多極細麵。",
            details = new
            {
                ingredients = new[] { "博多極細生麵", "慢火乳化豚骨白湯", "炙燒五花厚切叉燒 (2片)", "黃金流心溏心蛋", "有明海苔", "高知青蔥" },
                origin = "每日晨起鮮熬黑豚豚骨高湯",
                prepTime = "約 8-10 分鐘",
                prepTimeMinutes = 10,
                flavorTags = new[] { "濃厚白湯", "炙燒焦香", "爆汁溏心", "拉麵魂" }
            },
            customOptions = new
            {
                noodleFirmness = new[] { "麵硬 (博多風)", "適中", "偏軟" },
                soupRichness = new[] { "道地日式濃味", "台灣標準", "清淡少油" }
            },
            sides = new[]
            {
                new { id = "side-ramen-1", name = "博多替玉 (加麵一份)", price = 30, description = "現煮極細生拉麵" },
                new { id = "side-ramen-2", name = "日式唐揚炸雞 (4塊)", price = 65, description = "秘醃去骨雞腿肉，外酥內爆汁" },
                new { id = "side-ramen-3", name = "特製黃金煎餃 (5顆)", price = 55, description = "日式冰花脆皮，黑豚鮮肉餡" },
                new { id = "side-ramen-4", name = "炙燒厚切叉燒 (增量2片)", price = 50, description = "特選五花低溫慢煮再炙烤" },
                new { id = "side-ramen-5", name = "招牌黃金溏心蛋", price = 25, description = "柴魚秘醬低溫浸漬流心" }
            },
        },
        new
        {
            id = "dish-3",
            name = "極上黑毛和牛丼",
            subName = "くろげわぎゅうどん (Wagyu Beef Don)",
            category = "人氣推薦",
            price = 320,
            image = "images/wagyu.jpg",
            badge = "極致奢華",
            description = "嚴選日本A5級黑毛和牛薄切片，以甘甜洋蔥、清酒與特級本釀造醬油溫火慢燉，搭配生食級有機鮮蛋黃，肉質極致柔嫩細膩。",
            details = new
            {
                ingredients = new[] { "日本A5黑毛和牛", "淡路島甜洋蔥", "生食級初卵蛋", "新潟越光米", "七味唐辛子" },
                origin = "日本鹿兒島直送A5黑毛和牛",
                prepTime = "約 10-12 分鐘",
                prepTimeMinutes = 12,
                flavorTags = new[] { "油花如雪", "入口即化", "濃郁蛋香", "奢華盛宴" }
            },
            customOptions = new
            {
                riceAmount = new[] { "標準飯量", "增量大盛 (免費)", "少飯 (-30g)" },
                scallion = new[] { "多蔥 (免費增量)", "正常青蔥", "不要蔥" }
            },
            sides = new[]
            {
                new { id = "side-wagyu-1", name = "日本生食級溫泉蛋", price = 25, description = "恆溫低溫熟成滑嫩水潤" },
                new { id = "side-wagyu-2", name = "主廚鮮魚味噌湯", price = 35, description = "赤味噌與時令鮮魚豆腐熬製" },
                new { id = "side-wagyu-3", name = "日式胡麻秋葵冷盤", price = 30, description = "日本焙煎芝麻醬清脆爽口" },
                new { id = "side-wagyu-4", name = "北海道起司可樂餅", price = 45, description = "金黃酥脆馬鈴薯爆漿流心起司" }
            },
        },
        new
        {
            id = "dish-4",
            name = "極黑里肌炸豬排定食",
            subName = "とんかつ ていしょく (Tonkatsu Set)",
            category = "和風定食",
            price = 260,
            image = "images/tonkatsu.jpg",
            badge = "定食定番",
            description = "嚴選厚切黑豚大里肌，裹上鮮生麵包粉經兩道低溫熱油酥炸，外皮酥脆不油膩，肉質多汁飽滿，隨附高麗菜絲與秘磨芝麻豬排醬。",
            details = new
            {
                ingredients = new[] { "特選厚切黑豚大里肌 (200g)", "日式生麵包粉", "越光米飯", "特調赤味噌湯", "高山高麗菜絲" },
                origin = "台灣在地優良履歷黑豚",
                prepTime = "約 12-15 分鐘",
                prepTimeMinutes = 15,
                flavorTags = new[] { "厚切紮實", "酥脆金黃", "肉汁噴發", "定食經典" }
            },
            customOptions = new
            {
                riceAmount = new[] { "標準飯量", "增量大盛 (免費)", "少飯 (-30g)" },
                sauceStyle = new[] { "現磨芝麻豬排醬", "和風柚子芥末醬" }
            },
            sides = new[]
            {
                new { id = "side-tonkatsu-1", name = "廣島炸牡蠣 (2顆)", price = 75, description = "日本瀨戶內海產鮮美牡蠣酥炸" },
                new { id = "side-tonkatsu-2", name = "高湯茶碗蒸", price = 35, description = "高湯蒸蛋香氣醇厚" },
                new { id = "side-tonkatsu-3", name = "越光白米飯增量", price = 20, description = "單點一碗Q彈香甜白飯" },
                new { id = "side-tonkatsu-4", name = "宇治抹茶白玉甜湯", price = 50, description = "京都抹茶與Q彈手作白玉" }
            },
        },
        new
        {
            id = "dish-5",
            name = "炙燒鮭魚親子丼",
            subName = "さけ おやこどん (Aburi Salmon Don)",
            category = "人氣推薦",
            price = 290,
            image = "images/salmon.jpg",
            badge = "當日鮮採",
            description = "新鮮空運挪威大西洋鮭魚，表面經直火微炙釋放誘人油脂香氣，搭配晶瑩剔透、在口中爆漿的醬油漬鮭魚卵與天然紫蘇葉。",
            details = new
            {
                ingredients = new[] { "挪威深海鮭魚厚切 (5片)", "北海道醬油漬鮭魚卵", "日本大葉紫蘇", "現磨阿里山生山葵", "赤酢壽司飯" },
                origin = "挪威空運生食級冷藏鮭魚",
                prepTime = "約 8-10 分鐘",
                prepTimeMinutes = 10,
                flavorTags = new[] { "油脂豐美", "爆漿鮭魚卵", "直火炙香", "老饕最愛" }
            },
            customOptions = new
            {
                riceAmount = new[] { "標準飯量", "增量大盛 (免費)", "少飯 (-30g)" },
                wasabi = new[] { "附現磨山葵 (分開附)", "去山葵" }
            },
            sides = new[]
            {
                new { id = "side-salmon-1", name = "炙燒鮭魚肚握壽司 (2貫)", price = 70, description = "油花極致豐富的鮭魚肚握壽司" },
                new { id = "side-salmon-2", name = "鮮蝦蘆筍手卷", price = 55, description = "新鮮草蝦、鮮脆蘆筍與日式美乃滋" },
                new { id = "side-salmon-3", name = "極上蟹肉茶碗蒸", price = 50, description = "特選松葉蟹肉絲點綴高湯蒸蛋" },
                new { id = "side-salmon-4", name = "鮭魚荒焚味噌湯", price = 45, description = "鮮甜鮭魚骨肉與白味噌細火熬煮" }
            },
        },
        new
        {
            id = "dish-6",
            name = "大海老天婦羅蕎麥麵",
            subName = "てんぷら そば (Tempura Soba)",
            category = "和風定食",
            price = 220,
            image = "images/tempura.jpg",
            badge = "清雅道地",
            description = "信州二八蕎麥麵吸附甘甜優雅的鰹魚昆布柴魚高湯，佐以兩大尾金黃羽衣炸大蝦與時令南瓜、地瓜、香菇野菜天婦羅。",
            details = new
            {
                ingredients = new[] { "長野信州二八蕎麥麵", "野生特大草蝦 (2尾)", "時令野菜天婦羅", "枕崎產本枯節柴魚高湯", "鳴門魚板" },
                origin = "日本長野產蕎麥粉手打製作",
                prepTime = "約 10-12 分鐘",
                prepTimeMinutes = 12,
                flavorTags = new[] { "金黃酥脆", "清甜柴魚高湯", "天然蕎麥香", "溫和順口" }
            },
            customOptions = new
            {
                servingStyle = new[] { "暖心熱湯麵", "冰鎮冷沾麵 (附沾汁)" },
                scallion = new[] { "多蔥花", "正常蔥花", "不要蔥" }
            },
            sides = new[]
            {
                new { id = "side-tempura-1", name = "追加炸大海老 (一尾)", price = 60, description = "金黃酥脆特選野生大蝦" },
                new { id = "side-tempura-2", name = "揚出和風豆腐", price = 40, description = "外酥內嫩佐日式鰹魚醬汁與蘿蔔泥" },
                new { id = "side-tempura-3", name = "和風芥末山葵章魚", price = 45, description = "清脆章魚佐特調山葵風味冷菜" },
                new { id = "side-tempura-4", name = "冰鎮玄米抹茶", price = 30, description = "無糖炒焙玄米香氣冷泡茶" }
            }
        }
    }
};

var projectRoot = FindProjectRoot(builder.Environment.ContentRootPath);
var configuredDatabasePath = builder.Configuration["Database:Path"];
var databasePath = string.IsNullOrWhiteSpace(configuredDatabasePath)
    ? Path.Combine(projectRoot, "data", "tofushop.db")
    : Path.GetFullPath(configuredDatabasePath);
var store = new SqliteStore(databasePath);
await store.InitializeAsync(JsonSerializer.SerializeToElement(menu));
builder.Services.AddSingleton(store);
builder.Services.AddHostedService<OrderStatusBackgroundService>();

var app = builder.Build();

app.UseCors();

// 支援自動偵測 web/wwwroot 靜態檔案目錄 (支援從 /src 執行或從 /web/bin 執行)
string[] searchPaths =
[
    Path.Combine(AppContext.BaseDirectory, "..", "wwwroot"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "wwwroot"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "web", "wwwroot"),
    Path.Combine(AppContext.BaseDirectory, "wwwroot"),
    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
];

string? foundWwwroot = searchPaths
    .Select(p => Path.GetFullPath(p))
    .FirstOrDefault(p => Directory.Exists(p) && File.Exists(Path.Combine(p, "index.html")));

if (foundWwwroot != null)
{
    var fileServerOptions = new FileServerOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(foundWwwroot),
        RequestPath = "",
        EnableDefaultFiles = true
    };
    app.UseFileServer(fileServerOptions);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 1號 API: 菜單卡片清單 (/api/menu)
app.MapGet("/api/menu", async (CancellationToken cancellationToken) =>
{
    var menuData = await store.GetMenuAsync(cancellationToken);
    var items = menuData.Items.Select(item => new
    {
        id = item.GetProperty("id").GetString(),
        name = item.GetProperty("name").GetString(),
        subName = item.GetProperty("subName").GetString(),
        category = item.GetProperty("category").GetString(),
        price = item.GetProperty("price").GetInt32(),
        image = item.GetProperty("image").GetString(),
        badge = item.GetProperty("badge").GetString(),
        description = item.GetProperty("description").GetString()
    });

    return Results.Ok(new
    {
        restaurantName = menuData.RestaurantName,
        tagline = menuData.Tagline,
        categories = menuData.Categories,
        items
    });
});

// 2號 API: 單一料理詳細資訊與專屬副餐 (/api/menu/{dishId})
app.MapGet("/api/menu/{dishId}", async (string dishId, CancellationToken cancellationToken) =>
{
    var item = await store.GetMenuItemAsync(dishId, cancellationToken);

    return item is null
        ? Results.NotFound(new { message = $"找不到料理：{dishId}" })
        : Results.Ok(item.Value);
});

// 3號 API: 建立日式料理新訂單 (/api/orders)
app.MapPost("/api/orders", async (CreateOrderRequest? request, CancellationToken cancellationToken) =>
{
    if (request == null || string.IsNullOrWhiteSpace(request.CustomerName) || string.IsNullOrWhiteSpace(request.Phone))
    {
        return Results.BadRequest(new { success = false, message = "顧客姓名與電話為必填欄位" });
    }

    if (request.Items == null || request.Items.Count == 0)
    {
        return Results.BadRequest(new { success = false, message = "訂單品項不能為空" });
    }

    var diningType = request.DiningType?.Trim();
    var tableNo = request.TableNo?.Trim();
    var address = request.Address?.Trim();

    if (diningType is not ("內用" or "外帶" or "外送"))
    {
        return Results.BadRequest(new { success = false, message = "用餐方式無效" });
    }

    if (string.Equals(diningType, "內用", StringComparison.Ordinal) && string.IsNullOrWhiteSpace(tableNo))
    {
        return Results.BadRequest(new { success = false, message = "內用訂單必須填寫桌號" });
    }

    if (string.Equals(diningType, "外送", StringComparison.Ordinal) && string.IsNullOrWhiteSpace(address))
    {
        return Results.BadRequest(new { success = false, message = "外送訂單必須填寫地址" });
    }

    var detailItems = new List<OrderDetailItem>();
    foreach (var item in request.Items)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.DishId) || item.Quantity <= 0 || item.Subtotal < 0)
        {
            return Results.BadRequest(new { success = false, message = "訂單品項資料無效" });
        }

        var menuItem = await store.GetMenuItemAsync(item.DishId, cancellationToken);
        if (menuItem is null)
        {
            return Results.BadRequest(new { success = false, message = $"找不到料理：{item.DishId}" });
        }

        var dish = menuItem.Value;
        var estimatedPrepMinutes = dish.GetProperty("details").GetProperty("prepTimeMinutes").GetInt32();
        var sideNames = item.Sides?.Select(side => side?.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToList() ?? [];

        detailItems.Add(new OrderDetailItem(
            DishName: dish.GetProperty("name").GetString()!,
            CustomPreferences: item.CustomPreferences ?? "標準風味",
            Sides: sideNames,
            EstimatedPrepMinutes: estimatedPrepMinutes,
            Quantity: item.Quantity,
            Subtotal: item.Subtotal
        ));
    }

    var createdAt = DateTimeOffset.UtcNow;
    var estimatedOrderPrepMinutes = detailItems.Max(item => item.EstimatedPrepMinutes);
    var estimatedReadyAt = createdAt.AddMinutes(estimatedOrderPrepMinutes);

    var orderDetail = new OrderDetailDto(
        OrderId: "",
        Status: "料理製作中",
        EstimatedPrepMinutes: estimatedOrderPrepMinutes,
        EstimatedReadyAt: estimatedReadyAt,
        StatusUpdatedAt: createdAt,
        CustomerName: request.CustomerName,
        Phone: request.Phone,
        DiningType: diningType,
        TableNo: tableNo ?? "無",
        Address: address ?? "",
        Note: request.Note ?? "",
        TotalAmount: request.TotalAmount,
        Items: detailItems,
        CreatedAt: createdAt
    );

    var orderId = await store.CreateOrderAsync(orderDetail, cancellationToken);

    return Results.Ok(new
    {
        success = true,
        orderId,
        message = "日式料理訂單建立成功",
        createdAt,
        estimatedPrepMinutes = estimatedOrderPrepMinutes,
        estimatedReadyAt,
        status = orderDetail.Status
    });
});

// 4號 API: 訂單狀態與明細查詢 (/api/orders/{orderId})
app.MapGet("/api/orders/{orderId}", async (string orderId, CancellationToken cancellationToken) =>
{
    var order = await store.SyncOrderStatusAsync(orderId, cancellationToken);
    if (order is not null)
    {
        return Results.Ok(order);
    }
    return Results.NotFound(new { message = $"找不到訂單：{orderId}" });
})
    .WithName("GetOrderById")
    .WithSummary("查詢訂單明細與目前狀態")
    .WithDescription("依預估製作時間同步訂單狀態後，回傳訂單明細。")
    .Produces<OrderDetailDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// 5號 API: 依預估製作時間同步訂單狀態
app.MapPatch("/api/orders/{orderId}/status", async (string orderId, CancellationToken cancellationToken) =>
{
    var order = await store.SyncOrderStatusAsync(orderId, cancellationToken);
    if (order is null)
    {
        return Results.NotFound(new { message = $"找不到訂單：{orderId}" });
    }

    return Results.Ok(new
    {
        success = true,
        orderId = order.OrderId,
        status = order.Status,
        estimatedReadyAt = order.EstimatedReadyAt,
        statusUpdatedAt = order.StatusUpdatedAt,
        message = "訂單狀態已同步"
    });
})
    .WithName("SyncOrderStatus")
    .WithSummary("依預估製作時間同步訂單狀態")
    .WithDescription("使用伺服器時間與預估完成時間更新狀態；不接受呼叫端指定狀態。")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.Run();

static string FindProjectRoot(string contentRoot)
{
    var startPaths = new[] { AppContext.BaseDirectory, contentRoot, Directory.GetCurrentDirectory() };
    foreach (var startPath in startPaths)
    {
        for (var directory = new DirectoryInfo(startPath); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "TofuShopApi.csproj")))
            {
                return directory.FullName;
            }
        }
    }

    return contentRoot;
}

// 資料模型
public record OrderSideItem(
    string? Id,
    string Name,
    int Price
);

public record CreateOrderItem(
    string DishId,
    string DishName,
    string? CustomPreferences,
    List<OrderSideItem>? Sides,
    int Quantity,
    int Subtotal
);

public record CreateOrderRequest(
    string CustomerName,
    string Phone,
    string? DiningType,
    string? TableNo,
    string? Address,
    string? Note,
    List<CreateOrderItem> Items,
    int TotalAmount
);

public record OrderDetailItem(
    string DishName,
    string CustomPreferences,
    List<string> Sides,
    int EstimatedPrepMinutes,
    int Quantity,
    int Subtotal
);

public record OrderDetailDto(
    string OrderId,
    string Status,
    int EstimatedPrepMinutes,
    DateTimeOffset EstimatedReadyAt,
    DateTimeOffset StatusUpdatedAt,
    string CustomerName,
    string Phone,
    string DiningType,
    string TableNo,
    string Address,
    string Note,
    int TotalAmount,
    List<OrderDetailItem> Items,
    DateTimeOffset CreatedAt
);

public sealed class OrderStatusBackgroundService(SqliteStore store) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        do
        {
            await store.CompleteDueOrdersAsync(DateTimeOffset.UtcNow, stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
