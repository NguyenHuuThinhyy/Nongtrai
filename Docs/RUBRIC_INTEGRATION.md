# Local: nông trại thu nhỏ, trợ lý và Android

© HThinh.yy. Cập nhật phạm vi ngày06/10/2026. Nhánh codex/rubric-mobile-ar-ai-cloud.
Chủ dự án đã xác nhận gộp bản local vào main ngày 06/10/2026.

## Kiến trúc

- Unity giữ gameplay/save22 trên máy, không gửi save lên dịch vụ.
- Menu Nông trại AR mở miniature local trên PC và Android, không cần backend/camera/Internet. Camera là nền tùy chọn, đặt thủ công; không dò mặt phẳng/tracking tự động.
- CHAY_GAME.bat chạy FastAPI và Ollama native, ghi URL/mã ghép cặp rồi mở game. Model Qwen3 1.7B Q4_K_M chỉ nạp khi hỏi, keep_alive60s; weights/cache ngoài Git.
- Android gọi trợ lý trên PC cùng Wi-Fi. Chưa đóng model AI vào APK.
- Adafruit IO là tùy chọn gửi số liệu mô phỏng/điều khiển trạm tưới; chưa có tài khoản thì tắt. Mất mạng trở về tưới cục bộ.
- Đã bỏ Docker/Compose theo yêu cầu06/10. Không còn phụ thuộc container để chạy hay lưu dữ liệu.

## Khởi động

Xem [hướng dẫn trợ lý](ASSISTANT_START.md). CHAY_GAME.bat tự mở backend nhẹ rồi game;
CHAY_GAME_KHONG_TRO_LY.bat chơi offline. BAT_BACKEND_PC.bat giữ riêng dịch vụ PC;
BAT_BACKEND_CHO_DIEN_THOAI.bat mở API cho LAN. Mã ở Backend/.runtime/connection.txt,
không đưa vào Git/chat. Key Adafruit đặt Backend/.env, không nằm trong ứng dụng.

Model/runtime tải một lần qua nguồn chính thức, các lần sau tái sử dụng. Bản local8GB nên
đóng Unity Editor trong lúc chơi với AI. Lần hỏi đầu phải chờ model nạp; chưa có đo FPS/RAM mới.

## Nông trại thu nhỏ

PC: nhấn J mở/đóng; chuột phải xoay, lăn chuột phóng, click nền trống đặt, cây/công trình xem thông tin.
Android: kéo xoay, hai ngón phóng; nút xoay/phóng vẫn dùng được. Chế độ local mở ngay,
không gọi ARCore/khởi tạo XR session. Giữ thư viện XR cũ để tương thích source/build hiện tại;
chúng không phải điều kiện để mở miniature. Camera tùy chọn640×480/24FPS, mặc định tắt.
Mô hình gồm nhà, cây, vật nuôi và tối đa16ô ruộng từ trạng thái đang chơi; không nhân đôi gameplay.
C mở chat, H ẩn/hiện hướng dẫn khi chơi. Đóng miniature phục hồi vị trí/camera game.

Đây là miniature3D local và nền camera tùy chọn, không được ghi là đã nghiệm thu AR tracking thật.

## Cloud tùy chọn

Tạo Adafruit IO với feed farm-moisture, farm-growth, farm-pump-state, farm-pump-command.
Nhập ADAFRUIT_IO_USERNAME và ADAFRUIT_IO_KEY trong Backend/.env rồi khởi động lại backend.
Game xây trạm vùng đầu, kết nối đúng URL/mã rồi Bật cloud. Telemetry20giây; lệnh5giây.
Giữ hạn mức, lease/phiên/revision, bỏ retained/trùng/cũ, xác nhận kết quả thực thi.
Không có tài khoản thì chơi thường và dùng miniature/AI local vẫn được.

## Kiểm tra và bàn giao

- Tests backend/launcher, Unity Technology với nhập chat và local miniature; tiếp tục smoke gameplay khi thay đổi gameplay.
- Android: cần thử đa chạm, camera,15phút≥30FPS trên máy thật; chưa thay bằng test Windows.
- AI: kiểm trả lời theo source, không đánh đồng HTTP200 với đúng nội dung. Lượt model cũ ghi riêng theo commit.
- Gói Windows phải có EXE/Data/DLL, Backend, Tools và BAT. APK và Unity source đóng riêng, checksum/commit trong manifest.
- Cache/log/key/model weights không đưa vào Git. Hồ sơ cũ về container chỉ là minh chứng lịch sử, không phải cấu hình đang dùng.
- Docker nằm ngoài phạm vi hiện tại; không tự nhận điểm mục Docker trong rubric cũ.
