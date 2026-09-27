# Nông Trại – First Harvest

Game nông trại Unity cho Windows. Trồng cây bằng chuột trái, chăm vật nuôi, chế biến/chế tạo theo JSON, giao đơn tại hộp thư, mở đất, nâng cấp dụng cụ và khám phá địa hình khối sinh liên tục theo seed. Có túi đồ 36 ô dùng chung hotbar 9 ô, minh họa vật phẩm, bản đồ nông trại 2D và thông báo vật nuôi đói. Cả hai map nhớ vị trí khi chuyển qua lại. Khi chết: trả 100 xu để hồi sinh tại chỗ, giữ đồ; lựa chọn miễn phí về cổng và rơi tối đa 3 món. Một ngày game dài 18 phút, có bốn mùa, thời tiết, ngủ qua đêm và câu đố ứng phó bão. Menu chính có chế độ sáng tạo LV99 để bay và kiểm thử mà không sửa bản lưu chơi thường.

**Rương mới:** chuột phải mở thẳng bảng đồ; bấm từng món, Shift + click cả chồng hoặc **Lấy tất cả**. Giữ trái để đập vỡ và thả phần đồ còn lại. Không có câu hỏi; vẫn cần hạ quái canh.

**CombatComfort:** hồi sinh tại chỗ giá 100 xu; bọt biển đầy hong 10 giây ở đống lửa; kiếm có vòng ngắm rỗng với hỗ trợ đánh hơi lệch tâm, giữ tầm đánh và chặn bởi tường.

**Gói mới:** `DongGoi/NongTrai-Windows-ChestLoot-20260927.zip` và `DongGoi/NongTrai-Unity-ChestLoot-20260927.zip`. Báo cáo: [BAO_CAO_CHEST_LOOT.md](BAO_CAO_CHEST_LOOT.md). Gói trước được giữ dự phòng trong Recovery; runtime đang dùng là Windows-ChestLoot.

**Chơi ngay:** mở `D:\GAME_NongTrai\Builds\Windows-ChestLoot\NongTrai.exe`; giữ nguyên cả thư mục `Windows-ChestLoot` khi sao chép sang máy khác. Không cần cài Unity để chạy bản Windows. Xem [CHOI_GAME.md](CHOI_GAME.md) để biết điều khiển và vòng chơi.

**Bản Systems:** một xô nước dùng **chuột trái**: rỗng → múc hồ/sông, đầy → đặt nước → rỗng. Shop bán bọt biển hút nước trong 1 ô xung quanh rồi đầy. Hạt hết tự biến mất; bán được tất cả đồ trong túi. Thuê vòi tự tưới 350 xu/ngày, tối đa 3/ngày (LV3/5/7 mở 4/5/6), không nạp nước. Rương boss có bí pha lê và dâu hoàng kim. Quái nhảy qua khối cao 1 ô, thanh máu có số rõ hơn. © HThinh.yy ở góc màn hình.

**Mở dự án:** Unity Hub → Add project from disk → `D:\GAME_NongTrai`, dùng Unity 6000.3.22f1. Scene chính là `Assets/Farm/Scenes/Farm.unity`. Dự án dùng URP, Input System, Cinemachine, uGUI, TextMeshPro và Animation. `Assets/Farm/Editor/FarmProjectBuilder.cs` dựng scene qua `CreateScene`; lệnh `BuildWindows` build scene hiện tại mà không dựng lại. Tạo scene mới sẽ ghi đè `Farm.unity` và các prefab động vật/cây.

**Dữ liệu JSON:** `Assets/StreamingAssets/recipes.json` chứa 11 công thức máy; `Assets/StreamingAssets/crafting.json` chứa 25 công thức ghép tức thì, gồm cung gỗ, 5 mũi tên, bàn rèn và cổng hồi sinh. Mã hàng trong `FarmInventory.cs` từ 0 đến 77. Bản lưu v21 giữ TNT đã đặt/ngòi đang cháy, chỉ số/LV riêng từng vũ khí (kể cả đồ rơi), vé/kỷ lục Runner, nguồn nước đổ, quái canh rương, nhiên liệu máy và hai boss; đọc bản v2–v20.

**Lưu game:** nút Lưu game trong menu Esc ghi bản lưu. “Chơi lại từ đầu” có xác nhận, cất bản lưu cũ thành `.before-new-game-<thời gian>` và bắt đầu LV1 với map mới; không lưu phiên hiện tại. Đường dẫn trên Windows là `%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest\farm-manual-save.json`. Game tự tải bản lưu này khi khởi động; thoát không tự lưu. Chế độ sáng tạo dùng bản sao trong bộ nhớ, khóa nút lưu và bỏ toàn bộ thay đổi khi về menu hoặc thoát.

