# Nông Trại – First Harvest

Game nông trại Unity cho Windows. Bản hiện tại có trồng cây, chăn nuôi và chăm sóc vật nuôi, chế biến theo công thức JSON, mở đất theo cấp độ, nâng cấp dụng cụ, bốn đảo chức năng, NPC và chợ đấu giá mô phỏng chơi đơn. Một ngày game dài 10 phút, có bốn mùa, thời tiết, ngủ qua đêm và câu đố ứng phó bão. HUD có TextMeshPro, túi đồ dạng lưới, hotbar chín ô; game có nhạc nền, hiệu ứng và lưu thủ công.

**Chơi ngay:** mở `D:\GAME_NongTrai\Builds\Windows\NongTrai.exe`; giữ nguyên cả thư mục `Windows` khi sao chép sang máy khác. Không cần cài Unity để chạy bản Windows. Xem [CHOI_GAME.md](CHOI_GAME.md) để biết điều khiển và vòng chơi.

**Mở dự án:** Unity Hub → Add project from disk → `D:\GAME_NongTrai`, dùng Unity 6000.3.22f1. Scene chính là `Assets/Farm/Scenes/Farm.unity`. Dự án dùng URP, Input System, Cinemachine, uGUI và TextMeshPro. `Assets/Farm/Editor/FarmProjectBuilder.cs` dựng scene và build bản Windows qua `BuildWindows`. Chạy công cụ dựng lại sẽ ghi đè scene Farm, nên giữ bản sao nếu đã sửa scene bằng tay.

**Công thức chế biến:** sửa `Assets/StreamingAssets/recipes.json` với mã hàng trong `FarmInventory.cs`. Mỗi công thức ghi máy, số nguyên liệu, thành phẩm và thời gian. Mã hiện có: 0 lúa mì, 3 táo, 5 sữa, 8 bột mì, 9 bánh mì, 10 phô mai, 11 nước táo, 12 gỗ, 13 quặng, 14 ván, 15 kim loại.

**Lưu game:** chỉ nút Lưu game trong menu Esc ghi bản lưu. Đường dẫn trên Windows là `%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest\farm-manual-save.json`. Game tự tải bản lưu này khi khởi động; thoát không tự lưu. Bản lưu gồm ngày/giờ/mùa/thời tiết, vị trí, tiến độ đảo và các hệ thống nông trại.

Để build lại bằng PowerShell, đóng Editor đang mở cùng dự án rồi chạy:

```powershell
& 'D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\GAME_NongTrai' -executeMethod NongTrai.Editor.FarmProjectBuilder.BuildWindows -logFile 'D:\GAME_NongTrai\Logs\build.log'
```

Ảnh `ChatGPT Image Sep 20, 2026, 07_13_45 PM.png` là tham chiếu bố cục và màu sắc. Model hiện tại được dựng bằng primitive Unity; vẫn có thể thay bằng asset 3D về sau.
