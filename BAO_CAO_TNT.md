# TNT: đặt và châm bằng đuốc — 27/09/2026

## Cách dùng

1. Cầm TNT trên hotbar, bấm trái hoặc phải vào mặt địa hình/khối xây trong tầm 6 m ở map Khám phá.
2. TNT đã đặt nằm yên, không tự nổ. Chọn đuốc và bấm trái/phải vào TNT để châm. Đuốc không bị tiêu hao và không bị đặt nhầm lên TNT.
3. TNT nhấp nháy đủ **5 lần**, mỗi lần sáng 0,5 giây rồi tắt 0,5 giây, sau đó nổ. Có nhãn đếm 1/5–5/5, đổi trắng/đỏ, ánh sáng và tia lửa. Khi khựng hình, các pha vẫn hiện đủ nên thời gian có thể dài hơn 5 giây.

Tạm dừng hoặc về nông trại sẽ tạm dừng ngòi. Lưu/tải giữ cả TNT chưa châm lẫn nhịp ngòi đang cháy. Châm lặp không khởi động lại bộ đếm. Đặt lỗi báo lý do và không trừ vật phẩm. Giữ sát thương người chơi 25 HP trong phạm vi 4 m, phạm vi đào và luật bảo vệ tầng đáy/cổng như cũ.

## Thay đổi kỹ thuật

- Tách `FarmTnt.cs` khỏi `ExplorationLandmarks.cs`; chưa châm → đã châm → năm pha sáng/tắt → nổ một lần.
- `PlayerInteraction` xử lý TNT ở cả hai nút chuột trước thao tác khác; `FarmBuildingSystem` nhường thao tác châm cho TNT khi cầm đuốc; mining bỏ qua vật phẩm TNT.
- Vị trí đặt có collider và kiểm tra chồng lấn; chỉ trừ kho sau khi vị trí hợp lệ.
- Save20, đọc v2–19, thêm `TntRecord[]` chứa vị trí/ngòi/pha/thời gian còn lại. World reset xóa đối tượng TNT cũ trước khôi phục, tránh nhân bản.
- Không sửa scene, prefab, model nhập hay luật đổi map/hồi sinh. SHA256 scene trùng backup.

## Build và kiểm tra

- Runtime: `Builds/Windows-TNT/NongTrai.exe`, **100.947.275 byte** (~96,27 MiB), Unity6000.3.22f1 URP.
- Build: `Logs/build-tnt-release.log` — FARM_M1_BUILD_OK100947275.
- Test riêng bản đầu: `Logs/smoke-tnt-1.log` có FARM_TNT_OK.
- Bộ `FarmTntChecks` kiểm đặt/chưa châm, đuốc đúng, không tiêu hao đuốc, đủ năm nhịp, khựng hình, pause, save/load trước/sau châm, hố nổ, đặt lỗi không mất đồ và input chuột trái thật khi đang cầm đuốc.
- Full smoke + art: `Logs/smoke-tnt-release.log`, **exit 0**, FARM_TNT_OK và toàn bộ checkpoint cũ; không Exception/Error.
- Startup thường: `Logs/startup-tnt-release.log`, sống và phản hồi sau 8 giây; không lỗi.
- Đã xem ảnh `Builds/Windows-TNT/tnt-lit-preview.png`: nhãn nhịp, pha sáng và tia lửa hiển thị.

## Bàn giao

Backup: `Recovery/Before-TNT-20260927-142343/UnitySource.zip`. Gói Windows/Unity mới mang tên TNT-20260927 trong DongGoi; manifest ghi SHA256/kích thước. Không tải thêm asset; giấy phép CC0 của các model hiện có được giữ. Gói source không chứa Library/Temp/Logs/Builds/Recovery. Git mirror giữ working changes, không tự commit/push.
