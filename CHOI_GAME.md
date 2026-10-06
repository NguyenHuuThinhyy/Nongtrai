# Bản thử Android / AR / AI / Cloud

© HThinh.yy. Bản local được chủ dự án duyệt gộp main ngày 06/10/2026.

- **PC: chạy CHAY_GAME.bat** để tự mở backend nhẹ và ghi kết nối. Chờ “BACKEND SAN SANG”, vào game rồi nhấn C; AI chỉ nạp khi gửi câu hỏi, nhả RAM sau60giây không dùng. [Cách khởi động và xử lý lỗi](Docs/ASSISTANT_START.md).
- **BAT_BACKEND_PC.bat** giữ riêng backend chạy; **CHAY_GAME_KHONG_TRO_LY.bat** chơi offline. AR không cần backend; nút Hỏi trợ lý cần backend.

- **PC: C mở chatbot**, dùng được cả khi đang xem nông trại AR. Chat AI cần backend/model
  đã chạy và mã kết nối; nút Hướng dẫn vẫn đọc được khi mất mạng.
- **H ẩn/hiện bảng hướng dẫn**. Bảng có ghi phím này ngay trên nút Ẩn hướng dẫn.
- **AR trên PC: nhấn J để mở/đóng**, hoặc Trợ lý / AR / Cloud → Nông trại AR. Chuột phải xoay, lăn chuột phóng,
  click nền trống đặt mô hình thủ công, click cây/công trình xem thông tin.
  Bật webcam ghép hình camera với nông trại; thiếu webcam vẫn xem mô hình được.
  Đặt lại trả mô hình về giữa; Đóng AR/Esc tiếp tục chơi đúng vị trí cũ.
- Android: joystick di chuyển, vuốt bên phải xoay camera; giữ Chạy, chạm Nhảy.
  Dùng/Đánh = chuột trái; Tương tác = chuột phải. Cung giữ/thả nút, đào giữ nút,
  xô múc/đặt bằng một nút, Xoay cho công trình, hotbar9ô có ảnh vật phẩm.
- Menu chạm: túi/shop/máy/mở đất/chuồng/bản đồ và minigame/cho thú ăn/đổi góc/bay/lưu.
  Túi có Tách nửa, Chuyển nhanh và kéo thả. Runner/câu cá/nhà hàng dùng nút chạm.
- Trợ lý / AR / Cloud → Kết nối: địa chỉ PC cùng Wi-Fi và mã backend. Hướng dẫn
  luôn có sẵn khi backend tắt. Mã Adafruit chỉ nhập ở PC; bạn có thể tạo tài khoản sau.
- Nông trại AR: mô hình local mở ngay trên PC/Android; điện thoại kéo xoay, hai ngón phóng. Camera tùy chọn; chạm cây/công trình xem thông tin, Hỏi trợ lý cần backend; ĐóngAR quay về game.
- Cloud mô phỏng: Bậtcloud sau khi đã xây trạm vùng đầu; dashboard ON/OFF điều khiển
  trạm đó. Mất mạng trở về tưới cục bộ. Không cần mạng để chơi gameplay thường.
- Cài/build/backend/model: xem Docs/RUBRIC_INTEGRATION.md. Kiểm tra thủ công:
  Docs/ACCEPTANCE.md. APK đang là bản cài thử; AR/cloud thật cần thiết bị/tài khoản.

# Tìm số 2D — 02-10-2026

- **Tab → Tìm số 2D:** chọn số cần tìm trong lưới 7 × 7. Mỗi lượt có 5 câu, mỗi câu 5 giây. Click sai không cộng điểm; hết giờ chuyển câu tiếp.
- **Thưởng:** 20 xu/câu đúng, 5/5 thêm 100 xu; tối đa 3 lượt có điểm/ngày game. Lần 5/5 đầu tiên trong ngày thêm 1 đá nâng cấp. 0/5 hoặc thoát giữa lượt không mất lượt thưởng. Hết lượt vẫn luyện tập được.
- **Esc/nút góc phải:** về nông trại, giữ nguyên vị trí. Nông trại tạm dừng khi chơi 2D. Túi đầy thì đá thưởng rơi cạnh nhân vật. Chế độ sáng tạo chỉ luyện tập. Lưu thủ công giữ quota thưởng; thoát game vẫn theo cơ chế lưu hiện có.

