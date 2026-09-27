# Báo cáo sửa lỗi / cải tiến — 27/09/2026

## Đính chính chuyển map (đã kiểm tra)

Đổi map giữ nguyên vị trí đứng cuối của mỗi map, kể cả có hoặc không có cổng tự đặt. Chỉ chết ở Khám phá mới dùng cổng hồi sinh (thiếu cổng thì cổng gốc). Đã kiểm cả quay lại nơi cách cổng xa, lưu/tải khi đang ở nông trại, và đổi map sau hồi sinh không quay lại chỗ chết. Bản lưu vẫn v18, không cần xóa save. Backup trước đính chính: Recovery/Before-TravelFix-20260927/UnitySource.zip. Các gói trước sửa nằm ở OldPackages trong thư mục này.

## Phạm vi và phục hồi

Nguồn live: D:/GAME_NongTrai. Git mirror: D:/GAME_NongTrai/Nongtrai. Sao lưu trước sửa: Recovery/Before-Polish-20260927/UnitySource.zip. Chỉ chạy UpgradeFarmerInScene để sửa phần hiển thị người chơi và BuildWindowsCurrentScene; không gọi CreateScene, không ghi đè toàn bộ nông trại. Giữ các thay đổi đang có trong Git, không commit/push tự động.

Người dùng đính chính “thêm tóc” thành tăng tốc chạy. Bản cuối giữ tóc gốc Kenney; tăng chạy Shift từ 7 lên 9 m/s, đi 4 m/s, gia tốc 28 và hãm 34 m/s².

## Các yêu cầu

| Yêu cầu | Thực hiện |
|---|---|
| Nhảy, mũ | Mũ gắn vào xương head, giữ kích thước theo mét. Chặn dịch chuyển root trùng với CharacterController trong animation nhảy, cả Runner. |
| Vật phẩm bị gắn vào người | Đo mesh/bone trong Editor: tay phải Kenney dọc trục +X, lòng bàn tay tại (.25,0,.0285) đơn vị source. Cập nhật socket sau xoay xương; điều chỉnh hướng cán. Tái dùng material khi đổi vật cầm. |
| Runner khựng, dễ | Chia bước tối đa 1/60 s, không bỏ thời gian frame 50–500 ms; camera giảm lắc ngang/dọc, HUD 10 Hz. Tốc độ 10–26 m/s, 5 mức khó tại 0/200/500/900/1400 m. 1/2/3 hàng chướng ngại mỗi đoạn, mỗi hàng một làn an toàn khác; hàng cách ít nhất 12 m. Sau 500 m có xe rơm từ lề; hoàn tất dịch ngang từ trước 26 m. Không tạo thêm đoạn khi chạy (pool 6). |
| Quá nhiều khoáng sản | Than 1/61 và quặng 1/83 ô đủ điều kiện trong tầng đá, giảm khoảng 70% so với trước; giữ hệ voxel và các ô đã đào/đặt. |
| LV4 về LV1 | Xác định nút menu trước đây reload scene, mất phần chưa lưu. Đổi “Chơi game lại”, thêm Lưu rồi chơi lại / tải bản cũ bỏ phần chưa lưu / Hủy. Lưu thất bại không reload. Level/XP được lưu đầy đủ; không đặt lại cấp khi bấm mở xác nhận. |
| Boss | 2.400 HP; đập 38 HP, dưới 50% máu: 52 HP, nhanh hơn. Báo hiệu đòn 0,7/0,5 s, giữ giới hạn lãnh thổ. |
| Rèn kéo thả | 36 ô túi, 2 ô nhận vũ khí/đá. Bấm hoặc kéo thả, đúng loại ô; đóng bảng không mất đồ. Không tách bản sao inventory, chỉ trừ đá/xu khi rèn. |
| Sông và biển | Generator 4: sông quanh x=-36+8sin(z*.032), biển z<-64 với bờ cát. Mặt nước sinh theo vùng đang chơi; chân trời biển dùng 4 quad ngoài vùng tương tác. Có nổi/bơi cơ bản, Space ngoi lên; không có oxy. |
| Cổng hồi sinh đặt lại | Item70/type14; chế tạo 8 đá + 3 kim loại + 1 đá nâng cấp. 1 cổng người chơi ở Khám phá, không tính cổng gốc. Phá rơi lại vật phẩm. Đổi map nhớ vị trí đứng cuối cùng dù có/không có cổng. Chỉ chết mới hồi sinh tại cổng; thiếu cổng hoặc hai lối bị chặn thì hồi sinh ở cổng gốc. Lưu qua BuildingState. |

