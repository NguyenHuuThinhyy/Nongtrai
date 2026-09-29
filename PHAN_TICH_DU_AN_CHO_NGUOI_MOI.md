# Phân tích dự án Nông Trại – First Harvest cho người mới

> Tài liệu này được lập từ source hiện tại của project Unity trong repository. Mục tiêu là giúp một người mới hiểu dự án đang chứa gì, game chạy theo luồng nào, mỗi nhóm script chịu trách nhiệm gì, dữ liệu nằm ở đâu và nên sửa ở đâu.

## 1. Bức tranh tổng thể

Đây là game nông trại 3D góc nhìn thứ nhất/thứ ba, viết bằng Unity 6 và C#. Project không chỉ có vòng lặp trồng–thu hoạch; nó đã ghép nhiều hệ thống:

- Trồng 8 loại cây trên 80 ô ruộng, tưới, bón phân, cây đột biến.
- Trồng cây ăn quả, chặt cây, tài nguyên và chế biến.
- Nuôi bò, heo, cừu, gà; đói/vui, thức ăn, chuồng, sản phẩm.
- Túi 36 ô, hotbar 9 ô, độ bền và chỉ số riêng của vũ khí.
- Cửa hàng, tiền, cấp độ, XP, mở vùng đất và nâng cấp công cụ.
- Chế tạo tức thời theo `crafting.json` và máy chế biến theo `recipes.json`.
- Map nông trại và map khám phá voxel sinh theo seed, đào/đặt khối, nước động.
- Chiến đấu bằng kiếm/cung/TNT, thú hoang, quái, rương và boss.
- Ngày/đêm dài 18 phút, mùa, mưa/sương/bão, ngủ và câu đố thiên tai.
- Hai mini-game giao hàng/Runner, quest, đơn hàng và phần thưởng.
- Lưu thủ công bằng JSON, migration save cũ tới schema v21.
- Chế độ sáng tạo LV99, bay và không ghi đè save thường.
- Bộ smoke test chạy trong build để kiểm tra nhiều hệ thống.

Project dùng một scene build duy nhất: `Assets/Farm/Scenes/Farm.unity`. Scene hiện rất lớn: khoảng 1.029 GameObject, 250 MonoBehaviour và hơn 105.000 dòng YAML. Map khám phá không phải scene thứ hai; nó được đặt cách map nông trại khoảng 1.000 mét theo trục Y và được sinh/chuyển tới bằng code.

## 2. Những khái niệm Unity cần biết trước

Nếu mới học Unity, hãy đọc phần này trước khi đọc script:

| Khái niệm | Hiểu đơn giản | Ví dụ trong project |
|---|---|---|
| Scene | Một màn chứa GameObject và component đã lưu | `Farm.unity` |
| GameObject | Một đối tượng trong scene | Player, ô đất, chuồng, cây |
| Component | Một mảnh chức năng gắn lên GameObject | `FarmPlayer`, Collider, Renderer |
| MonoBehaviour | Class C# Unity tự gọi theo vòng đời | `Awake`, `Start`, `Update`, `FixedUpdate` |
| Prefab | Mẫu GameObject tái sử dụng | bò, heo, gà, cây táo |
| ScriptableObject | File dữ liệu độc lập với scene | `Crop0.asset`, `PlayerSettings.asset` |
| Inspector reference | Biến `public` được kéo-thả trong Editor | `FarmHud.player`, `FarmSave.shop` |
| Singleton | Một instance toàn cục truy cập qua `Instance` | `FarmShop.Instance`, `TimeManager.Instance` |
| Serialization | Chuyển trạng thái thành dữ liệu để lưu | `SaveData` → JSON |

Vòng đời thường gặp trong project:

1. `Awake()` đăng ký singleton và tạo trạng thái nền tảng.
2. `Start()` tạo UI/runtime object và nối sự kiện.
3. `Update()` xử lý input, thời gian và logic theo từng frame.
4. `FixedUpdate()` xử lý chuyển động vật lý của thú/quái.
5. `LateUpdate()` cập nhật camera, animation hoặc vật cầm sau khi nhân vật đã di chuyển.
6. `OnDestroy()` hủy material/runtime object và gỡ event.

## 3. Cấu trúc thư mục

### Thư mục cần đọc và đưa lên Git

- `Assets/Farm`: toàn bộ nội dung game do dự án tạo.
- `Assets/StreamingAssets`: JSON công thức được đọc lúc chạy.
- `Assets/TextMesh Pro`: font/shader chuẩn của TextMesh Pro.
- `Packages`: danh sách package Unity và lock file.
- `ProjectSettings`: thiết lập project, input, physics, URP, build scene.
- `Tools`: PowerShell hỗ trợ build Windows.
- Các file `.md` ở gốc: hướng dẫn chơi, bàn giao, nguồn asset và báo cáo.

### Thư mục sinh tự động hoặc sản phẩm đầu ra

- `Library`: cache import, mã package đã giải nén, shader cache. Unity có thể tạo lại; không phải source gameplay.
- `Temp`: file tạm của Unity.
- `Logs`: log Editor/build/test.
- `UserSettings`: thiết lập riêng trên máy người dùng.
- `Builds`: bản Windows đã build.
- `DongGoi`: các gói ZIP phát hành.

Không nên học logic game bằng cách đọc hàng nghìn file trong `Library/PackageCache`: đó là mã của Unity/package, không phải mã game của nhóm.

### Ý nghĩa `.meta`

