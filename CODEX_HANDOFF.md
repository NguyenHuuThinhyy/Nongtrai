# CẬP NHẬT ƯU TIÊN — 24/09/2026: TRẠNG THÁI MÃ MỚI NHẤT

Các mục cũ bên dưới là lịch sử và có thể sai về phiên bản lưu, hotbar hoặc tính năng. Khi mâu thuẫn, kiểm tra code trước.

- FarmSave schema v11 đọc v2–v11. Ghi túi đồ 36 ô, vật phẩm rơi, thú hoang, vị trí hai map, kho/rương, sức khỏe và hướng dẫn. Không tự lưu; sáng tạo LV99 dùng bản RAM.
- PlayerInteraction dùng ray từ dấu + để click trái tương tác trên nông trại; E mở bản đồ nhiệm vụ tương tác. ExplorationWorld hiển thị dấu + ở cả hai map. IslandManager nhớ và lưu vị trí từng map, Tab trở về đúng chỗ đã rời.
- FarmCraftOrders có 15 công thức trong Assets/StreamingAssets/crafting.json, danh sách cuộn và icon; hộp thư năm đơn/ngày với độ khó/thời hạn. FarmShop có ba trang 24 mặt hàng. FarmItemIconLibrary vẽ icon và FarmBuildingSystem có 13 loại khối, gồm rương và đống lửa.
- FarmNoticeBoard hiển thị sơ đồ tọa độ thu nhỏ và bản đồ E có marker click được cho ruộng, thú đói, đơn đủ hàng, rương và nguy hiểm. FarmStorage tạo nhà kho và rương khám phá rải thưa; AdventureWolves có sói đêm sợ đuốc/đống lửa và về hang khi sáng. FarmAnimal chờ ở ổ khi có sản phẩm, click ổ lấy, click con chỉ bế.
- TimeManager có một ngày 1080 giây/18 phút, tỉ lệ nắng/mưa/sương/bão 67/15/14/4%. Thông số 10 phút trong phần cũ đã lỗi thời.
- Build release ở Logs/build-release-20260924-den.log đã thành công; runtime smoke Logs/smoke-release-20260924-den.log có FARM_CROPS_SMOKE_OK và thoát 0. Build Windows ở Builds/Windows/NongTrai.exe. Ảnh preview nằm ngoài gói phát hành, trong Logs/Previews-20260924-final.

---
# CẬP NHẬT ƯU TIÊN — SỬA ĐÀO / NHẢY / HƯỚNG DẪN TAB

- ExplorationWorld.UpdateMiningRay dùng mask bỏ layer8 Player và IgnoreTriggers, ray24m từ camera nhưng kiểm tra tầm6m từ player. Sửa ray TPS tự trúng nhân vật và tầm8m tính từ camera cũ quá ngắn. Runtime và smoke dùng chung đường ray/hold .55s; không chỉ test MineCell trực tiếp.
- Bỏ OnGUI của mining, dùng TMP Canvas: tâm ngắm khi khám phá, tiến độ %, hướng dẫn khối quá xa/khu cổng/tầng đáy. FarmHudV2 thêm nhãn/nút Tab đổi map và nhắc Space.
- PlayerSettings jumpHeight tăng .8 ->1.6 trong class lẫn asset. Smoke giả lập Keyboard W+Space qua collider cao1m, kiểm tra vị trí vượt qua và đỉnh nhảy.
- Schema save vẫn v8, không đổi inventory/seed/chunk persistence.

---

# CẬP NHẬT ƯU TIÊN — MAP SINH LIÊN TỤC (v8)

- ExplorationWorld thay mesh cố định bằng Dictionary<Vector2Int,Chunk>, chunk16x16, cao32; radius2 và vùng giữ thêm1. Một chunk bổ sung/frame, đồng bộ3x3 collider khi dịch chuyển. Gỡ mesh/GameObject xa; chỉ giữ seed và HashSet<Vector3Int> các ô đào. Không sinh thế giới mới khi quay lại.
- GeneratorVersion1, seed ngẫu nhiên nếu chưa có; giữ core48x48 phiên bản trước, bên ngoài sinh đồng cỏ/cát/tuyết và hang/quặng bằng seed. Tọa độ âm dùng floor division.
- Map khám phá chuyển lên y1000 để mở rộng bốn hướng không đè farm. Origin=(175.5,996,-24.5), spawn=(200,1000.4,-20). FarmPlayer nhận biết bằng y>500; phục hồi khi dưới990. Tuyệt đối không dùng x>100 để phân biệt map mới; chỉ dùng trong migration cũ.
- FarmSave v8 migrate vị trí player/trees/buildings của map cũ +1000y; decode removed IDs v7 với stride18x48, giữ tiến độ/blueprint. Mọi thay đổi vẫn manual-save, creative không ghi.
- Biome vật liệu mới chưa có ID túi riêng: cát/tuyết/đất cho item25, đá21, quặng13. Không có sinh tồn/quái/multiplayer. Cây táo khởi đầu vẫn sáu cây; không có hệ rừng streaming.
- Test mới: chunk xa/tọa độ âm, cap49 sau stream, seed tái tạo, seed khác tạo terrain khác, đào lưu/tải, v7 cell migration và migration vị trí player. Thông tin hữu hạn48x48 ở các mục cũ phía dưới đã lỗi thời.
- Rủi ro/khoản chưa làm: Unity floating-point ở tọa độ rất xa, build nhiều khối vẫn giữ GameObject độc lập, chưa greedy mesh/job thread. Không quảng cáo vô hạn tuyệt đối hay đảm bảo FPS máy yếu.

---

# CẬP NHẬT ƯU TIÊN — 23/09/2026: CHỈ HAI MAP

Thông tin dưới mục này về bốn đảo, NPC, đấu giá và minigame đã lỗi thời. Ưu tiên code hiện tại và README/CHOI_GAME.

- Yêu cầu mới: nông trại theo hướng Avatar/Stardew/Hay Day và một map khám phá đào/xây khối; bỏ phần đảo dư.
- IslandManager chỉ chấp nhận map 0 (nông trại) và 1 (khám phá), đều mở từ đầu; Tab/cổng. Điểm đến (0,.4,14) và (200,.4,-20).
- FarmIslandsBuilder dựng hai cổng, không dựng Trung tâm/Thần bí/Công nghiệp. Đã xóa MysteryAltar, IslandTrap, IslandNpc, IslandGameKiosk, IslandAuctionKiosk và meta.
- ExplorationWorld tạo mesh/collider voxel hữu hạn 48x48x18, đồi, đá, quặng, hốc ngầm. Giữ chuột trái đào trong tầm 8m khi ngoài chế độ xây; G dùng FarmBuildingSystem hiện có. Bảo vệ tầng đáy và cổng. Đào 30 khối mở bản vẽ lò nung/đèn. Đất -> item25, đá ->21, quặng ->13. Gỗ vẫn cây táo/shop.
- Cả sáu máy trên nông trại; cấp thường tối đa99, bỏ thử thách mở giới hạn cấp.
- FarmSave v7 lưu ExplorationState (removed cell IDs/minedCount); đọc v2..7. Bản cũ hoàn vật liệu công trình x>100, giữ công trình farm; đưa người ở đảo cũ về cổng khám phá. Giữ bản vẽ cũ. IslandState giữ trường cũ chỉ để đọc tương thích/hoàn hàng đấu giá.
- BuildSmokeCheck đã chuyển test đảo cũ sang hai map, đào30, bảo vệ spawn, lưu/tải terrain. Giữ test cây/thú/nước/đơn/creative.
- Chỉ lưu thủ công. Creative LV99 không được ghi file lưu. Không cấp tiền vô hạn.
- Root D:/GAME_NongTrai là dự án Unity, clone Git là thư mục Nongtrai; đồng bộ sau build. Unity6000.3.22f1, lệnh build trong README.
- Giới hạn hiện tại: map hữu hạn, chưa có quái/sinh tồn/multiplayer/thế giới vô hạn. Mesh được dựng lại khi đào (cần chia chunk nếu tăng kích thước). Đánh giá cảm giác chơi dài hạn và các máy yếu cần chơi thử thực tế.

---

# CODEX HANDOFF — Nông Trại / First Harvest

> **CẬP NHẬT 2026-09-23:** Kế hoạch từng được ghi là “chưa triển khai” trong tài liệu này đã được triển khai sau khi handoff được tạo. Trạng thái đúng hiện nằm trong code: `FarmWaterSystem.cs`, `FarmCraftOrders.cs`, `CreativeModeManager.cs`, bản lưu v5, hotbar bắt buộc, cây 2/3/5 phút, túi 20 món và scene/build mới. README/CHOI_GAME cùng lịch sử Git mới hơn là nguồn chính xác hơn các đoạn trạng thái cũ bên dưới.

Trạng thái được kiểm tra tại `D:\GAME_NongTrai` sau commit `1cc34cc` trên nhánh `main`. Đây là **bản ghi hiện trạng**, không phải xác nhận rằng kế hoạch ở mục 9 đã được triển khai. Đường dẫn tương đối trong tài liệu này tính từ `D:\GAME_NongTrai` trừ khi ghi rõ khác. `D:\GAME_NongTrai\Nongtrai` là bản sao Git riêng; thư mục gốc mới là Unity project đang được build. Trong lần kiểm tra handoff, nội dung `Assets`, `Packages`, `ProjectSettings`, `README.md`, `CHOI_GAME.md`, `prompt.MD` và ảnh tham chiếu ở hai nơi giống nhau; Git clone sạch trước khi tạo tài liệu này.

