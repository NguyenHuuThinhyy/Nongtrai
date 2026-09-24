# Nông Trại – First Harvest

Game nông trại Unity cho Windows. Trồng cây bằng chuột trái, chăm vật nuôi, chế biến/chế tạo theo JSON, giao đơn tại hộp thư, mở đất, nâng cấp dụng cụ và khám phá địa hình khối sinh liên tục theo seed. Có túi đồ 36 ô dùng chung hotbar 9 ô, minh họa vật phẩm, bản đồ nông trại 2D và thông báo vật nuôi đói. Hai map nhớ vị trí khi chuyển qua lại. Một ngày game dài 18 phút, có bốn mùa, thời tiết, ngủ qua đêm và câu đố ứng phó bão. Menu chính có chế độ sáng tạo LV99 để bay và kiểm thử mà không sửa bản lưu chơi thường.

**Chơi ngay:** mở `D:\GAME_NongTrai\Builds\Windows\NongTrai.exe`; giữ nguyên cả thư mục `Windows` khi sao chép sang máy khác. Không cần cài Unity để chạy bản Windows. Xem [CHOI_GAME.md](CHOI_GAME.md) để biết điều khiển và vòng chơi.

**Mở dự án:** Unity Hub → Add project from disk → `D:\GAME_NongTrai`, dùng Unity 6000.3.22f1. Scene chính là `Assets/Farm/Scenes/Farm.unity`. Dự án dùng URP, Input System, Cinemachine, uGUI và TextMeshPro. `Assets/Farm/Editor/FarmProjectBuilder.cs` dựng scene và build bản Windows qua `BuildWindows`. Chạy công cụ dựng lại sẽ ghi đè scene Farm, nên giữ bản sao nếu đã sửa scene bằng tay.

**Dữ liệu JSON:** `Assets/StreamingAssets/recipes.json` chứa 7 công thức máy có thời gian và hàng đợi, gồm máy ủ phân; `Assets/StreamingAssets/crafting.json` chứa 18 công thức ghép tức thì tại bàn chế tạo, gồm xẻng, kiếm và rìu. Mã hàng trong `FarmInventory.cs` từ 0 đến 39, với nông sản đột biến và thịt nướng. Bản lưu v12 giữ trạng thái cây, phân bón, dụng cụ, chuồng đặt được, máu/đói, đơn hàng, thịt đang nướng và nước mưa.

**Lưu game:** chỉ nút Lưu game trong menu Esc ghi bản lưu. Đường dẫn trên Windows là `%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest\farm-manual-save.json`. Game tự tải bản lưu này khi khởi động; thoát không tự lưu. Chế độ sáng tạo dùng bản sao trong bộ nhớ, khóa nút lưu và bỏ toàn bộ thay đổi khi về menu hoặc thoát.

Để build lại bằng PowerShell, đóng Editor đang mở cùng dự án rồi chạy:

```powershell
& 'D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\GAME_NongTrai' -executeMethod NongTrai.Editor.FarmProjectBuilder.BuildWindows -logFile 'D:\GAME_NongTrai\Logs\build.log'
```

Ảnh `ChatGPT Image Sep 20, 2026, 07_13_45 PM.png` là tham chiếu bố cục và màu sắc. Model hiện tại được dựng bằng primitive Unity; vẫn có thể thay bằng asset 3D về sau.

Map khám phá tải/gỡ từng chunk 16×16 m quanh người chơi. Bản lưu v12 giữ seed, ô đã đào, túi đồ, thú hoang, vật phẩm rơi, vị trí hai map, kho/rương, cây đột biến, chuồng đặt được và trạng thái hướng dẫn; đọc được bản v2–v11. Chỉ bấm Lưu game mới ghi tiến độ.
