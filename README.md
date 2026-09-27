# Nông Trại – First Harvest

**Repo đã có cả bản chơi và gói tải:** [Builds/Windows-BowPhysics](Builds/Windows-BowPhysics) chứa EXE cùng đầy đủ dữ liệu; [DongGoi](DongGoi) chứa ZIP Windows/Unity. Chọn **Code → Download ZIP**, giải nén toàn bộ rồi chạy **CHAY_GAME.bat** ở thư mục gốc. Clone repo cũng có đủ bản chơi và source để build tiếp.

**Tải bản Windows và source Unity đầy đủ:** [GitHub Release BowPhysics](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/tag/bowphysics-20260927). Hướng dẫn cho thành viên mới: [HUONG_DAN_NHOM.md](HUONG_DAN_NHOM.md). Script build scene hiện tại: [Tools/Build-Windows.ps1](Tools/Build-Windows.ps1).

Game nông trại Unity cho Windows. Trồng cây bằng chuột trái, chăm vật nuôi, chế biến/chế tạo theo JSON, giao đơn tại hộp thư, mở đất, nâng cấp dụng cụ và khám phá địa hình khối sinh liên tục theo seed. Có túi đồ 36 ô dùng chung hotbar 9 ô, minh họa vật phẩm, bản đồ nông trại 2D và thông báo vật nuôi đói. Cả hai map nhớ vị trí khi chuyển qua lại. Khi chết: trả 100 xu để hồi sinh tại chỗ, giữ đồ; lựa chọn miễn phí về cổng và rơi tối đa 3 món. Một ngày game dài 18 phút, có bốn mùa, thời tiết, ngủ qua đêm và câu đố ứng phó bão. Menu chính có chế độ sáng tạo LV99 để bay và kiểm thử mà không sửa bản lưu chơi thường.

**Cung đã sửa:** tên bù độ rơi theo tâm ngắm, tốc độ theo lực kéo, va chạm theo từng đoạn ngắn; mỗi lần bắn mất 1 độ bền, cung hỏng cần sửa. Xem [BAO_CAO_BOW_PHYSICS.md](BAO_CAO_BOW_PHYSICS.md).

**Rương mới:** chuột phải mở thẳng bảng đồ; bấm từng món, Shift + click cả chồng hoặc **Lấy tất cả**. Giữ trái để đập vỡ và thả phần đồ còn lại. Không có câu hỏi; vẫn cần hạ quái canh.

**Chiến đấu và hồi sinh:** hồi sinh tại chỗ giá 100 xu; bọt biển đầy hong 10 giây ở đống lửa; kiếm có vòng ngắm rỗng với hỗ trợ đánh hơi lệch tâm, giữ tầm đánh và chặn bởi tường.

**Gói mới:** `DongGoi/NongTrai-Windows-BowPhysics-20260927.zip` và `DongGoi/NongTrai-Unity-BowPhysics-20260927.zip`. Báo cáo: [BAO_CAO_BOW_PHYSICS.md](BAO_CAO_BOW_PHYSICS.md). Gói trước được giữ dự phòng trong Recovery; runtime đang dùng là Windows-BowPhysics.

**Chơi ngay:** mở `D:\GAME_NongTrai\Builds\Windows-BowPhysics\NongTrai.exe`; giữ nguyên cả thư mục `Windows-BowPhysics` khi sao chép sang máy khác. Không cần cài Unity để chạy bản Windows. Xem [CHOI_GAME.md](CHOI_GAME.md) để biết điều khiển và vòng chơi.

**Nước và túi đồ:** một xô nước dùng **chuột trái**: rỗng → múc hồ/sông, đầy → đặt nước → rỗng. Shop bán bọt biển hút nước trong 1 ô xung quanh rồi đầy. Hạt hết tự biến mất; bán được tất cả đồ trong túi. Thuê vòi tự tưới 350 xu/ngày, tối đa 3/ngày (LV3/5/7 mở 4/5/6), không nạp nước. Rương boss có bí pha lê và dâu hoàng kim. Quái nhảy qua khối cao 1 ô, thanh máu có số rõ hơn. © HThinh.yy ở góc màn hình.

