# Kết quả kiểm tra nhánh rubric

© TriForge. Cập nhật 06/10/2026. Đây là kết quả kỹ thuật; chủ dự án chưa nghiệm thu.
Nhánh `codex/rubric-mobile-ar-ai-cloud`; không gộp main trước xác nhận của chủ dự án.

## Đợt local ngày06/10/2026

- Phạm vi mới: bỏ Dockerfile/Compose/CI container, miniature local mặc định PC/Android.
- Source `1a8a786`: 8 backend tests + 7 launcher tests PASS, gồm ghép cặp/bảo toàn save,
  không nạp model lúc mở game, chỉ dọn tiến trình do launcher tạo. Tiến trình/model trong launcher tests là mock.
- Windows PowerShell5 và PowerShell7: Start-Assistant -PrepareOnly PASS trên runtime sẵn có; không khởi chạy dịch vụ.
- CI Windows native: https://github.com/NguyenHuuThinhyy/Nongtrai/actions/runs/37471133514 — PASS15tests; không chạy container.
- Backend local thực tế chưa được khởi chạy lại: auto-review từng từ chối native startup “blocked by policy”.
  Chủ dự án cần chạy CHAY_GAME.bat để kiểm kết nối thật; không suy luận từ mock/prepare-only.
- Windows1a8a786: build PASS153.035.026bytes; Technology PC (chuột khóa) và touch PASS cả nhập E/X, C/H, phím J mở/đóng miniature local và phục hồi camera/vị trí. Log mới: windows-build-local-20261006.txt và windows-technology-local-20261006.txt; ảnh06-local-miniature.png đã kiểm trực quan.
- Android1a8a786: build PASS52.593.854bytes (~50,16MiB), chữ kýv2 hợp lệ, ARM64/API26–36. Log/manifest/signature có hậu tố local-20261006; chưa nghiệm thu thiết bị thật.
- Điện thoại/camera thật, cloud/account, tốc độ chơi và người dùng tự test: vẫn pending.
- Mô hình local không chứng minh AR plane tracking; Docker đã được bỏ theo yêu cầu, ngoài phạm vi nghiệm thu hiện tại.

## Kết quả lịch sử trước khi chuyển hoàn toàn sang local

| Kiểm tra | Kết quả | Minh chứng và giới hạn |
|---|---|---|
| Đồng bộ Git / nhà hàng | PASS | Fetch 05/10: restaurant-renovation `7633b40` đã có trong nhánh; remote chưa có cập nhật nhà hàng mới hơn |
| Unity Windows build | PASS | `windows-build.txt`: Succeeded, 0 errors; mã Unity `896faef`, có AR PC/C/H và sửa sàn |
| Full smoke + art | PASS | `windows-full.txt`: cây, quái/boss, rig, camera, cung/quỹ đạo/độ bền, nước/xô/bọt biển, rương, rèn, hồi sinh, portal, save22, Runner, thưởng 2D |
| Nhà hàng với bố cục chạm | PASS | `windows-restaurant-touch.txt`: các slab/chiếu nghỉ không chồng mặt, nền nhìn thấp hơn sàn và collider vẫn y=0; 30 công thức, save22/legacy21, phục vụ, bố trí, cầu thang, khách, câu cá |
| Tích hợp / input chạm / AR PC | PASS trên Windows | `windows-technology-touch.txt`: C mở chat; H ẩn/hiện hướng dẫn; AR PC mở lại, đặt/xoay/phóng, chat/quay về AR rồi game giữ vị trí/camera; chưa mở webcam thật |
| Tắt âm khi sửa/kiểm tra | PASS | Cả ba log 05/10 có `FARM_TEST_AUDIO_MUTED`; AudioListener volume=0 và pause=true trước khi nạp scene |
| Unity → HTTP → Qwen thật | PASS ngày 04/10 | `windows-services-native-1734.txt`, ảnh/chat/video: source `1734be7`, backend native; không phải Docker |
| Backend unit + BM25 | PASS | 8 tests; 30 câu có mục đúng trong top3, dữ liệu hướng dẫn Unity/backend giống nhau; model contract dùng mock |
| Model thật trên PC 8 GB | 27/30 đúng | `chat-model-pc-isolated.json` và review từng câu: Unity/game đã đóng, warm trung bình **15,494 s**, 0 lượt warm >30 s, lượt đầu 22,288 s |
| Docker / model thật / hội thoại | PASS trên CI, 30/30 nội dung đúng | Run37272084372 source a7dcc3b: checkout sạch, hai container healthy, HTTP thật, warm **5,353 s**, 0 lượt warm >30 s; hai câu hỏi tiếp nối, hai câu ngoài phạm vi, hai câu mới AR PC/webcam và C/H qua kiểm tra. Runner Ubuntu16GB, không thay thử PC/điện thoại |
| APK Android ARM64 IL2CPP | PASS build/signature/manifest | 52.592.078 bytes (~50,16 MiB), API26→36, GLES3, ARCore Optional, HTTP LAN; chữ ký v2 hợp lệ. Chưa cài/chơi trên điện thoại |
| Clone sạch / đủ runtime / khởi động bản trước | PASS ở d8f91b7 | Minh chứng lịch sử trước lần sửa AR PC; checkout này đã hợp nhất về D:/GAME_NongTrai, bản mới build/test tại đây |
| Video PC tự động | PASS | MP4 1280×720/5FPS/29,4s: gameplay → thưởng 2D → HTTP/model → preview miniature. Preview cây dùng trạng thái mẫu, không ghi save |
| Scene và save | PASS | Farm.unity giống main; SHA256 `c6566d428bb42535efeb421c376cf984eec5e761be6da945d10ed251bd179416`; save giữ22 và đọc21 |

