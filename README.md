# Nông Trại — First Harvest

**Đã cập nhật vòng chơi canh tác:** xem [CHOI_GAME.md](CHOI_GAME.md) để cày, gieo, tưới và thu hoạch. Các mô tả Giai đoạn 1 bên dưới là tài liệu nền tảng; giới hạn “cây chỉ minh họa” đã được thay bằng 80 ô đất tương tác trong bản mới. Chưa có lưu game.

Giai đoạn 1 của lộ trình trong `prompt.MD`: dự án Unity PC với địa hình mẫu, nhân vật đi bộ và camera. Ảnh `ChatGPT Image Sep 20, 2026, 07_13_45 PM.png` là tham chiếu bố cục, màu sắc và phong cách nông trại; không phải bộ model 3D có thể nhập trực tiếp. Toàn bộ hình học hiện tại là primitive Unity, không cần mua asset.

## Mở và chơi

1. Mở Unity Hub → **Add / Add project from disk** → chọn `D:\GAME_NongTrai`.
2. Chọn Unity **6000.3.22f1** đã cài tại `D:\Unity\Editors\6000.3.22f1`.
3. Chờ Package Manager và biên dịch hoàn tất. Dự án dùng URP 17.3.0, Input System 1.20.0, Cinemachine 3.1.5 và uGUI 2.0.0, phiên bản được cố định trong `Packages/manifest.json`.
4. Mở `Assets/Farm/Scenes/Farm.unity`, nhấn **Play**, bấm vào cửa sổ Game.
5. Nếu muốn dựng lại scene gốc: menu **Nong Trai → Create Milestone 1 Scene**. Lưu bản sao scene nếu đã chỉnh sửa vì thao tác này sẽ ghi lại scene Farm.
6. Bản Windows sau khi build nằm ở `Builds/Windows/NongTrai.exe`. Giữ nguyên toàn bộ thư mục Windows khi sao chép sang máy khác.

| Phím | Chức năng |
|---|---|
| WASD | Đi bộ theo hướng camera |
| Shift trái | Chạy |
| Space | Nhảy khi đứng trên đất |
| Chuột | Xoay camera |
| V | Chuyển ngôi thứ nhất / thứ ba |
| E | Đọc bảng gỗ khi ngắm trúng và đứng trong phạm vi 3 m |
| Esc | Tạm dừng, thả chuột; nhấn lại để tiếp tục |

## Cấu trúc và thiết lập Editor

Scene đã có sẵn các GameObject và reference; không cần kéo thả thủ công. `FarmProjectBuilder.cs` là công thức tạo scene có thể đọc và sửa.

| File/thư mục | Vai trò |
|---|---|
| `Assets/Farm/Scripts/Player/PlayerSettings.cs` | ScriptableObject tốc độ, nhảy, chuột, camera và tầm tương tác |
| `Assets/Farm/Scripts/Player/FarmInput.cs` | Action map của New Input System, vòng đời enable/disable/dispose |
| `Assets/Farm/Scripts/Player/FarmPlayer.cs` | CharacterController, trọng lực, chạy/nhảy, tạm dừng, phục hồi vị trí |
| `Assets/Farm/Scripts/Player/FarmCamera.cs` | Góc nhìn chuột, đổi camera và sphere cast tránh xuyên tường |
| `Assets/Farm/Scripts/Interaction/FarmSign.cs` | Đối tượng tương tác mẫu, sự kiện C# |
| `Assets/Farm/Scripts/Interaction/PlayerInteraction.cs` | Raycast theo tâm màn hình, giới hạn khoảng cách, phát sự kiện |
| `Assets/Farm/Scripts/UI/FarmHud.cs` | HUD, thông báo, menu tạm dừng qua uGUI |
| `Assets/Farm/Scripts/Core/BuildSmokeCheck.cs` | Kiểm tra bản build khi truyền cờ `-farmSmokeCheck` |
| `Assets/Farm/Editor/FarmProjectBuilder.cs` | Tạo URP, scene, asset cấu hình, thiết lập dự án và build Windows |
| `Assets/Farm/Scenes/Farm.unity` | Scene chơi chính |
| `Assets/Farm/Data/PlayerSettings.asset` | Thông số nhân vật có thể chỉnh trong Inspector |
| `Assets/Farm/Settings/` | URP pipeline và renderer |
| `Assets/Farm/Materials/` | Vật liệu placeholder có GPU instancing |