**Mở dự án:** Unity Hub → Add project from disk → `D:\GAME_NongTrai`, dùng Unity 6000.3.22f1. Scene chính là `Assets/Farm/Scenes/Farm.unity`. Dự án dùng URP, Input System, Cinemachine, uGUI, TextMeshPro và Animation. `Assets/Farm/Editor/FarmProjectBuilder.cs` dựng scene qua `CreateScene`; lệnh `BuildWindows` build scene hiện tại mà không dựng lại. Tạo scene mới sẽ ghi đè `Farm.unity` và các prefab động vật/cây.

**Dữ liệu JSON:** `Assets/StreamingAssets/recipes.json` chứa 11 công thức máy; `Assets/StreamingAssets/crafting.json` chứa 25 công thức ghép tức thì, gồm cung gỗ, 5 mũi tên, bàn rèn và cổng hồi sinh. Mã hàng trong `FarmInventory.cs` từ 0 đến 77. Bản lưu v21 giữ TNT đã đặt/ngòi đang cháy, chỉ số/LV riêng từng vũ khí (kể cả đồ rơi), vé/kỷ lục Runner, nguồn nước đổ, quái canh rương, nhiên liệu máy và hai boss; đọc bản v2–v20.

**Lưu game:** nút Lưu game trong menu Esc ghi bản lưu. “Chơi lại từ đầu” có xác nhận, cất bản lưu cũ thành `.before-new-game-<thời gian>` và bắt đầu LV1 với map mới; không lưu phiên hiện tại. Đường dẫn trên Windows là `%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest\farm-manual-save.json`. Game tự tải bản lưu này khi khởi động; thoát không tự lưu. Chế độ sáng tạo dùng bản sao trong bộ nhớ, khóa nút lưu và bỏ toàn bộ thay đổi khi về menu hoặc thoát.

Nhân vật hiện dùng Kenney Mini Characters biến thể `character-male-e.fbx` màu da sáng, cao 1,90 m, thêm mũ rơm và chuyển động tay/chân rõ hơn khi đi. Model có clip đi/chạy/nhảy/làm việc/đánh. Bò/heo/cừu dùng Quaternius FBX CC0; gà dùng OBJ CC0. Cây tán/cây ăn quả, bụi việt quất, lúa mì và bí ngô dùng Kenney Nature Kit CC0. Di chuyển của nhân vật tăng/giảm tốc mượt hơn, thú được nội suy khi đi và đứng yên khi tạm dừng. Người chơi mất máu khi rơi quá cao; cáo/rắn/sói lùi ra sau khi cắn. Xem [ASSET_SOURCES.md](Assets/Farm/Models/ASSET_SOURCES.md) để biết nguồn và giấy phép. Nhà/chuồng, máy móc, sói/cáo/rắn và một số cây trồng còn dùng visual dựng trong Unity; voxel vẫn giữ để đào/đặt/phá.

Để build bản Windows đang dùng, đóng Editor đang mở cùng dự án rồi chạy. Lệnh này dùng scene hiện tại và không gọi builder dựng lại:

```powershell
$env:FARM_BUILD_OUTPUT='Builds/Windows-BowPhysics'
& 'D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\GAME_NongTrai' -executeMethod NongTrai.Editor.FarmProjectBuilder.BuildWindowsCurrentScene -logFile 'D:\GAME_NongTrai\Logs\build.log'
```

Ảnh `ChatGPT Image Sep 20, 2026, 07_13_45 PM.png` là tham chiếu bố cục và màu sắc. Một phần visual vẫn được dựng bằng primitive Unity; xem bảng nguồn model để biết các nhóm đã thay và còn lại.