# 1. PROJECT OVERVIEW

- **Project:** game nông trại 3D chơi đơn cho Windows, tên sản phẩm `Nong Trai - First Harvest`, hãng trong Unity `Nong Trai Studio`.
- **Công nghệ đã xác minh:** Unity `6000.3.22f1`, C#, URP `17.3.0`, Input System `1.20.0`, Cinemachine `3.1.5`, uGUI `2.0.0` và TextMeshPro được cung cấp qua uGUI cùng TMP Essentials trong `Assets/TextMesh Pro`.
- **Mục tiêu lâu dài:** trồng cây, chăn nuôi, thu thập/chế biến, mở vùng đất và đảo, tương tác NPC, có thử thách theo thời gian/mùa. Yêu cầu mới nhất là làm vòng chơi có độ khó hơn, thêm nước, chế tạo, hộp thư đơn hàng và chế độ sáng tạo phục vụ test; **những tính năng mới nhất này chưa được code**.
- **Cách hoạt động hiện tại:** một scene `Assets/Farm/Scenes/Farm.unity` chứa nông trại và ba đảo khác trong cùng world, không phải bốn scene riêng. Người chơi đi/nhảy, tương tác vật gần bằng E, trồng ba cây, nuôi bò/heo/cừu/gà, vận hành máy, đi đảo bằng Tab/cổng và lưu thủ công trong menu Esc. Bản build Windows chạy không cần Unity trên máy đích nếu giữ đủ thư mục build.
- **Nguồn hình ảnh:** `ChatGPT Image Sep 20, 2026, 07_13_45 PM.png` ở root là ảnh tham chiếu; phần lớn model hiện dựng từ Unity primitives. `prompt.MD` là yêu cầu ban đầu, **không** phải đặc tả ưu tiên hơn yêu cầu mới hoặc code đang chạy.

# 2. CURRENT STATE

## Đã hoàn thành và có trong code/build hiện tại

- Trồng lúa mì/cà chua/đậu nành trên 80 ô đất, 4 vùng (20 ô/vùng); cày, gieo, tưới, thu hoạch, giai đoạn cây và sản lượng. Hiện `growthSeconds` là **35/45/55 giây**, 3 sản phẩm/ô; đất khô **ngừng lớn**. Shop bán cả ba gói 5 hạt với cùng giá **10 xu/gói**; cả ba nông sản bán **10 xu/đơn vị**. Đây là trạng thái cũ cần đổi ở mục 9.
- Hotbar 9 ô có chọn bằng phím 1–9/cuộn chuột và viền ô đang chọn. **Chỉ ba ô hạt đầu thực sự đổi hạt đang gieo**; thao tác E ở ruộng hiện chọn cuốc/tưới/liềm theo trạng thái ô đất, không theo hotbar.
- Shop, túi đồ 16 loại hàng (ID 0–15), bán từng loại/bán hết; 4 loài vật có độ no/vui, cho ăn bằng F, sản xuất trứng/sữa/len/thịt; chuồng riêng, cổng mở/đóng, nâng cấp và giới hạn gà 5 con/chuồng. Chuột trái nhấc vật nuôi, chuột phải thả vào đúng chuồng.
- Máy cối xay/lò bánh/thùng ủ/máy ép/xưởng cưa/lò nung; 6 công thức trong JSON, hàng đợi tối đa 5 lượt trên mỗi máy, nguyên liệu trừ khi xếp việc; lò nung cần bản vẽ từ Đảo Thần Bí.
- Mua thêm vùng đất theo LV/xu, nâng cuốc/bình tưới/liềm theo ba bậc với số ô xử lý 1/3/5; nhạc/âm thanh hành động, hạt khi thu hoạch, số nổi khi bán, menu chính/tạm dừng/cài đặt âm lượng.
- Đồng hồ 1 ngày game = 600 giây đời thực, bốn mùa × 28 ngày, năm, nắng/mưa/sương mù/bão, đổi ánh sáng và màu cỏ/lá. Mưa/bão tự tăng ẩm ô đất. Có câu đố 25 giây khi bão: đúng tránh thiệt hại, sai/hết giờ làm mất 30–80% sản phẩm và trì hoãn máy. Ngủ tại giường từ 18:00 đến trước 06:00 để sang sáng.
- HUD Canvas Scaler tham chiếu 1920×1080, TMP cho HUD/hotbar/túi đồ, một số panel khác còn dùng uGUI `Text`; quét tương tác gần qua `Physics.OverlapSphereNonAlloc`, gợi ý TextMeshPro trong thế giới, viền chọn. Không còn tâm ngắm dấu cộng cố định.
- Bốn đảo trong cùng scene: Nông Trại, Trung Tâm (NPC kết bạn qua trò chuyện, chợ đấu giá NPC mô phỏng chơi đơn, minigame), Thần Bí (mê cung/bẫy/di tích mở level cap 5→10 và bản vẽ), Công Nghiệp (tài nguyên và máy). Du hành Đảo Thần Bí cần LV3, Công Nghiệp cần LV4.
- Bản lưu JSON v4 chỉ ghi khi người chơi bấm **Lưu game**. Thoát không tự lưu; khởi động tải bản lưu nếu có. Loader đọc v2/v3/v4. Trạng thái gồm tiền, hạt, nông sản, vật nuôi/chuồng, tiến độ đất/dụng cụ, máy, thời gian/thời tiết, đảo, vị trí và tài nguyên.
- Bản Windows hiện tại và hai ZIP mới nhất trong `DongGoi` là gói **Islands**. Các ZIP cũ hơn trong cùng thư mục là bản cũ, không đại diện trạng thái hiện tại.

## Đang làm / chưa làm

- Yêu cầu triển khai cuối cùng đã được chốt thành kế hoạch nhưng lượt thi hành vừa bắt đầu thì bị ngắt **trước mọi chỉnh sửa code**. Không có `WaterSystem`, trạm tưới, bàn chế tạo, hộp thư, đơn hàng mới, mở công thức bằng đơn, chế độ sáng tạo, bay hay vật lý di chuyển mới trong code hiện tại.
- Thư mục gốc chỉ đang được thêm tài liệu handoff này; **không** coi tài liệu là triển khai tính năng.

## Hoạt động ổn định đã quan sát

- Build gần nhất ghi `FARM_M1_BUILD_OK` trong `Logs/build-release.log`.
- Smoke build Windows gần nhất ghi đủ bảy mốc `FARM_INVENTORY_ANIMALS_OK`, `FARM_SAVE_OK`, `FARM_SHOP_GATE_OK`, `FARM_ANIMALS_SIGNS_OK`, `FARM_EXPANSION_OK`, `FARM_ISLANDS_TIME_OK`, `FARM_CROPS_SMOKE_OK` trong `Logs/smoke-release.log`, không có dòng `Exception`, `not found` hoặc `Error` trong lần lọc cuối. Đây là kết quả test tự động hiện có, **không** xác nhận mọi ca người chơi hoặc các tính năng mới.
- Ảnh smoke 1280×720 cho thấy nhãn chữ khổng lồ và biển chữ ngược từng báo trước đó không còn tái hiện. Khả năng hiển thị ở mọi độ phân giải khác: **CHƯA XÁC MINH**.

# 3. ARCHITECTURE

- **Entry point runtime:** Unity mở `Assets/Farm/Scenes/Farm.unity`. `FarmProjectBuilder.CreateScene()` dựng lại scene, `BuildWindows()` gọi nó rồi build Windows Development. Scene có `FarmPlayer` + `FarmInput` + `FarmCamera`, `FieldManager`, Canvas `FarmHud` và các component hệ thống gắn với Canvas bởi `FarmShopBuilder.BuildShop()`.
- **Wiring:** `FarmProjectBuilder.BuildPlayer()` tạo player/camera/field; `BuildHud()` tạo Canvas/menu rồi `BuildShop()` gắn `FarmShop`, `FarmInventory`, `FarmSave`, `FarmExpansion`, `TimeManager`, `FarmProcessing`, `DisasterPuzzleManager`, `FarmBarnMenu`, `FarmHudV2`, `IslandManager`; component audio tạo riêng. Nhiều hệ thống lấy singleton `Instance`, một số tham chiếu được gán trực tiếp trong builder.
- **Đầu vào & tương tác:** `FarmInput` khai báo WASD, mouse look, Shift, Space, V, E, Esc. `FarmPlayer` dùng `CharacterController.Move` mỗi `Update`; `PlayerInteraction` đọc B/I/M/N/P/Tab/F và chuột, quét collider gần; mỗi đối tượng thực hiện `IInteractable.InteractionHint`, `CanInteract`, `Interact`, `SetHighlighted`. `InteractionOutline` vẽ LineRenderer quanh collider. `FarmHudV2` tự xử lý phím số/cuộn cho hotbar.
- **Ruộng:** `FieldManager` giữ `CropDefinition[]`, crop được chọn và số đã thu hoạch, tick toàn bộ `FarmPlot` mỗi khoảng 1 giây. `FarmExpansion.Work()` chọn hành động từ `PlotState`, tìm các ô gần, xử lý phạm vi 1/3/5, trừ hạt, cộng nông sản và XP. `FarmPlot.Tick()` hiện chỉ tăng tiến độ khi `Moisture > 0` và nước đầy cạn sau 75 giây; `TimeManager` bổ sung nước khi mưa/bão và tua cây khi ngủ.
- **Túi đồ/economy:** item ID 0–2 đọc/ghi `FieldManager.Harvested`; ID 3 là `FarmShop.Fruit`; ID 4–15 ánh xạ `FarmInventory.AnimalProducts[item-4]` (mảng dài 12). Shop giữ tiền/hạt/thức ăn; `FarmProcessing` đọc `Assets/StreamingAssets/recipes.json` và dùng item ID cố định. Thay đổi ID/mảng/giá phải cập nhật cùng nhau và giữ đọc bản lưu cũ.
- **Thời gian/đảo:** `TimeManager` điều khiển ngày, mùa, thời tiết, ánh sáng, mưa và ngủ; `DisasterPuzzleManager` xử lý bão. `IslandManager` quản lý di chuyển, NPC, chợ, minigame, di tích và `IslandState`. Các đảo nằm x≈0/200/400/600 trong một scene; `IslandManager.Travel()` kiểm tra LV rồi gọi `FarmPlayer.Teleport()`.
- **Lưu:** `FarmSave.Save()` serialize `SaveData` v4 bằng `JsonUtility`, viết tệp `.tmp` rồi `File.Replace`/`File.Move`, có `.bak` khi thay thế. `FarmSave.Start()` tự `Load()` khi không chạy smoke. `FarmHud.SaveNow()` là nút lưu duy nhất. Smoke dùng `pathOverride` trong thư mục cache tạm và dọn tệp test.
- **UI:** `FarmUi` tạo panel/nút uGUI và nhãn TMP; font TMP lấy từ `Assets/Farm/Resources/FarmFont.asset`, TMP Essentials ở `Assets/TextMesh Pro`. Canvas Scaler 1920×1080 Match 0.5. Một số panel cũ vẫn dùng `LegacyRuntime.ttf`.
- **Dependency vận hành:** URP asset/render settings, Input System, Cinemachine, TextMeshPro resources, Unity scene/prefab/`.meta` GUID, JSON recipe ID, Windows build data folder. Không có server/multiplayer cho NPC/chợ.