Mỗi asset Unity có file `.meta` chứa GUID. Scene/prefab tham chiếu asset qua GUID này. Không xóa hoặc tự tạo lại `.meta` tùy tiện, vì prefab/material/script có thể mất liên kết.

## 4. Công nghệ và cấu hình

- Unity Editor: `6000.3.22f1`.
- Render pipeline: Universal Render Pipeline 17.3.0 (URP), color space Linear.
- Input: Input System 1.20.0.
- Camera: Cinemachine 3.1.5.
- UI: uGUI 2.0.0 và TextMesh Pro.
- Physics: module Physics 3D.
- Tên công ty: `Nong Trai Studio`.
- Tên sản phẩm: `Nong Trai - First Harvest`.
- Độ phân giải mặc định: 1600 × 900.
- Build Settings chỉ bật `Assets/Farm/Scenes/Farm.unity`.

## 5. Scene được dựng và game khởi động như thế nào

Scene ban đầu được tạo bằng `FarmProjectBuilder.CreateScene()` trong thư mục Editor. Builder tạo địa hình nông trại, 80 ô ruộng, chuồng, Player, camera, UI, shop, máy móc, cổng và nối các reference public. Sau khi lưu scene, lúc chạy game Unity không gọi builder nữa; Unity deserialize chính scene đã lưu.

Các điểm trung tâm trong scene:

- `Player`: có `CharacterController`, `FarmPlayer`, `FarmInput`, `PlayerInteraction`, `AnimalCarry`, `FarmBow`.
- `Gameplay HUD`: có phần lớn manager như shop, inventory, save, time, expansion, processing, water, crafting, islands, building, exploration.
- `Field Manager`: chứa danh sách `CropDefinition` và tick 80 `FarmPlot`.
- `EventSystem`: nhận input cho uGUI.
- `Sun` và camera: ánh sáng ngày/đêm, URP và Cinemachine.

Luồng khởi động rút gọn:

```text
Unity nạp Farm.unity
  → Awake của các manager đăng ký Instance
  → Start tạo UI và các object runtime
  → FarmSave chờ 1 frame rồi Load()
  → người chơi chọn Chơi thường hoặc Sáng tạo
  → Update của Player / Interaction / Time / hệ thống chạy mỗi frame
  → Esc → Lưu game mới gọi FarmSave.Save()
```

Điểm đặc biệt: nhiều UI không được thiết kế sẵn trong Canvas mà được dựng bằng code qua `FarmUi.Panel`, `FarmUi.Button`, `FarmUi.TmpLabel`. Vì vậy muốn đổi bố cục thường phải sửa tọa độ/kích thước trong C#, không chỉ sửa scene.

## 6. Luồng input và tương tác

### Di chuyển

`FarmInput` tự tạo `InputActionMap` bằng code:

- WASD: di chuyển.
- Shift: chạy.
- Space: nhảy/ngoi nước.
- V: đổi góc nhìn.
- Esc: pause/menu.
- F8: bật/tắt bay trong Creative.
- Ctrl hoặc X: bay xuống.

`FarmPlayer.Update()` đọc action, xoay vector di chuyển theo yaw camera, tăng/giảm tốc mượt và gọi `CharacterController.Move`. Player hỗ trợ đi, chạy, bơi, bay, coyote time, jump buffer, lực hất, sát thương rơi và tự cứu khi lọt khỏi địa hình.

### Camera

`FarmCamera` giữ yaw/pitch, đặt Cinemachine camera ở góc thứ nhất hoặc thứ ba, sphere-cast để camera không xuyên tường và hỗ trợ rung camera.

### Tương tác thế giới

Mọi vật tương tác triển khai `IInteractable`, gồm bốn phần:

- `InteractionHint`: câu hướng dẫn.
- `CanInteract`: có được tương tác không.
- `Interact`: thực hiện hành động.
- `SetHighlighted`: bật/tắt viền chọn.

`PlayerInteraction` là bộ điều phối trung tâm. Nó raycast vào tâm camera, hoặc quét collider gần người chơi; sau đó ưu tiên nước, TNT, rương, lửa, vòi, thú, ruộng, cây, forge và các `IInteractable` khác. `InteractionOutline` vẽ khung vàng quanh collider đang chọn.

Đây là file đầu tiên nên kiểm tra khi một cú click “không tới đúng hệ thống”, bởi thứ tự các `if` trong `Update()` quyết định hành động nào được ưu tiên.

## 7. Hệ thống trồng trọt

### Dữ liệu cây

`CropDefinition` là ScriptableObject. Mỗi file `Crop0.asset`…`Crop5.asset` chứa tên, thời gian lớn, sản lượng, màu quả và prefab hình ảnh:

| Cây | Thời gian | Sản lượng |
|---|---:|---:|
| Lúa mì | 120 giây | 3 |
| Cà chua | 180 giây | 3 |
| Đậu nành | 300 giây | 3 |
| Bí ngô | 420 giây | 2 |
| Dâu ruộng | 540 giây | 2 |
| Hướng dương | 720 giây | 1 |

`FarmSpecialCrops.Install()` bổ sung các giống đặc biệt vào mảng runtime.

### State machine của ô ruộng

`FarmPlot` có bốn trạng thái:

```text
Untilled --cày--> Tilled --gieo--> Growing --đủ Growth--> Ready
   ^                                                    |
   +------------------ thu hoạch → Tilled --------------+
```

