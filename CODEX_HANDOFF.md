# Bản hiện tại — NumberMemory / 02-10-2026

## Nhánh và source

Làm việc trên codex/number-memory-rewards từ main ad74f22 (đã chứa VisualRedesign). Chỉ gộp main sau khi build Windows và các smoke đạt. Source live D:/GAME_NongTrai; Git mirror D:/GAME_NongTrai/Nongtrai; remote https://github.com/NguyenHuuThinhyy/Nongtrai.git.

Unity 6000.3.22f1, URP, Windows Mono. Scene Farm giữ nguyên: SHA256 d4d1ffaabbeddafe2b86b6732b3598fe518fb61254870cba6a4d79d3244e182d. Không đổi ProductName/EditorBuildSettings, không chạy FarmProjectBuilder.CreateScene hoặc RebuildSceneAndBuildWindows.

## Minigame 2D

Gói nhập thực tế D:/23714291_NguyenHuuThinh/Cau1_2D.unitypackage; tên người dùng ghi ban đầu không tồn tại. Chỉ nhập 9 asset riêng, bỏ 11 asset thuộc Packages để không ghi đè package cài đặt. Bản gốc và source trước sửa nằm Recovery/Before-NumberMemory-20261002.

- Assets/Exam2D/Runtime/NumberMemoryGame.cs: giữ 49 số, 5 câu, 5 giây và click sai không tính điểm. RewardProvider tách ví Farm khỏi assembly Midterm2D; chơi riêng có ví trong phiên.
- Assets/Scenes/Cau1_NumberMemory.unity: scene chơi độc lập. Assets/Exam2D/Resources/NumberMemory/NumberMemoryUI.prefab: chỉ Canvas/UI, không camera/EventSystem thứ hai.
- FarmRedesign.Theme bỏ qua Canvas của NumberMemoryGame để giữ màu/độ tương phản gốc.
- FarmNumberMemory.cs: Tab → Tìm số 2D; 20 xu/câu, +100 xu khi 5/5, tối đa 3 lượt có điểm/ngày, 1 đá item68 cho lần 5/5 đầu/ngày. Luyện tập sau quota/sáng tạo không thưởng. Túi đầy dùng WorldPickup. Esc trở về, giữ vị trí; nông trại tạm dừng.
- FarmSave schema vẫn 21, thêm trường optional numberMemory, lưu quota/đá/ngày/kỷ lục. Save cũ không có trường này vẫn đọc được. Không sửa ID item hoặc cơ chế lưu thủ công.
- Exam2DBuilder chỉ dựng riêng UI/scene 2D và giữ scene/config Farm. Export chỉ Assets/Exam2D + scene, không IncludeDependencies. NUMBER_MEMORY_PACKAGE_OUTPUT chọn file xuất; mặc định DongGoi/Cau1_2D.unitypackage.

## Kiểm tra và bàn giao

Build cuối: Logs/build-20261002-205230.log. FarmNumberMemoryChecks có cờ -farmSmokeCheck -farmNumberMemoryOnly và nằm trong full smoke. Nó kiểm chuột/timer thật/pause/200 xu+đá/quota/không thưởng trùng/lưu tải/thoát/túi đầy/chơi độc lập; ảnh render 1280×720 và 1000×1000 ở temporaryCachePath.

Kiểm tra bản Windows bằng -farmSmokeCheck -farmArtCheck; kiểm riêng visual bằng -farmSmokeCheck -farmRedesignOnly. Log hiện tại dưới Logs/smoke-number-memory*.log, kết quả bàn giao ở DongGoi/RELEASE-MANIFEST.json. Log/cache/save không đưa Git. Build scene đang lưu bằng Tools/Build-Windows.ps1 -OutputDirectory Builds/Windows-NumberMemory.

Runtime phát hành Builds/Windows-NumberMemory/NongTrai.exe; ZIP Windows/Unity và gói minigame ở DongGoi. CHAY_GAME.bat trỏ runtime mới. Giữ toàn bộ NongTrai_Data, DLL, D3D12, MonoBleedingEdge. Bản BowPhysics/VisualRedesign trước nằm Recovery/lịch sử Git; không đóng gói cache, debug symbols hay bản thử trùng.

## Hệ thống giữ nguyên

Cung bù trọng lực và mất độ bền mỗi phát; rương mở trực tiếp; một xô nước105; kiếm106/cung111 theo AdventureBag; rèn riêng vũ khí; hồi sinh tại chỗ100xu/miễn phí về cổng; đổi map nhớ vị trí. Inventory ItemCount78, 25 công thức. Creative không lưu dữ liệu thường. Model/nguồn/giấy phép giữ ở Assets/Farm/Models và Assets/ThirdParty; copyright HThinh.yy.