# Cung: ngắm, quỹ đạo và độ bền — BowPhysics / 27-09-2026

- Giữ chuột trái để kéo cung, thả để bắn; kéo đầy sau 0,8 giây (chỉ số rèn có thể rút ngắn).
- Tên bắn từ vị trí cung/tay, hướng về điểm dưới dấu + và bù độ rơi trong tầm lực kéo. Kéo mạnh tăng tốc độ tên từ 16 đến 40 m/s; tên chịu trọng lực 9,81 m/s². Mục tiêu quá xa với lực kéo hiện tại có thông báo, cần kéo mạnh hơn hoặc tiến gần. Quái đang di chuyển vẫn cần ngắm đón.
- Tường/khối giữa cung và mục tiêu chặn tên. Tên ghim vào vật thể/quái, có thể đến gần nhặt lại như trước.
- Mỗi lần bắn thành công mất **1 tên + 1 độ bền của đúng chiếc cung đang cầm**. Thả quá sớm, hết tên, tạm dừng hoặc cung hỏng không tiêu hao. Cung còn 0 độ bền không bắn được: mở túi, chọn cung → **Sửa dụng cụ: 20 xu**. Thanh cung hiện ĐB /100.
- Bản mới: `Builds/Windows-Rubric/NongTrai.exe`; chạy `CHAY_GAME.bat` ở gốc hoặc giải nén ZIP Windows Rubric trong DongGoi.

## Rương, hồi sinh và chiến đấu

- **Rương:** chuột phải mở thẳng bảng đồ, không giải câu hỏi. Click một món để lấy 1, Shift + click lấy cả chồng, hoặc bấm **LẤY TẤT CẢ**. Túi đầy thì phần còn lại nằm trong rương. Giữ chuột trái 0,8 giây để đập vỡ rương khám phá, thả toàn bộ đồ còn lại và vật phẩm rương. Rương có quái canh cần hạ quái trước. Đồ đã lấy không xuất hiện lại khi rời vùng hoặc lưu/tải.


- **Hồi sinh tại chỗ:** trả 100 xu, hồi đầy máu và giữ đồ ngay nơi ngã xuống. Không đủ xu thì nút bị khóa. Lựa chọn miễn phí vẫn về cổng và rơi tối đa 3 món.
- **Hong bọt biển:** đặt bọt biển đầy cách đống lửa tối đa 2 m, không có tường chắn, chờ 10 giây. Hoặc cầm bọt biển đầy rồi chuột phải vào đống lửa để hong từng chiếc trong 10 giây. Thu bọt biển khô để hút nước lần nữa. Hong khối đặt cạnh lửa bắt đầu lại nếu rời xa lửa hoặc tải game; bọt biển đang xử lý trong đống lửa lưu tiến độ như nướng thịt.
- **Ngắm kiếm:** cầm kiếm hiện vòng tròn viền rõ, giữa trong suốt; vàng khi bắt được mục tiêu. Quái hơi lệch tâm trong vòng vẫn đánh được. Không tăng tầm đánh, không xuyên tường, giữ hồi chiêu 0,3 giây. Đổi dụng cụ trở lại dấu +.




## TNT: đặt trước, châm sau

1. Đưa TNT lên hotbar và chọn. Ở map Khám phá, ngắm mặt đất/khối gần bạn rồi bấm **trái hoặc phải** để đặt. Đặt thất bại có thông báo, không mất TNT.
2. Đổi sang **đuốc**, ngắm khối TNT đã đặt và bấm **trái hoặc phải** để châm. Không tốn đuốc và không đặt đuốc lên TNT.
3. TNT nhấp nháy **5 lần** (khoảng 5 giây) rồi nổ; lùi ra xa ít nhất 4 m. TNT chưa châm sẽ nằm yên. Ngòi tạm dừng khi pause/về nông trại, lưu/tải tiếp đúng nhịp đang chạy.

## Điều khiển và hệ thống hiện tại