- `Tick(seconds)` giảm độ ẩm và tăng trưởng.
- Có nước: cây lớn bình thường; khô: vẫn lớn nhưng chỉ 20% tốc độ.
- `ApplyFertilizer()` cộng 12% tăng trưởng, 35% ẩm và có 8% đột biến.
- Cây đột biến đổi màu cầu vồng và bán gấp ba theo giá gốc.
- Hình cây được dựng lại theo bốn stage; lúa mì/bí có thể dùng prefab import, các cây khác dùng primitive.

`FieldManager` không để 80 ô tự `Update` logic nặng. Nó gom một tick mỗi giây rồi gọi `Tick` cho toàn bộ ruộng. Đây là một tối ưu tốt và dễ hiểu.

`FarmExpansion.Work()` bao quanh `FarmPlot.Work()` để kiểm tra vùng đã mở, dụng cụ đang cầm, độ bền, hạt, nước, bán kính nâng cấp và XP.

### Cây ăn quả và cây trang trí

- `FruitTree`: táo/lê/đào/việt quất, có giai đoạn trưởng thành, chu kỳ ra quả, tưới, bón, đột biến và chặt cây.
- `FarmDecorTree`: cây gỗ trang trí có thể chặt; lưu ID cây đã bị chặt.
- `FarmOrchardGate`: cổng vườn mở/đóng theo cấp độ.
- Cây ăn quả chỉ trồng trong vùng vườn phía đông, có kiểm tra khoảng cách tối thiểu.

## 8. Vật nuôi

`AnimalSpecies` định nghĩa Cow, Pig, Sheep, Chicken.

`FarmAnimal` quản lý từng con:

- `Hunger` và `Happiness` giảm theo phần ngày.
- `WellCared` khi cả hai ít nhất 35.
- Thức ăn đúng loài tăng nhiều hơn; cám dinh dưỡng đưa về 100%.
- Bò cho 3 sữa/45 giây, cừu cho 2 lông/60 giây, heo cho 6 thịt rồi rời chuồng.
- Gà không giữ cooldown riêng; `AnimalPen` cộng trứng theo số gà khỏe.
- Thú tự chọn điểm đi, né người và thú khác, về ổ nghỉ khi có sản phẩm.
- Di chuyển bằng Rigidbody kinematic trong `FixedUpdate`.

`AnimalPen` quản lý ranh giới, sức chứa, nâng cấp, trứng, máng ăn và điểm nghỉ. `AnimalCarry` cho nhấc/thả thú, nhưng chỉ thả hợp lệ vào chuồng đúng loài còn chỗ. `PaddockGate` xoay cửa và bật/tắt collider. `EggNest` và `AnimalRestSpot` là điểm lấy sản phẩm.

`FarmAnimalVisual` đồng bộ tốc độ animation imported với chuyển động thực.

## 9. Túi đồ, item và kinh tế

### Hai lớp dữ liệu dễ nhầm

`FarmInventory` là kho số lượng có thẩm quyền: nông sản, sản phẩm thú và vật liệu. `AdventureBag` là 36 slot, gồm 9 hotbar + 27 ô túi. `AdventureBag.Sync()` liên tục làm hai lớp khớp nhau.

Hệ quả khi lập trình:

- Vật phẩm stack thường: thay số lượng bằng `inventory.Add/Remove` rồi gọi/sống cùng `Sync`.
- Dụng cụ/vũ khí: tồn tại trực tiếp dưới dạng `BagSlot`, vì mỗi chiếc có độ bền và chỉ số riêng.
- Không nên tự tăng `slot.count` cho vật phẩm thường mà bỏ qua `FarmInventory`, vì lần Sync sau có thể sửa ngược.

### ID vật phẩm

Item thường là 0–77. Các ID chính:

- 0–3: lúa mì, cà chua, đậu nành, táo.
- 4–7: trứng, sữa, lông cừu, thịt heo sống.
- 8–19: bột/bánh/phô mai/nước táo và hàng chế biến.
- 20–25: khối gỗ, đá, gạch, kính, kim loại, cỏ.
- 26–39: bàn chế tạo, hạt cây, block chức năng, đồ ăn/phân/rương/lửa/đột biến.
- 40–51: hạt và sản phẩm cây nâng cao.
- 52–55: thức ăn từng loài.
- 56–62: vòi và thịt sống/chín.
- 63–70: tên, item cũ, forge, than, bình máu, đá nâng cấp, TNT, cổng hồi sinh.
- 71–77: item migration cũ, bọt biển, hạt boss và nông sản đặc biệt.

Dụng cụ dùng ID 100–111 trong `AdventureBag`: hạt nhanh, thức ăn, xẻng 104, xô 105, kiếm 106, rìu 107 và cung 111. Một số ID 108–110 là di sản migration.

`BagSlot` giữ `item`, `count`, `durability`, `forgeLevel`, `bonusDamage`, `criticalChance`, `haste`. Nhờ vậy hai cây kiếm cùng ID vẫn có thể có chỉ số khác nhau.

### Cửa hàng

`FarmShop` giữ tiền, hạt mua sẵn, thức ăn chung, số cây và giới hạn mua theo ngày. Shop có 42 offer chia 4 trang; nhiều món chỉ mua một lần/ngày, còn hạt và một số vật phẩm được mua không giới hạn. Nó cũng tạo `FarmPenPlacement` để đặt chuồng mua mới vào map.

`FarmInventory` chứa tên và giá bán 78 item. Bán item đột biến dùng giá riêng theo loại cây ×3. `SellAll()` của inventory chỉ bán nông sản; `AdventureBag.SellEverything()` bán cả hạt, nguyên liệu và dụng cụ.

