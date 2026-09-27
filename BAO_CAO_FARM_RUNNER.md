# Bàn giao Farm Runner — 27/09/2026

## Chạy bản mới

- EXE: `D:\GAME_NongTrai\Builds\Windows-3DArt\NongTrai.exe`. Sao chép cả thư mục runtime, không chỉ EXE.
- Unity 6000.3.22f1 URP; scene `Assets/Farm/Scenes/Farm.unity`.
- Tab → Farm Runner. Hộp thư ở nông trại vẫn giao đơn và mỗi đơn thưởng một vé.
- Các gói mới: `DongGoi/NongTrai-Windows-FarmRunner-20260927.zip`, `DongGoi/NongTrai-Unity-FarmRunner-20260927.zip`.
- Runtime trước nén: **100,885,906 byte (96.21 MiB)**, không tính ảnh kiểm tra/log/Burst debug. Kích thước ZIP và SHA-256 trong `DongGoi/RELEASE-MANIFEST.json`.

## Đối chiếu yêu cầu mới

| Yêu cầu | Đã thực hiện / cách dùng |
|---|---|
| Mini game chuyển hẳn map | Farm Runner dùng scene runtime riêng, camera toàn màn hình; nông trại tạm dừng, trở lại đúng vị trí |
| 3 làn, nhảy, trượt | A/D đổi làn khoảng 0,2s, W/Space nhảy, S trượt 0,7s/giảm chiều cao collider; buffer 150ms; CharacterController |
| Chạy vô hạn, tăng khó | 6 đoạn 40m tái sử dụng, 3 loại vật cản, từ 500m thêm hàng vật cản; mỗi hàng chừa làn đi được; tốc độ tăng 10→22m/s |
| Vé | 3 vé ban đầu; 1/lượt, +1/đơn hộp thư; hồi 1/2 giờ thực đến 3; mua 500 xu |
| Xu, quà mốc | Thu xu/nông sản hiếm; tiền khoảng cách lũy tiến +50 xu mỗi mốc km; 1km đá, 2km TNT, 3km bình máu, tiếp theo đá/bình máu; cộng ví chung một lần khi kết thúc |
| Một mạng và chơi lại | Va chạm kết thúc, có bảng kết quả/chơi lại; Esc chủ động kết thúc |
| Nhân vật nông dân, chạy | Kenney e cao1,90m, màu da sáng, áo kem/yếm xanh/ủng/mũ rơm; hoạt ảnh tay chân, bụi chân khi chạy; cùng model ở Runner |
| Cung và tên | Công thức cung/tên, thanh lực kéo màu; tên ghim vào vật thể/quái, đi gần nhặt lại |
| Ăn giữ 3 giây | Thực phẩm/thịt/bình máu67; mỗi lần giữ dùng một món, thả/đổi món hủy; bình máu hồi50HP |
| Nước trên mặt đất | Hồ tại (56,18) map Khám phá; đã xem ảnh xác nhận |
| Đổ/lan/chặn nước | Cầm bình64, phải vào đất/hố: rơi xuống/lan7ô; đặt khối đẩy nước khỏi ô; nguồn lưu trong save17 |
| Hang, quặng, than | Than block9 trong tầng đá; quặng vẫn giữ; sàn hang boss cố định không bị noise khoét mất |
| Nhiên liệu máy | Lò bánh/lò nung: gỗ30s, than120s; nạp qua UI hoặc tự lấy khi đang xử lý; nhiên liệu còn lại được lưu |
| Boss mặt đất có vùng | Đấu trường phẳng tại(88,24), bán kính14m; boss chủ động đánh trong vùng, quay về khi người chơi rời vùng; boss hang vẫn tại(72,72) |
| Làng/hố sâu/công trình | Làng có4nhà, đường giữa, giếng tại(24,76); hố sâu tại(-18,35); tế đàn8cột |
| Bàn rèn mở UI | Đã sửa colors[13] vượt mảng và thao tác mở bị mining chặn; chuột phải mở, chọn kiếm/cung/rìu và đá68 |
| Rèn khó dần | LV0→5, cần1→5đá; tỉ lệ100/83/66/49/32%; phí100+80*cấp; thất bại giữ cấp nhưng mất đá/xu |
| Rương độc lập | Mảng riêng, mở đúng rương được ngắm; deposit/withdraw/save/restore không ảnh hưởng rương khác; chặn chuyển khi đóng/mất rương |
| Rương câu đố cũ | Đúng thả đồ; sai xóa rương; một lần trả lời |
| TNT | Nhận ở2km hoặc chế tạo; cầm/phải đặt, nổ sau3s, đào khối trong vùng nhỏ và gây25HP nếu đứng gần |

## Test và hình ảnh

