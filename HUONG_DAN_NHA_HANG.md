# Hướng dẫn Nhà hàng Nông Trại

## Chạy bản Windows

Giải nén hoặc giữ nguyên toàn bộ thư mục `Builds/Windows-Restaurant`, sau đó chạy `CHAY_GAME.bat` ở thư mục gốc project hoặc mở `Builds/Windows-Restaurant/NongTrai.exe`. Không di chuyển riêng file `.exe` ra khỏi thư mục build.

## Mở và vận hành nhà hàng

Nhà hàng nằm phía nam nông trại, có ba tầng và 24 bàn. Ngắm vào vật dụng rồi nhấn chuột phải để mở bảng thao tác. Bảng hiệu trước cửa đổi giữa đóng và mở; khách mới chỉ vào khi quán mở. Bảng menu cho phép bật tối đa tám trong 30 món. Quán bắt đầu với sáu món dễ nấu.

Tầng trệt có bếp mở sau vách kính, cửa dành cho nhân viên và ô quầy chuyển món. Khách chỉ đi trong khu ăn uống; người chơi vào bếp qua cửa nhân viên. WC nam/nữ riêng ở tầng trệt và lầu 1. Cầu thang chữ U có chiếu nghỉ, đi lên xuống bằng WASD như bình thường, không cần nhảy. Đèn sảnh, bếp và WC bật/tắt ở công tắc trong nhà hàng.

Để nấu và phục vụ một đơn:

1. Đưa nông sản, trứng, sữa, thịt hoặc cá từ túi vào tủ lạnh/kho khô. Nhà bếp chỉ dùng phần nguyên liệu đã nhập vào kho bếp.
2. Chọn món ở bàn sơ chế. Nhấn Space hoặc nút đúng nhịp ba lần để lấy chất lượng tốt; thao tác chưa đạt vẫn giữ mẻ ở chất lượng chuẩn.
3. Chuyển mẻ tới đúng bếp, lò hoặc quầy lạnh. Với bếp và lò, nạp than hoặc gỗ từ túi. Canh ba nhịp khi nấu để giữ chất lượng tốt; khi đang nấu, đóng bảng thao tác để đồng hồ bếp tiếp tục.
4. Ra đĩa ở quầy bếp, rồi lấy món đã hoàn thành ở quầy chuyển món. Nếu túi đầy, đĩa vẫn đợi ở quầy.
5. Chọn đĩa trên hotbar, ngắm đúng khách đang chờ và nhấn chuột phải. Giao nhầm món không tiêu hao đĩa. Món đúng trả xu, XP và tiền tip theo thời gian chờ, chất lượng món và độ sạch.
6. Dọn bàn sau khi khách rời đi, rửa đĩa bẩn ở bồn rửa, đổ rác và lau thiết bị. Nhà vệ sinh, bồn rửa tay và công tắc đèn cũng có thể tương tác.

Bàn sơ chế, bếp, lò, quầy đồ uống và quầy ra món được di chuyển bằng bảng **Bố trí nội thất** ở tầng trệt. Chỉ mở bố trí khi quán đóng và đã hết khách. Chọn tầng, chọn đồ, click vị trí trên sơ đồ theo ô 0,5 m và xoay 90 độ. Nút **Áp dụng** từ chối bố trí chặn lối; **Hủy/Esc** bỏ các thay đổi chưa áp dụng.

## Câu cá và vật phẩm mới

Nhấn chuột phải vào cầu câu cá để mở trò chơi nhỏ ở giữa màn hình. Nhấn Space hoặc nút giật cần khi vạch chạy nằm trong vùng xanh. Cần ba lần đúng trước ba lần trượt, trong 25 giây. Esc hủy lượt; sau mỗi lượt chờ ba giây. Cá được cho vào túi để nấu, không tự bán.

Hồ câu ở phía đông sân chính, có lối lát nối từ sân nông trại, cổng mở và cầu gỗ tới cầu câu cũ. Đèn lối đi tự bật vào ban đêm.

Sáu loại cá và bốn loại thịt quái mới có công thức riêng. Các món được làm từ nông sản hiện có cùng nguồn thịt heo, bò, cừu, gà. Xem toàn bộ thành phần và số suất nấu được trong bảng menu/bếp.

Nhà hàng tạm dừng khi mở giao diện hoặc đi sang bản đồ sinh tồn. Dữ liệu kho bếp, mẻ nấu, món chờ, khách, doanh thu, vệ sinh và cách bố trí lưu cùng bản lưu nông trại phiên bản 22; bản lưu cũ vẫn được đọc.

## Nguồn model và giấy phép

Model Kenney và Poly Haven, cùng texture ambientCG, được phát hành theo CC0. Ghi chú nguồn và giấy phép gốc nằm trong [`Assets/ThirdParty/Restaurant`](Assets/ThirdParty/Restaurant/SOURCES_LICENSES.md). Model gốc lưu cùng project trong [`Assets/ThirdParty/Restaurant`](Assets/ThirdParty/Restaurant/). Bộ model được ghép thành nhà hàng trong runtime để giữ scene nông trại hiện tại nguyên vẹn.
