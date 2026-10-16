# 目的
使用 .NET Core 設計日式料理線上點餐系統之 Web API

# API 規格
## 1號 (商品清單查詢)
- 路由: `/api/menu`
- 方法: GET
- 輸入: 無
- 輸出僅包含菜單資訊及商品卡片欄位；不包含詳細食材履歷、客製化選項或副餐清單。點擊商品後，前端須另呼叫 2 號 API。
- 輸出: 
    ```json
    {
      "restaurantName": "弦月・本格日式料理屋",
      "tagline": "職人炭火炙烤・慢熬濃湯・旬鮮直送",
      "categories": ["全部料理", "人氣推薦", "經典拉麵", "和風定食"],
      "items": [
        {
          "id": "dish-1",
          "name": "特上蒲燒鰻魚飯",
          "subName": "とくじょう うなぎどん (Unagi Don)",
          "category": "人氣推薦",
          "price": 380,
          "image": "images/unagi.jpg",
          "badge": "主廚招牌",
          "description": "嚴選外銷等級頂級白鰻，經炭火慢烤三道蒲燒工法，刷上傳承百年甘醇秘醬，油脂豐腴入口即化，佐日本新潟越光米。"
        },
        {
          "id": "dish-2",
          "name": "招牌特濃豚骨叉燒拉麵",
          "subName": "とんこつ チャーシューメン (Tonkotsu Ramen)",
          "category": "經典拉麵",
          "price": 240,
          "image": "images/ramen.jpg",
          "badge": "人氣熱銷",
          "description": "熬煮18小時以上的大骨與豚脂乳化出純白濃郁湯頭，佐特製炙燒五花厚切叉燒、黃金流心溏心蛋與日本博多極細麵。"
        },
        {
          "id": "dish-3",
          "name": "極上黑毛和牛丼",
          "subName": "くろげわぎゅうどん (Wagyu Beef Don)",
          "category": "人氣推薦",
          "price": 320,
          "image": "images/wagyu.jpg",
          "badge": "極致奢華",
          "description": "嚴選日本A5級黑毛和牛薄切片，以甘甜洋蔥、清酒與特級本釀造醬油溫火慢燉，搭配生食級有機鮮蛋黃，肉質極致柔嫩細膩。"
        },
        {
          "id": "dish-4",
          "name": "極黑里肌炸豬排定食",
          "subName": "とんかつ ていしょく (Tonkatsu Set)",
          "category": "和風定食",
          "price": 260,
          "image": "images/tonkatsu.jpg",
          "badge": "定食定番",
          "description": "嚴選厚切黑豚大里肌，裹上鮮生麵包粉經兩道低溫熱油酥炸，外皮酥脆不油膩，肉質多汁飽滿，隨附高麗菜絲與秘磨芝麻豬排醬。"
        },
        {
          "id": "dish-5",
          "name": "炙燒鮭魚親子丼",
          "subName": "さけ おやこどん (Aburi Salmon Don)",
          "category": "人氣推薦",
          "price": 290,
          "image": "images/salmon.jpg",
          "badge": "當日鮮採",
          "description": "新鮮空運挪威大西洋鮭魚，表面經直火微炙釋放誘人油脂香氣，搭配晶瑩剔透、在口中爆漿的醬油漬鮭魚卵與天然紫蘇葉。"
        },
        {
          "id": "dish-6",
          "name": "大海老天婦羅蕎麥麵",
          "subName": "てんぷら そば (Tempura Soba)",
          "category": "和風定食",
          "price": 220,
          "image": "images/tempura.jpg",
          "badge": "清雅道地",
          "description": "信州二八蕎麥麵吸附甘甜優雅的鰹魚昆布柴魚高湯，佐以兩大尾金黃羽衣炸大蝦與時令南瓜、地瓜、香菇野菜天婦羅。"
        }
      ]
    }
    ```
- 測試方式: 檢查呼叫後是否回傳 HTTP 200 及包含日式料理基本清單（ID、品名、副標、分類、價格、圖片路徑、標籤、簡介）的 JSON，且商品項目不包含 `details`、`customOptions` 或 `sides`
- 測試通過: [X]

