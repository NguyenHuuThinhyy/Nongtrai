# Bản hiện tại — BowPhysics / 27-09-2026

Đọc BAO_CAO_BOW_PHYSICS.md và đầu CHOI_GAME.md. Runtime `Builds/Windows-BowPhysics/NongTrai.exe`, gói ZIP Windows/Unity BowPhysics trong DongGoi, launcher gốc đã trỏ bản mới. Git tiếp tục chứa đủ runtime/ZIP theo yêu cầu người dùng. Bản ChestLoot cũ ở Recovery/Before-BowPhysics-20260927 và lịch sử Git/Release.

FarmBow LateUpdate order200 sau Cinemachine100, aim ray tâm camera, nghiệm quỹ đạo thấp gravity9.81/speed16–40 theo charge. Origin holder bàn tay, kiểm vật cản ngực→tay; góc nhìn thứ nhất ngang ngực. FarmArrowProjectile dùng tích phân gia tốc hằng + sphere sweep radius.025 từng đoạn <=1/120s, tip làm origin. Khi bắn thành công mới Remove63 và DamageTool (thêm111), cung0 không bắn; HUD ĐB/100; hủy kéo khi pause/UI/đổi cung. Giữ save21, damage/rèn/nhặt tên. Không gọi CreateScene, hash scene giữ nguyên.

FarmBowChecks mới, tích hợp full smoke và cờ -farmBowOnly: 28 quỹ đạo/góc cao-thấp/đứng, 15/30/60/144FPS, hai ray camera, tườngmỏng, wear/ammo/broken/tap/pause. Logs/smoke-bow-only.log đạt; Logs/smoke-bow-full.log full+art exit0; startup-bow.log alive/responding15s không exception. Build log build-20260927-233710.log thành công100993487byte/169file. Ảnh ở Logs/BowPhysicsScreens. Tools/Build-Windows.ps1 đổi Start-Process -Wait sang process.WaitForExit vì wrapper cũ chờ Windows job dù Unity đã kết thúc; helper mới kiểm cú pháp, runtime build trước sửa helper.

Không tải asset mới. Cần giữ cache Library cục bộ để tránh nhập lại toàn bộ ở mỗi lần sửa; không đưa cache/log/save/debug lên Git.

## Làm việc trong dự án

- Unity 6000.3.22f1, URP. Dự án live: D:/GAME_NongTrai; Git mirror: D:/GAME_NongTrai/Nongtrai; main tại https://github.com/NguyenHuuThinhyy/Nongtrai.git.
- Scene: Assets/Farm/Scenes/Farm.unity. FarmProjectBuilder.CreateScene và RebuildSceneAndBuildWindows có thể ghi đè scene/prefab; build bằng BuildWindowsCurrentScene hoặc Tools/Build-Windows.ps1. Giữ .meta/GUID, collider, component gameplay và giấy phép asset.
- FarmSave dùng schema 21, đọc save cũ; ItemCount 78 và 25 công thức chế tạo. Không đổi ID vật phẩm hoặc xóa dữ liệu đang dùng nếu chưa có migration. Creative không ghi bản lưu thường; không tự lưu khi thoát.
- Tham khảo CHOI_GAME.md cho điều khiển và cơ chế hiện tại: một xô nước105 dùng trái, rương mở trực tiếp, miễn phí hồi sinh về cổng/trả100xu tại chỗ, đổi map nhớ vị trí, cung111 trừ độ bền mỗi phát.
- Player/ chứa di chuyển, camera, animation, vật cầm và cung; UI/ chứa túi đồ/HUD; Core/ chứa save, khám phá, rèn, nước và smoke. Công thức nằm ở Assets/StreamingAssets. Giữ namespace NongTrai và hợp đồng IInteractable.
- Model và giấy phép tại Assets/Farm/Models/ASSET_SOURCES.md. Nhân vật Kenney, cây Kenney, thú Quaternius/CC0; một số công trình, dụng cụ và quái vẫn là primitive. Voxel phải giữ khả năng đào/đặt/phá.
- Bản Windows phải đi kèm toàn bộ NongTrai_Data, MonoBleedingEdge, D3D12 và DLL. Repo theo dõi runtime hiện tại và các gói trong DongGoi; không đưa Library, Temp, Logs, Recovery hoặc save cá nhân lên Git.
- Khi cần kiểm tra: -farmSmokeCheck -farmArtCheck; riêng cung thêm -farmBowOnly. Chạy smoke với save tạm. Log của lần build/kiểm tra hiện tại được liệt kê trong BAO_CAO_BOW_PHYSICS.md.

## Dọn tài liệu và đóng gói

Chỉ giữ BAO_CAO_BOW_PHYSICS.md tại gốc. Các báo cáo bản trước và hướng dẫn lịch sử đã được bỏ khỏi thư mục làm việc, main và ZIP hiện tại theo yêu cầu người dùng. Lượt dọn này không sửa code gameplay hoặc runtime; gói được cập nhật tài liệu, source_git_commit và checksum. build_git_commit vẫn là 11cdedd8ff53c3e28695430808c5f17cb30dcffb. Không chạy lại build/smoke cho thay đổi tài liệu.
