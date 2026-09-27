# Bàn giao AdventureFeedback — 27/09/2026

## Bản chơi

- Windows: `Builds/Windows-AdventureFeedback/NongTrai.exe`.
- Unity 6000.3.22f1 / URP; scene `Assets/Farm/Scenes/Farm.unity`.
- Build release: `Logs/build-feedback-release.log`, `FARM_M1_BUILD_OK 100936715`.
- Runtime: **100.936.715 byte** (~96,26 MiB), 169 file; không tính ảnh kiểm tra, log hay Burst debug.
- Bản sao trước sửa: `Recovery/Before-AdventureFeedback-20260927-135229/UnitySource.zip`.
- Scene chính có SHA256 trùng bản sao trước sửa; không chạy CreateScene.

## Các yêu cầu đã xử lý

| Yêu cầu | Thay đổi |
|---|---|
| Đi thường nhanh hơn, chạy nhanh đói hơn | Đi 6 m/s, Shift 9 m/s. Vận tốc thực vượt tốc đi và đang giữ Shift: độ no giảm 0,12 điểm/giây; bình thường 0,03. |
| Quơ tay khi đánh/cuốc/đặt | Xoay tay theo trục nhân vật, vệt dụng cụ bám socket bàn tay, hạt thao tác dùng một emitter chung tối đa 256 hạt. Giữ mũ trên xương đầu, clamp chuyển động nhảy thừa của rig. |
| Chơi lại phải từ đầu | Esc → Chơi lại từ đầu → xác nhận. Cất save cũ thành `.before-new-game-<thời gian>`, tạo nông trại/map mới từ LV1. Hủy không đổi tiến độ. Mở EXE thông thường vẫn tải bản lưu thủ công. |
| Múc/đổ bình nước | Item71: bình rỗng, công thức 2 đá + 1 kim loại. Một bấm phải vào nước đổi đúng 1 bình rỗng thành bình đầy64. Đổ nguồn nước hoặc tiếp bình tưới trả lại vỏ rỗng. Bình tưới cũng múc trực tiếp được. |
| Đào bờ mà nước không lan | Nước tự nhiên đổ vào ô đào trống bằng cùng thuật toán lan của bình. Chảy xuống và lan tối đa 7 ô; tối đa 64 nguồn người chơi. Nước có truy vấn ray riêng, không thêm collider rắn lên mặt nước. |
| Không lên được bờ | Bơi nhận xung nhảy theo jumpHeight để vượt bờ một khối. Với vách cao hơn tầm nhảy, đào bậc để lên. |
| Đặt sát/ngay dưới chân, đào thẳng đứng | Pitch đến 90°, bỏ lệch vai khi ngắm xuống; tâm ngắm trùng ray. Bỏ bán kính cấm 1,25 m, giữ kiểm tra collider thật. Nhảy rồi đặt khối dưới chân; đào thẳng xuống, vẫn chặn tầng đáy và khu cổng bảo vệ. |
| Hồi chiêu chém | Mặc định 0,3 giây; bấm/giữ trái được. Chỉ số tốc đánh rút ngắn tối đa 20%. |
| Chỉ số rèn ngẫu nhiên | Mỗi chiếc vũ khí lưu riêng LV, sát thương, chí mạng, tốc đánh. Đổi chỉ số tốn 1 đá + 80 xu: +1–12 ST, 3–18% crit, 0–20% tốc. Crit gây 150% ST; cung hưởng tốc kéo/crit. |
| Kiếm LV5 có hiệu ứng | Ngoài ST theo LV, thêm 12 ST và hút 3 HP mỗi đòn cận chiến trúng; hạt màu hồng. Chỉ số giữ qua save/load, kéo đổi ô và rơi đồ khi chết. |
| Sông/hồ ngẫu nhiên | Generator5 sinh thêm hồ và nhánh sông theo seed ở hoang dã, giữ các địa danh. Save cũ giữ generator cũ để không đổi địa hình đã chơi; Chơi lại từ đầu tạo map mới. |
| Ít rương và có quái canh | Tần suất 1/24 chunk thay 1/9, tối đa 2 rương gần người chơi. Sói160HP/10ST, rắn220HP/12ST, gấu420HP/24ST, golem1400HP/40ST. Quái có báo đòn, giới hạn lãnh thổ và kiểm tra vật cản. Phần thưởng tăng theo bậc. |
| Rương đố vui | Hạ quái trước mới được giải đố; đúng rơi đồ, sai rương mất. Trạng thái hạ quái được lưu; quái bị hủy cùng rương khi unload, tránh attacker mồ côi. |
| Bán/bỏ kiếm và rìu | Chọn ô trong túi rồi Bán số lượng hoặc Bỏ vật phẩm đã chọn. Hỗ trợ cả cung; không chuyển nâng cấp sang vũ khí khác. |
| Hạt nước | Múc, đổ, bơi và tưới cây có hạt nước dùng vật liệu URP sẵn có. |
| Không thấy quái vẫn bị cắn | Sói/cáo/rắn/boss/quái canh kiểm độ cao và ray vật cản trước gây sát thương; chặn cắn xuyên vách/từ tầng dưới. |
| Quy tắc cổng đã đính chính | Đổi map nhớ vị trí; chỉ chết hồi sinh mới về cổng tự đặt, hoặc cổng gốc nếu không có. |