Save v18 đọc v2–17, ItemCount71, JSON26 công thức. Vị trí cuối của cả hai map vẫn lưu/đọc như cũ; đổi map quay lại vị trí đó. Sau khi chết, vị trí hồi sinh trở thành vị trí hiện tại mới. Generator4 giữ seed/excavated/additions; thay địa hình tự nhiên ven sông/biển ngoài vùng khởi đầu.

## Asset và giới hạn

Không tải thêm asset. Kenney Mini Characters/Nature Kit, Quaternius Farm Animals và Chicken của CDmir/TinyWorlds tiếp tục dùng giấy phép CC0 gốc kèm source. Xem Assets/Farm/Models/ASSET_SOURCES.md. Nhà, máy, dụng cụ, mũ, sói/cáo/rắn, Golem và cổng vẫn là geometry Unity; NPC chưa bổ sung. Không tuyên bố đã thay hết các primitive theo yêu cầu lịch sử. Khối đào/đặt/phá vẫn là voxel.

Lưu vẫn do người chơi chủ động. Chọn “Tải bản đã lưu” có thể mất cấp mới chưa lưu như nội dung xác nhận; chọn “Lưu tiến độ và chơi lại” để giữ. Sông/biển là nước voxel đơn giản, không mô phỏng sóng vật lý. Runner chưa có power-up/skin mới; không bảo đảm FPS trên mọi máy.

## Kiểm tra

- Build: Logs/build-travel-fix-final.log — FARM_M1_BUILD_OK 100909789; không lỗi biên dịch C#.
- Smoke đầy đủ: Logs/smoke-travel-fix-final.log — exit 0; các bài cũ cùng bộ FarmPolishChecks đều qua, không runtime exception.
- Shift chạy: đo 9,000001 m/s. Space/W vượt khối 1 m; mũ/socket qua animation Jump/Work/Attack.
- Runner: giữ thời gian frame 200 ms, đổi làn hoàn tất trong 250 ms; đi trên 2 km với pool 6, nhận đúng đá/TNT và không nhận tiền hai lần.
- LV4/65 XP: mở rồi hủy xác nhận không đổi cấp; Save/Load file tạm khôi phục đúng.
- Cổng: craft/đặt/chặn cổng thứ hai/restore/full file Save-Load/đi qua lại/chết hồi sinh/phá/fallback về cổng gốc đều qua.
- Bàn rèn: 36 ô + 2 đích kéo, bấm/kéo/drop sai loại/đóng không mất đá đều qua.
- Nước/than/quặng, rương độc lập, cung và nhặt tên, ăn 3 giây, boss sát thương trong vùng: đều qua. Mẫu quặng+than: 180/5489 ô đá (3,28%).
- Kiểm ảnh: ArtChecks (nhân vật đi/nhảy/làm việc/đánh, thú/cây/ngày/đêm), hand-106/107/111/67-preview.png, forge-drag-preview.png, river-preview.png, sea-preview.png trong Builds/Windows-3DArt. Cán kiếm dựng lên, không nằm dọc cẳng tay; camera chụp cuối không bị trụ cổng che.
- Startup thường: Logs/startup-travel-fix-final.log — còn chạy và Responding=True sau 8 s; chỉ dừng PID kiểm tra. Không dùng save thật để chạy smoke.
- Runtime Windows: **100,909,789 byte** (96.23 MiB), không tính ảnh kiểm tra/log/Burst debug. ZIP và SHA-256: DongGoi/RELEASE-MANIFEST.json.

## Bàn giao

- DongGoi/NongTrai-Windows-Polish-20260927.zip
- DongGoi/NongTrai-Unity-Polish-20260927.zip
- Chạy trực tiếp: Builds/Windows-3DArt/NongTrai.exe.
- Gói cũ được chuyển vào Recovery/Before-Polish-20260927/OldPackages; material tóc thử đã bỏ dùng được chuyển vào thư mục Unused sau kiểm tra GUID. Không xóa chức năng/asset còn tham chiếu.
- Source không chứa Library, Temp, log, build phụ hoặc các gói tải/backup. Git mirror giữ working changes, không commit/push.