## 2號 (單一商品詳細資訊與專屬副餐查詢)
- 說明: 使用者點擊料理圖片或卡片時呼叫此 API，取得該料理之完整履歷與專屬副餐搭配
- 路由: `/api/menu/{dishId}` (例如 `/api/menu/dish-1`)
- 方法: GET
- 輸入: 路徑參數 `dishId` (如: `dish-1` 或 `dish-2`)
- 輸出: 
    ```json
    {
      "id": "dish-1",
      "name": "特上蒲燒鰻魚飯",
      "subName": "とくじょう うなぎどん (Unagi Don)",
      "category": "人氣推薦",
      "price": 380,
      "image": "images/unagi.jpg",
      "badge": "主廚招牌",
      "description": "嚴選外銷等級頂級白鰻，經炭火慢烤三道蒲燒工法，刷上傳承百年甘醇秘醬，油脂豐腴入口即化，佐日本新潟越光米。",
      "details": {
        "ingredients": ["日本靜岡特選鰻魚", "新潟縣產越光米", "百年秘傳照燒汁", "有田山椒粉"],
        "origin": "台灣在地優質外銷無毒鰻魚直送",
        "prepTime": "約 12-15 分鐘",
        "prepTimeMinutes": 15,
        "flavorTags": ["炭火香氣", "油脂豐美", "主廚手作", "人氣 No.1"]
      },
      "customOptions": {
        "riceAmount": ["標準飯量", "增量大盛 (免費)", "少飯 (-30g)"],
        "flavor": ["標準醬汁", "醬汁增量", "少醬清爽"]
      },
      "sides": [
        { "id": "side-unagi-1", "name": "日式蒲燒鰻手卷", "price": 60, "description": "香酥海苔包覆蒲燒鰻與現切小黃瓜" },
        { "id": "side-unagi-2", "name": "極品鰻魚肝清湯", "price": 40, "description": "古法鰹魚昆布高湯清甜甘潤" },
        { "id": "side-unagi-3", "name": "和風干貝茶碗蒸", "price": 35, "description": "滑嫩高湯蒸蛋佐青森帆立貝" },
        { "id": "side-unagi-4", "name": "柚子白蘿蔔漬物", "price": 25, "description": "高知縣產柚子醋清脆爽口解膩" }
      ]
    }
    ```
- 測試方式: 傳入存在的 `dishId`（例如 `dish-1` 與 `dish-2`），檢查是否回傳 HTTP 200 及該道料理的完整商品資訊、詳細食材履歷、客製化選項與專屬副餐清單（確認不同商品回傳各自的副餐）；若不存在則回傳 404
- 測試通過: [X]

## 3號 (建立日式料理新訂單)
- 路由: `/api/orders`
- 方法: POST
- 輸入: 
    ```json
    {
      "customerName": "佐藤健太",
      "phone": "0912345678",
      "diningType": "內用",
      "tableNo": "3號桌",
      "address": "",
      "note": "少油少鹽，感謝",
      "items": [
        {
          "dishId": "dish-2",
          "dishName": "招牌特濃豚骨叉燒拉麵",
          "customPreferences": "麵硬 (博多風) / 道地日式濃味",
          "sides": [
            { "id": "side-ramen-2", "name": "日式唐揚炸雞 (4塊)", "price": 65 },
            { "id": "side-ramen-5", "name": "招牌黃金溏心蛋", "price": 25 }
          ],
          "quantity": 2,
          "subtotal": 660
        }
      ],
      "totalAmount": 660
    }
    ```
- 輸入規則: `diningType` 為 `內用` 時，`tableNo` 必填；`diningType` 為 `外送` 時，`address` 必填；外帶不需要桌號或地址
- 輸出: 
    ```json
    {
      "success": true,
      "orderId": "ORD-20261016-001",
      "message": "日式料理訂單建立成功",
      "createdAt": "2026-10-16T10:30:00Z",
      "estimatedPrepMinutes": 10,
      "estimatedReadyAt": "2026-10-16T10:40:00Z",
      "status": "料理製作中"
    }
    ```
- 規則: 後端依 `items[].dishId` 從菜單取得各品項的 `prepTimeMinutes`，不得信任前端傳入的預估時間或狀態；建立訂單時保存 `estimatedPrepMinutes`、`estimatedReadyAt` 與 `statusUpdatedAt`
- 狀態碼: 欄位錯誤、未知的 `dishId`、內用未填桌號或外送未填地址時回傳 `400 Bad Request`
- 測試方式: 發送 POST 請求檢查是否建立成功，傳回 HTTP 200 或 201、唯一訂單編號及伺服器計算的預估時間；確認預估值使用菜單資料且保存於訂單；另測試內用缺少桌號、外送缺少地址及未知 `dishId` 時皆回傳 `400 Bad Request`，有效外送地址需保存於訂單
- 測試通過: [X]

## 4號 (訂單狀態與明細查詢)
- 路由: `/api/orders/{orderId}`
- 方法: GET
- 輸入: 路徑參數 `orderId` (如: `ORD-20261016-001`)
- 輸出: 
    ```json
    {
      "orderId": "ORD-20261016-001",
      "status": "料理製作中",
      "createdAt": "2026-10-16T10:30:00Z",
      "estimatedPrepMinutes": 10,
      "estimatedReadyAt": "2026-10-16T10:40:00Z",
      "statusUpdatedAt": "2026-10-16T10:30:00Z",
      "customerName": "佐藤健太",
      "diningType": "內用",
      "tableNo": "3號桌",
      "address": "",
      "note": "少油少鹽，感謝",
      "totalAmount": 660,
      "items": [
        {
          "dishName": "招牌特濃豚骨叉燒拉麵",
          "customPreferences": "麵硬 (博多風) / 道地日式濃味",
          "sides": ["日式唐揚炸雞 (4塊)", "招牌黃金溏心蛋"],
          "estimatedPrepMinutes": 10,
          "quantity": 2,
          "subtotal": 660
        }
      ]
    }
    ```
