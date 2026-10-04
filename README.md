# Nông Trại — First Harvest

© HThinh.yy. Unity **6000.3.22f1**, URP, save **22**.

## Bản thử Android / AR / AI / Cloud

Nhánh **codex/rubric-mobile-ar-ai-cloud**, phát triển từ `bf7d0b3`.
**Chưa gộp main. Chỉ gộp sau khi chủ dự án tự test và xác nhận.**

- [Tải bản thử Windows, APK và source Unity](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/tag/rubric-preview-20261004).
- Trong checkout nhánh này, chạy **CHAY_GAME.bat** hoặc `Builds/Windows-Rubric/NongTrai.exe`; giữ nguyên cả thư mục build.
- [Hướng dẫn cho nhóm](HUONG_DAN_NHOM.md), [cách chơi](CHOI_GAME.md), [nhà hàng](HUONG_DAN_NHA_HANG.md).
- [Thiết lập Android/AR/chat/cloud/Docker](Docs/RUBRIC_INTEGRATION.md), [kết quả test](Docs/TEST_RESULTS.md), [checklist người dùng](Docs/ACCEPTANCE.md), [kịch bản demo](Docs/DEMO.md).
- Commit nguồn và SHA256 của các gói ở [DongGoi](DongGoi). ZIP Unity không chứa Library/cache; Unity tự tạo lại khi mở.

## Chức năng

Nông trại 3D, khám phá voxel theo seed, vật nuôi, cây trồng, máy chế biến,
chế tạo/rèn, nước/xô/bọt biển, cung theo quỹ đạo và độ bền, boss, hồi sinh,
nhà hàng ba tầng. Minigame Tìm số2D và Farm Runner có thưởng/quota trong save.
Đổi map nhớ vị trí; chết có lựa chọn trả100xu tại chỗ hoặc miễn phí về cổng.

Bản thử bổ sung joystick/vuốt/các nút cảm ứng, giao diện túi tách/chuyển nhanh,
nông trại AR thu nhỏ theo trạng thái ruộng, trợ lý tiếng Việt, trạm tưới điều khiển
qua Adafruit IO và backend Docker. Hướng dẫn tĩnh/gameplay vẫn hoạt động offline.
Cloud ghi rõ dữ liệu mô phỏng; chưa có cảm biến vật lý. Chat dùng model pretrained,
không huấn luyện lại. Các phần điện thoại/AR/cloud thật còn phải nghiệm thu trên thiết bị/tài khoản.

## Mở và build

Clone đúng nhánh để có các phần công nghệ mới:

```powershell
git clone --branch codex/rubric-mobile-ar-ai-cloud https://github.com/NguyenHuuThinhyy/Nongtrai.git
```

Unity Hub → Add project from disk → thư mục checkout. Mở
`Assets/Farm/Scenes/Farm.unity`. Android cần module Android Build Support/SDK/NDK/OpenJDK.
Không gọi FarmProjectBuilder.CreateScene/RebuildScene: có thể ghi đè scene/prefab.
Hai lệnh dưới build scene đang lưu:

```powershell
.\Tools\Build-Windows.ps1 -OutputDirectory Builds/Windows-Rubric
.\Tools\Build-Android.ps1
.\Tools\Start-Services.ps1 -DownloadModel
```

Bản APK ARM64 cài trực tiếp; chưa ký cho Google Play. ARCore Optional:
điện thoại không hỗ trợ AR vẫn chơi game thường. Chat Android cần PC cùng Wi-Fi;
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
Planning, tests, documentation đã chuẩn bị; ảnh/log chọn lọc ở Evidence/Rubric.
Video camera AR thật, dashboard thật, trải nghiệm Android15phút và người dùng tự test
phải bổ sung trước nghiệm thu. Kịch bản demo không thay thế video.
Điểm còn phụ thuộc giảng viên có chấp nhận pretrained model/IoT mô phỏng/uGUI2D hay không.