# 4. IMPORTANT FILES

Quy ước cột cuối: **thận trọng** = chỉ sửa sau khi xem quan hệ/test; **có thể mở rộng** = phù hợp thêm chức năng nhưng vẫn giữ API và compatibility. Không có file nào nên rewrite toàn bộ chỉ để làm task mới.

| Path | Vai trò, API và quan hệ | Mức sửa |
|---|---|---|
| `Assets/Farm/Scenes/Farm.unity` | Scene runtime duy nhất, serialize phần lớn world/HUD/player; được `FarmProjectBuilder.CreateScene()` ghi đè khi build. | Rất thận trọng; ưu tiên sửa builder. |
| `Assets/Farm/Editor/FarmProjectBuilder.cs` | `CreateScene`, `BuildWindows`, cấu hình URP/TMP, tạo ruộng/hồ/nhà/player/HUD; gọi partial builder. | Thận trọng: rebuild ghi scene/prefab/asset. |
| `Assets/Farm/Editor/FarmShopBuilder.cs` | `BuildShop`, wire toàn bộ hệ thống và dựng shop/chuồng/cây. Nơi phải wire hệ thống mới. | Có thể mở rộng, kiểm tra thứ tự khởi tạo. |
| `Assets/Farm/Editor/FarmIslandsBuilder.cs` | Dựng đảo, cổng, NPC, bẫy và resource node ở cùng scene. | Thận trọng với tọa độ/collider/điểm đến. |
| `Assets/Farm/Editor/FarmCharacters.cs` | Dựng hình nhân vật và vật nuôi/prefab primitive. | Có thể sửa hình, giữ prefab/script. |
| `Assets/Farm/Scripts/Player/FarmInput.cs` | InputAction cho di chuyển/nhìn/chạy/nhảy/E/Esc; cần mở rộng cho bay. | Thận trọng với phím đang dùng. |
| `Assets/Farm/Scripts/Player/FarmPlayer.cs` | `CharacterController`, pause, `Teleport`, trọng lực, phục hồi khi rơi; liên quan IslandManager/FarmSave/trap. | Thận trọng, cần smoke chuyển động. |
| `Assets/Farm/Scripts/Player/FarmCamera.cs` | Cinemachine góc nhìn thứ nhất/thứ ba và chống camera xuyên tường. | Giữ hai chế độ. |
| `Assets/Farm/Scripts/Player/PlayerSettings.cs` và `Assets/Farm/Data/PlayerSettings.asset` | Tốc độ 4/7, nhảy 0.8, gravity -22, camera; asset runtime. | Giữ đồng bộ code/asset. |
| `Assets/Farm/Scripts/Interaction/IInteractable.cs` | Interface 4 thành phần và `InteractionOutline`. Các object world triển khai interface. | Không đổi chữ ký tùy tiện. |
| `Assets/Farm/Scripts/Interaction/PlayerInteraction.cs` | Quét mục tiêu `OverlapSphereNonAlloc`, tương tác E, phím panel, F, nhấc/thả vật nuôi, nhãn world TMP. | Có thể thêm route; tránh làm vỡ tương tác cũ. |
| `Assets/Farm/Scripts/Interaction/IslandPortal.cs` | Cổng gọi `IslandManager.Travel(destination)`. | Giữ đích và level thường. |
| `Assets/Farm/Scripts/Crops/CropDefinition.cs` và `Assets/Farm/Data/Crop0.asset`…`Crop2.asset` | Định nghĩa tên, `growthSeconds`, yield, màu; builder hiện gán lại 35/45/55. | Sửa cả builder khi đổi thời gian. |
| `Assets/Farm/Scripts/Crops/FarmPlot.cs` | `PlotState`, `Work`, `Tick`, `AddMoisture`, `Restore`, hình cây; bị ảnh hưởng bởi nước/khô/chọn tool. | Thận trọng với save/smoke. |
| `Assets/Farm/Scripts/Crops/FieldManager.cs` | Danh sách cây, crop chọn, `Harvested`, tick ruộng. | Giữ ánh xạ crop ID. |
| `Assets/Farm/Scripts/Core/FarmExpansion.cs` | LV/XP, 4 vùng đất, nâng dụng cụ, `ToolRadius`, `Work`. Hiện không kiểm tra hotbar. | Có thể mở rộng, giữ region unlock. |
| `Assets/Farm/Scripts/Core/TimeManager.cs` | Đồng hồ 600s/ngày, mùa/thời tiết/mưa/ngủ; cần đồng bộ trạm tưới/đơn ngày mới. | Thận trọng với pause và ngủ. |
| `Assets/Farm/Scripts/Core/DisasterPuzzleManager.cs` | Câu đố bão và thiệt hại. | Tránh thay đổi ngẫu nhiên không liên quan. |
| `Assets/Farm/Scripts/Core/FarmProcessing.cs` và `Assets/StreamingAssets/recipes.json` | Công thức máy, hàng đợi, item ID, metal blueprint; cần nối unlock theo đơn nhưng không biến bàn chế tạo thành máy cũ. | Thận trọng với queue/save. |
| `Assets/Farm/Scripts/Core/IslandManager.cs` | Bản đồ, `Travel`, NPC, đấu giá, minigame, puzzle/bản vẽ; cần bỏ gate LV chỉ trong mode sáng tạo. | Không bỏ gate của mode thường. |
| `Assets/Farm/Scripts/Core/FarmSave.cs` | SaveData v4, `Save`, `Load`, đường lưu, tương thích v2/v3/v4; nơi thêm schema v5/mode. | Rất thận trọng; không auto-save. |
| `Assets/Farm/Scripts/Core/BuildSmokeCheck.cs` | Chạy test trong player khi có `-farmSmokeCheck`, viết ảnh preview và log mốc FARM_*. | Cập nhật assertion khi thay balance. |
| `Assets/Farm/Scripts/UI/FarmHud.cs` | Main/pause/settings/instructions/save/resume/quit; nơi thêm chọn creative và vô hiệu lưu. | Giữ menu Esc/lưu thủ công. |
| `Assets/Farm/Scripts/UI/FarmHudV2.cs` | HUD TMP và hotbar 9 ô, `SelectedSlot`; hiện chỉ ô 1–3 gọi `FieldManager.Select`. | Giữ layout/slot ID khi gate action. |
| `Assets/Farm/Scripts/UI/FarmInventory.cs` | 16 item ID, giá, Add/Remove/Count/Sell, lưới 4×4; sẽ cần thêm hàng và cuộn. | Rất thận trọng với index/save. |
| `Assets/Farm/Scripts/UI/FarmShop.cs` | Tiền, hạt, thức ăn, giá gói hạt, mua vật nuôi/cây/chuồng. | Giữ giao dịch và seed array. |
| `Assets/Farm/Scripts/UI/FarmUi.cs` | Factory UI, nhãn TMP/legacy. | Có thể mở rộng, giữ font TMP. |
| `Assets/Farm/Scripts/Animals/` | FarmAnimal, AnimalPen, AnimalCarry, EggNest, PaddockGate, AnimalSpecies. | Giữ sức chứa/sản phẩm/nhấc thả. |
| `Assets/Farm/Resources/FarmFont.asset` và `Assets/TextMesh Pro/` | Font game và TMP Essentials đã phải thêm để bản build không lỗi. | Không xóa/đổi GUID tùy tiện. |
| `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt` | Phiên bản dependency/Unity. | Không nâng version tùy tiện. |
| `README.md`, `CHOI_GAME.md` | Chạy/build và hướng dẫn hiện tại. | Cập nhật sau khi code mới chạy thật. |

# 5. CHANGES MADE IN THIS THREAD