## 10. Crafting, chế biến, giao đơn và nâng cấp

### Chế tạo tức thời

`FarmCraftOrders` đọc `StreamingAssets/crafting.json`, hiển thị bàn chế tạo, kiểm nguyên liệu rồi tạo output. File hiện có 25 công thức: bó nông sản, vật liệu, thức ăn, rương/lửa, công cụ, cung/tên, forge, bình máu, TNT và cổng hồi sinh.

`CraftingTable` chỉ là adapter `IInteractable` gọi `FarmCraftOrders.OpenCraft()`.

### Máy chế biến

`FarmProcessing` đọc `recipes.json`. Hiện có 11 công thức máy: bột, bánh, phô mai, nước táo, ván, kim loại, phân bón và bốn loại thức ăn thú.

- Queue được lưu bằng `ProcessingRecord`.
- Lò bánh/lò nung dùng nhiên liệu: gỗ hoặc than.
- `Advance(seconds)` cho phép tiến độ chạy khi chơi và khi ngủ qua đêm.
- Công thức/máy có thể bị khóa theo cấp hoặc blueprint.
- Bão có thể gây hỏng phần trăm tiến độ.

`ProcessingMachine` là component gắn lên máy trong scene để mở đúng trang máy.

### Đơn hàng và mini-game

- `FarmCraftOrders` còn tạo 5 đơn giao hàng/ngày, giao, reroll và lưu trạng thái.
- `FarmDeliveryRush`: mini-game phản xạ giao hàng riêng, route/lane/chướng ngại và nâng xe.
- `FarmRunner`: chạy ba làn, nhảy/trượt, vé hồi theo thời gian thực, điểm cao và mốc thưởng.
- `FarmNoticeBoard`: bảng nhiệm vụ theo stage, nhận thưởng một lần.

### Rèn

`FarmForge` chọn đúng slot kiếm/rìu/cung trong túi. Rèn tăng level; reroll tạo bonus damage, critical chance và haste. Kiếm LV5 có bonus đặc biệt. Cung dùng haste để rút ngắn thời gian kéo. Dữ liệu chỉ số nằm trên từng `BagSlot`, không chỉ theo loại vũ khí.

## 11. Xây dựng, kho và vật phẩm thế giới

`FarmBuildingSystem` ánh xạ item khối sang loại block, hiện preview, xoay/đặt/tháo/phá và lưu `PlacedBlockRecord`. Một số loại đặc biệt tạo hành vi:

- Bàn chế tạo.
- Rương đồ.
- Đống lửa.
- Forge.
- Cổng hồi sinh.

`CampfireCooker` nướng thịt và hong bọt biển; trạng thái nấu được lưu trong record block.

`FarmStorage` quản lý kho cố định và rương khám phá:

- Kéo/thả giữa túi và kho.
- Mở rương, lấy một/cả stack/lấy tất cả.
- Rương khám phá có key bền vững, guard tier và loot đặc biệt.
- Đập rương làm rơi đồ còn lại nhưng không nhân đôi loot.

`WorldPickup` tạo vật phẩm rơi, tự hút khi người chơi tới gần và lưu/khôi phục cả item thường lẫn vũ khí có chỉ số.

## 12. Map khám phá voxel

`ExplorationWorld` là hệ thống lớn nhất về địa hình:

- Mỗi chunk rộng 16×16, cao 40 block.
- Chỉ giữ các chunk trong bán kính quanh người chơi; chunk xa được hủy.
- Seed + Perlin noise/hash tạo đồng cỏ, núi tuyết, đồi cát, hang, sông, hồ, biển và cây.
- `removed` ghi block đã đào; `additions` ghi block đã đặt; vì vậy không cần lưu toàn bộ thế giới.
- Mesh chỉ dựng các mặt block tiếp xúc không khí để giảm số tam giác.
- `MeshCollider` được xây lại khi địa hình đổi.
- Đào block cho vật phẩm tùy loại, XP và có thể mở blueprint sau 30 block.
- Cây non lớn theo stage; lá rơi khi mất thân đỡ.
- Có hang boss cố định và boss mặt đất/landmark.

Map khám phá dùng `Origin = (175.5, 996, -24.5)`. Nông trại nằm gần Y=0; khám phá nằm gần Y=1000. `IslandManager` teleport giữa hai vùng và nhớ vị trí gần nhất ở mỗi vùng.

`ExplorationLandmarks` sinh công trình/điểm mốc và boss theo seed. `AdventureWildlife` stream thú hoang theo khu vực, giữ record chết/hồi sinh/sinh sản.

## 13. Nước

Nước được chia làm ba lớp:

1. `FarmWaterSystem`: kinh tế và UI của xô, bơm, trạm tưới, vòi thuê.
2. `FarmVoxelWater`: mô phỏng nguồn nước/lan nước và mesh hiển thị chung.
3. `FarmSurfaceWater`: phiên bản grid cho map nông trại dùng collider thường.

`FarmVoxelWater` dùng flood fill có giới hạn reach:

- Nước ưu tiên rơi xuống.
- Nếu bị chặn phía dưới, nước lan ngang và giảm reach.
- `sources` là nguồn do người chơi đặt; `wet` là toàn bộ ô đang có nước.
- Nước tự nhiên của sông/biển được thêm quanh chunk hiện tại.
- Block xây dựng có thể chặn/đẩy nước.

