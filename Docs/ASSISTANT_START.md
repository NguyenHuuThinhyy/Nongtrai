# Chơi với chatbot trên PC

© HThinh.yy. Cập nhật 06/10/2026.

## Trên PC

1. Đóng game đang mở. Giải nén **toàn bộ** gói Windows mới, giữ thư mục Backend và Tools cạnh các file BAT/EXE.
2. Mở **CHAY_GAME.bat** (hoặc CHAY_GAME_CO_TRO_LY.bat).
3. Lần đầu máy mới cần Internet để tải Python, Ollama (~1,91 GB) và Qwen3 (~1,36 GB). Những lần sau dùng dữ liệu đã tải. Đóng Unity Editor trên máy8GB để dành RAM.
4. Chờ dòng **BACKEND SAN SANG** sau khi kiểm tra kết nối/mã và model đã tải. Game tự mở; **C** để hỏi. Địa chỉ/mã kết nối được ghi tự động. Chế độ mặc định nhẹ: model chỉ nạp vào RAM khi gửi câu hỏi, lần hỏi đầu có thể cần vài chục giây; sau60giây không dùng thì nhả model khỏi RAM. Dòng sẵn sàng chỉ xác nhận backend/ghép cặp, không khẳng định AI đã trả lời thử.
5. Giữ cửa sổ trình khởi động trong lúc chơi. Thoát game sẽ đóng các dịch vụ do lần khởi động này mở. Dịch vụ có sẵn của bạn không bị tắt.

**CHAY_GAME_KHONG_TRO_LY.bat** mở game offline ngay. Mở trực tiếp NongTrai.exe cũng không tự khởi động backend. Khi đó vẫn chơi và xem AR được, nhưng chat AI cần mở backend riêng.

**BAT_BACKEND_PC.bat** chỉ mở trợ lý, giữ dịch vụ chạy đến khi bạn đóng cửa sổ/Ctrl+C. Sau khi thấy sẵn sàng, mở game để đọc kết nối mới. Không cần tải lại model trên máy đã chuẩn bị runtime. Để kiểm tra thêm một câu hỏi thật trước khi mở game: `Tools/Start-Assistant.ps1 -VerifyModel`.

**AR không cần backend: nhấn J mở/đóng trên PC.** Trợ lý / AR / Cloud → Nông trại AR → Bật webcam nếu cần. PC đặt mô hình thủ công trên nền webcam, không dò mặt phẳng tự động. Backend chỉ cần cho phần Hỏi trợ lý. H ẩn/hiện hướng dẫn.

## Điện thoại

- Trên PC chạy **BAT_BACKEND_CHO_DIEN_THOAI.bat**, giữ cửa sổ mở. PC và điện thoại cùng Wi-Fi.
- Trong game điện thoại → Kết nối: nhập `http://IP-LAN-CUA-PC:8000`; mã nằm trong file **Backend/.runtime/connection.txt** trên PC. Không gửi file này lên Git/chat.
- Cho phép Python trên mạng Private trong Windows Firewall nếu Windows hỏi. Script không tự đổi tường lửa.
- Lưu & kiểm tra rồi mở chat. Mô hình nông trại local trên điện thoại không cần backend hoặc ARCore; camera chỉ là tùy chọn nền.

## Khi có lỗi

- **Failed to connect / chưa kết nối backend:** đóng game, chạy CHAY_GAME.bat và chờ đủ bốn bước. Đừng chỉ mở EXE.
- **Cổng8000 đang dùng / mã khác / backend cũ:** đóng cửa sổ backend cũ của bạn rồi mở lại launcher mới. Không cần xóa save.
- **Model chưa sẵn sàng:** kiểm tra Internet trong lần tải đầu; xem lỗi ở cửa sổ và Backend/.runtime/api.log hoặc ollama.log. Launcher không mở game với thông báo sẵn sàng giả.
- **Đường dẫn runtime không còn tồn tại:** máy phát triển có thể dùng Backend/runtime.local.json để trỏ tới runtime sẵn có. Sửa đúng đường dẫn hoặc đổi tên file cấu hình đó để dùng bản tải mặc định trong `%LOCALAPPDATA%/NongTraiAssistant`.
- **Muốn chơi ngay khi tải lỗi:** chạy CHAY_GAME_KHONG_TRO_LY.bat.

Runtime/model được lưu riêng, không đưa weights hay mã kết nối vào source. Backend/.env giữ khóa riêng. Cấu hình kết nối cũ được sao lưu trước khi thay, dữ liệu gameplay/save không bị sửa.

## Công nghệ và kiểm tra

Launcher PC dùng Python/Ollama native. Đã bỏ Docker/Compose theo yêu cầu06/10. Save/game/model cache đều nằm trên máy; Start-Services.ps1 là lối gọi tương thích tới launcher local.

Nguồn tải: [Python3.12.10](https://www.python.org/downloads/release/python-31210/), [Ollama0.12.3](https://github.com/ollama/ollama/releases/tag/v0.12.3), [pip zipapp](https://pip.pypa.io/en/stable/installation/#standalone-zip-application). Hai ZIP runtime kiểm SHA256; model kiểm digest theo Backend/MODEL-MANIFEST.json.

Kiểm tra launcher tự động chỉ dùng tiến trình giả và thư mục tạm; không chứng minh backend đã chạy trên PC. Lần khởi chạy native từ công cụ Codex trước đây bị duyệt tự động từ chối “blocked by policy”, nên cần chủ dự án tự chạy BAT để kiểm tra kết nối thật. Các lượt model trước có minh chứng lịch sử riêng trong Docs/TEST_RESULTS.md.