Để tự thiết lập hoặc thay model:

1. **Player** dùng layer `Player` (8), có CharacterController cao 1.85 m, radius 0.32 m, center Y = 0.925; FarmInput, FarmPlayer và PlayerInteraction cùng nằm trên object này.
2. FarmPlayer tham chiếu asset `Data/PlayerSettings.asset`, child `Visual`, và FarmCamera trên `Farm Cinemachine Camera`.
3. **Main Camera** mang tag `MainCamera`, có Camera, AudioListener, UniversalAdditionalCameraData và CinemachineBrain. Brain cập nhật LateUpdate.
4. **Farm Cinemachine Camera** có CinemachineCamera và FarmCamera. Camera pose được FarmCamera tính rồi CinemachineBrain đưa ra Main Camera. Obstacle Mask = Default (1), loại trừ layer Player.
5. PlayerInteraction tham chiếu Player và Main Camera. Đối tượng tương tác cần collider trên layer Default và FarmSign trên collider hoặc cha.
6. **Farm HUD** dùng Screen Space Overlay, CanvasScaler 1600 × 900. EventSystem dùng InputSystemUIInputModule. Nút menu đã lưu listener trong scene.
7. Đổi model nhân vật trong child **Visual**, giữ root, component và reference. Primitive nhân vật chưa có rig hay animation bước đi.
8. Nhà kho, silo, cây, hàng rào, mặt ao và luống cây nằm trong **Environment**. Thay chúng bằng model miễn phí/có quyền sử dụng phù hợp, giữ collider cho vật cản. Ao hiện chỉ là mặt minh họa có nền đất bên dưới.
9. Trong **Project Settings → Graphics / Quality**, dùng `FarmURP`; **Player → Active Input Handling** là `Input System Package (New)`. Layer Player được công cụ thiết lập tự động.

## Kiểm tra thủ công

- Bấm Play: nhìn thấy nông trại, HUD, nhân vật đội mũ; không có vật liệu hồng hoặc lỗi Console.
- Đi/chạy/nhảy; kiểm tra không nhảy liên tục trên không, không xuyên nhà/hàng rào và không thoát biên bản đồ.
- Đi sát vật cản, xoay camera: camera thứ ba thu khoảng cách khi gặp collider.
- Nhấn V hai lần: ngôi thứ nhất ẩn model, ngôi thứ ba hiện lại model.
- Đến bảng gỗ ở gần `(2.6, 0, 7)`, ngắm vào mặt bảng: xuất hiện gợi ý E. Nhấn E để đọc; ra xa hơn 3 m không còn tương tác được.
- Nhấn Esc: di chuyển và nhìn dừng, chuột được thả; nút Tiếp tục và phím Esc phục hồi điều khiển. Chuyển sang ứng dụng khác cũng tạm dừng.
- Kiểm tra trong build Windows, bao gồm nút Thoát game (Application.Quit không thoát Editor).

## Build lại bằng PowerShell

Đóng Editor đang mở cùng dự án trước khi chạy:

```powershell
& 'D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\GAME_NongTrai' -executeMethod NongTrai.Editor.FarmProjectBuilder.BuildWindows -logFile 'D:\GAME_NongTrai\Logs\build.log'
```

Lệnh này dựng lại scene mặc định rồi build. Để build scene đã chỉnh sửa, dùng **File → Build Profiles → Windows → Build**, chọn Farm trong Scene List.

## Phạm vi

Đây là nền tảng chơi được của giai đoạn 1, chưa phải game mô phỏng hoàn chỉnh. Cây chỉ để minh họa, mặt ao chưa có vật lý nước, nhân vật chưa có animation; không hiển thị tiền/thời tiết giả. Các manager thời gian, thời tiết, đất, cây, kinh tế và lưu game sẽ bổ sung đúng từng giai đoạn trong prompt. Bước tiếp theo là **Giai đoạn 2: TimeManager + WeatherManager**.

Thiết kế camera có tham khảo tài liệu chính thức: https://docs.unity.cn/Packages/com.unity.cinemachine@3.1/manual/setup-follow-camera.html