- **WASD:** đi 6 m/s. **Shift:** chạy 9 m/s, hao độ no gấp 4 khi thực sự chạy nhanh.
- **Ngắm dưới chân:** kéo chuột xuống hết; giữ trái để đào thẳng xuống. Cầm khối, nhảy rồi bấm trái để đặt ngay dưới chân. Không được đặt chồng vào người.
- **Ra khỏi nước:** giữ Space và hướng về bờ. Có thể đào một bậc nếu bờ cao hơn tầm nhảy.
- **Một xô nước duy nhất:** chọn **ô 6 — Xô nước**. **Rỗng:** ngắm hồ/sông trong 6 m, bấm **trái** để đầy. **Đầy:** bấm **trái** lên đất/khối xây để đặt nguồn nước, xô về rỗng. Không dùng chuột phải, không cần chế tạo bình khác. Cây được tưới bằng vòi hoặc nước đặt cạnh ruộng. Nếu xô đã bán/bỏ, mua lại ở trang 4 của shop (50 xu).
- **Bọt biển:** shop trang 4, 80 xu/khối. Chọn trên hotbar, trái để đặt cạnh nước. Hút nước trong khối 3×3×3 (1 ô mỗi hướng) rồi chuyển **đầy**, không hút lần nữa. Đổi sang dụng cụ khác rồi **phải vào bọt biển** để thu vào túi; có thể bán khô/đầy. Nguồn bị hút mất thì phần dòng chảy từ nguồn đó cũng rút. Đặt nước mới gần vùng đã hút sẽ cho nước lan trở lại.
- **Vòi tự tưới:** máy bơm/bảng quản lý nước → thuê 350 xu, nhận vào túi; chọn rồi trái lên đất để đặt. Tự tưới bán kính 6 m trong **1 ngày game** từ khi đặt, không nạp nước. Hết hạn tự thu hồi; ngủ qua ngày cũng tính vào hạn thuê. Thuê tối đa **3/ngày**, LV3/5/7 tăng lên **4/5/6**. Phải vào vòi để thu hồi sớm và kết thúc lượt thuê, không hoàn vật phẩm/lượt/xu.
- **Dọn túi:** hạt dùng hết tự mất ô; mua lại sẽ vào ô trống. Mọi vật phẩm, hạt và dụng cụ đều bán được. Nút **BÁN TẤT CẢ ĐỒ TRONG TÚI** bán cả dụng cụ; xô/xẻng/kiếm/rìu có thể mua lại. Nút bán nông sản tại shop vẫn chỉ bán nông sản.
- **Hạt khám phá:** hạ Golem hang/tế đàn để xuất hiện rương thưởng, hoặc mở rương do Golem bậc 4 canh. Mở hoặc đập rương để nhận **2 hạt bí pha lê + 1 hạt dâu hoàng kim**. Không bán hạt này tại shop, không có công thức chế tạo. Gieo trên đất đã cày, chăm tưới rồi thu hoạch/bán.
- **Quái và thanh máu:** Golem, quái canh rương, sói/cáo/rắn có thể nhảy qua vật cản cao 1 ô khi đủ khoảng trống. Thanh máu quái hiện tên và HP/tối đa; thanh máu người chơi lớn hơn ở góc trái dưới.
- **Save cũ:** các bình phụ64/71 được bỏ và hoàn theo giá bán (28/12 xu), giữ/cấp một xô mặc định nếu có bình cũ; còn nước thì xô đầy. Save21 giữ bọt biển đầy/vùng đã hút, vòi và lượt thuê trong ngày, nguồn nước và hạt đặc biệt.
- **Chém:** bấm hoặc giữ trái, hồi chiêu gốc 0,3 giây. Cuốc/đánh/đặt có quơ tay, vệt dụng cụ và hạt thao tác.
- **Rèn:** chọn đúng chiếc kiếm/rìu/cung trong lưới túi. RÈN tăng LV; ĐỔI CHỈ SỐ tốn 1 đá + 80 xu, đổi cả ba chỉ số: +1–12 sát thương, 3–18% chí mạng, 0–20% tốc đánh (cung: tốc kéo). Chí mạng gây 150% sát thương. Kiếm LV5 thêm +12 sát thương và hút 3 máu mỗi đòn trúng.
- **Bán/bỏ kiếm, rìu, cung:** mở túi bằng I/B, bấm chọn ô rồi Bán số lượng hoặc Bỏ vật phẩm đã chọn. Chỉ số không chuyển sang vũ khí khác.
- **Rương hiếm:** hạ quái canh trước khi mở hoặc đập rương. Sói → rắn → gấu → golem có sức mạnh/phần thưởng tăng theo bậc; mở để chọn đồ cần lấy hoặc đập vỡ cho đồ rơi ra.
- **Chơi lại từ đầu:** Esc → Chơi lại từ đầu → xác nhận. Bắt đầu LV1 với map/túi mới; bản lưu cũ được cất dự phòng, không lưu phiên hiện tại. Hủy thì tiếp tục đúng vị trí/cấp độ.
- **Lưu và mở lại game thông thường:** vẫn dùng bản lưu thủ công. Save cũ giữ địa hình cũ; sông/hồ theo seed mới có trong map tạo mới. Đổi map nhớ vị trí; hồi sinh miễn phí về cổng đã đặt hoặc cổng gốc; trả 100 xu thì hồi sinh tại chỗ.

