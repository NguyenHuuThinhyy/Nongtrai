# Nông Trại — First Harvest

© HThinh.yy. Unity **6000.3.22f1**, URP, save **22**.

## Bản mới: AI offline trên Windows / Android

Chủ dự án đã cho phép đẩy và gộp bản hiện tại vào **main** ngày 08/10/2026.

- [Tải Windows ZIP có EXE, APK Android và source Unity kèm model](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/tag/localai-20261008).
- Mở `CHAY_GAME.bat` hoặc EXE; nhấn **C** / Chat mới nạp AI. Đóng/ẩn chat hoặc chuyển ứng dụng sẽ hủy xử lý và giải phóng model. Không cần backend, PC khác hay Internet để chat.
- APK khoảng **1,16 GB**, gồm Qwen3 1.7B do chủ dự án cung cấp. Lần mở chat đầu trên Android chuẩn bị model từ APK; cần thêm khoảng 1,2 GB trống. Khuyến nghị ARM64, 6 GB RAM.
- **J** mở mô hình nông trại local, **H** ẩn/hiện hướng dẫn. Camera tùy chọn.
- [AI offline](Docs/LOCAL_CHAT.md), [hướng dẫn cho nhóm](HUONG_DAN_NHOM.md), [cách chơi](CHOI_GAME.md), [nhà hàng](HUONG_DAN_NHA_HANG.md).
- Source Git giữ Assets/.meta, Packages, ProjectSettings, Backend, Tools, Docs và Evidence. Gói chơi/model ở Release; `DongGoi/RELEASE-MANIFEST.json` và `SHA256SUMS.txt` ghi commit và checksum.
- Build Windows/Android thành công; việc duyệt gộp của chủ dự án không thay thế kiểm tra runtime. Chưa đo CPU/RAM hay chạy chatbot trên điện thoại thật.

Bản đóng gói ngày 08/10/2026 được build từ commit `d82e56a`, gồm cập nhật nhà/nông trại, sân và lối đi nhà hàng mới nhất. Các file lớn được tải tại Release liên kết ở trên; tải đủ ZIP Windows để có EXE và dữ liệu đi kèm.

## Chức năng

Nông trại 3D, khám phá voxel theo seed, vật nuôi, cây trồng, máy chế biến,
chế tạo/rèn, nước/xô/bọt biển, cung theo quỹ đạo và độ bền, boss, hồi sinh,
nhà hàng ba tầng. Minigame Tìm số2D và Farm Runner có thưởng/quota trong save.
Đổi map nhớ vị trí; chết có lựa chọn trả100xu tại chỗ hoặc miễn phí về cổng.

Bản thử bổ sung joystick/vuốt/các nút cảm ứng, giao diện túi tách/chuyển nhanh,
nông trại AR thu nhỏ theo trạng thái ruộng, trợ lý tiếng Việt, trạm tưới điều khiển
qua Adafruit IO (backend tùy chọn). Chat mặc định chạy trực tiếp trong game. Hướng dẫn tĩnh/gameplay vẫn hoạt động offline.
Cloud ghi rõ dữ liệu mô phỏng; chưa có cảm biến vật lý. Chat dùng model pretrained,
không huấn luyện lại. Các phần điện thoại/AR/cloud thật còn phải nghiệm thu trên thiết bị/tài khoản.

## Mở và build

Trên máy chủ dự án, chỉ dùng **D:\GAME_NongTrai**: mở thư mục này trong Unity Hub
hoặc chạy `CHAY_GAME.bat`. Source cũ được cất riêng tại `D:\NongTrai_LuuTru`;
không chép đè vào dự án đang dùng. Manifest trong `DongGoi` ghi commit nguồn/build của từng gói hiện tại.

Clone main để có bản đã được chủ dự án duyệt:

```powershell
git clone --branch main https://github.com/NguyenHuuThinhyy/Nongtrai.git
```

GGUF không nằm trong Git. Tải source Unity ZIP ở Release để có model và license; nếu dùng checkout Git, lấy thư mục `Backend/models` từ ZIP vào checkout.

Unity Hub → Add project from disk → thư mục checkout. Mở
`Assets/Farm/Scenes/Farm.unity`. Android cần module Android Build Support/SDK/NDK/OpenJDK.
Không gọi FarmProjectBuilder.CreateScene/RebuildScene: có thể ghi đè scene/prefab.
Hai lệnh dưới build scene đang lưu:

```powershell
.\Tools\Build-Windows.ps1
.\Tools\Build-Android.ps1
```

Bản APK ARM64 cài trực tiếp; chưa ký cho Google Play. ARCore Optional:
điện thoại không hỗ trợ AR vẫn chơi game thường. Chat mặc định offline trên Android; chỉ lựa chọn AI qua PC cần Wi-Fi.
Adafruit IO cần Internet và tài khoản tạo sau. Không đưa Backend/.env hoặc API key vào Git.

## Dữ liệu và giấy phép

JSON chuẩn ở `Assets/StreamingAssets`: công thức máy, chế tạo và nhà hàng.
Android dùng bản Resources được Configure đồng bộ trước build. Lưu thủ công trong menu;
khởi động đọc save cũ, thoát không tự lưu. Chơi lại từ đầu có xác nhận và cất save dự phòng.
Creative khóa lưu và bỏ phiên thay đổi khi quay về normal. Cấu hình/chat lưu riêng.

Asset/giấy phép giữ tại [nguồn model gốc](Assets/Farm/Models/ASSET_SOURCES.md)
và Assets/ThirdParty. Model Qwen3 1.7B Q4_K_M Apache2.0, tải riêng khoảng1,36GB,
[nguồn Qwen](https://huggingface.co/Qwen/Qwen3-1.7B),
[phân phối Ollama](https://ollama.com/library/qwen3:1.7b),
[manifest](Backend/MODEL-MANIFEST.json), [giấy phép](Backend/licenses/Qwen3-Apache-2.0.txt).
Voxel/collider/gameplay được giữ để đào/đặt/phá hoạt động đúng.

## Rubric và bàn giao

Xem [bảng tiêu chí → chức năng → kiểm thử → minh chứng](Docs/ACCEPTANCE.md).
Planning, tests, documentation và [video demo tự động trên PC](Evidence/Rubric/demo-pc-automated.mp4)
đã có; ảnh/log chọn lọc ở Evidence/Rubric. Chatbot đạt 30/30 câu trong lượt kiểm tra
CI lịch sử ngày05/10 (warm5,35giây), trước khi bỏ container và đổi hướng dẫn local ngày06/10.
Lượt PC8GB trước đó ở bản cũ đạt27/30, warm15,49 giây; bản backend cuối chưa đo lại trên PC.
Camera thật, dashboard thật, trải nghiệm Android15phút và người dùng tự test
phải bổ sung trước nghiệm thu. Kịch bản demo không thay thế video.
Điểm còn phụ thuộc giảng viên có chấp nhận pretrained model/IoT mô phỏng/uGUI2D hay không.