`FarmSponge` hút vùng 3×3×3, chuyển từ item khô sang đầy và có thể hong ở lửa. `IrrigationStation` tưới ruộng trong bán kính; vòi thuê có thời hạn một ngày game và giới hạn thuê theo cấp.

## 14. Thời gian, mùa, thời tiết

`TimeManager.DayLengthSeconds = 1080`, tức một ngày game dài 18 phút thực.

- 28 ngày/mùa, 4 mùa/năm, 112 ngày/năm.
- Màu cỏ/lá thay theo mùa.
- Xác suất ngày mới: chủ yếu nắng, sau đó mưa, sương và ít bão.
- Mưa/bão tưới ruộng tự động.
- Ánh sáng mặt trời, ambient probe, màu trời và fog được tính lại theo giờ.
- `SleepUntilMorning()` fast-forward cây, thú, trứng, máy chế biến và thời tiết.

`DisasterPuzzleManager` mở câu hỏi khi bão; đáp án ảnh hưởng tiền/nguyên liệu/tiến độ. `FarmBed` gọi hệ thống ngủ.

## 15. Chiến đấu, máu và hồi sinh

`AdventureWolves` thực tế là manager sức khỏe/nguy hiểm toàn map khám phá:

- Quản lý HP người chơi, hồi phục, chết và popup hồi sinh.
- Hồi sinh tại chỗ mất 100 xu và giữ đồ.
- Hồi sinh miễn phí về cổng, làm rơi tối đa 3 món.
- Sinh sói ban đêm và thú săn ban ngày.

Các enemy:

- `NightWolf`, `DayPredator`: đuổi, cắn, bị đánh, rơi loot.
- `FarmChestGuard`: guard rương nhiều tier.
- `CaveBoss`: Golem 2.400 HP, phase mạnh dưới nửa máu.
- `FarmEnemyJump`: helper cho quái nhảy vật cản cao một block.
- `FarmEnemyHealthBar`: thanh HP world-space dùng chung.

### Kiếm

`FarmSwordAim` raycast và hỗ trợ bắt mục tiêu hơi lệch tâm nhưng vẫn kiểm tra tầm đánh/tường. `FarmSwordReticle` vẽ vòng ngắm UI. `FarmPlayer.TryAttack()` giữ cooldown, còn `FarmForge.ResolveMelee()` cộng chỉ số/chí mạng.

### Cung

`FarmBow` chạy ở execution order 200, sau camera. Giữ chuột tạo charge, tốc độ từ 16–40 m/s. `TryLaunchVelocity()` giải quỹ đạo đạn đạo thấp có gravity 9,81 m/s².

`FarmArrowProjectile`:

- Tích phân gia tốc chính xác theo bước tối đa 1/120 giây.
- Sphere-cast bán kính nhỏ để không xuyên tường ở FPS thấp.
- Ghim vào mục tiêu/vật thể; có thể nhặt lại.
- Chỉ trừ một mũi tên và một độ bền khi bắn hợp lệ.

### TNT

`FarmTnt` đặt trên map khám phá, chờ đuốc châm, nháy 5 lần rồi đào vùng hình cầu. Ngòi chỉ tiến khi map khám phá đang chạy và không pause. Phase/ngòi được lưu game.

## 16. UI

- `FarmHud`: menu chính, pause, settings, overlay, thông báo và lưu game.
- `FarmHudV2`: HUD gameplay, 9 hotbar, tiền/cấp/thời gian/tọa độ/máu/no.
- `FarmUi`: factory tạo Panel, Button, Text/TMP dùng chung.
- `FarmInventory`: màn thống kê và bán item; cũng là kho số lượng nền.
- `AdventureBag`: lưới 36 slot, kéo-thả, tách stack, quick move, bán/bỏ/sửa.
- `FarmShop`: shop 4 trang và đặt chuồng.
- `FarmBarnMenu`: quản lý/nâng chuồng.
- `FarmNoticeBoard`: quest.
- `FarmTutorialCoach`: hướng dẫn nhiều trang cho người mới.
- `FarmItemIconLibrary`: sinh/cache sprite icon bằng code và ánh xạ item→icon.
- `WorldSignText`: cập nhật bảng chữ ngoài thế giới.
- `ShopCounter`: adapter mở shop.

`FarmHud.WorldClickSuppressed` là chốt an toàn quan trọng: click UI không được rơi xuyên xuống thế giới và vô tình đào/đặt/bắn.

## 17. Lưu game

`FarmSave` dùng `JsonUtility`, schema hiện tại là v21. File mặc định trên Windows:

```text
%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest\farm-manual-save.json
```

`SaveData` gom trạng thái của:

- tiền, hạt, nông sản, inventory, túi và chỉ số vũ khí;
- ruộng, cây, thú, chuồng, cổng, tài nguyên;
- cấp, XP, ngày, mùa/thời tiết, mở đất, nâng tool;
- máy chế biến/nhiên liệu, đơn hàng, Runner/Delivery Rush;
- vị trí ở hai map, voxel đã đào/đặt, nước, TNT;
- block xây, rương, loot rơi, wildlife, boss và HP.

Save an toàn theo quy trình:

1. Ghi file `.tmp`.
2. Nếu đã có save, `File.Replace` tạo `.bak`.
3. Nếu chưa có, move `.tmp` thành save chính.

Khi “chơi lại”, save cũ được đổi tên `.before-new-game-<timestamp>`. Creative từ chối `Save()`. Game chỉ tự load lúc bắt đầu; thoát game không tự save.

Migration đáng chú ý:

