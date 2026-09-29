# Nguồn model 3D đang dùng

Chỉ các file có giấy phép CC0 dưới đây được đưa vào `Assets/Farm/Models/Imported`. File giấy phép gốc nằm cạnh từng nhóm. File tải gốc và model cũ đã bỏ nằm trong `Recovery/AssetDownloads` và `Recovery/ArtRepair-20260925/UnusedAssets`, ngoài source và build.

| Nhóm | Tác giả / gói | Nguồn | Giấy phép | Định dạng, gói tải | Đang dùng |
|---|---|---|---|---|---|
| Nhân vật người chơi | Kenney, Mini Characters | https://kenney.nl/assets/mini-characters | CC0 1.0 | FBX, 2.4 MB | `character-male-e.fbx` (da sáng, mũ rơm dựng trong scene), clip idle/đi/chạy/nhảy/làm việc/đánh. Các biến thể không dùng a/b/c/d/f đã chuyển vào `Recovery/Before-Runner-20260927/UnusedCharacters` sau khi kiểm tra GUID không còn tham chiếu trong scene/prefab/asset. |
| Bò, heo, cừu | Quaternius, LowPoly Animated Farm Animal Pack | https://opengameart.org/content/lowpoly-animated-farm-animal-pack | CC0 1.0 | FBX, 7.0 MB | `Cow.fbx`, `Pig.fbx`, `Sheep.fbx`, hoạt ảnh idle/đi khi có clip; collider và gameplay giữ nguyên. |
| Gà | CDmir, cộng tác TinyWorlds • Chicken (animated) | https://opengameart.org/content/chicken-animated | CC0 1.0 | OBJ/MTL, 6.2 MB | Mesh gà cùng chuyển động đi nhẹ bằng script. |
| Cây, bụi, lúa mì, bí ngô | Kenney, Nature Kit 2.1 | https://kenney.nl/assets/nature-kit | CC0 1.0 | FBX, 10.6 MB | Cây cảnh, cây ăn quả, bụi việt quất, lúa mì nhiều giai đoạn, bí ngô. |
| Gói đạo cụ đã tải nhưng chưa tích hợp | Kenney, Survival Kit 2.0 | https://kenney.nl/assets/survival-kit | CC0 1.0 | FBX, 1.9 MB | Kiểm tra GUID thấy không có scene/prefab/script sử dụng. Đã chuyển cả gói/giấy phép vào `Recovery/Before-Runner-20260927/Unused-SurvivalKit`, không đưa vào source/build phát hành. |

Nhà, máy móc, máng ăn, sói/cáo/rắn, boss Golem, bàn rèn, nhà/chướng ngại Runner, cung gỗ, mũ rơm và nhiều cây trồng vẫn dùng visual dựng trong Unity. NPC chưa có trong scene. Map khám phá giữ khối voxel để đào, đặt và phá không bị sai. Các model c, d, f trong gói Mini Characters vẫn ở thư mục Recovery vì chưa có NPC sử dụng.

Nhân vật e được phối trang phục làm việc xanh/kem bằng mesh/material URP, giữ texture mặt gốc; dùng chung ở nông trại và Runner. Cây ven đường Runner và cây gỗ phục hồi dùng prefab Kenney Nature Kit CC0.

## AdventureFeedback — 27/09/2026

Không tải thêm asset. Sói, rắn, gấu và golem canh rương trong `FarmChestGuard.cs` là visual primitive tự dựng, dùng vật liệu URP chung theo từng quái. Hiệu ứng dụng cụ/nước dùng lại `Resources/FarmParticles.mat`. Các model nhập và giấy phép CC0 nêu trên giữ nguyên.

## Visual Redesign toàn game — 28/09/2026

Toàn bộ asset mới nằm riêng trong `Assets/ThirdParty/VisualRedesign`; từng gói giữ nguyên `License.txt`. Các prefab đã chuẩn hóa cho URP được sinh vào `Assets/Farm/Resources/FarmRedesign/Models`. Script chỉ tắt renderer cũ rồi gắn hình mới; gameplay root, collider, ID vật phẩm và dữ liệu save không đổi.