Các file minh chứng ở `Evidence/Rubric`. Mã build Unity và mã backend được ghi riêng;
Lượt PC đo ở source1734be7. Backend05/10 bổ sung xử lý hội thoại, lọc khớp từ đơn lẻ và mục có từ khóa riêng; thời gian trên PC của bản backend cuối chưa đo lại, CI kiểm riêng.
PC: Intel i3-1115G4 / Intel UHD / khoảng8GB, Windows11; Python3.12.10,
Ollama0.12.3, Qwen3:1.7b Q4_K_M (digest trong Backend/MODEL-MANIFEST.json).
Chat là QA trích xuất: model chọn mục, API trả đoạn hướng dẫn có nguồn; không huấn luyện model.

## Giới hạn chất lượng chatbot

Lượt PC cũ sai câu3 (xô rỗng),26 (thưởng Tìm số),27 (giá Runner). Lượt Docker mới đạt 30/30 sau khi
thu hẹp kết quả theo từ khóa riêng trong cả metadata và nội dung; từ chung như PC/phím không chọn chủ đề.
Đã đối chiếu toàn bộ 30 đáp án/đoạn nguồn; không coi HTTP200 là câu trả lời đúng.
30/30 chỉ áp dụng bộ câu hỏi này. Review là của Codex, không phải giảng viên.
Câu ngoài phạm vi phải báo thiếu thông tin; hướng dẫn tĩnh vẫn đọc được khi backend tắt.

## Các mục còn chờ

| Kiểm tra | Trạng thái | Phụ thuộc |
|---|---|---|
| Android mọi thao tác, đa chạm, keyboard, background, kéo thả | Pending | Chưa có thiết bị kết nối ADB hoặc máy được chọn |
| Android 15 phút, trung bình ≥30FPS | Pending | Chưa đo trên điện thoại; video5FPS không phải benchmark |
| AR camera/plane/tracking/install/permission/reopen | Pending | Cần điện thoại ARCore thật; desktop chỉ kiểm cấu hình và miniature |
| Webcam PC thật, quyền camera/bận camera/mở tắt | Pending | PC view đã chạy; kiểm tự động không kích hoạt webcam |
| Adafruit IO hai chiều | Pending | Chủ dự án chưa có tài khoản, sẽ tạo sau; không dùng key thật |
| Docker trên PC / HTTP từ điện thoại | Pending | Cài Docker tự động và lần khởi động dịch vụ native tiếp theo bị auto-review chặn “blocked by policy”; Docker kiểm trên CI |
| Video liền mạch Android → AR → AI → dashboard | Pending | Kịch bản trong DEMO.md; PC video không chứng minh AR/cloud thật |
| Chủ dự án tự chơi và đồng ý merge | Pending | Không đánh dấu thay chủ dự án |

## Những lỗi đã sửa trong quá trình kiểm tra

- AR PC trước đây chỉ báo cần Android: nay có miniature/đặt thủ công/xoay/phóng và nút bật/tắt nền webcam; chưa có dò mặt phẳng tự động trên PC.
- C mở chatbot trong gameplay/AR; H được ghi trong bảng hướng dẫn, ẩn/hiện qua kiểm tra runtime.
- Sàn trệt trùng mặt nền, chiếu nghỉ tầng trên chồng slab: tách mặt hiển thị và phân vùng slab; giữ độ cao tầng/collider nền, đi cầu thang qua kiểm tra.
- Smoke test đọc lỗi của log lượt trước và so vị trí sau khi vật lý đã chạy tiếp: dùng log riêng, đo trạng thái ngay lúc mở/đóng UI.
- Model chọn cloud cho webcam: thu hẹp mục khi có từ khóa chỉ xuất hiện ở một mục trong metadata và toàn văn; kiểm cả 30 câu cũ và hai câu mới bằng model thật.
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

Bản build/ZIP cũ được chuyển ra D:/NongTrai_LuuTru sau khi bản Windows thay thế đã qua
kiểm tra; không đưa cache, log tạm, bí mật hay bản tải trùng vào Git/source ZIP.

CI cuối: https://github.com/NguyenHuuThinhyy/Nongtrai/actions/runs/37272084372; raw response, review và conversation trong Evidence/Rubric/docker-ci.
Ảnh `05-ar-pc.png` và `restaurant-floor-0/1/2.png` là ảnh runtime bản sửa, đã kiểm trực quan.