**Nguồn xác minh:** Git có bốn commit tuần tự `06edf60` → `aacce4f` → `fae66b0` → `1cc34cc`; `origin/main` hiện là `1cc34cc`. Bản tóm tắt trước compaction và lịch sử commit cho thấy chuỗi chức năng dưới đây. Việc một file trong commit đầu tiên có được tạo trong chính task Codex này hay từ bước chuẩn bị ban đầu: **CHƯA XÁC MINH**. Danh sách A/M/D đầy đủ do Git ghi được chép nguyên trạng ở cuối mục này. `A` = tạo, `M` = sửa; **không có `D`** trong lịch sử bốn commit đã kiểm tra. Các file `.meta` và TMP Essentials được giữ trong manifest vì Unity cần GUID, dù không liệt kê chúng riêng trong mục 4.

1. `06edf60` — tạo dự án Unity cơ bản: cấu hình/packages/scene/asset, builder, crop, FieldManager, FarmPlot, player/input/camera, farm animal, HUD, smoke test, README/CHOI_GAME và ảnh tham chiếu. Lý do: xây game nông trại đầu tiên từ prompt/ảnh.
2. `aacce4f` — tạo `FarmShopBuilder.cs`, prefabs vật nuôi/cây, `AnimalCarry`, `AnimalPen`, `AnimalSpecies`, `EggNest`, `PaddockGate`, `FarmSave`, `FruitTree`, `FarmInventory`, `FarmShop`, `ShopCounter`; sửa builder/scene/FarmAnimal/FarmPlot/PlayerInteraction/HUD/docs/smoke. Lý do: shop, chuồng riêng, sản phẩm vật nuôi, túi đồ, tương tác và lưu.
3. `fae66b0` — sửa `FarmSave.cs`, `FarmProjectBuilder.cs`, scene, prefabs và docs. Lý do: bỏ lưu khi thoát; chỉ bấm nút **Lưu game** mới ghi.
4. `1cc34cc` — tạo `FarmIslandsBuilder.cs`, vật liệu đảo, `FarmFont.asset`, `DisasterPuzzleManager`, `FarmAudio`, `FarmEffects`, `FarmExpansion`, `FarmProcessing`, `IslandManager`, `TimeManager`, `FarmBed`, `FishingPier`, `IInteractable`, các MonoBehaviour đảo/tài nguyên, `FarmBarnMenu`, `FarmHudV2`, `FarmUi`, JSON công thức và TMP Essentials. Sửa builder/wiring, scene, prefab, các hệ thống động vật/ruộng/save/tương tác/player/HUD/shop/inventory/docs/packages. Lý do: bốn đảo, NPC/chợ chơi đơn, chu kỳ ngày-mùa-thời tiết, bão, TMP HUD, chế biến/đất/dụng cụ và giao diện.

**Sửa lỗi trong commit cuối được ghi nhận qua quá trình build/smoke:** tách từng MonoBehaviour của đảo ra file `.cs` đúng tên để Unity không báo Missing Script; nhập TMP Essentials và tạo FarmFont để hết lỗi TMP runtime; thay truy cập `worldLabel.textContainer` (API trả null) bằng `rectTransform`, giảm kích thước nhãn world để hết chữ phóng rất lớn; thay ký tự đồng xu không có glyph bằng `XU`; chỉnh sáng bình minh; biển có mặt trước/sau hiển thị đúng trong ảnh smoke. Các thử nghiệm trung gian tạo log/ảnh trong thư mục ignored, không phải source cuối.

**Không có thay đổi source sau `1cc34cc`:** lượt yêu cầu triển khai nước/chế tạo/creative mới chỉ gửi commentary rồi bị ngắt. Tài liệu handoff này là file mới duy nhất của lượt hiện tại, nằm ở project root ngoài Git clone. `IslandObjects.cs` là tên file trung gian được nhắc trong bản tóm tắt quá trình sửa Missing Script, hiện không tồn tại trong source và không xuất hiện trong bốn commit; việc nó từng được tạo rồi xóa ở trạng thái chưa commit là **CHƯA XÁC MINH bằng Git**. Danh sách Git phía dưới chỉ khẳng định không có file **đã commit** bị xóa.

<details><summary>Manifest A/M/D chính xác từ 4 commit Git — gồm Unity .meta và TMP assets</summary>