- `Logs/build-runner-final.log`: `FARM_M1_BUILD_OK 100885906`, Unity build thành công.
- `Logs/smoke-runner-final.log`: tiến trình thoát **0**, không có Exception/error runtime trong log. Toàn bộ smoke cũ và test mới chạy trên save tạm.
- Runner đã mô phỏng qua **2.050m**, giữ đúng6đoạn pool, nhận đúng đá/TNT, hồi/mua/thưởng vé, không trả thưởng hai lần. Đây là kiểm thử chức năng, không phải benchmark FPS.
- Test riêng: rương độc lập cả snapshot/restore, ăn đủ3s và hủy, nước nguồn/lan/chặn/restore/tỷ lệ mesh, quặng/than, gỗ30s/than120s, tên nhặt lại, bàn rèn mở/chọn/tỉ lệ, boss tấn công trong phạm vi.
- Save17: test ghi/đọc vé và nhiên liệu, migration save cũ; creative không ghi save người chơi.
- `Logs/startup-runner-final.log`: chạy bình thường ngoài smoke trong8giây, tiến trình sống/phản hồi, không ghi nhận exception khởi động. Chưa có kiểm thử chơi liên tục nhiều giờ.
- Đã xem ảnh: `Builds/Windows-3DArt/runner-preview.png`, `water-preview.png`, `surface-boss-preview.png`, `village-preview.png`, `forge-preview.png`, `ArtChecks/farmer.png`; bộ art check còn bao phủ vật nuôi, cây, cây trồng và cảnh ngày/đêm.

## Model, giấy phép và phần còn dùng hình khối

- Người chơi: Kenney Mini Characters e, CC0; mesh/material trang phục và da sửa trong Unity, rig/clip giữ. Cây Runner và cây gỗ phục hồi dùng prefab từ Kenney Nature Kit CC0.
- Bò/heo/cừu: Quaternius CC0; gà: [CDmir, cộng tác TinyWorlds — Chicken (animated)](https://opengameart.org/content/chicken-animated), CC0. File nguồn/giấy phép gà được bổ sung cạnh asset.
- Cây ăn quả/bụi/lúa mì/bí ngô đã có asset Kenney. **Nhà/chuồng/làng, máy móc, rương, bàn rèn, Golem, sói/cáo/rắn, dụng cụ và một số cây trồng vẫn có visual dựng từ hình khối.** Không tuyên bố đã thay toàn bộ model theo yêu cầu ban đầu. NPC chưa được bổ sung.
- Gói Survival Kit đã tải từ trước nhưng chưa được scene/prefab/script sử dụng; chuyển vào Recovery và ghi đúng trạng thái trong `Assets/Farm/Models/ASSET_SOURCES.md`.
- Đợt này không tải model mới/asset trả phí; dùng lại tài sản CC0 đã có.

## Giới hạn bản demo

- Chưa có chọn nam/nữ, mở skin theo cấp, cánh/nhảy đôi hoặc ragdoll ngã.
- Nước là mô phỏng ô giới hạn7ô/64nguồn người chơi, chưa có bơi/áp lực/đẩy vật thể. Tên đang ghim tồn tại tối đa3phút và chưa được lưu qua thoát game. TNT đang đếm nổ cũng chưa lưu qua thoát game.
- Cấp rèn áp dụng theo **loại vũ khí**, không theo từng bản sao kiếm/cung/rìu trong túi.
- Làng là công trình khám phá; chưa có dân làng/chuỗi nhiệm vụ riêng. Nhà/công trình vẫn là mẫu đơn giản.
- Save cũ nâng sang generator3 để có địa danh; các ô đào/đặt được giữ, địa hình nền tại vùng địa danh có thay đổi. Bản lưu chỉ ghi khi bấm Lưu game.

## Sao lưu, dọn file, Git

- Source trước sửa: `Recovery/Before-Runner-20260927/UnitySource.zip`; Git mirror trước đồng bộ: `GitMirror-before-sync.zip` cùng thư mục.
- Các nhân vật a/b/c/d/f không còn tham chiếu và gói Survival Kit chưa dùng đã **di chuyển**, kèm meta/giấy phép, vào Recovery. Không xóa asset đang có tham chiếu. Các bản ZIP cũ chuyển vào Recovery sau khi gói mới thành công.
- Không chạy CreateScene/full builder. Chỉ cập nhật visual người chơi trong scene hiện có, tạo prefab tài nguyên và build scene hiện tại.
- Git mirror được đồng bộ ở `D:\GAME_NongTrai\Nongtrai`; thay đổi để ở working tree cho người dùng xem, chưa commit/push. Source ZIP không có Library/Temp/Logs/Builds/Recovery/Git mirror.
