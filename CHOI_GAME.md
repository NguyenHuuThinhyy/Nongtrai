# Bản cập nhật canh tác

## Túi đồ, vật nuôi và lưu game

- **I** mở túi đồ để xem lúa mì, cà chua, đậu nành, táo, trứng, sữa, lông cừu và thịt heo. Mỗi dòng có **Bán 1** và **Bán hết**; shop vẫn có nút bán tất cả sản phẩm.
- Chuồng bò, heo, cừu, gà tách riêng. Chuồng gà ban đầu tối đa **5 con**. Trong shop có thể mua thêm chuồng gà thứ hai (thêm 5 chỗ).
- Đứng gần vật nuôi, ngắm vào nó và **nhấp chuột trái** để nhấc. Đi vào chuồng đúng loài, hướng về chỗ trống và **nhấp chuột phải** để thả. Không thể thả nhầm chuồng hoặc ngoài chuồng.
- **Bò:** ngắm và nhấn E lấy 3 sữa, chờ 45 giây để lấy tiếp. **Cừu:** E lấy 2 lông, chờ 60 giây. **Heo:** E lấy 6 thịt; con heo rời chuồng sau khi lấy thịt, cần mua con mới nếu muốn tiếp tục nuôi.
- **Gà:** cứ 30 giây trong game, mỗi con gà đang ở chuồng tạo 1 trứng; mỗi ổ chứa tối đa 25 trứng. Ngắm ổ rơm trong chuồng gà và nhấn **E** để gom trứng vào túi đồ. Gà đang được cầm không đẻ trứng.
- Game **chỉ lưu khi bạn nhấn Lưu game** trong menu **Esc**. Thoát game không lưu thay đổi kể từ lần bấm lưu gần nhất. Mở lại game sẽ tải bản lưu thủ công gần nhất; nếu chưa từng bấm lưu, game bắt đầu mới. Tiền, túi đồ, hạt, ô đất, cây táo, vật nuôi, trứng và cửa chuồng được lưu tại `%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest\farm-manual-save.json` trên Windows. Bản mới không đọc file `farm-save.json` của bản cũ.

## Cửa chuồng, shop và menu Esc (bản mới)

- **Esc** mở menu tạm dừng. Chọn **Hướng dẫn**, **Tiếp tục** hoặc **Thoát game**. Hướng dẫn không hiện trên màn hình chơi.
- Đến cửa màu vàng ở cạnh trái chuồng, ngắm vào cửa và nhấn **E** để mở/đóng. Không đóng được khi đứng ngay trong lối cửa. Vật nuôi tiếp tục ở trong phạm vi khu chăn nuôi.
- **B** mở shop ở bất kỳ đâu; hoặc đến quầy mái đỏ gần nhà kho, ngắm quầy và nhấn **E**.
- Bắt đầu với **1.000 xu**, mỗi loại hạt **5 hạt**. Mỗi lần gieo trừ 1 hạt; mua 5 hạt giá 10 xu.
- Giá vật nuôi: bò 220, heo 120, cừu 150, gà 60 xu. Bò, heo và cừu mỗi chuồng chứa tối đa 4 con. Chuồng gà đầu chứa tối đa 5 con; chuồng gà thứ hai giá 400 xu và có cửa riêng.
- **Cây táo giá 150 xu**, tự trồng tại khu vườn phía tây (x = -28 hoặc -33); tối đa 6 cây. Lứa đầu chín sau 30 giây; ngắm thân cây và nhấn E thu 5 táo, sau 60 giây có lứa tiếp theo.
- Trong shop, nút **Bán toàn bộ nông sản** bán cây trồng giá 10 xu/đơn vị, táo giá 15 xu/quả. Nông sản đã bán bị trừ khỏi túi, không thể bán lặp lại.
- Tiến trình và tiền chỉ được lưu khi nhấn nút Lưu game theo mục mới ở trên.

## Nhân vật và vật nuôi

