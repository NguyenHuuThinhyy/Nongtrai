# Kết quả kiểm tra nhánh rubric

© HThinh.yy. Cập nhật 05/10/2026. Đây là kết quả kỹ thuật; chủ dự án chưa nghiệm thu.
Nhánh `codex/rubric-mobile-ar-ai-cloud`; không gộp main trước xác nhận của chủ dự án.

## Kết quả đã có

| Kiểm tra | Kết quả | Minh chứng và giới hạn |
|---|---|---|
| Đồng bộ Git / nhà hàng | PASS | Fetch 05/10: restaurant-renovation `7633b40` đã có trong nhánh; remote chưa có cập nhật nhà hàng mới hơn |
| Unity Windows build | PASS | `windows-build.txt`: Succeeded, 0 errors; mã Unity `0f245e5`, không đổi trong `a2ed113` |
| Full smoke + art | PASS | `windows-full.txt`: cây, quái/boss, rig, camera, cung/quỹ đạo/độ bền, nước/xô/bọt biển, rương, rèn, hồi sinh, portal, save22, Runner, thưởng 2D |
| Nhà hàng với bố cục chạm | PASS | `windows-restaurant-touch.txt`: 30 công thức, save22/legacy21, phục vụ, bố trí, cầu thang, khách và câu cá; chưa thay thử ngón tay trên điện thoại |
| Tích hợp / input chạm / miniature | PASS trên Windows | `windows-technology-touch.txt`: cạnh nút, dữ liệu Android, hướng dẫn, pause, cloud fallback, cây snapshot và model; không phải AR tracking thật |
| Tắt âm khi sửa/kiểm tra | PASS | Cả ba log 05/10 có `FARM_TEST_AUDIO_MUTED`; AudioListener volume=0 và pause=true trước khi nạp scene |
| Unity → HTTP → Qwen thật | PASS ngày 04/10 | `windows-services-native-1734.txt`, ảnh/chat/video: source `1734be7`, backend native; không phải Docker |
| Backend unit + BM25 | PASS | 8 tests; 30 câu có mục đúng trong top3, dữ liệu hướng dẫn Unity/backend giống nhau; model contract dùng mock |
| Model thật trên PC 8 GB | 27/30 đúng | `chat-model-pc-isolated.json` và review từng câu: Unity/game đã đóng, warm trung bình **15,494 s**, 0 lượt warm >30 s, lượt đầu 22,288 s |
| Docker / model thật / hội thoại | PASS trên CI, 27/30 nội dung đúng | Run37264020925 source d8f91b7: checkout sạch, hai container healthy, HTTP thật, warm **7,751 s**, 0 lượt >30 s; hai câu hỏi tiếp nối và hai câu ngoài phạm vi qua kiểm tra. Runner Ubuntu16GB, không thay thử PC/điện thoại |
| APK Android ARM64 IL2CPP | PASS build/signature/manifest | 52.548.958 bytes (~50,11 MiB), API26→36, GLES3, ARCore Optional, HTTP LAN; chữ ký v2 hợp lệ. Chưa cài/chơi trên điện thoại |
| Clone sạch / đủ runtime / khởi động | PASS | Nongtrai-Moi clone từ remote; 183 file EXE/data/APK có SHA256 giống bản đã test, Technology touch qua kiểm tra khi backend tắt |
| Video PC tự động | PASS | MP4 1280×720/5FPS/29,4s: gameplay → thưởng 2D → HTTP/model → preview miniature. Preview cây dùng trạng thái mẫu, không ghi save |
| Scene và save | PASS | Farm.unity giống main; SHA256 `c6566d428bb42535efeb421c376cf984eec5e761be6da945d10ed251bd179416`; save giữ22 và đọc21 |

