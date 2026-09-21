# Bản cập nhật canh tác

## Nhân vật và vật nuôi

- Nhân vật mới có khuôn mặt, tóc, mũ có đai, áo yếm và túi áo; tay chân chuyển động khi đi/chạy. Xoay camera bằng chuột để nhìn mặt nhân vật.
- Khu vật nuôi ở bên phải đường chính, trước nhà kho nhỏ: hai heo, hai gà, một bò và một cừu. Chúng tự di chuyển, nghỉ, tránh nhau và ở trong phạm vi chuồng. Esc cũng dừng vật nuôi.
- Vật nuôi hiện để quan sát; chưa có cho ăn, sinh sản hoặc thu sữa/trứng.
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

Hạt và nước hiện miễn phí, không giới hạn để thử vòng chơi. Chưa có cửa hàng, giá bán, lưu game hay thời tiết; thoát game sẽ mất vụ đang trồng và số nông sản. Model vẫn là mô hình đơn giản, chưa phải đồ họa hoàn thiện.

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