- Nhân vật mới có khuôn mặt, tóc, mũ có đai, áo yếm và túi áo; tay chân chuyển động khi đi/chạy. Xoay camera bằng chuột để nhìn mặt nhân vật.
- Khu vật nuôi ở bên phải đường chính, trước nhà kho nhỏ: hai heo, hai gà, một bò và một cừu. Chúng tự di chuyển, nghỉ, tránh nhau và ở trong phạm vi chuồng. Esc cũng dừng vật nuôi.
- Vật nuôi hiện chưa có cho ăn hoặc sinh sản; sản phẩm được lấy theo hướng dẫn ở đầu tài liệu.
- Chữ bảng dùng shader có kiểm tra độ sâu để mặt chữ phía sau bị tấm bảng che; không còn dùng shader chữ mặc định hiển thị xuyên vật thể.

Mở `D:\GAME_NongTrai\Builds\Windows\NongTrai.exe`. Giữ nguyên toàn bộ thư mục Windows. Nếu đang chạy bản cũ, thoát rồi mở lại file này.

## Trồng và thu hoạch

1. Đi vào khu ruộng ở bên trái hoặc bên phải đường chính bằng WASD.
2. Di chuột nhìn xuống ô đất trong phạm vi khoảng 3 m. Ô đang ngắm sẽ đổi màu và hiện thao tác.
3. Nhấn **E** để cày đất.
4. Chọn hạt bằng **1: lúa mì**, **2: cà chua**, **3: đậu nành**. Nhấn **E** để gieo.
5. Nhấn **E** lần nữa để tưới. Đất ướt chuyển màu sẫm; HUD hiển thị phần trăm lớn và độ ẩm.
6. Chờ 35 / 45 / 55 giây tương ứng từng cây. Cây chỉ lớn khi còn nước; E tưới bổ sung. Menu tạm dừng cũng dừng sinh trưởng.
7. Khi hiện **Thu hoạch**, nhấn **E**. Mỗi ô cho 3 nông sản; bộ đếm góc phải tăng đúng loại.
8. Ô đất sau thu hoạch có thể gieo lại ngay.

Nước hiện miễn phí; hạt dùng túi đồ và shop theo mục cập nhật ở trên. Chưa có thời tiết. Model vẫn là mô hình đơn giản, chưa phải đồ họa hoàn thiện.

**Điều khiển khác:** Shift chạy, Space nhảy, V đổi ngôi thứ nhất/thứ ba, Esc tạm dừng/thả chuột.

## Thay đổi

- Sửa hướng chữ trên bảng, thêm chữ mặt sau, thu nhỏ để vừa bảng.
- Thay cây trang trí bằng 80 ô đất có thể cày/gieo/tưới/thu hoạch.
- Ba CropDefinition ScriptableObject trong `Assets/Farm/Data/Crop0.asset` đến `Crop2.asset` cho phép chỉnh thời gian lớn, sản lượng và màu nông sản.
- FarmPlot lưu trạng thái ô, độ ẩm và tiến độ; FieldManager cập nhật mỗi giây và dừng khi pause.
- Cây có thân, lá và phần nông sản hiện dần qua bốn giai đoạn.
- HUD cho biết hạt đang chọn, số lượng đã thu hoạch và thao tác tại ô đất.

## Kiểm tra trong Editor

Mở `Assets/Farm/Scenes/Farm.unity` và Play. Thử đủ ba loại cây theo quy trình trên; không tưới thì tiến độ phải giữ ở 0%. Sau thu hoạch, kiểm tra bộ đếm tăng 3 và gieo lại không cộng thêm nông sản. Kiểm tra bảng từ cả hai phía và đổi camera bằng V.

`Logs/build.log` ghi kết quả build; `Logs/smoke.log` ghi kiểm tra tự động. Bộ kiểm tra dùng tick tăng tốc để kiểm tra trạng thái, không thay đổi tốc độ phát triển của game thông thường.
