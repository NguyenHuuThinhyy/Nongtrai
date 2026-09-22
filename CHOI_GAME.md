# Nông Trại – cách chơi bản mở rộng

Mở `D:\GAME_NongTrai\Builds\Windows\NongTrai.exe`, giữ nguyên toàn bộ thư mục `Windows` khi chép sang máy khác. Ở menu chính, chọn **Vào nông trại**. Mở Unity Editor nếu muốn sửa game; người chơi trên máy khác không cần cài Unity.

| Phím | Tác dụng |
|---|---|
| WASD, Shift, Space | Di chuyển, chạy, nhảy |
| Chuột, V | Nhìn, đổi góc nhìn |
| E | Cày, gieo, tưới, thu hoạch; dùng quầy/máy; lấy sản phẩm; mở cửa chuồng |
| 1, 2, 3 | Chọn lúa mì, cà chua, đậu nành |
| B | Shop mua hạt, vật nuôi, cây táo, chuồng gà |
| I | Túi đồ, bán từng loại hoặc bán hết |
| M | Máy chế biến và hàng đợi |
| N | Cấp độ, mua vùng đất và nâng cấp dụng cụ |
| P | Quản lý chuồng, mua thức ăn và chuồng mới |
| Tab | Mở bản đồ bốn đảo và dịch chuyển |
| F | Cho vật nuôi đang ngắm ăn |
| Chuột trái/phải | Nhấc vật nuôi / thả vào đúng chuồng |
| 1–9, cuộn chuột | Chọn ô hotbar |
| Esc | Tạm dừng; mở hướng dẫn, chỉnh âm lượng, lưu, tiếp tục hoặc thoát |

## Vòng chơi

Ngắm ô đất gần người và nhấn E lần lượt để cày, gieo, tưới, thu hoạch. Cây có bốn giai đoạn chuyển động khi lớn. Cây chỉ lớn khi đủ nước. Bán sản phẩm trong túi để kiếm xu; thu hoạch, chăm vật nuôi, chế biến và bán hàng cho XP. Nâng cấp **cuốc, bình tưới, liềm** từ đồng lên bạc rồi vàng bằng bảng N: số ô tác động cùng lúc là 1, 3, 5. Bốn vùng ruộng, mỗi vùng 20 ô, mở lần lượt bằng xu khi đạt cấp 1, 2, 4, 6; vùng đầu mở sẵn. Giá mở vùng sau tăng dần: 500, 1.200, 2.400 xu.

Gà, bò, cừu và heo có độ no, độ vui. Một ngày trong game dài **10 phút**; độ no và vui giảm theo thời gian. Ngắm con vật và nhấn F để dùng một thức ăn. Vật nuôi cần đủ no và vui mới tạo sản phẩm. Gà đẻ trứng theo chu kỳ tại ổ, mỗi chuồng gà tối đa 5 con. Bò cho sữa mỗi 45 giây, cừu cho len mỗi 60 giây. Heo cho thịt một lần. Bảng P bán 10 thức ăn giá 50 xu, xây thêm chuồng bò/cừu và nâng cấp chuồng. Nâng cấp chuồng bò, cừu, heo tăng sức chứa; nâng chuồng gà rút ngắn chu kỳ đẻ, vẫn giữ tối đa 5 con. Shop B bán chuồng gà thứ hai.

Nhấn M để mở xưởng, hoặc đến bốn máy gần quầy shop và nhấn E. Công thức nằm trong `Assets/StreamingAssets/recipes.json`; mỗi máy có hàng đợi tối đa 5 lượt, nguyên liệu được trừ khi xếp việc và thành phẩm vào túi khi hết thời gian. Giá bán mỗi thành phẩm cao hơn tổng giá nguyên liệu:

| Máy | Công thức | Chờ | Giá bán |
|---|---|---:|---:|
| Cối xay | 3 lúa mì → 2 bột mì | 25 giây | 44 xu |
| Lò bánh | 2 bột mì → 1 bánh mì | 35 giây | 65 xu |
| Thùng ủ | 2 sữa → 1 phô mai | 40 giây | 65 xu |
| Máy ép | 3 táo → 2 nước táo | 30 giây | 70 xu |
| Xưởng cưa | 2 gỗ → 1 ván | 45 giây | 34 xu |
| Lò nung | 2 quặng → 1 kim loại | 55 giây | 50 xu |

## Ngày, mùa, bão và các đảo

Đồng hồ trên HUD hiển thị năm, ngày, giờ, mùa và thời tiết. Mỗi mùa dài 28 ngày; lá và cỏ đổi màu theo mùa. Trời mưa hoặc bão tự tưới ruộng. Khi có bão, trả lời câu hỏi gia cố trong 25 giây: đúng thì không thiệt hại, sai hoặc hết giờ sẽ mất 30–80% nông sản và máy bị chậm. Từ 18:00 đến trước 06:00, vào nhà và nhấn E ở giường để ngủ đến sáng.

Nhấn Tab hoặc dùng cổng để đi giữa **Đảo Nông Trại**, **Đảo Trung Tâm**, **Đảo Thần Bí** và **Đảo Công Nghiệp**. Đảo Trung Tâm có NPC để trò chuyện hai ngày rồi kết bạn, chợ đấu giá nông sản với người mua NPC và trò chơi ba rương. Đảo Thần Bí mở ở LV3, có mê cung, bẫy và di tích; đạt LV5 rồi giải câu đố tại di tích để mở giới hạn LV10 và lấy bản vẽ hiếm. Đảo Công Nghiệp mở ở LV4, có gỗ, quặng, xưởng cưa và lò nung; luyện kim cần bản vẽ từ di tích. Các hoạt động xã hội và đấu giá hiện hoạt động **chơi đơn với NPC**.

Trong menu Esc, chỉ nút **Lưu game** ghi bản lưu. Thoát game không tự lưu. Mở lại game tải bản lưu thủ công gần nhất. Bản lưu cũ phiên bản 2 vẫn được đọc; bản mới lưu thêm cấp độ, vùng đất, dụng cụ, thức ăn, độ no/vui, chuồng nâng cấp, hàng đợi chế biến, âm lượng, ngày/giờ/thời tiết và tiến độ đảo. Đường dẫn lưu trên Windows: `%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest\farm-manual-save.json`.

## Build lại

Đóng Unity Editor đang mở cùng dự án, rồi chạy:

```powershell
& 'D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\GAME_NongTrai' -executeMethod NongTrai.Editor.FarmProjectBuilder.BuildWindows -logFile 'D:\GAME_NongTrai\Logs\build.log'
```

Lệnh này dựng lại scene và bản Windows. Nếu đã sửa scene trong Editor, hãy giữ bản sao trước khi chạy.