Nhân vật hiện dùng Kenney Mini Characters biến thể `character-male-e.fbx` màu da sáng, cao 1,90 m, thêm mũ rơm và chuyển động tay/chân rõ hơn khi đi. Model có clip đi/chạy/nhảy/làm việc/đánh. Bò/heo/cừu dùng Quaternius FBX CC0; gà dùng OBJ CC0. Cây tán/cây ăn quả, bụi việt quất, lúa mì và bí ngô dùng Kenney Nature Kit CC0. Di chuyển của nhân vật tăng/giảm tốc mượt hơn, thú được nội suy khi đi và đứng yên khi tạm dừng. Người chơi mất máu khi rơi quá cao; cáo/rắn/sói lùi ra sau khi cắn. Xem [ASSET_SOURCES.md](Assets/Farm/Models/ASSET_SOURCES.md) để biết nguồn và giấy phép. Nhà/chuồng, máy móc, sói/cáo/rắn và một số cây trồng còn dùng visual dựng trong Unity; voxel vẫn giữ để đào/đặt/phá.

Để build bản Windows đang dùng, đóng Editor đang mở cùng dự án rồi chạy. Lệnh này dùng scene hiện tại và không gọi builder dựng lại:

```powershell
$env:FARM_BUILD_OUTPUT='Builds/Windows-ChestLoot'
& 'D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\GAME_NongTrai' -executeMethod NongTrai.Editor.FarmProjectBuilder.BuildWindowsCurrentScene -logFile 'D:\GAME_NongTrai\Logs\build.log'
```

Ảnh `ChatGPT Image Sep 20, 2026, 07_13_45 PM.png` là tham chiếu bố cục và màu sắc. Một phần visual vẫn được dựng bằng primitive Unity; xem bảng nguồn model để biết các nhóm đã thay và còn lại.

## Sửa TNT — 27/09/2026

Cầm TNT, ngắm mặt đất/khối trong tầm 6 m ở Khám phá, bấm trái hoặc phải để đặt. TNT chưa châm không tự nổ. Đổi sang đuốc, ngắm TNT và bấm trái/phải để châm; đuốc không bị tiêu hao. TNT nhấp nháy **5 lần** (mỗi lần sáng 0,5 giây rồi tắt 0,5 giây), sau đó nổ. Khi khựng hình vẫn hiện đủ các nhịp; pause hoặc về nông trại tạm dừng ngòi. Bản lưu giữ TNT và tiến độ ngòi. Báo cáo: [BAO_CAO_SYSTEMS.md](BAO_CAO_SYSTEMS.md).

## Cập nhật AdventureFeedback — 27/09/2026

- Đi thường 6 m/s, Shift 9 m/s; khi chạy nhanh thực sự, hao độ no gấp 4 (0,12 thay vì 0,03 điểm/giây). Camera ngắm thẳng xuống, đào dưới chân và nhảy đặt khối sát chân; kiểm tra va chạm thay cho bán kính cấm.
- Múc nước bằng **bình rỗng**, một lần bấm phải = một bình. Bình đầy đặt nguồn nước và trả vỏ rỗng; bình tưới có thể múc trực tiếp. Nước tự nhiên lan vào bờ đào trống; có hạt nước và lực nhảy khỏi nước.
- Động tác cuốc/đánh/đặt và vệt dụng cụ; chém mặc định hồi 0,3 giây. Quái không cắn xuyên vách/từ tầng khác.
- Rèn từng chiếc kiếm/rìu/cung; đổi chỉ số tốn 1 đá + 80 xu. Kiếm LV5 thêm 12 sát thương và hút 3 HP/đòn trúng. Bán/bỏ vũ khí tại túi đồ.
- Rương tự sinh giảm tần suất từ 1/9 xuống 1/24 chunk, tối đa 2 rương hoạt động gần người chơi. Sói/rắn/gấu/golem canh bốn bậc, bậc cao thưởng tốt hơn; hạ quái rồi mở lấy đồ hoặc đập rương.
- Map mới dùng generator 5, có hồ và nhánh sông theo seed trong vùng hoang dã. Save cũ giữ generator cũ để bảo toàn địa hình đã chơi. Chọn **Chơi lại từ đầu** để tạo map mới.
- Đổi map vẫn nhớ vị trí; chỉ chết mới về cổng hồi sinh.

## Lịch sử: bản Polish (các thông số bên dưới đã được thay bởi bản mới)


