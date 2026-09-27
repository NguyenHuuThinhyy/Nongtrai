# Tải game và tiếp tục phát triển — HThinh.yy

## Chơi ngay từ repo

Bấm **Code → Download ZIP**, giải nén toàn bộ, mở **CHAY_GAME.bat** ở thư mục gốc. Khi dùng `git clone`, có thể chạy ngay file này. Bản EXE và mọi thư mục đi kèm ở `Builds/Windows-BowPhysics`; gói Windows/Unity ở `DongGoi`.

## Chơi bản Windows từ Releases

1. Mở [Release BowPhysics](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/tag/bowphysics-20260927).
2. Tải `NongTrai-Windows-BowPhysics-20260927.zip`, giải nén toàn bộ rồi chạy `CHAY_GAME.bat` hoặc `NongTrai.exe`.
3. Giữ EXE cùng `NongTrai_Data`, `MonoBleedingEdge`, `D3D12` và các DLL trong gói. Không cần Unity để chơi. Đọc `CHOI_GAME.md` đi kèm.

## Lấy đầy đủ source để sửa và build tiếp

```powershell
git clone https://github.com/NguyenHuuThinhyy/Nongtrai.git
cd Nongtrai
```

Hoặc tải `NongTrai-Unity-BowPhysics-20260927.zip` ở Release và giải nén. Git clone phù hợp để làm việc nhóm, cập nhật và gửi thay đổi; ZIP source dành cho mở dự án không cần Git.

- Cài Unity Hub và **Unity 6000.3.22f1** trên Windows; bảo đảm Editor có hỗ trợ build Windows. Dự án dùng URP, bản Windows hiện tại dùng backend Mono.
- Unity Hub → Add project from disk → chọn thư mục chứa `Assets`, `Packages`, `ProjectSettings`.
- Chờ import asset và tải package lần đầu (cần mạng), rồi mở `Assets/Farm/Scenes/Farm.unity`, bấm Play.
- Repo/source ZIP có toàn bộ scene, prefab, script, model, texture, vật liệu, `.meta`, JSON và cấu hình/package lock cần thiết. Giữ giấy phép trong `Assets/Farm/Models` và `COPYRIGHT.md`.
- `Library`, `Temp`, `Logs`, cấu hình IDE cá nhân do mỗi máy tạo lại, không cần tải từ máy bàn giao.

## Build Windows bằng PowerShell

Lưu scene, đóng Editor của dự án rồi chạy từ thư mục dự án:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Build-Windows.ps1 -UnityEditor "C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Unity.exe"
```

Thay đường dẫn Unity theo máy của bạn. Script tự tìm vị trí Hub mặc định hoặc `D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe` nếu bỏ `-UnityEditor`. Có thể đặt biến `UNITY_EDITOR_PATH`. Kết quả ở `Builds/Windows/NongTrai.exe`; log có thời gian trong `Logs`.

Script gọi `NongTrai.Editor.FarmProjectBuilder.BuildWindowsCurrentScene`, chỉ build scene đang lưu. **Không chạy `CreateScene` hoặc `RebuildSceneAndBuildWindows`**: hai lệnh đó có thể ghi đè scene/prefab.

Kiểm tra bản build khi cần:

```powershell
& .\Builds\Windows\NongTrai.exe -farmSmokeCheck -farmArtCheck -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -force-d3d11 -logFile "$PWD\Logs\smoke-team.log"
```

Smoke dùng save tạm. Bản lưu chơi thường nằm trong `%USERPROFILE%\AppData\LocalLow\Nong Trai Studio\Nong Trai - First Harvest`, không có trong repo/gói tải.

## Gửi thay đổi cho nhóm

Tạo nhánh riêng, sửa và kiểm tra trước khi gửi PR. Commit cả `.meta` mới, source và cấu hình liên quan. Theo yêu cầu bàn giao, bản build và ZIP hiện tại được commit tại Builds/Windows-BowPhysics và DongGoi, đồng thời có trên Releases. Khi cập nhật bản phát hành, đồng bộ EXE, toàn bộ dữ liệu/DLL và checksum cùng nhau. Không commit cache, log, bản build thử hoặc save cá nhân. Đọc `CODEX_HANDOFF.md` để biết hệ thống đang dùng.

Release có `RELEASE-MANIFEST.json` và `SHA256SUMS.txt`. File manifest phân biệt commit build game với commit tài liệu bàn giao; mã gameplay của hai commit được đối chiếu giống nhau.
