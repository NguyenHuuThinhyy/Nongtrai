# Kịch bản trình diễn và ghi hình

© HThinh.yy. Video PC đã có; kịch bản Android/AR/cloud bên dưới chờ thiết bị và tài khoản.

## Minh chứng đã ghi

[demo-pc-automated.mp4](../Evidence/Rubric/demo-pc-automated.mp4): game Windows chạy thật,
điều khiển chạm giả lập trên PC, Tìm số 2D 5/5 nhận 200 xu + đá trong phiên kiểm tra,
Unity gọi HTTP → FastAPI native → Qwen3 thật, và miniature với cây ở các giai đoạn.
Nguồn quay: commit 1734be7, ngày 04/10/2026. Video 1280×720, 5 FPS, 29,4 giây,
không âm thanh; đã kiểm tra trực quan các khung gameplay, thưởng, chat và miniature.
Phiên demo không ghi save gameplay; trạng thái ruộng trong preview là dữ liệu mẫu.
Đây không phải phép đo FPS, tracking AR trên điện thoại hoặc Adafruit thật.

Để quay lại, khởi động backend chẩn đoán localhost:8000 với mã test
`local-unity-smoke-only` và model đã tải, rồi chạy:

```powershell
.\Tools\Capture-Demo.ps1 -Ffmpeg 'duong-dan/ffmpeg.exe'
```

Script tắt âm trước khi nạp game, cất và phục hồi lịch sử chat, ghi PNG vào Temp rồi
mã hóa MP4. Không chạy script khi đang dùng lịch sử chat trong một phiên game khác.

## Chuẩn bị

Checkout nhánh rubric, kiểm SHA256 gói, đóng Unity Editor nếu PC8GB.
Khởi động backend local/model bằng launcher. Điện thoại cùng Wi-Fi, nhập URL/mã; Adafruit IO dashboard mở.
Xây trạm vùng đầu trong phiên normal hoặc chuẩn bị save22 demo. Ghi tên thiết bị,
OS, commit/build, Python/Ollama/model digest. Không quay màn hình `.env`/API key.

## Video liền mạch 5–8 phút

1. Mở game Android; di chuyển bằng joystick, vuốt camera, nhảy/chạy, cuốc/gieo/tưới.
2. Bản đồ → Tìm số2D → hoàn tất → quay về3D, đối chiếu xu/đá và quota.
3. Túi kéo đồ vào hotbar; kéo/thả cung bắn vào mục tiêu, xem độ bền giảm;
   xô rỗng lấy hồ, đặt nước, hút bằng bọt biển. Mở rương/nhà hàng/Runner bằng chạm.
4. Menu → AR; mô hình local mở ngay khi tắt mạng, bật camera nền nếu muốn. Đặt,
   xoay/phóng, chạm cây, xem độ ẩm/tiến độ, hỏi trợ lý ngay từ AR.
5. Hỏi “Cung hỏng sửa thế nào?”, “LV5 thuê mấy vòi?”. Thấy nguồn tham chiếu.
   Câu ngoài phạm vi và backend tắt: thông báo/hướng dẫn tĩnh, game vẫn chơi.
6. Bật cloud. Ghi game và dashboard cùng lúc: độ ẩm/tiến độ xuất hiện,
   OFF/ON đổi trạm đã xây. Thử ngắt mạng: tưới trở về cục bộ.
7. Mở kết nối `/health`, cho thấy backend local sẵn sàng; ghi rõ model chỉ nạp khi hỏi.
8. Thoát AR, tiếp tục chơi, xác nhận vị trí/save không thay đổi. Chốt phiên bản.

## Tệp minh chứng cần thêm

- `android-device.md`: máy/OS/ARCore, cách đo FPS15phút, trung bình và triệu chứng nhiệt.
- `demo-integration.mp4` hoặc link video không chứa key, có commit/build trong khung đầu.
- `adafruit-dashboard.png`, `local-backend-status.txt`, `local-miniature.mp4`.
- Cập nhật từng mục trong ACCEPTANCE.md. Nếu thiếu thiết bị/tài khoản, ghi chưa kiểm tra.