- Chấp nhận save v2–v21.
- Chuyển tọa độ map khám phá cũ lên Y≈1000.
- Đổi gỗ/item cũ, xẻng trùng và bottle/xô cũ.
- Hoàn tiền các bình nước bị loại.
- Gán generator version tương thích để không phá thế giới cũ.

Khi thêm field mới, phải tăng version nếu cần và cung cấp giá trị mặc định/migration. Không đổi ID item hoặc thứ tự mảng tùy tiện.

## 18. Chế độ sáng tạo

`CreativeModeManager`:

- Bắt đầu ở LV99, mở vùng/tool, thêm tài nguyên thử nghiệm.
- Cho phép bay F8 và bay xuống Ctrl/X.
- Không ghi save thường.
- Khi quay về menu hoặc thoát, mọi thay đổi creative bị bỏ.
- “Chơi lại từ đầu” có popup xác nhận và gọi archive save.

## 19. Giải thích từng file runtime

### `Scripts/Player`

- `FarmInput.cs`: khai báo action bàn phím/chuột bằng Input System.
- `FarmPlayer.cs`: movement, pause, jump/swim/fly, fall damage, teleport và attack cooldown.
- `FarmCamera.cs`: camera thứ nhất/thứ ba, yaw/pitch, collision và shake.
- `FarmerAnimation.cs`: điều khiển Animator, procedural tay/chân, action trigger, socket đồ cầm.
- `HeldItemVisual.cs`: dựng/đổi model đồ đang cầm theo hotbar và cập nhật bow origin.
- `FarmConsumption.cs`: giữ trái 3 giây để ăn/uống; chứa thêm hiệu ứng bụi chạy.
- `FarmBow.cs`: kéo/bắn cung, quỹ đạo và projectile tên.
- `PlayerSettings.cs`: ScriptableObject tốc độ, nhảy, gravity, sensitivity, camera và interaction.

### `Scripts/Interaction`

- `IInteractable.cs`: contract chung và khung outline.
- `PlayerInteraction.cs`: router input/raycast cho tất cả tương tác.
- `FarmBed.cs`: ngủ tới sáng.
- `FarmSign.cs`: bảng có event/message.
- `FishingPier.cs`: cooldown câu cá và thưởng xu/XP.
- `IslandPortal.cs`: cổng chuyển vùng qua `IslandManager`.
- `ResourceNode.cs`: node đá/quặng có cooldown hồi sinh.

### `Scripts/Crops`

- `CropDefinition.cs`: schema dữ liệu cây.
- `FarmPlot.cs`: state machine ruộng, growth, moisture, mutation và visual stage.
- `FieldManager.cs`: crop đang chọn, kho harvest và tick ruộng theo batch.
- `FruitTree.cs`: cây ăn quả/bụi, trồng, tưới, bón, hái, chặt.
- `FarmDecorTree.cs`: cây gỗ chặt được và persistence.
- `FarmOrchardGate.cs`: cổng báo khóa/mở vườn.

### `Scripts/Animals`

- `AnimalSpecies.cs`: enum bốn loài.
- `FarmAnimal.cs`: AI đơn giản, care, sản phẩm, animation state.
- `AnimalPen.cs`: chuồng, capacity, upgrade, trứng, máng và thu hoạch hàng loạt.
- `AnimalCarry.cs`: nhấc/thả thú.
- `AnimalRestSpot.cs`: lấy sản phẩm ở ổ nghỉ.
- `EggNest.cs`: lấy trứng.
- `PaddockGate.cs`: cửa chuồng.
- `FarmAnimalVisual.cs`: cầu nối movement và Animator imported.
- `AdventureWildlife.cs`: thú hoang, spawn/stream, chiến đấu, cho ăn/sinh sản và save.

### `Scripts/Core` – gameplay chính

- `FarmExpansion.cs`: level/XP, mở vùng, nâng tool và thao tác ruộng nhiều ô.
- `TimeManager.cs`: ngày, mùa, weather, lighting và sleep fast-forward.
- `IslandManager.cs`: menu map, teleport và nhớ vị trí.
- `FarmSave.cs`: serialize, load, backup và migration.
- `FarmProcessing.cs`: máy chế biến JSON, queue, nhiên liệu và khóa tiến trình.
- `FarmCraftOrders.cs`: crafting JSON, đơn giao hàng và adapter bàn/hộp thư.
- `FarmBuildingSystem.cs`: preview/đặt/phá block và block chức năng.
- `FarmStorage.cs`: warehouse/rương/loot UI và guard metadata.
- `FarmWaterSystem.cs`: xô, trạm, máy bơm và vòi thuê.
- `FarmVoxelWater.cs`: mô phỏng nước chung và mesh.
- `FarmSurfaceWater.cs`: nước riêng vùng nông trại.
- `FarmSponge.cs`: hút/hong bọt biển.
- `ExplorationWorld.cs`: terrain voxel, chunk streaming, đào/cây/biome/boss state.
- `ExplorationLandmarks.cs`: landmark và boss mặt đất.
- `AdventureWolves.cs`: HP, hồi sinh, sói và predator.
- `CaveBoss.cs`: AI Golem/boss.
- `FarmChestGuard.cs`: quái canh rương theo tier.
- `FarmEnemyHealthBar.cs`: UI máu world-space.
- `FarmEnemyJump.cs`: helper nhảy vật cản.
- `FarmSwordAim.cs`: chọn/đánh mục tiêu kiếm.
- `FarmTnt.cs`: vòng đời TNT và explosion.
- `FarmForge.cs`: UI rèn và chỉ số vũ khí.
- `FarmRunner.cs`: mini-game chạy ba làn.
- `FarmDeliveryRush.cs`: mini-game giao hàng.
- `FarmSpecialCrops.cs`: cài cây đặc biệt và loot boss.
- `WorldPickup.cs`: item rơi và persistence.
- `FarmTravelPortal.cs`: cổng hồi sinh tự đặt.
- `DisasterPuzzleManager.cs`: puzzle bão.
- `CreativeModeManager.cs`: normal/creative/restart.
- `FarmAudio.cs`: tự tạo nhạc/effect clip và volume.
- `FarmEffects.cs`: popup/particle, animation cây.
- `FarmActionFeedback.cs`: particle phản hồi thao tác và kiểm đường đánh.

