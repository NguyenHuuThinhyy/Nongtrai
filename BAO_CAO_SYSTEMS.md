# Systems — cập nhật theo yêu cầu HThinh.yy, 27/09/2026

## Đã làm

| Yêu cầu | Thay đổi |
|---|---|
| Loại nước bằng bọt biển | Shop bán80xu. Đặt cạnh nước, hút trong1ô mỗi hướng (3×3×3), chuyển đầy. Thu lại/bán; đầy không hút lần nữa. Lưu được trạng thái. |
| Hạt về0/dọn túi | Hạt hết biến mất; mua lại hiện trong ô trống. Bán mọi loại hàng/hạt/dụng cụ, thêm bán toàn túi kể cả stock hạt vượt sức chứa. Dụng cụ cơ bản có thể mua lại. |
| Hạt đặc biệt từ khám phá | Bí pha lê/dâu hoàng kim trong rương Golem hang/tế đàn và rương do Golem bậc4 canh. Đúng câu đố thả2+1hạt; gieo/thu hoạch/bán được. Khôngshop/craft. |
| Quái biết nhảy | Golem và quái canh, sói/cáo/rắn dùng dò vật cản và nhảy khối1ô, giữ collider và giới hạn chiều cao. |
| Máu rõ hơn | Thanh máu quái có tên, HP/max; thanh người chơi lớn hơn. Tải save tắt bảng hồi sinh cũ. |
| Hợp nhất bình nước | Chỉ xô105: trái khi rỗng múc, trái khi đầy đặt và vềrỗng. Bỏ2recipe bình phụ. Save cũ hoàn giá bán bình phụ28/12xu, giữ/cấp một xô. |
| Thuê vòi không nạp |350xu/vòi, tự tưới6m trong1ngàygame từ khi đặt.3lượt/ngày; LV3/5/7 mở4/5/6. Ngủ qua ngày tính vào hạn. Thu hồi sớm kết thúc thuê.|
| Git/copyright | Đầy đủ source/meta/assets/docs/license; ©HThinh.yy góc phải dưới, comment mã mới và COPYRIGHT.md. |

Nước tự nhiên/vùng đã hút dùng mô phỏng ô giới hạn; đặt nguồn mới gần vùng hút cho nước lan lại. Xóa một nguồn cũng làm phần dòng chảy phụ thuộc nguồn đó rút. Không mô phỏng chất lỏng vật lý đầy đủ. Quái nhảy khối1ô khi có khoảng đầu, chưa phải tìm đường qua mọi địa hình phức tạp.

## Build và kiểm tra

Unity6000.3.22f1 URP; scene Farm hiện tại, không dựng lại. Hash scene trùng backup. Build Logs/build-systems-final.log: FARM_M1_BUILD_OK100978671. Runtime169file,100.978.671byte (~96,30MiB), Builds/Windows-Systems/NongTrai.exe.

Test riêng đã qua toàn bộ nhóm mới. Full smoke/art cuối **exit0**, Logs/smoke-systems-final.log, gồm toàn bộ checkpoint cũ và FARM_SYSTEMS_OK. Startup Logs/startup-systems-final.log sống/phản hồi sau8giây, không Exception/Error. Đã xem forge-systems-health-preview.png: thanh HP Golem2300/2400, người62/100 và ©HThinh.yy rõ. Kiểm mới có input trái thật xô, hạt0/rebuy/bán5000hạt, bọtbiển/migration/save, vòi không nạp/hết hạn/lượt theoLV, rương/quiz/hạt hiếm/gieo/thu, boss nhảy vượtblock và thanh máu. Bộ cũ kiểm trồng/trại, map, portal/save/LV, TNT5nháy, rèn, Runner, cung, model/vật liệu.

Backup nguồn và mirror: Recovery/Before-Systems-20260927-215005. Bản WaterCan cũ chuyển vào OldRelease của backup. Windows/Unity ZIP và SHA256 trong DongGoi/RELEASE-MANIFEST.json; không đưa cache/build/log vào Git hoặc sourceZIP. Không thêm asset tải ngoài, giữ CC0/source licenses hiện có.