```text
COMMIT 06edf60 Add Unity farming game with crops, player and roaming animals

A	.gitignore
A	Assets/DefaultVolumeProfile.asset
A	Assets/DefaultVolumeProfile.asset.meta
A	Assets/Farm.meta
A	Assets/Farm/Art.meta
A	Assets/Farm/Art/DepthFont.shader
A	Assets/Farm/Art/DepthFont.shader.meta
A	Assets/Farm/Data.meta
A	Assets/Farm/Data/Crop0.asset
A	Assets/Farm/Data/Crop0.asset.meta
A	Assets/Farm/Data/Crop1.asset
A	Assets/Farm/Data/Crop1.asset.meta
A	Assets/Farm/Data/Crop2.asset
A	Assets/Farm/Data/Crop2.asset.meta
A	Assets/Farm/Data/PlayerSettings.asset
A	Assets/Farm/Data/PlayerSettings.asset.meta
A	Assets/Farm/Editor.meta
A	Assets/Farm/Editor/FarmCharacters.cs
A	Assets/Farm/Editor/FarmCharacters.cs.meta
A	Assets/Farm/Editor/FarmProjectBuilder.cs
A	Assets/Farm/Editor/FarmProjectBuilder.cs.meta
A	Assets/Farm/Materials.meta
A	Assets/Farm/Materials/Animal black.mat
A	Assets/Farm/Materials/Animal black.mat.meta
A	Assets/Farm/Materials/Animal ivory.mat
A	Assets/Farm/Materials/Animal ivory.mat.meta
A	Assets/Farm/Materials/Barn red.mat
A	Assets/Farm/Materials/Barn red.mat.meta
A	Assets/Farm/Materials/Cheeks.mat
A	Assets/Farm/Materials/Cheeks.mat.meta
A	Assets/Farm/Materials/Denim.mat
A	Assets/Farm/Materials/Denim.mat.meta
A	Assets/Farm/Materials/Eyes and boots.mat
A	Assets/Farm/Materials/Eyes and boots.mat.meta
A	Assets/Farm/Materials/Farmer denim.mat
A	Assets/Farm/Materials/Farmer denim.mat.meta
A	Assets/Farm/Materials/Farmer shirt.mat
A	Assets/Farm/Materials/Farmer shirt.mat.meta
A	Assets/Farm/Materials/Farmer skin.mat
A	Assets/Farm/Materials/Farmer skin.mat.meta
A	Assets/Farm/Materials/Foliage.mat
A	Assets/Farm/Materials/Foliage.mat.meta
A	Assets/Farm/Materials/Grass.mat
A	Assets/Farm/Materials/Grass.mat.meta
A	Assets/Farm/Materials/Hair.mat
A	Assets/Farm/Materials/Hair.mat.meta
A	Assets/Farm/Materials/Ivory.mat
A	Assets/Farm/Materials/Ivory.mat.meta
A	Assets/Farm/Materials/Path.mat
A	Assets/Farm/Materials/Path.mat.meta
A	Assets/Farm/Materials/Pig pink.mat
A	Assets/Farm/Materials/Pig pink.mat.meta
A	Assets/Farm/Materials/Pocket blue.mat
A	Assets/Farm/Materials/Pocket blue.mat.meta
A	Assets/Farm/Materials/Pond.mat
A	Assets/Farm/Materials/Pond.mat.meta
A	Assets/Farm/Materials/Roof.mat
A	Assets/Farm/Materials/Roof.mat.meta
A	Assets/Farm/Materials/SignText.mat
A	Assets/Farm/Materials/SignText.mat.meta
A	Assets/Farm/Materials/Silo.mat
A	Assets/Farm/Materials/Silo.mat.meta
A	Assets/Farm/Materials/Skin.mat
A	Assets/Farm/Materials/Skin.mat.meta
A	Assets/Farm/Materials/Soil.mat
A	Assets/Farm/Materials/Soil.mat.meta
A	Assets/Farm/Materials/Straw.mat
A	Assets/Farm/Materials/Straw.mat.meta
A	Assets/Farm/Materials/Timber.mat
A	Assets/Farm/Materials/Timber.mat.meta
A	Assets/Farm/Prefabs.meta
A	Assets/Farm/Scenes.meta
A	Assets/Farm/Scenes/Farm.unity
A	Assets/Farm/Scenes/Farm.unity.meta
A	Assets/Farm/Scripts.meta
A	Assets/Farm/Scripts/Animals.meta
A	Assets/Farm/Scripts/Animals/FarmAnimal.cs
A	Assets/Farm/Scripts/Animals/FarmAnimal.cs.meta
A	Assets/Farm/Scripts/Core.meta
A	Assets/Farm/Scripts/Core/BuildSmokeCheck.cs
A	Assets/Farm/Scripts/Core/BuildSmokeCheck.cs.meta
A	Assets/Farm/Scripts/Crops.meta
A	Assets/Farm/Scripts/Crops/CropDefinition.cs
A	Assets/Farm/Scripts/Crops/CropDefinition.cs.meta
A	Assets/Farm/Scripts/Crops/FarmPlot.cs
A	Assets/Farm/Scripts/Crops/FarmPlot.cs.meta
A	Assets/Farm/Scripts/Crops/FieldManager.cs
A	Assets/Farm/Scripts/Crops/FieldManager.cs.meta
A	Assets/Farm/Scripts/Interaction.meta
A	Assets/Farm/Scripts/Interaction/FarmSign.cs
A	Assets/Farm/Scripts/Interaction/FarmSign.cs.meta
A	Assets/Farm/Scripts/Interaction/PlayerInteraction.cs
A	Assets/Farm/Scripts/Interaction/PlayerInteraction.cs.meta
A	Assets/Farm/Scripts/Player.meta
A	Assets/Farm/Scripts/Player/FarmCamera.cs
A	Assets/Farm/Scripts/Player/FarmCamera.cs.meta
A	Assets/Farm/Scripts/Player/FarmInput.cs
A	Assets/Farm/Scripts/Player/FarmInput.cs.meta
A	Assets/Farm/Scripts/Player/FarmPlayer.cs
A	Assets/Farm/Scripts/Player/FarmPlayer.cs.meta
A	Assets/Farm/Scripts/Player/FarmerAnimation.cs
A	Assets/Farm/Scripts/Player/FarmerAnimation.cs.meta
A	Assets/Farm/Scripts/Player/PlayerSettings.cs
A	Assets/Farm/Scripts/Player/PlayerSettings.cs.meta
A	Assets/Farm/Scripts/UI.meta
A	Assets/Farm/Scripts/UI/FarmHud.cs
A	Assets/Farm/Scripts/UI/FarmHud.cs.meta
A	Assets/Farm/Scripts/UI/WorldSignText.cs
A	Assets/Farm/Scripts/UI/WorldSignText.cs.meta
A	Assets/Farm/Settings.meta
A	Assets/Farm/Settings/FarmRenderer.asset
A	Assets/Farm/Settings/FarmRenderer.asset.meta
A	Assets/Farm/Settings/FarmURP.asset
A	Assets/Farm/Settings/FarmURP.asset.meta
A	Assets/Resources.meta
A	Assets/UniversalRenderPipelineGlobalSettings.asset
A	Assets/UniversalRenderPipelineGlobalSettings.asset.meta
A	CHOI_GAME.md
A	ChatGPT Image Sep 20, 2026, 07_13_45 PM.png
A	Packages/manifest.json
A	Packages/packages-lock.json
A	ProjectSettings/AudioManager.asset
A	ProjectSettings/ClusterInputManager.asset
A	ProjectSettings/DynamicsManager.asset
A	ProjectSettings/EditorBuildSettings.asset
A	ProjectSettings/EditorSettings.asset
A	ProjectSettings/GraphicsSettings.asset
A	ProjectSettings/InputManager.asset
A	ProjectSettings/MemorySettings.asset
A	ProjectSettings/MultiplayerManager.asset
A	ProjectSettings/NavMeshAreas.asset
A	ProjectSettings/Physics2DSettings.asset
A	ProjectSettings/PresetManager.asset
A	ProjectSettings/ProjectSettings.asset
A	ProjectSettings/ProjectVersion.txt
A	ProjectSettings/QualitySettings.asset
A	ProjectSettings/SceneTemplateSettings.json
A	ProjectSettings/TagManager.asset
A	ProjectSettings/TimeManager.asset
A	ProjectSettings/UnityConnectSettings.asset
A	ProjectSettings/VFXManager.asset
A	ProjectSettings/VersionControlSettings.asset
A	README.md
A	prompt.MD
COMMIT aacce4f Add species pens, animal handling, inventory and saves

M	Assets/Farm/Editor/FarmCharacters.cs
M	Assets/Farm/Editor/FarmProjectBuilder.cs
A	Assets/Farm/Editor/FarmShopBuilder.cs
A	Assets/Farm/Editor/FarmShopBuilder.cs.meta
A	Assets/Farm/Prefabs/Animal0.prefab
A	Assets/Farm/Prefabs/Animal0.prefab.meta
A	Assets/Farm/Prefabs/Animal1.prefab
A	Assets/Farm/Prefabs/Animal1.prefab.meta
A	Assets/Farm/Prefabs/Animal2.prefab
A	Assets/Farm/Prefabs/Animal2.prefab.meta
A	Assets/Farm/Prefabs/Animal3.prefab
A	Assets/Farm/Prefabs/Animal3.prefab.meta
A	Assets/Farm/Prefabs/AppleTree.prefab
A	Assets/Farm/Prefabs/AppleTree.prefab.meta
M	Assets/Farm/Scenes/Farm.unity
A	Assets/Farm/Scripts/Animals/AnimalCarry.cs
A	Assets/Farm/Scripts/Animals/AnimalCarry.cs.meta
A	Assets/Farm/Scripts/Animals/AnimalPen.cs
A	Assets/Farm/Scripts/Animals/AnimalPen.cs.meta
A	Assets/Farm/Scripts/Animals/AnimalSpecies.cs
A	Assets/Farm/Scripts/Animals/AnimalSpecies.cs.meta
A	Assets/Farm/Scripts/Animals/EggNest.cs
A	Assets/Farm/Scripts/Animals/EggNest.cs.meta
M	Assets/Farm/Scripts/Animals/FarmAnimal.cs
A	Assets/Farm/Scripts/Animals/PaddockGate.cs
A	Assets/Farm/Scripts/Animals/PaddockGate.cs.meta
M	Assets/Farm/Scripts/Core/BuildSmokeCheck.cs
A	Assets/Farm/Scripts/Core/FarmSave.cs
A	Assets/Farm/Scripts/Core/FarmSave.cs.meta
M	Assets/Farm/Scripts/Crops/FarmPlot.cs
A	Assets/Farm/Scripts/Crops/FruitTree.cs
A	Assets/Farm/Scripts/Crops/FruitTree.cs.meta
M	Assets/Farm/Scripts/Interaction/PlayerInteraction.cs
M	Assets/Farm/Scripts/UI/FarmHud.cs
A	Assets/Farm/Scripts/UI/FarmInventory.cs
A	Assets/Farm/Scripts/UI/FarmInventory.cs.meta
A	Assets/Farm/Scripts/UI/FarmShop.cs
A	Assets/Farm/Scripts/UI/FarmShop.cs.meta
A	Assets/Farm/Scripts/UI/ShopCounter.cs
A	Assets/Farm/Scripts/UI/ShopCounter.cs.meta
M	CHOI_GAME.md
M	README.md
COMMIT fae66b0 Save farm progress only when player chooses Save

M	Assets/Farm/Editor/FarmProjectBuilder.cs
M	Assets/Farm/Prefabs/Animal0.prefab
M	Assets/Farm/Prefabs/Animal1.prefab
M	Assets/Farm/Prefabs/Animal2.prefab
M	Assets/Farm/Prefabs/Animal3.prefab
M	Assets/Farm/Prefabs/AppleTree.prefab
M	Assets/Farm/Scenes/Farm.unity
M	Assets/Farm/Scripts/Core/FarmSave.cs
M	CHOI_GAME.md
M	README.md
COMMIT 1cc34cc Add seasons, weather, island activities and TMP HUD

A	Assets/Farm/Editor/FarmIslandsBuilder.cs
A	Assets/Farm/Editor/FarmIslandsBuilder.cs.meta
M	Assets/Farm/Editor/FarmProjectBuilder.cs
M	Assets/Farm/Editor/FarmShopBuilder.cs
A	Assets/Farm/Materials/Central island.mat
A	Assets/Farm/Materials/Central island.mat.meta
A	Assets/Farm/Materials/Industrial island.mat
A	Assets/Farm/Materials/Industrial island.mat.meta
A	Assets/Farm/Materials/Island stone.mat
A	Assets/Farm/Materials/Island stone.mat.meta
A	Assets/Farm/Materials/Mystery island.mat
A	Assets/Farm/Materials/Mystery island.mat.meta
A	Assets/Farm/Materials/Sea.mat
A	Assets/Farm/Materials/Sea.mat.meta
M	Assets/Farm/Prefabs/Animal0.prefab
M	Assets/Farm/Prefabs/Animal1.prefab
M	Assets/Farm/Prefabs/Animal2.prefab
M	Assets/Farm/Prefabs/Animal3.prefab
M	Assets/Farm/Prefabs/AppleTree.prefab
A	Assets/Farm/Resources.meta
A	Assets/Farm/Resources/FarmFont.asset
A	Assets/Farm/Resources/FarmFont.asset.meta
M	Assets/Farm/Scenes/Farm.unity
M	Assets/Farm/Scripts/Animals/AnimalPen.cs
M	Assets/Farm/Scripts/Animals/EggNest.cs
M	Assets/Farm/Scripts/Animals/FarmAnimal.cs
M	Assets/Farm/Scripts/Animals/PaddockGate.cs
M	Assets/Farm/Scripts/Core/BuildSmokeCheck.cs
A	Assets/Farm/Scripts/Core/DisasterPuzzleManager.cs
A	Assets/Farm/Scripts/Core/DisasterPuzzleManager.cs.meta
A	Assets/Farm/Scripts/Core/FarmAudio.cs
A	Assets/Farm/Scripts/Core/FarmAudio.cs.meta
A	Assets/Farm/Scripts/Core/FarmEffects.cs
A	Assets/Farm/Scripts/Core/FarmEffects.cs.meta
A	Assets/Farm/Scripts/Core/FarmExpansion.cs
A	Assets/Farm/Scripts/Core/FarmExpansion.cs.meta
A	Assets/Farm/Scripts/Core/FarmProcessing.cs
A	Assets/Farm/Scripts/Core/FarmProcessing.cs.meta
M	Assets/Farm/Scripts/Core/FarmSave.cs
A	Assets/Farm/Scripts/Core/IslandManager.cs
A	Assets/Farm/Scripts/Core/IslandManager.cs.meta
A	Assets/Farm/Scripts/Core/TimeManager.cs
A	Assets/Farm/Scripts/Core/TimeManager.cs.meta
M	Assets/Farm/Scripts/Crops/FarmPlot.cs
M	Assets/Farm/Scripts/Crops/FruitTree.cs
A	Assets/Farm/Scripts/Interaction/FarmBed.cs
A	Assets/Farm/Scripts/Interaction/FarmBed.cs.meta
M	Assets/Farm/Scripts/Interaction/FarmSign.cs
A	Assets/Farm/Scripts/Interaction/FishingPier.cs
A	Assets/Farm/Scripts/Interaction/FishingPier.cs.meta
A	Assets/Farm/Scripts/Interaction/IInteractable.cs
A	Assets/Farm/Scripts/Interaction/IInteractable.cs.meta
A	Assets/Farm/Scripts/Interaction/IslandAuctionKiosk.cs
A	Assets/Farm/Scripts/Interaction/IslandAuctionKiosk.cs.meta
A	Assets/Farm/Scripts/Interaction/IslandGameKiosk.cs
A	Assets/Farm/Scripts/Interaction/IslandGameKiosk.cs.meta
A	Assets/Farm/Scripts/Interaction/IslandNpc.cs
A	Assets/Farm/Scripts/Interaction/IslandNpc.cs.meta
A	Assets/Farm/Scripts/Interaction/IslandPortal.cs
A	Assets/Farm/Scripts/Interaction/IslandPortal.cs.meta
A	Assets/Farm/Scripts/Interaction/IslandTrap.cs
A	Assets/Farm/Scripts/Interaction/IslandTrap.cs.meta
A	Assets/Farm/Scripts/Interaction/MysteryAltar.cs
A	Assets/Farm/Scripts/Interaction/MysteryAltar.cs.meta
M	Assets/Farm/Scripts/Interaction/PlayerInteraction.cs
A	Assets/Farm/Scripts/Interaction/ResourceNode.cs
A	Assets/Farm/Scripts/Interaction/ResourceNode.cs.meta
M	Assets/Farm/Scripts/Player/FarmPlayer.cs
A	Assets/Farm/Scripts/UI/FarmBarnMenu.cs
A	Assets/Farm/Scripts/UI/FarmBarnMenu.cs.meta
M	Assets/Farm/Scripts/UI/FarmHud.cs
A	Assets/Farm/Scripts/UI/FarmHudV2.cs
A	Assets/Farm/Scripts/UI/FarmHudV2.cs.meta
M	Assets/Farm/Scripts/UI/FarmInventory.cs
M	Assets/Farm/Scripts/UI/FarmShop.cs
A	Assets/Farm/Scripts/UI/FarmUi.cs
A	Assets/Farm/Scripts/UI/FarmUi.cs.meta
M	Assets/Farm/Scripts/UI/ShopCounter.cs
A	Assets/StreamingAssets.meta
A	Assets/StreamingAssets/recipes.json
A	Assets/StreamingAssets/recipes.json.meta
A	Assets/TextMesh Pro.meta
A	Assets/TextMesh Pro/Fonts.meta
A	Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt
A	Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt.meta
A	Assets/TextMesh Pro/Fonts/LiberationSans.ttf
A	Assets/TextMesh Pro/Fonts/LiberationSans.ttf.meta
A	Assets/TextMesh Pro/Resources.meta
A	Assets/TextMesh Pro/Resources/Fonts & Materials.meta
A	Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Drop Shadow.mat
A	Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Drop Shadow.mat.meta
A	Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset
A	Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset.meta
A	Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat
A	Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat.meta
A	Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset
A	Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset.meta
A	Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt
A	Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt.meta
A	Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt
A	Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt.meta
A	Assets/TextMesh Pro/Resources/Sprite Assets.meta
A	Assets/TextMesh Pro/Resources/Sprite Assets/EmojiOne.asset
A	Assets/TextMesh Pro/Resources/Sprite Assets/EmojiOne.asset.meta
A	Assets/TextMesh Pro/Resources/Style Sheets.meta
A	Assets/TextMesh Pro/Resources/Style Sheets/Default Style Sheet.asset
A	Assets/TextMesh Pro/Resources/Style Sheets/Default Style Sheet.asset.meta
A	Assets/TextMesh Pro/Resources/TMP Settings.asset
A	Assets/TextMesh Pro/Resources/TMP Settings.asset.meta
A	Assets/TextMesh Pro/Shaders.meta
A	Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl
A	Assets/TextMesh Pro/Shaders/SDFFunctions.hlsl.meta
A	Assets/TextMesh Pro/Shaders/TMP_Bitmap-Custom-Atlas.shader
A	Assets/TextMesh Pro/Shaders/TMP_Bitmap-Custom-Atlas.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_Bitmap-Mobile.shader
A	Assets/TextMesh Pro/Shaders/TMP_Bitmap-Mobile.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_Bitmap.shader
A	Assets/TextMesh Pro/Shaders/TMP_Bitmap.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF Overlay.shader
A	Assets/TextMesh Pro/Shaders/TMP_SDF Overlay.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF SSD.shader
A	Assets/TextMesh Pro/Shaders/TMP_SDF SSD.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-HDRP LIT.shadergraph
A	Assets/TextMesh Pro/Shaders/TMP_SDF-HDRP LIT.shadergraph.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-HDRP UNLIT.shadergraph
A	Assets/TextMesh Pro/Shaders/TMP_SDF-HDRP UNLIT.shadergraph.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile Masking.shader
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile Masking.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile Overlay.shader
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile Overlay.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile SSD.shader
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile SSD.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile-2-Pass.shader
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile-2-Pass.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Surface-Mobile.shader
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Surface-Mobile.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Surface.shader
A	Assets/TextMesh Pro/Shaders/TMP_SDF-Surface.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-URP Lit.shadergraph
A	Assets/TextMesh Pro/Shaders/TMP_SDF-URP Lit.shadergraph.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF-URP Unlit.shadergraph
A	Assets/TextMesh Pro/Shaders/TMP_SDF-URP Unlit.shadergraph.meta
A	Assets/TextMesh Pro/Shaders/TMP_SDF.shader
A	Assets/TextMesh Pro/Shaders/TMP_SDF.shader.meta
A	Assets/TextMesh Pro/Shaders/TMP_Sprite.shader
A	Assets/TextMesh Pro/Shaders/TMP_Sprite.shader.meta
A	Assets/TextMesh Pro/Shaders/TMPro.cginc
A	Assets/TextMesh Pro/Shaders/TMPro.cginc.meta
A	Assets/TextMesh Pro/Shaders/TMPro_Mobile.cginc
A	Assets/TextMesh Pro/Shaders/TMPro_Mobile.cginc.meta
A	Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc
A	Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc.meta
A	Assets/TextMesh Pro/Shaders/TMPro_Surface.cginc
A	Assets/TextMesh Pro/Shaders/TMPro_Surface.cginc.meta
A	Assets/TextMesh Pro/Sprites.meta
A	Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt
A	Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt.meta
A	Assets/TextMesh Pro/Sprites/EmojiOne.json
A	Assets/TextMesh Pro/Sprites/EmojiOne.json.meta
A	Assets/TextMesh Pro/Sprites/EmojiOne.png
A	Assets/TextMesh Pro/Sprites/EmojiOne.png.meta
M	CHOI_GAME.md
M	Packages/manifest.json
M	Packages/packages-lock.json
A	ProjectSettings/ShaderGraphSettings.asset
M	README.md
```

