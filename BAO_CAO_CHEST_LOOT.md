# Rương mở trực tiếp — ChestLoot — HThinh.yy

- Bỏ toàn bộ bảng câu hỏi, đáp án và xử lý đúng/sai của rương khám phá và rương boss.
- Chuột phải mở bảng đồ: click lấy 1, Shift + click lấy cả chồng, kéo thả hoặc bấm **LẤY TẤT CẢ**. Túi đầy thì đồ còn lại ở nguyên trong rương.
- Giữ chuột trái 0,8 giây để đập rương khám phá. Đồ còn lại và vật phẩm rương rơi ra; có thể đập rương chưa mở. Gọi phá lại không thả trùng đồ.
- Quái canh vẫn phải bị hạ trước khi mở hoặc phá. Rương người chơi đặt và kho vẫn dùng giao diện cất/lấy đồ.
- Ghi nội dung rương sau mỗi lần chuyển đồ và trước khi rời vùng; giữ đúng phần đồ còn lại khi quay lại hoặc lưu/tải. Giữ trường `unlocked` để đọc save cũ, không dùng để yêu cầu câu đố. Không đổi phiên bản save21.
- Lưới hàng hóa được thu gọn để đủ 78 loại vật phẩm, không đè lên nút lấy đồ ở hàng cuối.

## Bàn giao

Runtime: `Builds/Windows-ChestLoot/NongTrai.exe`. Gói Windows/Unity mang tên `ChestLoot-20260927` trong `DongGoi`. Bản sao source trước sửa nằm tại `Recovery/Before-ChestLoot-20260927/UnitySource.zip`.

Không chạy CreateScene, không thêm asset ngoài hoặc sửa quy tắc câu hỏi của sự kiện khác trong game.

## Kết quả kiểm tra

- Build Windows thành công: `Logs/build-chest-loot-final.log`, `FARM_M1_BUILD_OK 100987215` (169 file runtime, khoảng 96,31 MiB).
- Smoke đầy đủ và art check: `Logs/smoke-chest-loot-final.log`, exit **0**. Đã kiểm mở trực tiếp, lấy một/tất cả, đập chưa mở, không thả trùng, quái canh, rương boss lấy một phần rồi lưu/tải và rời vùng/quay lại, hạt đặc biệt và các hệ thống cũ.
- Khởi động thường: `Logs/startup-chest-loot.log`, chạy và phản hồi sau 8 giây, không exception.
- Đã xem ảnh bảng lấy đồ: `Logs/ChestLootScreens/chest-direct-loot-preview.png`, các hàng đồ và nút lấy không chồng nhau.
- SHA256 scene trùng bản sao trước sửa; source, meta, tài liệu đồng bộ vào Git. Gói Windows/Unity có SHA256 trong `DongGoi/RELEASE-MANIFEST.json`.
