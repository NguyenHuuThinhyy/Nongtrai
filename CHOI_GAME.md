# Nông Trại – cách chơi bản mở rộng

Mở `D:\GAME_NongTrai\Builds\Windows\NongTrai.exe`, giữ nguyên toàn bộ thư mục `Windows` khi chép sang máy khác. Ở menu chính, chọn **Vào nông trại**. Mở Unity Editor nếu muốn sửa game; người chơi trên máy khác không cần cài Unity.

| Phím | Tác dụng |
|---|---|
| WASD, Shift, Space | Di chuyển, chạy, nhảy cao 1,6 m để qua khối 1 m |
| Chuột, V | Nhìn, đổi góc nhìn |
| E | Tương tác với ô đất, hồ, trạm tưới, bàn chế tạo, hộp thư, quầy/máy, vật thể và cửa chuồng |
| 1, 2, 3 | Chọn lúa mì, cà chua, đậu nành |
| 5, 6, 7 | Chọn cuốc, bình tưới, liềm trước khi thao tác ruộng |
| B hoặc I | Mở túi đồ; dùng nút **Mở cửa hàng** trong túi để vào shop |
| M | Máy chế biến và hàng đợi |
| N | Cấp độ, mua vùng đất và nâng cấp dụng cụ |
| P | Quản lý chuồng, mua thức ăn và chuồng mới |
| Tab | Mở hai bản đồ Nông trại / Khám phá và dịch chuyển |
| F | Cho vật nuôi đang ngắm ăn |
| Chuột trái/phải | Nhấc vật nuôi / thả vào đúng chuồng |
| 8, 9 | Chọn rìu để hạ cây táo lấy khối gỗ / giỏ để hái táo mà vẫn giữ cây |
| 1–9, cuộn chuột | Chọn nhanh ô hotbar; tên, icon và công dụng hiện ngay phía trên thanh |
| G hoặc nút Xây dựng trong túi | Bật/tắt xây dựng sau khi có Bàn chế tạo; 1–7 chọn khối, trái đặt, phải tháo, R xoay |
| Esc | Đóng bảng đang mở hoặc tạm dừng; mở hướng dẫn, chỉnh âm lượng, lưu, tiếp tục hoặc thoát |
| F8 | Bật/tắt bay trong chế độ sáng tạo; Space lên, X hoặc Ctrl xuống, Shift bay nhanh |

## Vòng chơi

Ngắm ô đất gần người, chọn đúng ô hotbar rồi nhấn E: **5 cày, 1–3 gieo, 6 tưới, 7 thu hoạch**. Lúa mì/cà chua/đậu nành cần 2/3/5 phút khi được giữ ẩm; lúc khô vẫn lớn ở 20% tốc độ. Bình tưới chứa 8/16/24 nước theo bậc, nạp miễn phí tại hồ. Từ LV2 có thể dùng bảng cạnh hồ để mua trạm tưới cho từng vùng với giá 600/900/1.200/1.500 xu; mỗi trạm chứa 32 nước và có sẵn 8 nước khi mới xây. Trạm phun trong bán kính 6 m khi còn nước, tự tưới các ô khô dưới 20%. Mang nước từ hồ tới trạm và nhấn E để nạp tiếp.

Gói 5 hạt lúa mì/cà chua/đậu nành có giá 20/40/75 xu; sản phẩm bán 8/18/32 xu mỗi đơn vị. Bán sản phẩm trong túi để kiếm xu; thu hoạch, chăm vật nuôi, chế biến và giao đơn cho XP. Nâng cấp **cuốc, bình tưới, liềm** từ đồng lên bạc rồi vàng bằng bảng N: số ô tác động cùng lúc là 1, 3, 5. Bốn vùng ruộng, mỗi vùng 20 ô, mở lần lượt bằng xu khi đạt cấp 1, 2, 4, 6; vùng đầu mở sẵn. Giá mở vùng sau tăng dần: 500, 1.200, 2.400 xu.

