# 目的
使用 .NET Core 設計日式料理線上點餐系統之 Web API

# API 規格
## 1號 (菜單清單查詢)
- 路由: `/api/menu`
- 方法: GET
- 輸入: 無
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
- 測試方式: 檢查呼叫後是否回傳 HTTP 200 及包含日式料理基本清單（ID、品名、副標、分類、價格、圖片路徑、簡介）的 JSON
- 測試通過: [ ]

## 2號 (單一料理詳細資訊與專屬副餐查詢)
- 說明: 使用者點擊料理圖片或卡片時呼叫此 API，取得該料理之完整履歷與專屬副餐搭配
- 路由: `/api/menu/{dishId}`
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
        "flavorTags": ["炭火香氣", "油脂豐美", "主廚手作", "人氣 No.1"]
      },
      "customOptions": {
        "riceAmount": ["標準飯量", "增量大盛 (+0元)", "少飯 (-30g)"],
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
- 測試方式: 傳入存在的 `dishId`（例如 `dish-1` 與 `dish-2`），檢查是否回傳 HTTP 200 及該道料理的詳細食材履歷、客製化選項與其專屬的副餐清單（確認每個商品副餐不同）；若不存在則回傳 404
- 測試通過: [ ]

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
- 輸出: 
    ```json
    {
      "success": true,
      "orderId": "ORD-20261016-001",
      "message": "日式料理訂單建立成功",
      "createdAt": "2026-10-16T10:30:00Z"
    }
    ```
- 測試方式: 發送 POST 請求檢查是否建立成功，傳回 HTTP 200 或 201，且回傳唯一訂單編號
- 測試通過: [ ]

## 4號 (訂單狀態與明細查詢)
- 路由: `/api/orders/{orderId}`
- 方法: GET
- 輸入: 路徑參數 `orderId` (如: `ORD-20261016-001`)
- 輸出: 
    ```json
    {
      "orderId": "ORD-20261016-001",
      "status": "料理製作中",
      "customerName": "佐藤健太",
      "diningType": "內用",
      "tableNo": "3號桌",
      "note": "少油少鹽，感謝",
      "totalAmount": 660,
      "items": [
        {
          "dishName": "招牌特濃豚骨叉燒拉麵",
          "customPreferences": "麵硬 (博多風) / 道地日式濃味",
          "sides": ["日式唐揚炸雞 (4塊)", "招牌黃金溏心蛋"],
          "quantity": 2,
          "subtotal": 660
        }
      ],
      "createdAt": "2026-10-16T10:30:00Z"
    }
    ```
- 測試方式: 傳入已存在的 `orderId` 檢查是否回傳完整日式料理訂單明細、自選副餐與當前製作狀態（待處理/料理製作中/餐點已完成）
- 測試通過: [ ]

# 測試
每支 API 必須按照規定測試方式進行測試，通過後標記 [X]

# 其他
- 原始碼存到 `/src` 資料夾
- 編譯後存檔到 `/web/bin` 資料夾
