# Tiêu chí → chức năng → kiểm thử → minh chứng

© HThinh.yy. Chủ dự án phải tự kiểm tra và xác nhận trước khi gộp main.
Không tự quy đổi các mục dưới đây thành 100/100; giảng viên chấm theo rubric chi tiết.

| Thành phần rubric | Chức năng/code | Kiểm tra phải đạt | Minh chứng cần nộp |
|---|---|---|---|
| Unity 2D/3D /20 | Farm + NumberMemory + Runner + nhà hàng | Full smoke; nhận thưởng một lần; chơi cảm ứng | Log smoke + video liền mạch |
| AR + deep learning /20 | FarmAR, AR Foundation/Core6.3.5; Qwen3/Ollama/BM25 | Tracking Android thật; model sinh câu; ≥27/30 câu đúng | Video AR camera thật + raw 30 câu + thời gian |
| Mobile /20 | FarmControls/FarmInput/MobileUI; APK ARM64 | Toàn bộ luồng chạm, 15 phút trung bình≥30FPS | APK + thiết bị/OS + screen recording + đo FPS |
| IO cloud /20 | FarmServices + FarmCloud, Adafruit IO TLS | Telemetry/dashboard/lệnh thật, offline/trùng/phiên | Video dashboard và game cùng lúc |
| Docker /20 | Dockerfile, compose, Start-Services | Checkout sạch → Compose → model → API từ điện thoại | docker compose ps/log, health, video |

## Supporting work (thiếu sẽ bị trừ 20 điểm một lần)

| Mục | Hồ sơ |
|---|---|
| Planning | RUBRIC_INTEGRATION.md: phạm vi, lịch, điều kiện nghiệm thu |
| Testing | TEST_RESULTS.md, bộ test Unity/backend, bộ 30 câu |
| Evidence of integration | Evidence/Rubric + video theo DEMO.md; phần chưa thực hiện ghi pending |
| Documentation | README, CHOI_GAME, CODEX_HANDOFF, hướng dẫn kết nối/build |
| Demonstration | DEMO.md chuẩn bị luồng; cần bổ sung video thật, không coi kịch bản là video |

## Checklist chủ dự án (chưa đánh dấu tự động)

- [ ] Windows: toàn bộ gameplay, cung/độ bền/nước/đồ/rương/hồi sinh đúng.
- [ ] Minigame 2D: 5/5 nhận 200 xu và đá theo quota; không thưởng trùng.
- [ ] Android: ghi model máy, Android/version, resolution và cài được APK.
- [ ] Android: joystick+vuốt+nhảy+chạy đa chạm; giữ/thả cung, đào, nước, đặt/xoay, creative.
- [ ] Android: túi/rương/chế tạo/rèn kéo thả/tách/chuyển nhanh; nhà hàng/câu cá/Runner/Tìm số.
- [ ] Android: bàn phím tiếng Việt, tai thỏ, app background/quay lại; chơi15phút≥30FPS.
- [ ] AR: quyền camera, hỗ trợ/không hỗ trợ, đặt/xoay/phóng/tap cây, tracking mất/phục hồi.
- [ ] AR: thoát/chat/thoát app/quay lại không làm mất vị trí/vật phẩm.
- [ ] Chat: 30 câu đối chiếu source, ≥27 đúng; câu ngoài phạm vi/tiếp nối/hủy/offline.
- [ ] Cloud: tài khoản/feed/dashboard thật; thấy số liệu và bật/tắt đúng trạm đã xây.
- [ ] Cloud: mạng rớt/retained/trùng/phiên khác; tưới cục bộ vẫn chạy.
- [ ] Docker: máy khác checkout sạch, Compose build/pull/start/health, điện thoại gọi được API.
- [ ] Demo: quay liền mạch luồng2D→3D→AR→AI→cloud.
- [ ] Chủ dự án xác nhận **đồng ý gộp main**.

Ghi kết quả thực tế/ảnh/video vào Evidence/Rubric, cập nhật TEST_RESULTS trước khi đề nghị merge.