</details>

# 6. IMPORTANT DECISIONS

- Chợ đấu giá và kết bạn tại Đảo Trung Tâm là **NPC/mô phỏng chơi đơn trước**, không có multiplayer/server; user đã xác nhận.
- Lưu **thủ công**: thoát không ghi; game tải bản lưu gần nhất khi khởi động. Giữ hành vi này khi thêm tính năng mới.
- Một scene chứa bốn đảo, di chuyển bằng Tab hoặc cổng; đảo Thần Bí/Công Nghiệp có gate LV trong chế độ thường.
- `IInteractable` + quét mục tiêu gần bằng `Physics.OverlapSphereNonAlloc`, viền và gợi ý world-space thay cho dấu cộng cố định.
- Công thức máy trong `StreamingAssets/recipes.json`, dùng item ID của `FarmInventory`; bàn chế tạo được thống nhất là hệ thống **ghép nhiều nguyên liệu ra hàng giao hộp thư**, khác với máy xếp hàng hiện tại.
- Kế hoạch mới đã chọn: cây được tưới chín 2/3/5 phút; khô vẫn lớn 20%; lấy nước ở hồ; trạm tự tưới trong vùng nhưng **người chơi phải chở nước từ hồ tới nạp**, không tự tạo nước.
- Hộp thư: hai đơn ngẫu nhiên/ngày, một đơn craft và một đơn nông sản/chế biến; được đổi **một đơn mỗi 5 phút chơi thực tế**, không tính pause/ngủ. Đơn hoàn thành mở dần bánh mì/phô mai/nước táo. Đây là **quyết định thiết kế chưa triển khai**.
- Chế độ sáng tạo được chốt là **dịch chuyển mọi đảo không cần LV và bay**, không cấp tài nguyên/xu vô hạn; bắt đầu từ bản sao trong bộ nhớ của bản lưu hiện có, **không lưu**, rời mode bỏ mọi thay đổi. Mode thường vẫn giữ LV và bản lưu. Đây là **quyết định thiết kế chưa triển khai**.
- User ưu tiên tối ưu đi bộ/nhảy/va chạm, không yêu cầu đại tu camera. Đây là **quyết định thiết kế chưa triển khai**.
- Giải pháp trung gian bị bỏ: nhiều MonoBehaviour trong một file làm scene báo Missing Script; TMP không có Essentials gây lỗi runtime; `TextMeshPro.textContainer` obsolete trả `null`; ký tự `◉` không có glyph. Bản cuối đã dùng file tách riêng, TMP resources và `rectTransform`, nhãn nhỏ, `XU`.
- Build không được đưa cả `Library`, `Temp`, `Logs`, `Builds` vào Git/source ZIP. Các ZIP cũ vẫn còn để tham khảo, nhưng gói mới nhất có tiền tố `NongTrai-Islands`.