### `Scripts/Core` – kiểm thử tự động

- `BuildSmokeCheck.cs`: orchestrator smoke test lớn; chạy khi có `-farmSmokeCheck`, tự thao tác hệ thống, chụp ảnh và thoát với mã lỗi/thành công.
- `AdventureChecks.cs`: kiểm tra adventure/combat.
- `FarmAdventureFeedbackChecks.cs`: kiểm tra phản hồi đánh/tương tác.
- `FarmBowChecks.cs`: kiểm tra quỹ đạo, FPS, va chạm, ammo và durability.
- `FarmNewFeaturesChecks.cs`: kiểm tra nhóm tính năng mới và screenshot.
- `FarmPolishChecks.cs`: kiểm tra polish/UI/visual.
- `FarmSystemsChecks.cs`: kiểm tra liên hệ giữa các hệ thống.
- `FarmTntChecks.cs`: kiểm tra đặt/châm/lưu/nổ TNT.
- `FarmV12Checks.cs`: regression cho schema/tính năng v12.
- `FarmVisualChecks.cs`: kiểm tra visual/model.
- `FarmWaterCanChecks.cs`: kiểm tra xô/nước/bọt biển/vòi.

### `Scripts/UI`

- `FarmUi.cs`: hàm tạo UI dùng chung.
- `FarmHud.cs`: menu/overlay/settings/save/notification.
- `FarmHudV2.cs`: HUD gameplay và hotbar.
- `FarmInventory.cs`: kho số lượng, giá/tên và bán.
- `AdventureBag.cs`: 36 slot, kéo-thả, tool/weapon, no và ăn.
- `FarmShop.cs`: shop, giới hạn ngày và đặt chuồng.
- `FarmBarnMenu.cs`: quản lý chuồng.
- `FarmNoticeBoard.cs`: quest.
- `FarmTutorialCoach.cs`: tutorial.
- `FarmItemIconLibrary.cs`: icon procedural/cache.
- `FarmSwordReticle.cs`: vòng ngắm kiếm.
- `ShopCounter.cs`: tương tác mở shop.
- `WorldSignText.cs`: chữ trên bảng 3D.

## 20. Các script Editor

Các file dưới `Assets/Farm/Editor` chỉ chạy trong Unity Editor, không được đưa vào game build:

- `FarmProjectBuilder.cs`: tạo toàn bộ scene mốc, vật liệu, ruộng, Player/UI và build Windows. `CreateScene()` có thể ghi đè scene.
- `FarmCharacters.cs`: helper dựng farmer/animal, gắn model và rig/Animator.
- `FarmShopBuilder.cs`: dựng chuồng, shop, cây và nối các manager gameplay.
- `FarmIslandsBuilder.cs`: dựng các khu/cổng liên quan map.
- `FarmFarmerUpgrade.cs`: thay riêng visual farmer trong scene hiện tại và tạo prefab cây runtime.
- `FarmImportedAssetPipeline.cs`: AssetPostprocessor chuẩn hóa model/material/animation imported cho URP; helper tạo controller.
- `FarmPolishUpgrade.cs`: audit/nâng cấp polish rig.
- `FarmVisualAudit.cs`: import lại asset, log bounds/material/bone và render gallery kiểm tra.

Cảnh báo quan trọng: `CreateScene()` và `RebuildSceneAndBuildWindows()` có thể ghi đè `Farm.unity` và prefab động vật/cây. Nếu chỉ muốn build scene đang chỉnh, dùng `BuildWindowsCurrentScene()` hoặc `Tools/Build-Windows.ps1`.

## 21. Asset, prefab, model, material và animation

- `Data`: ScriptableObject crop và player settings.
- `Prefabs`: 4 loài vật, cây táo và các visual.
- `Models/Imported`: Kenney/Quaternius assets; nguồn và giấy phép ở `ASSET_SOURCES.md`.
- `Materials`: material URP cho môi trường, farmer, thú và model import.
- `Animations`: controller của farmer và ba loài vật imported.
- `Resources`: asset cần load bằng `Resources.Load`, như font, material particle/unlit và cây runtime.
- `Art/DepthFont.shader`: shader chữ/nhãn có xử lý depth.
- `Settings/FarmURP.asset`, `FarmRenderer.asset`: URP pipeline/renderer.

Một phần art dùng FBX/OBJ imported; một phần vẫn được tạo bằng `GameObject.CreatePrimitive`. Vì vậy visual có thể nằm trong prefab/scene hoặc được sinh hoàn toàn bằng code.

## 22. Điểm mạnh kiến trúc

