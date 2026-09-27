# Bình tưới múc và đặt nước — 27/09/2026

## Cách dùng

1. Chọn **ô 6 — Bình tưới có sẵn**. Không phải chế tạo bình khác.
2. Đến hồ nông trại hoặc sông/hồ/biển Khám phá, ngắm **mặt nước** trong tầm 6 m và bấm **chuột phải** một lần để múc đầy.
3. Ngắm đất trống hoặc khối xây trong tầm 6 m, bấm **chuột phải** để đổ toàn bộ nước hiện có. Nước rơi xuống rồi lan ngang tối đa 7 ô. Bấm phải vào nguồn đã đặt để múc lại; dòng nước từ nguồn đó rút đi.
4. **Chuột trái vẫn tưới cây**, dùng 1 nước. Đặt thất bại không mất nước. Nếu đang mang bình dự phòng, bình tiếp theo tự nạp như trước.

Đất nông trại có thể nhận nguồn nước; ô trồng cây, máy móc và động vật không phải vị trí đặt nguồn. Khám phá dùng địa hình voxel và khối xây hiện có. Nước tự nhiên múc không cạn. Giới hạn 64 nguồn mỗi map; mô phỏng ô có giới hạn, không mô phỏng chất lỏng vật lý đầy đủ.

## Lỗi đã sửa

- Bình tưới105 trước đây chỉ múc tại Khám phá, chưa đổ được. Nay dùng bình mặc định cho toàn bộ vòng múc/đổ.
- Hồ nông trại không có collider nước nên tia ray cũ chỉ chạm đáy. Nay nhận vùng nước theo bounds của mặt hồ/đáy, vẫn tôn trọng vật cản phía trước.
- Nguồn đổ nông trại có mesh chung, không tạo collider chặn người; nguồn Khám phá dùng lưới cũ.
- Lượng nước trong bình và nguồn ở hai map lưu/tải được. Thêm farmSources tùy chọn vào save20, tương thích save cũ; xử lý cả auto-load trước Start.
- Hướng dẫn HUD/tutorial ghi chuột phải múc/đổ, chuột trái tưới. Giữ bình dự trữ64/bình rỗng71 cho save/công thức cũ.

## Kiểm tra

- Unity6000.3.22f1 URP, build scene hiện tại; không gọi CreateScene. Hash scene không thay đổi.
- `Logs/build-water-can-release.log`: FARM_M1_BUILD_OK **100958875**.
- `Logs/smoke-water-can-release.log`: **exit 0**, FARM_WATER_CAN_OK cùng đầy đủ smoke/art cũ, gồm TNT.
- Kiểm input chuột phải thật để múc hồ và đổ đất bằng ô6; không có bình64/71 trong túi; không tiêu thụ nước máy bơm; ngắm sai/vật cản; nước lan; thu lại nguồn; save/load cả2map; chuột trái vẫn tưới cây.
- `Logs/startup-water-can-release.log`: khởi động thường, sống/phản hồi sau8giây, không Exception/Error.
- Xem ảnh `Builds/Windows-WaterCan/water-can-flow-preview.png`: nguồn nước đã đổ lan trên mặt đất, chảy xuống bậc địa hình.

## Bàn giao

Runtime `Builds/Windows-WaterCan/NongTrai.exe`: **100.958.875 byte**, 169 file chạy game (~96,28 MiB). Gói Windows/Unity WaterCan-20260927 và SHA256/kích thước chính xác trong `DongGoi/RELEASE-MANIFEST.json`. Gói Windows giữ đủ DLL/Data/MonoBleedingEdge/D3D12, hướng dẫn và giấy phép; không chứa ảnh kiểm thử hay BurstDebugInformation.

Nguồn live: D:/GAME_NongTrai; mirror Git: D:/GAME_NongTrai/Nongtrai. Backup nguồn và mirror trong `Recovery/Before-WaterCan-20260927-144955`; bộ TNT cũ chuyển sang OldRelease. Không thêm asset ngoài, giữ nguồn/giấy phép CC0 hiện có. Không thay model trong bản sửa nước này. Gói Unity không gồm Library/Temp/Logs/Builds/Recovery/Git. Giữ nguyên các thay đổi Git đang làm, không tự commit/push.
