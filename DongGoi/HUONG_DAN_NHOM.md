# Tải bản mới và phát triển tiếp

© TriForge. Chủ dự án cho phép phát hành và gộp main ngày 08/10/2026.

1. [GitHub Release LocalAI](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/tag/triforge-20261008-280c7dc) có Windows ZIP, APK và source Unity ZIP kèm model/license.
2. Windows: giải nén toàn bộ ZIP, giữ EXE/Data/DLL; mở CHAY_GAME.bat hoặc NongTrai.exe. C mở chat; J mở mô hình local; H ẩn hướng dẫn. AI chỉ nạp khi chat mở, đóng chat sẽ hủy xử lý và giải phóng model.
3. Android: cài APK ARM64 trên Android 8+. Gói khoảng 1,16 GB; lần mở chat đầu cần thêm khoảng 1,2 GB trống để chuẩn bị model. Không cần PC/Internet. Khuyến nghị 6 GB RAM; chưa nghiệm thu trên điện thoại thật.
4. Unity: dùng source ZIP, hoặc clone main rồi lấy Backend/models từ ZIP vào checkout. Editor 6000.3.22f1, Android Build Support/SDK/NDK/OpenJDK; mở Assets/Farm/Scenes/Farm.unity. Chọn Nong Trai → Technology → Prepare local chat model trước khi Play lần đầu. Không chạy FarmProjectBuilder dựng scene.
5. Build: Tools/Build-Windows.ps1 và Tools/Build-Android.ps1, đầu ra Builds/Windows-LocalAI và Builds/Android-LocalAI. Plugin đã có; muốn biên dịch lại dùng Tools/Build-LocalChat.ps1. Đóng gói bằng Tools/Package-LocalChat.ps1 với commit thực tế dùng build.

Source Git giữ Assets/.meta, Packages, ProjectSettings, Backend, Tools, Docs, Evidence và giấy phép. Bản Windows và APK/model dùng Git LFS trong Builds; ZIP tải độc lập và model cho source nằm tại Release. Manifest/checksum ở DongGoi. Không chia sẻ .env, API key, cache, log hoặc save cá nhân.

AI qua PC và Adafruit IO là tùy chọn riêng; xem Docs/ASSISTANT_START.md. Gameplay và AI mặc định chạy offline. Việc chủ dự án cho phép gộp không xác nhận đã đo tốc độ/CPU/RAM hay chạy APK trên thiết bị thật; ghi kết quả vào Docs/ACCEPTANCE.md.
