# CombatComfort — 27/09/2026 — HThinh.yy

## Đã thay đổi

- Trả **100 xu** hồi sinh đầy máu ngay tại vị trí ngã, giữ túi đồ và xóa vận tốc rơi/đẩy còn lại. Thiếu xu khóa lựa chọn; bấm lại không bị trừ thêm. Hồi sinh miễn phí vẫn về cổng và rơi tối đa 3 món. Đổi map vẫn nhớ vị trí như trước.
- Bọt biển đầy đặt cách đống lửa tối đa **2 m**, không có tường ngăn, được hong khô sau **10 giây**. Có thể cầm bọt biển đầy và chuột phải vào lửa để hong từng chiếc; dùng luồng xử lý và lưu tiến độ sẵn có của đống lửa. Bọt biển khô thu lại, đặt xuống hút nước tiếp. Khối đặt cạnh lửa bắt đầu lại thời gian hong sau khi rời nguồn nhiệt hoặc tải game; trạng thái khô/đầy vẫn được lưu.
- Kiếm thay dấu `+` bằng vòng tròn có viền sáng/tối và giữa trong suốt, đường kính 11% chiều cao khung nhìn. Vàng khi có mục tiêu. Hỗ trợ đánh quái hơi lệch tâm trong vòng; giữ tầm đánh 6 m (Golem 7 m), hồi chiêu, sát thương và độ bền. Kiểm tra đường nhìn từ camera và người chơi để tránh chém xuyên tường. Mỗi đòn chọn một mục tiêu.
- Không thêm asset bên ngoài; không đổi scene, luật đổi map hoặc định dạng save21. Mã mới ghi © HThinh.yy; giữ giấy phép model hiện có.

## Kiểm tra

- Build Unity 6000.3.22f1, scene hiện tại, không chạy CreateScene: `Logs/build-combat-comfort-package.log`, `FARM_M1_BUILD_OK 100987695`.
- Smoke đầy đủ và art check: `Logs/smoke-combat-comfort-package.log`, exit **0**. Có kiểm hồi sinh ở cả hai map, thiếu xu, trừ đúng tiền/không trừ hai lần; hong khô, lưu/tải trạng thái và thu lại; đánh lệch tâm, chặn tường, giới hạn tầm, hồi chiêu, đổi tâm khi đổi dụng cụ. Các kiểm tra cũ về nước, TNT, cổng, Runner, rương, chế tạo và save đều qua.
- Startup: `Logs/startup-combat-comfort.log`, còn chạy và phản hồi sau 8 giây, không exception.
- Đã xem ảnh `Logs/CombatComfortScreens/forge-combat-sword-circle-preview.png`: vòng ngắm vàng, tâm trong suốt và thanh HP hiển thị.
- SHA256 scene trùng bản sao trước sửa. Không sửa file save của người chơi để test.

## Bàn giao và dọn file

- Runtime: `Builds/Windows-CombatComfort/NongTrai.exe`, **169 file / 100.987.695 byte** (~96,31 MiB). Giữ toàn bộ thư mục runtime khi sao chép.
- Windows ZIP và Unity ZIP mang tên `CombatComfort-20260927` trong `DongGoi`; SHA256 và commit được ghi trong `RELEASE-MANIFEST.json`.
- Đã xóa cache Library, 251 log cũ, 18 thư mục log/debug/ảnh cũ, 24 ZIP Windows cũ và 6 runtime cũ trong Recovery: khoảng **3,70 GiB**, tính cả runtime Systems cũ đã thay bằng ZIP dự phòng. Lần mở Unity tiếp theo cần tạo lại Library.
- Giữ Assets/meta/license, Packages, ProjectSettings, tài liệu và bản sao source. Không đưa cache, build, log hay ZIP lên Git. Bản Systems được giữ dưới dạng ZIP dự phòng; ảnh kiểm tra mới được gom vào Logs.
- Chi tiết các file đã dọn nằm trong `Recovery/Before-CombatComfort-20260927/`; source dự phòng ở `UnitySource.zip` cùng thư mục.