# 7. DO NOT BREAK

- Giữ namespace `NongTrai`, interface `IInteractable` và các method `CanInteract`, `Interact`, `SetHighlighted`, `InteractionHint` vì nhiều object đang implement. Unity MonoBehaviour nên có `.cs` cùng tên class khi attach; giữ `.meta` GUID và scene/prefab references.
- Giữ `FarmProjectBuilder.BuildWindows()` và `CreateScene()` hoặc cập nhật nhất quán lệnh build/docs. Builder tái dựng `Farm.unity` và prefab; không sửa scene bằng tay rồi build khi chưa đưa thay đổi vào builder.
- Giữ item ID hiện hành: 0–2 crop, 3 táo, 4 trứng, 5 sữa, 6 len, 7 thịt, 8 bột, 9 bánh, 10 phô mai, 11 nước táo, 12 gỗ, 13 quặng, 14 ván, 15 kim loại. Mở rộng ID từ 16 trở đi, không đổi ID cũ; đồng bộ `FarmInventory`, JSON recipe và save.
- Giữ `FarmShop.Seeds` độ dài 3, `FieldManager.Harvested` độ dài 3, `FarmExpansion.ToolTiers` độ dài 3 và `UnlockedRegions` độ dài 4 trừ khi có migration đầy đủ. `SaveData` v4 hiện đọc v2/v3/v4; v5 mới phải tiếp tục đọc v2–v4. Không tự lưu khi thoát và không cho creative ghi đè bản lưu thường.
- Giữ 4 loài/chuồng riêng, gà tối đa 5 con/chuồng, nhấc trái/thả phải, F cho ăn, thu trứng tại ổ, sữa/len có cooldown, heo cho thịt rồi rời chuồng.
- Giữ 1 ngày 600s, mùa 28 ngày, mưa tưới ruộng, bão có puzzle, ngủ tới 06:00; khi sửa tốc độ cây phải cập nhật cả tick lúc ngủ/test.
- Giữ 4 đảo, cổng, NPC/chợ chơi đơn, di tích/bản vẽ và gate LV ở **mode thường**. Bypass LV chỉ cho du hành ở creative; không tắt cơ chế khác ngoài yêu cầu.
- Giữ font `FarmFont.asset`/TMP Essentials và tỷ lệ Canvas 1920×1080; không tái đưa nhãn tương tác khổng lồ/chữ đảo chiều. Giữ Input System mới; không chuyển lẫn UnityEngine.Input cũ.
- Giữ build Windows kèm `NongTrai_Data`, `MonoBleedingEdge`, DLL/UnityPlayer; một EXE riêng lẻ không đủ để chạy trên máy khác.

# 8. CURRENT BUGS / TECHNICAL DEBT

| Vấn đề | Triệu chứng & file liên quan | Nguyên nhân/đã thử/kết quả | Hướng tiếp theo |
|---|---|---|---|
| Hotbar không ràng buộc dụng cụ | Có thể cày dù không chọn cuốc, tưới dù không chọn bình; `FarmHudV2.cs`, `FarmExpansion.cs`, `FarmPlot.cs`. | `SelectedSlot` chỉ chọn crop khi `<3`; `FarmExpansion.Work` tự suy tool từ `PlotState`. User xác nhận đây là nghĩa của “vật phẩm bị ghi đè”. Chưa có fix. | Gate thao tác theo slot, hiển thị phím cần chọn, test không tiêu vật phẩm khi chọn sai. |
| Cây quá nhanh, đất khô dừng hẳn | 35/45/55s, nước đầy kéo 75s; `FarmProjectBuilder.cs`, Crop assets, `FarmPlot.cs`, smoke. | Cả ba cây chín sau một lần tưới. Chưa chỉnh theo kế hoạch mới. | 120/180/300s khi ẩm, 20% tốc độ lúc khô, giá khác nhau; sửa test cũ đang assert khô `Growth==0`. |
| Hồ chỉ trang trí; không có nước hữu hạn | `FarmProjectBuilder.cs` tạo `Pond surface - decorative`, `FarmPlot.Work` tưới không trừ nước; không có WaterSystem/pump. | Chưa thử giải pháp trong code. | Tạo điểm lấy nước, lượng nước bình và trạm tự tưới có bồn được nạp thủ công. |
| Chưa có craft/mail/đơn | `FarmProcessing.cs` chỉ có máy một-input và queue; `FarmInventory.cs` chỉ 16 loại lưới 4×4; chưa có mailbox. | Thiết kế được chốt qua các câu hỏi nhưng chưa code. | JSON craft nhiều nguyên liệu, 4 item mới, túi cuộn, hộp thư và giới hạn reroll. |
| Chưa có creative/test mode | `IslandManager.Travel` gate LV; `FarmHud` chỉ một nút vào game; `FarmSave` tự tải và có nút lưu; `FarmPlayer` chỉ đi/nhảy. | Chưa triển khai. | Mode riêng không lưu, clone trạng thái lưu vào RAM, bypass LV riêng cho travel, bay va chạm, reset scene khi rời. |
| Di chuyển vật lý còn tối giản | `FarmPlayer.cs` một `CharacterController.Move`, `isGrounded` đơn giản; rơi dưới y=-10 luôn về spawn ban đầu ở đảo nông trại. | Smoke chỉ kiểm tra đứng đất ban đầu; dốc/bậc/fall island khác: **CHƯA XÁC MINH**. | Cải thiện ground probe/jump buffer/coyote/slope/terminal speed, respawn đảo hiện tại, thêm test thực tế. |
| UI không thuần TMP, tỷ lệ khác chưa kiểm đủ | `FarmUi.Label`, shop/menu dùng uGUI `Text`; inventory TMP 4×4 hiện vừa 16 item. | Ảnh smoke 1280×720 hiện đọc được; 1920×1080 và màn hình hẹp: **CHƯA XÁC MINH**. | Khi thêm item, dùng ScrollRect và kiểm nhiều tỷ lệ. |
| Đồng hồ đấu giá có thể chạy khi game tạm dừng | `IslandManager.Update` trừ `Time.unscaledDeltaTime` khi có listing, không kiểm `player.Paused`. | Suy ra trực tiếp từ code; việc đây có phải lỗi theo ý định sản phẩm hay không: **CHƯA XÁC MINH**. | Khi làm bộ đếm 5 phút cho hộp thư, định nghĩa rõ loại thời gian; không vô tình đổi đấu giá nếu task không cần. |
| Lỗi cũ từng thấy, hiện đã xử lý trong bản smoke cuối | Chữ trên biển ngược, nhãn world phóng to, NullReference `textContainer`, TMP thiếu font/glyph. | Sửa mặt biển/nhãn, TMP Essentials/FarmFont, `rectTransform`, `XU`; build/smoke cuối không còn log lỗi tương ứng. | Không tái sử dụng `textContainer`; kiểm ảnh sau mọi thay đổi UI. |

Không có crash runtime còn tồn tại đã được xác nhận từ log smoke cuối. Những vấn đề ngoài smoke và quan sát ảnh: **CHƯA XÁC MINH**.

# 9. NEXT TASK

**Task được user giao nhưng chưa thi hành:** triển khai toàn bộ kế hoạch “Cập nhật nông trại, đơn hàng và chế độ sáng tạo để kiểm thử” đã được người dùng yêu cầu implement. Lượt implementation trước bị ngắt sau lời báo bắt đầu, trước khi sửa file. Các con số/quy tắc sau là **đặc tả mục tiêu**, không phải trạng thái hiện tại.