- Mũ bám xương đầu; giữ tóc gốc của model; điểm cầm nằm ở lòng bàn tay, cập nhật sau animation. CharacterController điều khiển độ cao nhảy, bỏ dịch chuyển thừa của xương gốc. Theo đính chính mới nhất: đi 4 m/s, chạy Shift tăng từ 7 lên 9 m/s, gia tốc 28 và hãm 34 m/s².
- Runner có 5 mức khó (0/200/500/900/1400 m), tốc độ tăng 10–26 m/s; làn an toàn đổi giữa các hàng, xe rơm tràn ra từ lề sau 500 m. Pool 6 đoạn; chia bước va chạm, camera ít nảy, HUD 10 Hz. Xe hoàn tất đổi vị trí trước người chơi ít nhất 26 m; luôn có một làn trống mỗi hàng.
- Esc → **Chơi game lại** mở xác nhận: lưu rồi chơi lại / tải bản đã lưu và bỏ phần chưa lưu / hủy. Không tự mất LV khi mới bấm nút. Bản lưu v18 giữ LV/XP; đọc save cũ. Chưa lưu rồi chọn tải lại vẫn mất tiến độ chưa lưu, được ghi rõ trên nút.
- Bàn rèn có 36 ô túi, ô vũ khí và ô đá. Bấm hoặc kéo thả; chỉ tiêu hao khi rèn. Đóng bảng không mất món đang chọn.
- Boss 2.400 HP; đòn thường 38, dưới nửa máu 52 sát thương, đuổi nhanh hơn; báo hiệu trước đòn đập 0,7/0,5 giây. Quặng/than trong tầng đá giảm khoảng 70% (than 1/61, quặng 1/83 cơ hội mỗi ô đủ điều kiện).
- Sông phía tây quanh (-36,0), biển phía nam quanh (0,-75), tính theo ô khám phá trên HUD. Nhân vật nổi/bơi cơ bản; Space để ngoi lên, không có hệ thống oxy. Nguồn nước đổ vẫn theo cơ chế voxel.
- **Cổng hồi sinh**: bàn chế tạo → 8 khối đá + 3 kim loại + 1 đá nâng cấp. Đặt tại Khám phá (G hoặc hotbar), tối đa 1 cổng tự đặt; cổng gốc độc lập. Chuột phải vào trụ cổng về trại; Tab đổi map vẫn trở về vị trí đứng cuối cùng. Chỉ khi chết ở Khám phá mới hồi sinh tại cổng. Giữ trái phá và nhặt cổng để đặt lại. Khi hồi sinh: không có cổng hoặc cả hai lối ra bị chặn → cổng gốc. Vị trí cổng lưu cùng công trình khi Lưu game.
- Generator 4 thêm sông/biển ngoài vùng khởi đầu. Giữ ô đã đào/đặt; bờ sông/biển thay đổi địa hình tự nhiên ở vùng mới. Không xóa save để cập nhật.

## Bản cập nhật Farm Runner (27/09/2026)

- **Tab → Farm Runner:** chuyển sang map 3D riêng toàn màn hình. A/D đổi làn, W/Space nhảy, S trượt 0,7 giây, Esc kết thúc. Tốc độ 10–26 m/s; 6 đoạn 40 m tái sử dụng, mỗi hàng chướng ngại chừa ít nhất một làn. Một va chạm kết thúc lượt.
- 1 vé/lượt; khởi đầu 3 vé. Giao một đơn hộp thư nhận 1 vé, mua vé 500 xu. Hồi 1 vé/2 giờ thực, tối đa 3 vé hồi; vé thưởng/mua có thể vượt 3. Ví nông trại dùng chung. Mỗi km thưởng thêm 50 xu; tiền khoảng cách tăng 50 xu/km ở các km tiếp theo. Mốc 1: đá nâng cấp, 2: TNT, 3: bình máu, các mốc tiếp theo đá/bình máu. Tiến độ chỉ ghi khi chọn Lưu game.
- **Ăn/uống:** cầm thực phẩm hoặc bình máu rồi giữ trái đủ 3 giây. Thả tay/đổi món hủy tiến độ; mỗi lần giữ chỉ dùng một món. Bình máu hồi 50 HP, thịt sống hồi 10 no, thịt nướng hồi 40 no. Chuột phải vào lửa để nướng thịt.
- **Cung:** thanh lực màu, giữ trái kéo/thả bắn. Tên ghim vào vật thể/quái, đi đến gần nhặt lại; tên nằm lại tối đa 3 phút trong phiên chơi. Cấp rèn cung tăng sát thương.
- **Bàn rèn:** chế tạo/đặt rồi chuột phải mở (trái cũng mở khi không cầm rìu). Bấm hoặc kéo kiếm/cung/rìu và đá từ lưới túi đồ vào hai ô; LV0–5, cần 1–5 đá, tỉ lệ 100/83/66/49/32%. Thất bại mất đá/xu, không tụt cấp. Đá có từ Runner, boss hoặc chế tạo.
- **Map khám phá:** hồ tại ô (56,18), tế đàn phẳng tại (88,24), làng tại (24,76), hố sâu tại (-18,35); tọa độ hiện dưới HUD. Golem mặt đất chủ động đánh trong phạm vi tế đàn; boss hang vẫn ở (72,72). Bản lưu cũ lên generator 4, giữ các ô đã đào/đặt; địa hình vùng địa danh thay đổi theo bản mới.
- Cầm bình nước, chuột phải vào đất/hố trong map khám phá để đổ. Nước rơi xuống rồi lan ngang tối đa 7 ô; đặt khối chiếm ô sẽ đẩy nước khỏi ô đó. Nguồn được lưu, dòng chảy dựng lại. Ở nông trại bình này bổ sung 8 nước bình tưới.
- Than xuất hiện trong tầng đá. Lò bánh/lò nung dùng gỗ (30 giây) hoặc than (120 giây), nạp trong UI máy hoặc tự lấy nhiên liệu khi có việc. Máy khác giữ cách chạy cũ. TNT: cầm TNT bấm trái/phải đặt trong Khám phá; cầm đuốc bấm vào TNT để châm. Nhấp nháy đủ 5 lần mới nổ, đào phạm vi nhỏ và có thể gây sát thương người chơi.
- Rương đặt lưu đồ độc lập. Rương khám phá mở trực tiếp, giữ trái đập vỡ sẽ rơi đồ; không còn câu hỏi.

