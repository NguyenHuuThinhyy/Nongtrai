Bạn là một Senior Unity Developer và Game Designer chuyên về game mô phỏng nông nghiệp (farming simulation). Hãy giúp tôi xây dựng một game nông trại 3D mô phỏng thực tế trên Unity, chạy trên PC.

## 1. TỔNG QUAN DỰ ÁN
- Thể loại: Mô phỏng nông nghiệp thực tế (kiểu Farming Simulator, nhưng quy mô nhỏ hơn, tập trung vào hệ thống cây trồng và kinh tế).
- Góc nhìn: Ngôi thứ nhất hoặc thứ ba (có thể chuyển đổi), người chơi đi bộ và lái được máy móc.
- Nền tảng: PC (Windows), điều khiển bằng bàn phím + chuột (hỗ trợ tay cầm là tùy chọn).
- Đồ họa: 3D realistic ở mức vừa phải, ưu tiên hiệu năng ổn định.

## 2. CÔNG NGHỆ
- Unity 2022.3 LTS (hoặc Unity 6), ngôn ngữ C#.
- Render pipeline: URP.
- Input System mới (New Input System), Cinemachine cho camera.
- Dữ liệu cấu hình dùng ScriptableObject (loại cây, vật nuôi, máy móc, vật phẩm).
- UI: Unity UI (uGUI) hoặc UI Toolkit.
- Chỉ dùng asset miễn phí hoặc primitive/placeholder; ghi rõ chỗ nào tôi cần thay bằng model thật.

## 3. HỆ THỐNG CỐT LÕI (làm theo thứ tự ưu tiên)

**3.1 Player & Camera**
- Di chuyển, chạy, nhảy nhẹ, tương tác bằng phím E.
- Lên/xuống phương tiện (máy kéo).

**3.2 Thời gian, thời tiết, mùa**
- Chu kỳ ngày/đêm (tốc độ chỉnh được), 4 mùa, mỗi mùa có nhiệt độ và lượng mưa khác nhau.
- Thời tiết: nắng, mưa, khô hạn, sương giá. Ảnh hưởng trực tiếp đến cây trồng.

**3.3 Đất và canh tác thực tế**
- Chia đất thành lưới ô (grid) hoặc field, mỗi ô lưu: độ ẩm, dinh dưỡng (N-P-K), độ pH, trạng thái (chưa cày / đã cày / đã gieo).
- Quy trình: cày đất → gieo hạt → tưới/bón phân → chăm sóc → thu hoạch.
- Luân canh: trồng liên tục một loại cây sẽ làm đất kiệt dinh dưỡng, giảm năng suất.

**3.4 Cây trồng**
- Bắt đầu với 4-5 loại: lúa mì, ngô, khoai tây, cà chua, đậu nành.
- Mỗi cây có: mùa vụ phù hợp, số giai đoạn sinh trưởng, nhu cầu nước/dinh dưỡng, năng suất, giá bán cơ bản.
- Sâu bệnh và cỏ dại xuất hiện ngẫu nhiên, cần xử lý bằng thuốc hoặc biện pháp phù hợp.
- Năng suất cuối cùng phụ thuộc vào chất lượng chăm sóc, thời tiết và đất.

**3.5 Máy móc & dụng cụ**
- Máy kéo (tractor) điều khiển được, có vật lý cơ bản (WheelCollider).
- Gắn/tháo nông cụ: máy cày, máy gieo hạt, máy phun/bón, máy gặt.
- Nhiên liệu tiêu hao, chi phí bảo trì.

**3.6 Kinh tế**
- Tiền, cửa hàng mua hạt giống/phân bón/thuốc/nhiên liệu, chợ bán nông sản.
- Giá thị trường biến động theo mùa và cung cầu.
- Mua thêm đất, mua/nâng cấp máy móc, có thể vay vốn.

**3.7 Kho & Inventory**
- Túi đồ người chơi, kho chứa nông sản (silo/kho), giới hạn dung lượng.

**3.8 Lưu/Tải game**
- Save/Load dạng JSON: tiền, thời gian, trạng thái từng ô đất, vật phẩm, máy móc.

**3.9 UI/UX**
- HUD: tiền, ngày/mùa, thời tiết, nhiên liệu, công cụ đang dùng.
- Menu: chính, tạm dừng, cửa hàng, inventory, thông tin ô đất khi nhìn vào.

## 4. YÊU CẦU KIẾN TRÚC CODE
- Tách hệ thống rõ ràng (TimeManager, WeatherManager, FieldManager, CropSystem, EconomyManager, SaveSystem...).
- Giao tiếp giữa các hệ thống bằng event (C# events/UnityEvent), tránh phụ thuộc chéo.
- Dữ liệu tách khỏi logic bằng ScriptableObject.
- Code sạch, có comment tiếng Việt cho các đoạn quan trọng, đặt tên nhất quán, dễ mở rộng.
- Tối ưu: không dùng Update() cho việc không cần thiết, cập nhật ô đất theo tick (theo giờ game) thay vì mỗi frame; cân nhắc object pooling và GPU instancing cho cây trồng.

## 5. CÁCH LÀM VIỆC (rất quan trọng)
- Chia dự án thành các giai đoạn (milestone) nhỏ, mỗi lượt chỉ làm MỘT giai đoạn.
- Với mỗi giai đoạn, hãy đưa:
  1. Mục tiêu của giai đoạn đó.
  2. Danh sách file/script cần tạo (tên file + đường dẫn thư mục).
  3. Code C# đầy đủ, chạy được, không viết tắt hay bỏ dở.
  4. Hướng dẫn thao tác trong Unity Editor từng bước (tạo GameObject, gắn component, thiết lập Layer/Tag, kéo thả reference...).
  5. Cách kiểm tra để biết giai đoạn đã chạy đúng.
- Nếu có chỗ chưa rõ hoặc cần tôi quyết định, hãy hỏi trước khi viết code.
- Khi tôi báo lỗi, hãy yêu cầu tôi gửi nội dung lỗi trong Console và phân tích nguyên nhân trước khi sửa.

## 6. LỘ TRÌNH ĐỀ XUẤT
- Giai đoạn 1: Tạo project, cấu trúc thư mục, Player controller + camera, địa hình đơn giản.
- Giai đoạn 2: TimeManager (ngày/đêm/mùa) + WeatherManager.
- Giai đoạn 3: Hệ thống ô đất + cày đất + gieo hạt.
- Giai đoạn 4: Sinh trưởng cây trồng, tưới nước, dinh dưỡng, thu hoạch.
- Giai đoạn 5: Inventory + cửa hàng + kinh tế.
- Giai đoạn 6: Máy kéo và nông cụ.
- Giai đoạn 7: Sâu bệnh, luân canh, giá thị trường biến động.
- Giai đoạn 8: Save/Load, UI hoàn chỉnh, tối ưu hiệu năng.

Hãy bắt đầu bằng việc xác nhận bạn đã hiểu yêu cầu, tóm tắt lại ngắn gọn kế hoạch, rồi bắt tay vào Giai đoạn 1.