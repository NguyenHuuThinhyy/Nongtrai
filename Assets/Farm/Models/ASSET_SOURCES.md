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