Bản demo Runner dùng nông dân hiện có; chưa có chọn giới tính, cửa hàng skin hoặc power-up cánh/nhảy đôi. Nhà làng, chướng ngại và Golem dùng mesh đơn giản dựng trong Unity. Nước là mô phỏng ô có giới hạn, chưa có bơi/dòng chảy đẩy vật thể. Không tải thêm asset ngoài các gói CC0 đã ghi nguồn.

Nhân vật e được phối trang phục làm việc xanh/kem bằng mesh/material URP, giữ texture mặt gốc; dùng chung ở nông trại và Runner. Cây ven đường Runner và cây gỗ phục hồi dùng prefab Kenney Nature Kit CC0.

Báo cáo kiểm tra, đối chiếu yêu cầu và phần chưa hoàn thiện: [BAO_CAO_FARM_RUNNER.md](BAO_CAO_FARM_RUNNER.md). Gói Windows/Unity mới nằm trong `DongGoi`, có hậu tố `FarmRunner-20260927`.


## Thành viên khác lấy mã nguồn

```powershell
git clone https://github.com/NguyenHuuThinhyy/Nongtrai.git
cd Nongtrai
git pull --ff-only origin main
```

Unity Hub → Add project from disk → **thư mục vừa clone** (thư mục chứa Assets/Packages/ProjectSettings). Dùng Unity 6000.3.22f1, mở Assets/Farm/Scenes/Farm.unity. Chờ Unity tự tạo Library và tải Packages. Không chạy CreateScene hoặc RebuildSceneAndBuildWindows vì sẽ ghi đè scene/prefab đã nâng cấp.

Build bằng `NongTrai.Editor.FarmProjectBuilder.BuildWindowsCurrentScene`, biến `FARM_BUILD_OUTPUT=Builds/Windows-ChestLoot`; dùng đường dẫn Unity.exe trên máy của bạn và `-projectPath` trỏ vào thư mục clone. Git chứa source, asset/model cùng giấy phép, cấu hình, meta, tài liệu và smoke checks; Library/Temp/Logs/Builds/DongGoi/Recovery không đưa lên Git. Bản ZIP Windows/Unity được đóng gói riêng tại máy bàn giao.

Xem CODEX_HANDOFF.md trước khi sửa; chạy game build với `-farmSmokeCheck -farmArtCheck` để kiểm tra toàn bộ, hoặc thêm `-farmSystemsOnly` để kiểm tra riêng các hệ thống mới. Kiểm thử dùng save tạm. Quyền sở hữu mã/thiết kế gốc: [COPYRIGHT.md](COPYRIGHT.md); giữ giấy phép asset của bên thứ ba.

## File cần giữ khi làm việc nhóm

- Git chứa toàn bộ `Assets` (kèm `.meta` và giấy phép), `Packages`, `ProjectSettings` và tài liệu. Các file này đủ để Unity mở dự án sau khi clone.
- `Library`, `Temp`, `Obj`, `Logs`, cấu hình IDE cá nhân và dữ liệu debug do Unity tạo được bỏ qua trong Git. Unity tự tạo lại cache khi mở dự án; lần mở đầu có thể lâu hơn.
- `Builds`, `DongGoi`, `Recovery` là dữ liệu bàn giao/khôi phục trên máy, không đưa vào source Git. Bản chạy mới nhất ở `Builds/Windows-ChestLoot`; giữ cả thư mục khi sao chép.
- Giữ bản sao source trước khi sửa scene. Chỉ dọn bản build cũ sau khi đã phân biệt với bản mới đang chạy; không xóa asset, `.meta`, script tương thích save hoặc giấy phép chỉ vì không thấy dùng trong scene.
