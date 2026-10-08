# Tìm số 2D — TriForge

Minigame do người dùng cung cấp qua Cau1_2D.unitypackage. Giữ lưới 7 × 7, 49 số không trùng (0–99), 5 câu, 5 giây/câu và bốn màu của bản gốc.

## Chơi riêng

Unity 6000.3.22f1, uGUI và Input System. Mở Assets/Scenes/Cau1_NumberMemory.unity rồi Play. Mỗi câu đúng nhận 20 xu; đúng 5/5 thêm 100 xu và 1 đá nâng cấp. Chơi riêng dùng ví trong phiên, không ghi PlayerPrefs hay giả lập inventory nông trại.

## Trong nông trại

Tab → Tìm số 2D. FarmNumberMemory cấp xu thật và đá item68. Tối đa 3 lượt hoàn thành có điểm được nhận xu/ngày game; chỉ lượt 5/5 đầu tiên nhận đá. Hết quota vẫn luyện tập được; 0/5 hoặc thoát giữa lượt không mất quota. Sáng tạo không nhận thưởng. Esc/nút góc phải trở về nông trại. Túi đầy thì đá rơi cạnh nhân vật.

NumberMemoryGame không tham chiếu mã Farm; delegate RewardProvider nối phần thưởng, không cần sửa assembly để chơi riêng. Resources/NumberMemory/NumberMemoryUI.prefab chỉ chứa UI, dùng EventSystem hiện có của Farm.

## Công cụ

Tools → Midterm → Build Question 1 chỉ dựng lại scene/prefab 2D sinh tự động. Nó giữ scene đang mở, ProductName và Build Settings; hãy đóng scene 2D trước khi dựng lại. Không chạy FarmProjectBuilder.CreateScene.

Tools → Midterm → Export Question 1 package xuất DongGoi/Cau1_2D.unitypackage, hoặc đường dẫn NUMBER_MEMORY_PACKAGE_OUTPUT. Gói chỉ chứa Assets/Exam2D và scene 2D; không chứa bản sao Packages, cache hoặc tài liệu nộp bài ở đường dẫn máy khác.

Copyright (c) TriForge. Giấy phép của package Unity phụ thuộc giữ nguyên trong Packages dự án; gói 2D không phân phối lại source các package đó.
