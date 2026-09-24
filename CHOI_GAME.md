# Nông Trại – hướng dẫn chơi

Mở `D:\GAME_NongTrai\Builds\Windows\NongTrai.exe`, giữ nguyên cả thư mục `Windows` khi chuyển sang máy khác. Chọn **Vào nông trại** trong menu. Không cần cài Unity để chơi bản Windows. Trong game, nhấn **H** để mở hướng dẫn từng bước.

| Điều khiển | Tác dụng |
|---|---|
| WASD / Shift / Space / chuột | Đi / chạy / nhảy / nhìn quanh; V đổi góc nhìn |
| Dấu + giữa màn hình | Ngắm ô đất, vật thể, máy hoặc khối muốn thao tác |
| Chuột trái ở nông trại | Cày, gieo, tưới, gặt hoặc mở vật thể đang ngắm; giữ trái để phá khối đã xây |
| Chuột trái ở Khám phá | Giữ để đào/đánh; chọn khối trong hotbar rồi click để đặt |
| Chuột phải | Dùng món đang cầm: ăn, gieo cây gỗ, bón phân, cho thú hoang ăn; thả thú đang bế |
| 1–9 / lăn chuột | Đổi nhanh ô đang cầm trên hotbar |
| B | Túi đồ 36 ô, hàng 9 ô dưới là hotbar; kéo-thả hoặc Shift+click để chuyển vật phẩm |
| E | Mở bản đồ nhiệm vụ đúng tọa độ nông trại hoặc vùng khám phá; click dấu hoặc danh sách để xem vị trí |
| Tab | Mở hai bản đồ Nông trại / Khám phá; khi quay lại sẽ ở vị trí đã rời đi |
| F | Cho vật nuôi đang ngắm ăn bằng thức ăn đã mua |
| G / M / N / P | Bảng xây / chế biến / đất và dụng cụ / chuồng trại |
| Esc | Tạm dừng, lưu thủ công, chỉnh âm lượng, hướng dẫn và thoát |
| F8 khi sáng tạo | Bật/tắt bay; Space lên, X xuống, Shift bay nhanh |

## Mười phút đầu tiên

1. Đến các ô ruộng, chọn **5 Cuốc** rồi ngắm đất và bấm trái. Chọn **1–3 Hạt giống** rồi bấm trái để gieo.
2. Chọn **6 Bình tưới**, đến hồ và bấm trái để nạp 8 nước. Quay về ruộng, ngắm ô đã gieo và bấm trái để tưới. Cây được giữ ẩm sẽ lớn nhanh; chọn **7 Liềm** để gặt khi chín.
3. Bấm **B** xem túi, mở cửa hàng để mua hạt, thú, khối xây hoặc vật trang trí. Bảng bên trái cho thấy sơ đồ nông trại, đơn hàng và vật nuôi đói.
4. Đến **bàn gỗ trước nhà**, bấm trái để chế tạo; đến **hộp thư đỏ**, bấm trái để xem 5 đơn. Click một đơn để xem số hàng đang có/cần giao, rồi chọn **Giao** hoặc **Đổi**. Danh sách chế tạo có 15 món và cuộn được.
5. Bấm **Tab → Khám phá**, giữ trái để đào lấy tài nguyên. Chọn khối trên hotbar rồi click trái vào mặt khối để đặt. Phím G mở bảng 13 loại khối. Trở về bằng Tab và tiếp tục tại đúng vị trí cũ.

Mỗi ngày game dài **18 phút đời thực**, với 28 ngày mỗi mùa. Thời tiết nắng xuất hiện thường hơn mưa; mưa/bão tưới ruộng. Lúa mì, cà chua và đậu nành cần 2/3/5 phút khi đủ ẩm, khi khô chỉ lớn ở 20% tốc độ. Bình tưới chứa 8/16/24 nước theo bậc; trạm tưới mua từ LV2 và nhận nước mang từ hồ. Phân bón cho cây đang lớn tăng 12% tiến độ và thêm ẩm: đặt món trên hotbar, ngắm ruộng rồi click phải.

Vật nuôi có độ no và vui; bản đồ E báo nơi có con đói. Ngắm thú và bấm F để dùng thức ăn mua ở shop. Cám dinh dưỡng chế tại bàn hoặc mua ở shop: chọn trong hotbar, ngắm thú rồi click phải. Gà cho trứng trong ổ; bò cho sữa, cừu cho len, heo cho thịt. Khi sản phẩm sẵn sàng, thú về ổ nằm và có biểu tượng trên đầu; **click trái vào ổ nằm để lấy**, click trái trực tiếp vào thú chỉ nhấc thú. Chuột phải thả vào đúng chuồng.

Sáu máy chế biến có công thức trong `Assets/StreamingAssets/recipes.json` và hàng đợi tối đa 5 lượt/máy. Bàn chế tạo đọc `Assets/StreamingAssets/crafting.json`: bó nông sản, gói đậu, giỏ táo, đèn, bàn chế tạo, bậc gỗ, đuốc, hàng rào, ván cầu, bánh táo, mứt, cám, phân bón, rương đồ và đống lửa. Bàn có sẵn trước nhà; có thể chế tạo thêm bàn bằng 5 khối gỗ. Cây táo cho táo khi dùng giỏ (ô 9), hoặc cho 6 khối gỗ khi hạ bằng rìu (ô 8). Năm đơn mỗi ngày có độ khó, thời hạn và phần thưởng khác nhau; có thể đổi một đơn sau 5 phút chơi. Tổng 2/4/6 đơn giao sẽ mở bánh mì/phô mai/nước táo.

Map Khám phá sinh địa hình theo seed khi đi xa và tải từng chunk 16 × 16 m. Có cây gỗ lớn theo giai đoạn, lá, quặng, thú hoang bò/cừu và một ít rương chứa đồ. Chuột phải mở rương; giữ trái để phá rương, đồ bên trong rơi ra. Đêm (18:00–06:00) có sói đuổi và cắn; cầm đuốc hoặc đứng gần đuốc/đống lửa đã đặt để xua sói. Ban ngày sói về hang. Dùng đúng công cụ giúp đào nhanh và lấy vật liệu; đồ rơi sẽ bay vào túi nếu còn chỗ. Có thể dùng lúa mì hoặc cám để dụ và cho thú hoang ăn, cho hai con cùng loài ăn gần nhau để sinh sản. Đào 30 khối mở bản vẽ lò nung và đèn. Công trình và địa hình đã đào được lưu khi nhấn **Lưu game**. Nhà kho ở gần nhà nông trại cho cất/lấy từng món; rương tự chế có thể đặt ở cả hai map và cất đồ riêng.

**Chỉ nút Lưu game trong menu Esc ghi tiến độ.** Thoát không tự lưu. Chế độ sáng tạo dùng bản sao trong bộ nhớ, LV99, bật bay và mở cả hai map; rời chế độ sẽ bỏ toàn bộ thay đổi thử nghiệm. Bản lưu v11 vẫn đọc được v2–v10. Đường dẫn bản lưu Windows: `%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest\farm-manual-save.json`.

Để build lại, đóng Unity Editor đang mở cùng dự án rồi chạy:

```powershell
& 'D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\GAME_NongTrai' -executeMethod NongTrai.Editor.FarmProjectBuilder.BuildWindows -logFile 'D:\GAME_NongTrai\Logs\build.log'
```
