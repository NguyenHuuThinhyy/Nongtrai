# Kịch bản trình diễn và ghi hình

© HThinh.yy. Đây là kịch bản, chưa thay thế video nghiệm thu.

## Chuẩn bị

Checkout nhánh rubric, kiểm SHA256 gói, đóng Unity Editor nếu PC8GB.
Khởi động Docker/model. Điện thoại cùng Wi-Fi, nhập URL/mã; Adafruit IO dashboard mở.
Xây trạm vùng đầu trong phiên normal hoặc chuẩn bị save22 demo. Ghi tên thiết bị,
OS, commit/build, Docker/Ollama/model digest. Không quay màn hình `.env`/API key.

## Video liền mạch 5–8 phút

1. Mở game Android; di chuyển bằng joystick, vuốt camera, nhảy/chạy, cuốc/gieo/tưới.
2. Bản đồ → Tìm số2D → hoàn tất → quay về3D, đối chiếu xu/đá và quota.
3. Túi kéo đồ vào hotbar; kéo/thả cung bắn vào mục tiêu, xem độ bền giảm;
   xô rỗng lấy hồ, đặt nước, hút bằng bọt biển. Mở rương/nhà hàng/Runner bằng chạm.
4. Menu → AR; quay cả điện thoại và bàn/sàn để thấy tracking thật. Đặt,
   xoay/phóng, chạm cây, xem độ ẩm/tiến độ, hỏi trợ lý ngay từ AR.
5. Hỏi “Cung hỏng sửa thế nào?”, “LV5 thuê mấy vòi?”. Thấy nguồn tham chiếu.
   Câu ngoài phạm vi và backend tắt: thông báo/hướng dẫn tĩnh, game vẫn chơi.
6. Bật cloud. Ghi game và dashboard cùng lúc: độ ẩm/tiến độ xuất hiện,
   OFF/ON đổi trạm đã xây. Thử ngắt mạng: tưới trở về cục bộ.
7. Mở terminal `docker compose ps`, `/health`, cho thấy hai container/model sẵn sàng.
8. Thoát AR, tiếp tục chơi, xác nhận vị trí/save không thay đổi. Chốt phiên bản.

## Tệp minh chứng cần thêm

- `android-device.md`: máy/OS/ARCore, cách đo FPS15phút, trung bình và triệu chứng nhiệt.
- `demo-integration.mp4` hoặc link video không chứa key, có commit/build trong khung đầu.
- `adafruit-dashboard.png`, `docker-status.txt`, `ar-tracking.mp4`.
- Cập nhật từng mục trong ACCEPTANCE.md. Nếu thiếu thiết bị/tài khoản, ghi chưa kiểm tra.
