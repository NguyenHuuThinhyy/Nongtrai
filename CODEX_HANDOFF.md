# Handoff local — 06/10/2026

© HThinh.yy. Dự án D:/GAME_NongTrai, nhánh codex/rubric-mobile-ar-ai-cloud.
Chủ dự án xác nhận gộp bản local vào main ngày 06/10/2026. Không chạy builder dựng scene.

- Đã bỏ Dockerfile/Compose và CI container. Workflow hiện tại kiểm backend/launcher trên Windows.
- CHAY_GAME.bat → Tools/Start-Assistant.ps1 → Backend/launcher.py. Kiểm runtime/model digest/health/pair,
  ghi farm-connection.json rồi mở game. AI chỉ nạp khi hỏi, keep_alive60s; -VerifyModel thử câu hỏi thật.
- CHAY_GAME_KHONG_TRO_LY.bat chơi ngay offline. BAT_BACKEND_PC.bat giữ backend, bản CHO_DIEN_THOAI dùng LAN.
- .env/.runtime/runtime.local.json đều ignored. File runtime.local.json trên máy trỏ cache sẵn trong D:/NongTrai_LuuTru.
  Không đóng model weights, Python/Ollama binaries hay khóa vào Git. Máy mới launcher tải một lần.
- FarmAR.Open mở miniature local trên mọi platform, không cần server/XR session. PC chuột phải/lăn chuột;
  mobile kéo/hai ngón; camera tùy chọn640×480/24FPS, mặc định tắt. Giữ snapshot tối đa16ruộng.
  Thư viện/provider XR cũ vẫn còn để tương thích build; chế độ mặc định không gọi Begin tracking.
- FarmServices C mở chat, H ẩn/hiện tutorial. FarmHud không đóng bảng vì E/X khi input focus.
- Save22 giữ trên máy, đọc21. URL/history chat độc lập save. Farm.unity SHA
  c6566d428bb42535efeb421c376cf984eec5e761be6da945d10ed251bd179416 không đổi.
- Cloud Adafruit tùy chọn: chưa có tài khoản, giữ tắt; mất mạng tưới local. Không gửi gameplay save.
- Gói Windows phải chứa Backend/Tools/BAT bên cạnh EXE/Data/DLL. Source và APK đóng riêng.
  Release hiện tại rubric-preview-20261006. Manifest ghi nguồn build và nguồn gói riêng.

Xem Docs/TEST_RESULTS.md cho kết quả mới và lịch sử riêng theo commit. Không coi test mock là dịch vụ thật.
Native service startup từ công cụ từng bị auto-review chặn 'blocked by policy'; không thử lại/bypass.
Đã chuẩn bị runtime/imports, cần chủ dự án chạy BAT và test thực tế. Không bật âm khi sửa/kiểm tra.
Smoke bằng Tools/Test-Windows.ps1 dùng -farmMute. Việc gộp bản này vào main đã được chủ dự án cho phép.
