# Mở bản thử và phát triển tiếp

© HThinh.yy. Nhánh `codex/rubric-mobile-ar-ai-cloud`; chưa gộp main.

1. Clone nhánh này hoặc tải Unity ZIP từ release `rubric-preview-20261005`.
2. Chơi Windows: giải nén trọn ZIP Windows rồi chạy NongTrai.exe. Nếu clone repo,
   dùng CHAY_GAME.bat ở gốc; không tách EXE khỏi NongTrai_Data/DLL/MonoBleedingEdge/D3D12.
3. Android: tải NongTrai.apk, cho phép cài ứng dụng từ nguồn tải, cài trên Android8+
   ARM64. AR cần máy hỗ trợ ARCore; game thường không cần ARCore.
4. Unity: Hub Add project → checkout, Editor6000.3.22f1, scene Farm.
   Unity tạo Library khi import; cần Internet để tải packages. Không chạy builder dựng scene.
5. Build: Tools/Build-Windows.ps1 và Build-Android.ps1. Backend: Start-Services.ps1.
   Xem Docs/RUBRIC_INTEGRATION.md để cài Docker và tạo dashboard.

Giữ Assets + tất cả .meta, Packages + packages-lock, ProjectSettings, Backend, Tools,
Docs, Evidence và giấy phép. Không chia sẻ cache/log tạm, Backend/.env, model weights
hoặc save cá nhân. Source zip chứa đầy đủ các mục phát triển, bỏ runtime build để giảm trùng;
Windows ZIP và APK là các gói chơi riêng.

Backend/compose.yaml gồm API và OllamaCPU. Model tải vào volume. Chưa có tài khoản
Adafruit IO thì để username/key trống; không bật cloud. Các chức năng game offline vẫn chạy.

Ghi bug/test vào Docs/ACCEPTANCE.md với thiết bị, commit/build, ảnh/video và bước tái hiện.
Không merge main cho tới khi HThinh.yy tự test và xác nhận.