- 規則: 查詢時先依目前 UTC 時間同步訂單狀態，再回傳最新狀態與預估時間；若目前時間已達或超過 `estimatedReadyAt`，狀態為 `餐點已完成`
- 測試方式: 傳入已存在的 `orderId` 檢查是否回傳完整日式料理訂單明細、自選副餐、預估製作時間、預計完成時間及依目前時間同步後的狀態；若不存在則回傳 `404`
- 測試通過: [X]

## 5號 (依預估製作時間同步訂單狀態)
- 說明: 重新依伺服器時間與訂單預估完成時間計算並保存狀態；不接受呼叫端指定狀態
- 路由: `/api/orders/{orderId}/status`
- 方法: PATCH
- 輸入: 路徑參數 `orderId` (如: `ORD-20261016-001`)；不需要 request body
- 更新規則: 若目前時間早於 `estimatedReadyAt`，狀態維持 `料理製作中`；若目前時間已達或超過 `estimatedReadyAt`，更新為 `餐點已完成`。狀態不得由 `餐點已完成` 倒退；每次狀態實際變更時更新 `statusUpdatedAt`
- 成功輸出:
    ```json
    {
      "success": true,
      "orderId": "ORD-20261016-001",
      "status": "餐點已完成",
      "estimatedReadyAt": "2026-10-16T10:40:00Z",
      "statusUpdatedAt": "2026-10-16T10:41:00Z",
      "message": "訂單狀態已同步"
    }
    ```
- 狀態碼: 訂單不存在時回傳 `404 Not Found`；成功同步（包括狀態尚未到預計完成時間、因此維持原狀）時回傳 `200 OK`
- 測試方式: 使用可控制的測試時間，確認預計完成時間前狀態維持 `料理製作中`，到達或超過預計完成時間後更新為 `餐點已完成` 並保存；再呼叫 4號 API 確認最新狀態及 `statusUpdatedAt`；另測試不存在的 `orderId` 回傳 `404 Not Found`
- 測試通過: [X]

### 訂單預估製作時間與狀態推算規則
- 菜單詳細資料保留給畫面顯示的 `prepTime` 範圍文字，另提供整數 `prepTimeMinutes` 作為後端計算依據；若原始資料為範圍（例如「約 8-10 分鐘」），使用上限 `10` 分鐘作為保守估算。
- 目前菜單估算值：`dish-1` 15 分鐘、`dish-2` 10 分鐘、`dish-3` 12 分鐘、`dish-4` 15 分鐘、`dish-5` 10 分鐘、`dish-6` 12 分鐘。
- 訂單建立時由後端依各品項菜單資料計算並保存每筆明細的 `estimatedPrepMinutes`，以及訂單層級的 `estimatedPrepMinutes`（整數分鐘）、`estimatedReadyAt`（UTC ISO 8601 時間）與 `statusUpdatedAt`；`estimatedReadyAt = createdAt + 訂單 estimatedPrepMinutes`。`statusUpdatedAt` 是狀態最後實際寫入資料庫的 UTC 時間。不可由前端指定這些欄位。
- 多道不同料理假設可同時製作，訂單時間採所有明細 `estimatedPrepMinutes` 的最大值，而非將不同料理時間相加。例如 10 分鐘拉麵與 15 分鐘鰻魚飯同單，預估為 15 分鐘。相同料理多份視為同批製作，不按數量倍增；副餐不另加時間，視為包含在主餐估算內。若廚房實際上必須依序製作，應改採各品項時間加總，並依實際作業流程調整規則。
- 新訂單初始狀態為 `料理製作中`；預估時間到達後自動轉為 `餐點已完成`。`待處理` 不由此時間規則自動產生，若需表示排隊等待，需另定義開始製作事件與計時起點。
- 後端應由背景工作定期同步到期訂單；4號查詢及5號同步 API 也須即時計算並保存狀態，避免背景工作停機期間狀態過期。此狀態為時間估算，不代表廚房確認實際完成或顧客已取餐。

# 測試
每支 API 必須按照規定測試方式進行測試，通過後標記 [X]

# 其他
- 原始碼存到 `/src` 資料夾
- 編譯後存檔到 `/web/bin` 資料夾
- 資料庫使用 SQLite，儲存到 `/data` 資料夾