Gà, bò, cừu và heo có độ no, độ vui. Một ngày trong game dài **10 phút**; độ no và vui giảm theo thời gian. Ngắm con vật và nhấn F để dùng một thức ăn. Vật nuôi cần đủ no và vui mới tạo sản phẩm. Gà đẻ trứng theo chu kỳ tại ổ, mỗi chuồng gà tối đa 5 con. Bò cho sữa mỗi 45 giây, cừu cho len mỗi 60 giây. Heo cho thịt một lần. Bảng P bán 10 thức ăn giá 50 xu, xây thêm chuồng bò/cừu và nâng cấp chuồng. Nâng cấp chuồng bò, cừu, heo tăng sức chứa; nâng chuồng gà rút ngắn chu kỳ đẻ, vẫn giữ tối đa 5 con. Mở shop từ nút trong túi đồ để mua chuồng gà thứ hai.

Nhấn M để mở xưởng, hoặc đến máy và nhấn E để chỉ xem công thức của máy đó. Cả sáu máy ở nông trại; xưởng cưa và lò nung nằm phía tây. Máy quay khi đang chế biến. Công thức nằm trong `Assets/StreamingAssets/recipes.json`; mỗi máy có hàng đợi tối đa 5 lượt, nguyên liệu được trừ khi xếp việc và thành phẩm vào túi khi hết thời gian. Giá bán mỗi thành phẩm cao hơn tổng giá nguyên liệu:

| Máy | Công thức | Chờ | Giá bán |
|---|---|---:|---:|
| Cối xay | 3 lúa mì → 2 bột mì | 25 giây | 44 xu |
| Lò bánh | 2 bột mì → 1 bánh mì | 35 giây | 65 xu |
| Thùng ủ | 2 sữa → 1 phô mai | 40 giây | 65 xu |
| Máy ép | 3 táo → 2 nước táo | 30 giây | 70 xu |
| Xưởng cưa | 2 khối gỗ → 1 ván | 45 giây | 34 xu |
| Lò nung | 2 quặng → 1 kim loại | 55 giây | 50 xu |

Bàn chế tạo cố định và hộp thư nằm trước nhà. Bàn ghép tức thì Bó nông sản, Gói đậu, Giỏ táo, Đèn thủ công và Bàn chế tạo theo `Assets/StreamingAssets/crafting.json`. Hộp thư tạo hai đơn phù hợp tiến độ mỗi ngày; giao đủ một lần nhận 150% giá thường, XP và khối xây. Có thể đổi một đơn sau mỗi 5 phút chơi thực tế; đồng hồ đổi đơn dừng khi game tạm dừng. Hoàn thành tổng 2/4/6 đơn lần lượt mở bánh mì/phô mai/nước táo.

Để xây, chọn ô 8 và dùng rìu hạ cây táo lấy **6 khối gỗ**, hoặc mua gỗ ở **trang 2 của shop**. Chọn ô 9 và dùng giỏ nếu chỉ muốn hái **5 táo**, cây vẫn còn. Dùng 5 khối gỗ chế tạo Bàn chế tạo. Nhấn G hoặc nút Xây dựng trong túi, đặt bàn trước rồi chọn các khối gỗ, đá, gạch, kính, kim loại hoặc cỏ. Tất cả sáu loại đều bán trong trang 2; khối đá còn lấy ở mỏ, và đơn giao hàng thưởng khối. Tháo khối bằng chuột phải sẽ trả vật phẩm về túi. Bàn chế tạo đã đặt cũng dùng E để mở công thức. Chế độ sáng tạo vẫn phải có vật liệu, không tạo vật phẩm vô hạn.

## Ngày, mùa, bão và các đảo

Đồng hồ trên HUD hiển thị năm, ngày, giờ, mùa và thời tiết. Mỗi mùa dài 28 ngày; lá và cỏ đổi màu theo mùa. Trời mưa hoặc bão tự tưới ruộng. Khi có bão, trả lời câu hỏi gia cố trong 25 giây: đúng thì không thiệt hại, sai hoặc hết giờ sẽ mất 30–80% nông sản và máy bị chậm. Từ 18:00 đến trước 06:00, vào nhà và nhấn E ở giường để ngủ đến sáng.

Nhấn Tab hoặc dùng cổng để đi giữa **Nông trại** và **Khám phá**, cả hai mở từ đầu. Đã bỏ ba khu chức năng cũ (Trung tâm, Thần bí, Công nghiệp), đấu giá NPC, minigame và cổng giới hạn cấp. Cấp tối đa thường là 99.

