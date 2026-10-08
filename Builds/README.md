# Tải game — bản 08/10/2026

© HThinh.yy. Source game ở nhánh `main`. Các gói đầy đủ được phát hành trên GitHub Release:

| Gói | Tải trực tiếp | Dung lượng |
|---|---|---|
| Windows có EXE và toàn bộ dữ liệu/model | [Windows ZIP](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/download/localai-20261008/NongTrai-Windows-20261008-d82e56a.zip) | 1,17 GB |
| Android ARM64, model offline trong APK | [Android APK](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/download/localai-20261008/NongTrai-Android-20261008-d82e56a.apk) | 1,16 GB |
| Unity đầy đủ kèm model và giấy phép để build tiếp | [Unity source ZIP](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/download/localai-20261008/NongTrai-Unity-20261008-d82e56a.zip) | 1,19 GB |

Windows: giải nén toàn bộ ZIP, mở `NongTrai.exe` hoặc `CHAY_GAME.bat`. Giữ thư mục dữ liệu, DLL và model cạnh EXE.

Android: cài APK trên điện thoại ARM64, cho phép cài từ nguồn tải. Lần mở chat đầu cần thêm khoảng 1,2 GB trống. Chat local không cần PC/Internet.

Unity: giải nén source ZIP, mở bằng Unity **6000.3.22f1**. Scene chính: `Assets/Farm/Scenes/Farm.unity`. Cài Android Build Support nếu build APK. Clone Git riêng cần lấy `Backend/models` từ source ZIP.

Nhấn **C** mở chat, **J** mở nông trại thu nhỏ local, **H** ẩn/hiện hướng dẫn.

[Release và checksum](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/tag/localai-20261008). Build từ commit `d82e56a`: Windows/Android đều thành công, 0 lỗi. Đã đối chiếu kích thước và SHA-256 trên GitHub. Chưa nghiệm thu EXE/chat hoặc APK trên thiết bị thật.

Gói chơi và model được lưu ở Release do dung lượng lớn; cache Unity, log tạm và khóa bí mật không thuộc source bàn giao.