### Hướng dẫn các hệ thống có sẵn


- **Bàn rèn:** chuột phải mở bàn đã đặt; bấm hoặc kéo kiếm/cung/rìu và đá từ túi vào hai ô. Bấm Rèn mới trừ đá/xu. Bấm ô đã chọn để bỏ chọn.
- **Sông/biển:** Tab → Khám phá; sông ở phía tây (-36,0), biển phía nam (0,-75), theo tọa độ ô HUD. Tự nổi trên nước, Space để ngoi; đặt ván cầu nếu muốn đi qua nhanh.
- **Cổng hồi sinh:** chế tạo bằng 8 khối đá + 3 kim loại + 1 đá nâng cấp. Chọn cổng trong hotbar/G, đặt ở Khám phá với khoảng trống 2,5 × 3 m. Chỉ có một cổng tự đặt, không tính cổng gốc. Chuột phải vào trụ để về trại. Tab đổi map luôn nhớ vị trí đứng cuối cùng; khi chọn hồi sinh miễn phí mới về cổng này. Giữ trái phá, nhặt lại rồi chuyển chỗ. Khi hồi sinh miễn phí: chưa có cổng/cổng bị chặn hai phía → hồi sinh ở cổng gốc. Nhớ Lưu game để giữ vị trí cổng.
- **Boss:** 2.400 HP, đập 38 HP; dưới nửa máu chạy nhanh và đập 52 HP. Né khi hiện chữ báo đòn đập; chuẩn bị bình máu và vũ khí rèn.
- **Runner:** tăng khó tại 200/500/900/1400 m, đổi làn trống giữa các hàng; sau 500 m có xe rơm từ lề. A/D đổi làn, W/Space nhảy, S trượt. Tốc độ tối đa 26 m/s.
- Theo đính chính: **giữ Shift chạy 9 m/s** (trước 7), đi 6 m/s; tăng tốc/hãm nhanh hơn. Mũ theo xương đầu, giữ tóc gốc.

## Farm Runner, ăn uống và lưu game

- **Tab → Farm Runner:** A/D đổi làn, W/Space nhảy, S trượt, Esc kết thúc. Một va chạm kết thúc lượt. Mất 1 vé/lượt, khởi đầu 3 vé; giao một đơn hộp thư nhận 1 vé hoặc mua vé 500 xu. Hồi 1 vé/2 giờ thực, tối đa 3 vé hồi. Mốc 1 km thưởng đá nâng cấp, 2 km TNT, 3 km bình máu.
- **Ăn/uống:** cầm thực phẩm hoặc bình máu rồi giữ trái 3 giây. Thả tay/đổi món hủy; mỗi lần giữ dùng một món. Bình máu hồi 50 HP. Cầm thịt sống rồi chuột phải vào đống lửa để nướng.
- **Máy chế biến:** M hoặc mở máy; chọn lò bánh/lò nung, nạp gỗ (30 giây) hoặc than (120 giây).
- **Lưu:** Esc → Lưu game. Mở lại game sẽ tải bản lưu thủ công; thoát không tự lưu. Chế độ sáng tạo dùng dữ liệu trong bộ nhớ và không ghi đè bản lưu thường.

## Mở game và dự án

Giải nén toàn bộ ZIP Windows rồi chạy CHAY_GAME.bat/NongTrai.exe. Khi tải bằng Code → Download ZIP, launcher nằm ở gốc repo. Hướng dẫn mở source Unity và build tiếp: [HUONG_DAN_NHOM.md](HUONG_DAN_NHOM.md).