Map Khám phá sinh thêm địa hình theo **seed** khi bạn đi xa, theo cả bốn hướng; không còn dừng ở ô 48 × 48 m. Có đồng cỏ, đồi cát, núi tuyết, đá và quặng/hốc ngầm. Khu xuất phát 48 × 48 m được giữ để tương thích bản cũ; phần đất mới bên ngoài thay đổi theo seed. Mỗi thế giới chưa có bản lưu nhận seed ngẫu nhiên. Khi bấm Lưu game, seed được giữ cố định: quay lại vẫn gặp cùng địa hình, các khối đã đào không tự mọc lại.

Nhấn V để dùng góc nhìn thứ nhất, hướng dấu chấm vào đất/đá trong tầm 6 m tính từ nhân vật, giữ chuột trái khoảng 0,55 giây để đào. Đất/cát/tuyết trả khối cỏ, đá trả khối đá, quặng trả nguyên liệu luyện kim. Đào 30 khối mở bản vẽ lò nung và đèn thủ công. HUD hiện phần trăm đào và thông báo khi khối quá xa hoặc thuộc khu bảo vệ. Khu cổng và tầng đáy không đào được. Dùng G để xây bằng vật liệu trong túi, Tab để trở về. Khu vào map có sáu cây táo ban đầu để hái trái hoặc chặt lấy gỗ. Gỗ lấy từ cây táo bằng rìu hoặc mua shop; không có quặng gỗ.

HUD phiêu lưu hiển thị seed, tên địa hình và tọa độ. Game tải từng vùng 16 × 16 m quanh nhân vật, gỡ vùng xa rồi tái tạo khi quay lại; các thay đổi đào và công trình vẫn giữ. Sương xa che ranh giới tải. Đây là thế giới mở rộng theo vùng trên máy chơi đơn, chưa có quái, sinh tồn hay multiplayer; đi quá xa vẫn chịu giới hạn độ chính xác tọa độ của Unity.

Chế tạo bàn từ 5 khối gỗ, bật G và đặt bàn trước để xây các khối khác. Chế độ xây: 1–7/cuộn chuột chọn khối, chuột trái đặt, chuột phải tháo, R xoay, G thoát. Có thể xây ở cả hai map. Các khối đào và công trình chỉ được lưu khi bấm Lưu game.

Bản lưu v8 đọc được bản cũ: giữ công trình nông trại, hoàn vật liệu của công trình ở đảo cũ vào túi, chuyển nhân vật ở đảo cũ về cổng khám phá. Quyền dùng bản vẽ cũ vẫn giữ. Với bản v7, khu khám phá và công trình được chuyển cùng nhau sang vị trí kỹ thuật mới để địa hình mở rộng không đè lên nông trại; giữ các ô đã đào.

Trong menu chính, **Chế độ sáng tạo** đọc một bản sao của bản lưu thủ công, tạm nâng nhân vật lên **LV99**, bật bay ngay và cho đi mọi đảo. F8 bật/tắt bay; WASD di chuyển, Space lên, X hoặc Ctrl xuống và Shift bay nhanh. HUD luôn hiện các phím bay khi đang bay. Chế độ này vẫn giữ va chạm, không cấp tiền/vật phẩm vô hạn, khóa nút Lưu game và bỏ thay đổi khi về menu hoặc thoát. Khi vào lại chế độ thường, cấp độ cũ trong bản lưu được khôi phục.

Trong menu Esc, chỉ nút **Lưu game** ghi bản lưu khi chơi thường. Thoát game không tự lưu. Mở lại game tải bản lưu thủ công gần nhất. Loader vẫn đọc bản lưu v2–v5; bản v6 lưu thêm các khối đã đặt và tự đổi tài nguyên gỗ cũ thành khối gỗ. Đường dẫn lưu trên Windows: `%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest\farm-manual-save.json`.

## Build lại

Đóng Unity Editor đang mở cùng dự án, rồi chạy:

```powershell
& 'D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\GAME_NongTrai' -executeMethod NongTrai.Editor.FarmProjectBuilder.BuildWindows -logFile 'D:\GAME_NongTrai\Logs\build.log'
```

Lệnh này dựng lại scene và bản Windows. Nếu đã sửa scene trong Editor, hãy giữ bản sao trước khi chạy.

HUD luôn có nhãn **[TAB] ĐỔI BẢN ĐỒ** bên trái, dưới nút túi đồ. Giữ W và nhấn Space để nhảy lên/qua khối.