## Kiểm tra

**Bản release cuối đã kiểm tra thành công:**

- `Logs/smoke-feedback-release.log`: toàn bộ smoke + art check, **exit 0**; không Exception/Error.
- Các checkpoint `FARM_FEEDBACK_*_OK`, `FARM_ADVENTURE_FEEDBACK_OK`, `FARM_PORTAL_OK`, `FARM_RUNNER_OK`, `FARM_NEW_FEATURES_OK`, `FARM_CROPS_SMOKE_OK` đều có trong log.
- `Logs/startup-feedback-release.log`: mở EXE không bật chế độ test, sống và phản hồi sau 8 giây; không Exception/Error. Chỉ đóng tiến trình kiểm tra do phiên này tạo.
- Đã xem ảnh quơ tay/vệt dụng cụ sau lần chỉnh trục cuối, tay cầm kiếm, bảng rèn LV5, mặt nước và quái canh.
- Đây là kiểm tra trên máy hiện tại; chưa có số đo FPS chuẩn hóa cho các cấu hình khác.

Các kiểm tra thêm nằm trong `FarmAdventureFeedbackChecks.cs`: tốc độ/độ no bằng input thật; ngắm/đào thẳng đứng; đặt dưới chân và chặn chồng người; bảo toàn số bình khi múc/đổ; nước lan qua bờ đào; nhảy khỏi nước; hồi chiêu; góc tay; vật cản/độ cao quái; reroll, LV5, save/load, vũ khí riêng biệt và metadata đồ rơi; bán/bỏ; khóa rương bởi guardian; giải đố sau khi hạ quái; archive save. Test dùng file ở temporaryCachePath, không sửa save người dùng.

Ảnh đã kiểm: tay cầm kiếm, động tác đánh, giao diện rèn, sông/nước lan, bốn quái canh. Ảnh kiểm ở `Builds/Windows-AdventureFeedback`, không đưa vào gói runtime.

## Model và giấy phép

Không tải thêm asset. Giữ Kenney Mini Characters/Nature Kit, Quaternius Cow/Pig/Sheep, Chicken CDmir/TinyWorlds, đều có giấy phép CC0 trong source. URL/tác giả/định dạng/kích thước tải đã ghi ở `Assets/Farm/Models/ASSET_SOURCES.md`; bản Windows kèm giấy phép trong `Licenses`.

Quái canh sói/rắn/gấu/golem dùng primitive tự dựng, chưa phải model nhập mới. Nhà/chuồng/máy móc/dụng cụ và một số cây trồng vẫn dùng visual Unity; NPC chưa có. Voxel được giữ để đào/đặt/phá hoạt động đúng.

## Source và gói

Save19, ItemCount72, 27 công thức ghép, generator5 cho map mới. Git mirror giữ working changes, không commit/push tự động. Không đưa Library, Temp, Logs, Builds, Recovery hoặc gói tải trùng vào source ZIP. Manifest trong `DongGoi/RELEASE-MANIFEST.json` ghi kích thước và SHA256 hai gói.