Các file minh chứng ở `Evidence/Rubric`. Mã build Unity và mã backend được ghi riêng;
Lượt PC đo ở source1734be7. Backend05/10 bổ sung xử lý hội thoại và lọc khớp từ đơn lẻ; thời gian trên PC của bản backend cuối chưa đo lại, CI kiểm riêng.
PC: Intel i3-1115G4 / Intel UHD / khoảng8GB, Windows11; Python3.12.10,
Ollama0.12.3, Qwen3:1.7b Q4_K_M (digest trong Backend/MODEL-MANIFEST.json).
Chat là QA trích xuất: model chọn mục, API trả đoạn hướng dẫn có nguồn; không huấn luyện model.

## Giới hạn chất lượng chatbot

Lượt PC cũ và lượt Docker cuối đều sai câu3 (xô rỗng),26 (thưởng Tìm số),27 (giá Runner). Đã đối chiếu toàn bộ
30 câu với đáp án và source-derived manual; không coi HTTP200 là câu trả lời đúng.
27/30 đạt ngưỡng kế hoạch nhưng chưa tuyệt đối. Review là của Codex, không phải giảng viên.
Câu ngoài phạm vi phải báo thiếu thông tin; hướng dẫn tĩnh vẫn đọc được khi backend tắt.

## Các mục còn chờ

| Kiểm tra | Trạng thái | Phụ thuộc |
|---|---|---|
| Android mọi thao tác, đa chạm, keyboard, background, kéo thả | Pending | Chưa có thiết bị kết nối ADB hoặc máy được chọn |
| Android 15 phút, trung bình ≥30FPS | Pending | Chưa đo trên điện thoại; video5FPS không phải benchmark |
| AR camera/plane/tracking/install/permission/reopen | Pending | Cần điện thoại ARCore thật; desktop chỉ kiểm cấu hình và miniature |
| Adafruit IO hai chiều | Pending | Chủ dự án chưa có tài khoản, sẽ tạo sau; không dùng key thật |
| Docker trên PC / HTTP từ điện thoại | Pending | Cài Docker tự động và lần khởi động dịch vụ native tiếp theo bị auto-review chặn “blocked by policy”; Docker kiểm trên CI |
| Video liền mạch Android → AR → AI → dashboard | Pending | Kịch bản trong DEMO.md; PC video không chứng minh AR/cloud thật |
| Chủ dự án tự chơi và đồng ý merge | Pending | Không đánh dấu thay chủ dự án |

## Những lỗi đã sửa trong quá trình kiểm tra

- Miniature tham chiếu prefab đất thiếu: dùng crops_dirtDoubleRow có sẵn; chuyển bò khỏi chuồng che khuất, kiểm cây50%/ẩm65%.
- Model tự sinh sai luật: dùng model chọn nguyên mục hướng dẫn, giữ điều kiện/ngoại lệ; vẫn báo các câu chọn nhầm.
- Câu tiếp nối mất chủ đề: đưa câu đã xác định chủ đề vào cả retrieval và model;
  giữ chủ đề qua nhiều câu rút gọn, không kéo câu ngoài phạm vi về chủ đề cũ.
- Full smoke bật Creative sớm: chuyển kiểm tra Creative sau các kiểm tra save normal;
  giữ nguyên quy tắc khóa save Creative.
- Android NDK lồng thư mục: chuẩn hóa module; manifest cho HTTP LAN và camera không bắt buộc.
- AR khởi động XR ngay khi chơi: mở theo menu; provider availability/install tồn tại qua đóng màn hình;
  XROrigin CameraYOffset=0 phù hợp camera AR cầm tay.
- Menu/HUD chạm chồng minimap: đặt lại vị trí, áp dụng safe area, đổi gợi ý sang nút chạm.
- Backend khởi động lại không có command: trở về tưới cục bộ, không giữ OFF từ phiên cũ.

Bản build/ZIP cũ được chuyển vào Recovery cục bộ sau khi bản Windows thay thế đã qua
kiểm tra; không đưa cache, log tạm, bí mật hay bản tải trùng vào Git/source ZIP.

CI cuối: https://github.com/NguyenHuuThinhyy/Nongtrai/actions/runs/37264020925; raw response, review và conversation trong Evidence/Rubric/docker-ci.