1. **Hotbar + cân bằng:** E chỉ cày với slot 5, gieo với slot 1–3, tưới với slot 6, thu hoạch với slot 7; slot sai chỉ gợi ý. Cây 120/180/300 giây khi ẩm, khô 20%; gói 5 hạt giá 20/40/75, sản phẩm bán 8/18/32. Cần cập nhật builder **và** Crop assets, Shop, Inventory, FarmPlot, Expansion, HUD/hint và smoke.
2. **Nước:** hồ refill bình 8/16/24 theo tier, mỗi ô một đơn vị; mưa không tốn. Một trạm/vùng đất đã mở, từ LV2, giá 600/900/1200/1500, bồn 32, nạp từ bình mang ở hồ, tự tưới ô trong bán kính 6m khi ẩm <20%. Cần đặt object/`IInteractable`, hệ thống tick/pause/sleep, UI và save.
3. **Craft & mailbox:** bàn/hộp thư trước nhà; JSON craft nhiều input → 4 item mới: Bó nông sản = 2 lúa mì+1 cà chua; Gói đậu = 2 đậu nành+1 lúa mì; Giỏ táo = 3 táo+1 ván; Đèn thủ công = 2 ván+1 kim loại. Hai công thức đầu mở sẵn; món sau chỉ mở khi nguồn nguyên liệu tương ứng khả dụng. Hai đơn random khả thi/ngày (một craft, một nông sản/chế biến); giao đủ một lần bằng E lấy 150% giá bán thường + XP; hết hạn ngày sau không mất đồ. Đổi **một** đơn mỗi 5 phút chơi thực (không pause/ngủ), chọn đơn khác có thể làm. Số đơn hoàn thành mở bánh mì 2, phô mai 4, nước táo 6; flour sẵn, ván/metal vẫn theo đảo/bản vẽ. Bảng craft không thay máy queue cũ. Giá bán cụ thể của 4 item craft và XP/order **CHƯA CHỐT bằng user**, nên cần quyết định nội bộ hợp lý, hiển thị rõ, giữ tổng thưởng lớn hơn giá nguyên liệu. Inventory phải mở từ 16 lên 20 ID và có cuộn, không đổi ID 0–15.
4. **Di chuyển + creative:** sửa tiếp đất/mép bậc/dốc, nhảy có buffer ngắn, giới hạn tốc độ rơi, respawn tại đảo đang ở; flight vẫn va chạm. Nút “Chế độ sáng tạo” ở main menu, dùng bản sao RAM của save hiện có (nếu không có thì mặc định), Tab/cổng đi 4 đảo ngay LV1, F8 bật bay, WASD/Space lên/Ctrl xuống/Shift nhanh. Không vô hạn xu/vật phẩm, không bỏ gate của gameplay khác. Badge sáng tạo, vô hiệu Save, rời mode bỏ thay đổi và mở normal phải tải lại save; **không** ghi file lưu khi test.
5. **Migration + release:** SaveData v5 thêm nước/pump/4 item mới/đơn/reroll/unlock recipe; đọc v2–v4, giữ công thức người chơi cũ từng dùng. Cập nhật `BuildSmokeCheck`, docs, build Windows, kiểm ảnh/UI, ZIP Windows+Unity, đồng bộ GitHub `main` theo uỷ quyền trước đây. **Chỉ làm bước release sau khi code và test đúng**.

**Bước đầu tiên nên thực hiện:** kiểm `git status` tại `D:\GAME_NongTrai\Nongtrai` và so source với Unity root; đọc các file mục 4, đặc biệt builder/wiring, `FarmPlot`, `FarmExpansion`, `FarmInventory`, `FarmSave`, `BuildSmokeCheck`. Chia thay đổi theo hệ thống và sửa test assertion cũ đồng thời với luật chơi mới. Tránh build khi Unity Editor khác đang mở project. Sau đó compile/build/smoke và chơi thử các ca tương tác thực tế.

**Hoàn thành khi:** tính năng đúng các quy tắc trên; bản save cũ tải được và creative không đổi file save; mode thường vẫn gate LV; Windows build thành công; smoke mới qua; ảnh không chồng chữ/ô ở ít nhất 1280×720 và 1920×1080; ZIP mới và GitHub cùng source/build đã kiểm tra. Các tiêu chí chưa được kiểm tra thực tế trong bản hiện tại.

# 10. HOW TO RUN / TEST

## Chạy và mở Editor

- Chạy Windows: `D:\GAME_NongTrai\Builds\Windows\NongTrai.exe` cùng toàn bộ thư mục `Builds\Windows`. ZIP chạy mới nhất hiện là `D:\GAME_NongTrai\DongGoi\NongTrai-Islands-Windows.zip`; source ZIP mới nhất `D:\GAME_NongTrai\DongGoi\NongTrai-Islands-Unity.zip`. Các ZIP khác mang tên cũ là bản cũ.
- Unity Hub → Add project from disk → `D:\GAME_NongTrai` → Unity `6000.3.22f1` → scene `Assets/Farm/Scenes/Farm.unity` → Play. Git clone `Nongtrai` là bản sao mã, không phải projectPath đang build.
- Game tự tải `%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest\farm-manual-save.json` nếu có. **Không xóa bản lưu người dùng để test**; smoke có `pathOverride` trong `Application.temporaryCachePath`.

## Build và smoke hiện có

Đóng Unity Editor khác đang mở cùng project. Trong PowerShell tại `D:\GAME_NongTrai`:

```powershell
& 'D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\GAME_NongTrai' -executeMethod NongTrai.Editor.FarmProjectBuilder.BuildWindows -logFile 'D:\GAME_NongTrai\Logs\build-next.log'
```

`BuildWindows` **dựng lại và ghi đè scene/asset** rồi build Development Windows vào `Builds\Windows`; nếu đã sửa scene thủ công, chuyển sửa đó vào builder trước. Khi build xong kiểm `FARM_M1_BUILD_OK` và không có compile/build exception. Smoke hiện tại:

```powershell
& 'D:\GAME_NongTrai\Builds\Windows\NongTrai.exe' -farmSmokeCheck -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -force-d3d11 -logFile 'D:\GAME_NongTrai\Logs\smoke-next.log'
```

Kiểm `FARM_*_OK`, `Exception`, `Error`, `not found` trong log. Test có thể tạo ảnh `*-preview.png` ở `Builds\Windows`; xem trực quan menu, túi, các biển/nhân vật. Sau đổi luật khô/tưới/giá, sửa các assertion cũ của `BuildSmokeCheck` trước khi dùng nó làm tiêu chí pass. Cần thêm test creative: normal LV1 không đến Đảo Thần Bí/Công Nghiệp; creative LV1 đến đủ đảo; fly/va chạm; bỏ mode không đổi hash của save gốc. Kiểm thao tác tay tại hồ, trạm, bàn craft, hộp thư và save migration; smoke hiện tại **chưa** bao phủ chúng.

## Git/package

- Git repo: `D:\GAME_NongTrai\Nongtrai`, remote `https://github.com/NguyenHuuThinhyy/Nongtrai.git`, nhánh `main`, commit hiện `1cc34cc` và `origin/main` trùng. Root Unity project không phải Git repo. Sau khi thay source root cần đồng bộ `Assets`, `Packages`, `ProjectSettings`, `README.md`, `CHOI_GAME.md` vào clone rồi kiểm diff/commit/push; không đưa `Library`, `Temp`, `Logs`, `Builds` vào Git.
- Gói source cần `Assets`, `Packages`, `ProjectSettings`, docs, `.gitignore`, `prompt.MD`, ảnh tham chiếu. Gói Windows cần EXE, `NongTrai_Data`, `MonoBleedingEdge`, D3D12/DLL kèm; ảnh preview/log không cần. Gói Islands hiện có SHA-256: Unity `A32D946CA3C2918C06AD6C7EFBD8F288520188025054F8B2600B00BF2613E389`; Windows `A65C31E165F4F330450F444C364AF3ECD94B36B16D4E9CB92212F57A90B4F12E`.

# 11. ENVIRONMENT

- OS đã xác minh: **Microsoft Windows 11 Home Single Language**, version `10.0.26200`, build `26200`; shell PowerShell.
- Unity Editor đã dùng: `D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe`; `ProjectSettings/ProjectVersion.txt` khớp `6000.3.22f1` revision `1c726e1fb402`.
- Build target trong builder: `StandaloneWindows64`, `BuildOptions.Development`; Unity Linear color space, URP, Input System mới, layer 8 = Player; 1600×900 mặc định, FullScreenWindow, Canvas ref 1920×1080.
- Packages từ manifest: Cinemachine `3.1.5`, Input System `1.20.0`, URP `17.3.0`, uGUI `2.0.0`; TMP Essentials asset nằm trong project. Git đã xác minh `2.53.0.windows.3`. Phiên bản .NET độc lập bên ngoài Unity: **CHƯA XÁC MINH** và chưa cần cho workflow này.
- Filesystem đã xác minh: project root `D:\GAME_NongTrai`, Git clone `D:\GAME_NongTrai\Nongtrai`; `Assets`/`Packages`/`ProjectSettings` và root docs/reference giống clone trước khi viết handoff; `Builds`, `DongGoi`, `Logs`, `Library` nằm ở root, không track trong clone.

# 12. INSTRUCTIONS FOR NEXT CODEX

1. Đọc toàn bộ file này trước.
2. Kiểm tra code hiện tại trước khi sửa.
3. Không giả định code vẫn giống phiên bản được mô tả nếu repository cho thấy khác.
4. Không rewrite/refactor phần đang chạy ổn nếu task không yêu cầu.
5. Giữ compatibility với hệ thống hiện tại.
6. Trước khi sửa, xác định các file và dependency liên quan.
7. Sau khi sửa, kiểm tra lỗi compile/build/test nếu có thể.
8. Không xóa chức năng hiện có chỉ để giải quyết lỗi mới.
9. Nếu HANDOFF và code thực tế mâu thuẫn, ưu tiên code thực tế và báo rõ sự khác biệt.
10. Tiếp tục từ mục NEXT TASK.

**Ưu tiên đặc biệt:** đây là handoff cho task IMPLEMENT vừa bị ngắt, **không** phải yêu cầu lặp lại lập kế hoạch. Những quy tắc mới ở mục 9 đã được user thống nhất qua các lượt hỏi; hãy thực hiện nhưng kiểm tra source/test thực tế trước. Nếu `CODEX_HANDOFF.md` chỉ có ở Unity root mà không ở Git clone, đó là chủ ý của yêu cầu tạo handoff hiện tại, không phải dấu hiệu mất file source.
