# Handoff — LocalAI, 06/10/2026

© HThinh.yy. Chủ dự án yêu cầu dọn bản cũ, đẩy Git và gộp vào main trong lượt hiện tại.

- Code game/build đã tạo từ 6e0f6ae; Windows runtime 1.265.433.848 byte, APK 1.161.665.701 byte, 0 lỗi build. Chưa chạy inference trong EXE, chưa thử APK/đo CPU/RAM thật. Không bật âm thanh khi sửa.
- FarmLocalChat gọi farm_chat.dll/libfarm_chat.so trong game. Model Qwen3-1.7B-Q4_K_M.gguf do chủ dự án đặt tại Backend/models; SHA256 b139949c5bd74937ad8ed8c8cf3d9ffb1e99c866c823204dc42c0d91fa181897, 1.107.409.472 byte. Apache 2.0; llama.cpp b7199 MIT; giấy phép đi kèm gói.
- Chỉ mở ChatPanel mới nạp AI. Đóng/ẩn chat, xem hướng dẫn hoặc chuyển ứng dụng hủy chuẩn bị/trả lời và giải phóng native model sau worker kết thúc. Mở lại chờ lần cũ dừng xong. Android giữ file model đã kiểm tra, không lấy lại từ APK mỗi lần.
- J mở miniature local, camera tùy chọn, không cần server/XR tracking. C mở chat, H ẩn hướng dẫn. Save22 không đổi; URL/history chat riêng.
- Backend Python/Ollama chỉ dùng nếu chủ động chọn AI qua PC. Adafruit tùy chọn, chưa có tài khoản, mặc định tắt. Không xóa backend/asset/gameplay đang dùng. Docker đã bỏ.
- Builds chỉ giữ Windows-LocalAI và Android-LocalAI. Package-LocalChat là công cụ đóng gói hiện hành. Build/gói cũ và công cụ đóng gói cũ được cất ngoài dự án ở D:/NongTrai_LuuTru; không ghi đè trở lại.
- Release mới localai-20261006 gồm Windows ZIP có EXE, APK và Unity source ZIP kèm model. Source Git bỏ build/cache/weights; clone cần lấy Backend/models từ source ZIP. DongGoi giữ manifest/checksum của bản mới.
- Farm.unity SHA256 c6566d428bb42535efeb421c376cf984eec5e761be6da945d10ed251bd179416 không đổi. Không chạy FarmProjectBuilder.

Xem Docs/LOCAL_CHAT.md, HUONG_DAN_NHOM.md và Docs/TEST_RESULTS.md (kết quả lịch sử theo commit). Không ghi các test/runtime chưa chạy là đã đạt. Native Python/Ollama service startup từ công cụ từng bị auto-review chặn; không thử lại/bypass. Nhánh và main chỉ được cập nhật theo yêu cầu đã cho phép của chủ dự án.
