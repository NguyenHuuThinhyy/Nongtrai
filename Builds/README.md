# Tải game — bản 08/10/2026

## Chạy trực tiếp từ checkout main

Thư mục này có đầy đủ bản Windows và APK, được quản lý bằng **Git LFS**. Cài Git LFS trước khi clone, hoặc chạy trong checkout:

```powershell
git lfs install
git pull --ff-only origin main
git lfs pull
```

- Windows: mở `Builds/Windows-LocalAI/NongTrai.exe`. Giữ nguyên toàn bộ thư mục Windows-LocalAI.
- Android: chép `Builds/Android-LocalAI/NongTrai.apk` sang điện thoại ARM64 và cài.
- Nếu tải bằng nút **Download ZIP** mà file lớn còn là con trỏ Git LFS, dùng lệnh trên hoặc tải gói Release bên dưới.

## Gói tải độc lập


© TriForge. Source game ở nhánh `main`. Các gói đầy đủ được phát hành trên GitHub Release:

| Gói | Tải trực tiếp | Dung lượng |
|---|---|---|
| Windows có EXE và toàn bộ dữ liệu/model | [Windows ZIP](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/download/triforge-20261008-0b798d9/NongTrai-Windows-20261008-TriForge-0b798d9.zip) | 1,17 GB |
| Android ARM64, model offline trong APK | [Android APK](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/download/triforge-20261008-0b798d9/NongTrai-Android-20261008-TriForge-0b798d9.apk) | 1,16 GB |
| Unity đầy đủ kèm model và giấy phép để build tiếp | [Unity source ZIP](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/download/triforge-20261008-0b798d9/NongTrai-Unity-20261008-TriForge-0b798d9.zip) | 1,19 GB |

Windows: giải nén toàn bộ ZIP, mở `NongTrai.exe` hoặc `CHAY_GAME.bat`. Giữ thư mục dữ liệu, DLL và model cạnh EXE.

Android: cài APK trên điện thoại ARM64, cho phép cài từ nguồn tải. Lần mở chat đầu cần thêm khoảng 1,2 GB trống. Chat local không cần PC/Internet.

Unity: giải nén source ZIP, mở bằng Unity **6000.3.22f1**. Scene chính: `Assets/Farm/Scenes/Farm.unity`. Cài Android Build Support nếu build APK. Clone Git riêng cần lấy `Backend/models` từ source ZIP.

Nhấn **C** mở chat, **J** mở nông trại thu nhỏ local, **H** ẩn/hiện hướng dẫn.

[Release và checksum](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/tag/triforge-20261008-0b798d9). Build từ commit `0b798d9`: Windows/Android đều thành công, 0 lỗi. Đã đối chiếu kích thước và SHA-256 trên GitHub. Chưa nghiệm thu EXE/chat hoặc APK trên thiết bị thật.

Bản Windows/APK có trong main qua Git LFS; Release giữ ZIP tải độc lập và source kèm model. Cache Unity, log tạm và khóa bí mật không thuộc source bàn giao.
