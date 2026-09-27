# Sửa cung — BowPhysics / HThinh.yy

## Nguyên nhân và thay đổi

- Trước đây ray camera xác định điểm ngắm nhưng tên bay thẳng ban đầu rồi rơi; không có bù trọng lực. Điểm sinh còn lệch khỏi tay và đọc camera trong Update trước khi camera cập nhật xong.
- FarmBow chạy LateUpdate sau camera/Cinemachine (execution order 200, Cinemachine 100). Lấy điểm dưới tâm +, tính vận tốc theo nghiệm quỹ đạo thấp với trọng lực 9,81 m/s². Hỗ trợ ngắm thẳng lên/xuống, chênh cao và camera lệch vai.
- Lực kéo đổi tốc độ 16–40 m/s; giữ tối thiểu 18% lực mới bắn. Ngoài tầm với lực hiện tại thì báo, tên vẫn chịu trọng lực và rơi. Không khóa mục tiêu hoặc tự đuổi quái đang di chuyển.
- Điểm sinh lấy từ holder của cung trong góc nhìn thứ ba; góc nhìn thứ nhất dùng ngang ngực. Kiểm tra vật cản từ ngực đến tay để không sinh tên bên kia tường. Collider tên vô hiệu hóa ngay khi tạo.
- Tích phân gia tốc hằng với bước tối đa 1/120 giây, SphereCast bán kính 0,025 m dọc mỗi đoạn để chặn xuyên khối khi FPS thấp. Đầu tên là điểm va chạm, thân tên nằm phía sau; giữ ghim/nhặt lại tên.
- AdventureBag.DamageTool nhận cung 111. Một phát thành công trừ một tên và một độ bền của đúng cung đang chọn. Hết độ bền chặn bắn, thanh cung hiện ĐB /100. Cơ chế sửa 20 xu và chỉ số rèn giữ nguyên.
- Hủy kéo khi pause, thao tác UI hoặc đổi sang chiếc cung khác; thả quá sớm/hết tên/cung hỏng không tiêu hao.

## Phạm vi

Không dựng lại scene, không đổi save 21, công thức chế tạo, thông số quái hay các vũ khí khác. Backup nguồn: Recovery/Before-BowPhysics-20260927/UnitySource.zip. Bản mới dùng Builds/Windows-BowPhysics; bản trước giữ trong Recovery/Git history.

## Kiểm tra và gói bàn giao

Kết quả build/smoke và checksum được ghi khi hoàn tất tại DongGoi/RELEASE-MANIFEST.json. FarmBowChecks tích hợp trong smoke đầy đủ và chạy riêng với -farmSmokeCheck -farmBowOnly: 28 quỹ đạo ở 15/30/60/144 FPS, bắn thẳng đứng, ray hai góc camera, vật cản mỏng, lực kéo, hao bền từng cung, hết tên/hỏng/pause/thả sớm.

### Kết quả thực tế

- Build Unity thành công: `Logs/build-20260927-233710.log`, `FARM_M1_BUILD_OK 100993487`, editor ghi return code 0. Runtime 169 file / 100.993.487 byte.
- Smoke riêng: `Logs/smoke-bow-only.log`, `FARM_BOW_PHYSICS_OK`.
- Smoke đầy đủ + art: `Logs/smoke-bow-full.log`, exit 0; có kiểm tra cung mới, nước/TNT/rương/save/rèn/kiếm và các hệ thống cũ. Không thấy exception. Ảnh kiểm tra được chuyển vào `Logs/BowPhysicsScreens`; đã xem ảnh cầm cung.
- Khởi động thường: `Logs/startup-bow.log`, còn chạy và Responding=True sau 15 giây, không exception.
- SHA256 scene trước/sau trùng nhau; source Assets/Packages/ProjectSettings khớp mirror Git.
- Build helper đổi sang `WaitForExit()` của riêng editor để tránh PowerShell chờ cả Windows job sau khi Unity đã thoát. Đã kiểm cú pháp helper sau sửa; build bên trên chạy trước thay đổi helper này.