- Có state record rõ ràng cho từng hệ thống và migration save lâu dài.
- Dùng chunk streaming thay vì giữ thế giới voxel vô hạn trong scene.
- Tick ruộng theo batch, pool/stream ở mini-game và world.
- Combat cung xử lý tunnelling/FPS thấp khá cẩn thận.
- UI click có chốt ngăn xuyên xuống gameplay.
- Smoke tests thực sự chạy trong build và kiểm nhiều regression khó.
- Builder giúp tái tạo scene và chuẩn hóa imported assets.

## 23. Điểm cần cẩn thận/kỹ thuật nợ

- Nhiều manager dùng singleton và `FindFirstObjectByType`; dễ dùng nhưng phụ thuộc thứ tự khởi tạo và khó unit test.
- `Gameplay HUD` gắn quá nhiều trách nhiệm, gần với “God Object host”.
- `PlayerInteraction.Update()` có chuỗi điều kiện dài; thêm action mới dễ xung đột ưu tiên click.
- Item dùng số nguyên và nhiều mảng song song. Chèn/đổi ID có thể phá save, recipe và icon.
- UI tạo bằng tọa độ tuyệt đối trong code; responsive và chỉnh bằng Designer khó.
- Nhiều material được tạo runtime; phải hủy đúng và cân nhắc batching/memory.
- `FindObjectsByType` xuất hiện trong các thao tác lặp; với scene lớn hơn cần cache/index.
- Nhiều class được đặt chung một file; người mới khó định vị class theo tên file.
- Scene 105.000 dòng và được builder sinh; merge conflict scene sẽ khó xử lý.
- Map nông trại/map khám phá phân biệt bằng `position.y > 500`; đơn giản nhưng là coupling không hiển nhiên.

## 24. Cách đọc project theo thứ tự hợp lý

Không nên đọc 10.000 dòng theo alphabet. Thứ tự tốt cho người mới:

1. Mở `Farm.unity`, chọn Player và Gameplay HUD để xem component/reference.
2. Đọc `FarmInput` → `FarmPlayer` → `FarmCamera`.
3. Đọc `IInteractable` → `PlayerInteraction`.
4. Đọc `CropDefinition` → `FarmPlot` → `FieldManager` → `FarmExpansion.Work`.
5. Đọc `FarmInventory` và `AdventureBag` cùng lúc.
6. Đọc `FarmShop`, `FarmCraftOrders`, `FarmProcessing`.
7. Đọc `FarmAnimal` → `AnimalPen` → `AnimalCarry`.
8. Đọc `TimeManager` và `FarmSave`.
9. Sau cùng mới đọc `ExplorationWorld`, water, combat và smoke tests.

## 25. Ví dụ mở rộng an toàn

### Thêm một loại cây mới

1. Tạo `CropDefinition` asset mới.
2. Thêm vào `FieldManager.crops` nhưng giữ thứ tự cũ.
3. Quyết định seed item/product item mới; thêm cuối bảng ID, không chen giữa.
4. Cập nhật tên/giá/icon, shop/crafting nếu cần.
5. Cập nhật save array/migration.
6. Thêm visual branch hoặc prefab stage.
7. Thêm smoke test gieo–tưới–lưu–load–thu hoạch.

### Thêm một vật tương tác

1. Viết component `MonoBehaviour, IInteractable`.
2. Có collider trên chính object hoặc child.
3. Cài hint, điều kiện, hành động và outline.
4. Nếu cần ưu tiên đặc biệt, thêm nhánh vào `PlayerInteraction`; nếu không, generic scan tự tìm thấy.
5. Nếu có trạng thái bền vững, thêm record vào hệ thống save phù hợp.

### Thêm item mới

1. Chỉ thêm ID ở cuối để giữ tương thích save.
2. Cập nhật `ItemCount`, `itemNames`, `unitPrices`, icon mapping.
3. Cập nhật các mảng save và migration.
4. Thêm recipe/shop/drop.
5. Kiểm `AdventureBag.Sync`, stack size, edible/tool/block mapping.

## 26. Build và kiểm tra

Chạy game có sẵn bằng `CHAY_GAME.bat` hoặc EXE trong `Builds/Windows-BowPhysics`.

Mở source:

1. Unity Hub → Add project from disk.
2. Chọn thư mục gốc repository.
3. Dùng Unity 6000.3.22f1.
4. Mở `Assets/Farm/Scenes/Farm.unity`.
5. Nhấn Play.

Build scene hiện tại bằng `Tools/Build-Windows.ps1`. Không chạy builder tái tạo scene nếu bạn muốn giữ chỉnh sửa thủ công.

Smoke test dùng tham số dòng lệnh như `-farmSmokeCheck`, `-farmBowOnly`, `-farmSystemsOnly`, `-farmWaterCanOnly`, `-farmTntOnly`, `-farmFeedbackOnly`, `-farmPolishOnly` và `-farmArtCheck`.

## 27. Kết luận ngắn

Đây là project thiên về “code-driven Unity”: scene giữ object/reference nền, nhưng UI, world động, hiệu ứng và nhiều visual được tạo bằng code. Ba trục quan trọng nhất để hiểu toàn bộ game là:

1. `PlayerInteraction` quyết định click làm gì.
2. `FarmInventory` + `AdventureBag` quyết định vật phẩm nằm ở đâu.
3. `FarmSave` gom snapshot/restore của mọi hệ thống.

Khi đã hiểu ba trục này, các hệ thống trồng trọt, thú, crafting, voxel, nước và combat chỉ là những module cắm vào cùng luồng tương tác–inventory–save.