| Gói | Nguồn chính thức | Giấy phép | Phần đang dùng |
|---|---|---|---|
| Kenney Survival Kit | https://kenney.nl/assets/survival-kit | CC0 | Bàn chế tạo/rèn, rương, hàng rào, lửa trại, dụng cụ, nguyên liệu và vật phẩm cầm tay |
| Kenney Food Kit | https://kenney.nl/assets/food-kit | CC0 | Nông sản, thức ăn, thịt, chai, túi hạt và vật phẩm rơi |
| Kenney Nature Kit | https://kenney.nl/assets/nature-kit | CC0 | Cây, bụi, đá, cầu, hoa và cây trồng nhiều giai đoạn |
| Kenney Factory Kit | https://kenney.nl/assets/factory-kit | CC0 | Máy chế biến, bánh răng quay, trạm tưới và máy bơm |
| Kenney Building Kit | https://kenney.nl/assets/building-kit | CC0 | Tường, mái và bậc xây dựng |
| Kenney Mini Characters | https://kenney.nl/assets/mini-characters | CC0 | Nhân vật nông dân mới và animation |
| Kenney UI Pack Adventure | https://kenney.nl/assets/ui-pack-adventure | CC0 | Panel và nút HUD dạng gỗ |
| Kenney Car Kit | https://kenney.nl/assets/car-kit | CC0 | Xe giao hàng và chướng ngại Runner |
| Quaternius Farm Buildings | https://quaternius.com/packs/farmbuildings.html | CC0 | Chuồng lớn, kho và silo |
| Quaternius Ultimate Crops | https://quaternius.com/packs/ultimatecrops.html | CC0 | Bụi quả và cây trồng theo giai đoạn |
| Quaternius Ultimate Animated Animals | https://quaternius.com/packs/ultimateanimatedanimals.html | CC0 | Bò, cáo, sói có animation |
| Quaternius Easy Enemy | https://quaternius.com/packs/easyenemy.html | CC0 | Rắn hang động |
| Quaternius Medieval Weapons | https://quaternius.com/packs/medievalweapons.html | CC0 | Kiếm, cung và mũi tên |
| Kenney City Kit (Suburban) | https://kenney.nl/assets/city-kit-suburban | CC0 | `building-type-n`: nhà ở hai tầng thay cho khối nhà/silo cũ; collider cửa, giường và logic ngủ vẫn thuộc root gameplay cũ |
| PagDev Mailbox (OpenGameArt) | https://opengameart.org/content/mailbox | CC-BY 4.0 | Hộp nhận/giao bưu kiện PBR dùng cho điểm giao đơn; ghi công tác giả PagDev trong `ReadMe.txt` |

## Landscape pass — 28/09/2026

Không bổ sung asset có giấy phép mới. Đợt này tái sử dụng các model CC0 đã tải từ Kenney Nature Kit và Quaternius Farm Buildings:

- `BigBarn`, `OpenBarn`, `SmallBarn`, `Silo_House`, `Well`: thay toàn bộ nhà nông trại và làng ở map khám phá.
- `tree_detailed`, `tree_oak`, `tree_pineRoundA`: thay cây cảnh nông trại và cây voxel nhìn thấy ở map khám phá; collider voxel vẫn được giữ riêng cho đào/chặt cây.
- `rock_largeA/B`, `grass_leafs`, `flower_redA`, `flower_yellowC`: dựng viền hồ, thảm cỏ và hoa trang trí.
- Mặt hồ cong bất quy tắc, bờ đất và gợn nước được tạo bằng mesh runtime; đây là hình học trình bày, không thay `WaterSource`, vùng bơi hay collider đáy hồ.

## Grass interaction và đường chính — 28/09/2026

| Gói | Nguồn chính thức | Giấy phép | Phần đang dùng |
|---|---|---|---|
| Poly Haven, Stony Dirt Path | https://polyhaven.com/a/stony_dirt_path | CC0 | Diffuse và DirectX normal 1K cho đường chính/sân trước; texture lặp theo kích thước mặt đường |
| Kenney Nature Kit, `grass_leafs` | https://kenney.nl/assets/nature-kit | CC0 | Cỏ 3D trên bãi cỏ; `FarmGrassMotion` tạo gió nhẹ và uốn cỏ ra xa khi người chơi tới gần |

Bản sửa hồ thu hẹp dải đất sát nước, thêm dải cỏ chuyển tiếp và emission nhẹ cho mặt nước để bờ không biến thành mảng đen vào ban đêm. File giấy phép Poly Haven được giữ tại `Assets/ThirdParty/VisualRedesign/PolyHaven_StonyDirtPath/License.txt`.

## Nhà ở, bàn chế tạo, hộp thư và bờ hồ kín — 28/09/2026

- Nhà ở dùng `building-type-n` từ Kenney City Kit (Suburban), có hình dáng nhà dân rõ ràng hơn; model và `License.txt` nằm tại `Assets/ThirdParty/VisualRedesign/city-kit-suburban`.
- Bàn chế tạo dùng `workbench` từ Kenney Survival Kit CC0. Hộp thư dùng model Mailbox game-ready của PagDev (CC-BY 4.0), giữ nguyên file ghi công tại `Assets/ThirdParty/VisualRedesign/OpenGameArt_Mailbox/ReadMe.txt`.
- Hai model chỉ thay renderer của object `CraftingTable` và `DeliveryMailbox`; collider, `IInteractable`, nhãn nổi và callback mở bảng chế tạo/giao đơn không đổi.
- Hồ có thêm một nền đất kín lớn hơn hốc cũ, bờ đất chồng lên mặt cỏ và thành bờ hai mặt kéo sâu xuống dưới. Ba lớp này ngăn lộ nền trời/khe trắng ở mép hồ từ góc nhìn thấp. Riêng mặt bờ đất có một `MeshCollider` trùng khít để nhân vật đứng trên viền mà không lún; vùng nước và trigger cũ không đổi.
