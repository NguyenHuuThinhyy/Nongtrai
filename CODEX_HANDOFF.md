# Handoff — nhánh rubric Android / AR / AI / Cloud

© HThinh.yy. Nền bf7d0b3. Nhánh codex/rubric-mobile-ar-ai-cloud.
Worktree D:/GAME_NongTrai/Nongtrai-Rubric; dự án cũ D:/GAME_NongTrai giữ nguyên.
Không merge main trước khi người dùng tự test và xác nhận. Không chạy builder dựng scene.
Backup trước sửa: Recovery/Before-Rubric-20261004 (ngoài Git/source zip).

## Triển khai

- FarmControls / FarmInput: desktop và cảm ứng, snapshot cạnh nút theo frame;
  bàn phím/chuột trực tiếp chỉ còn trong wrapper hoặc checks.
- FarmMobileUI: joystick, look, hold/release, hotbar, menu và Runner; safe area;
  FarmPanelFit cho modal. Windows -farmTouch xem bố cục. Android runtime URP giảm
  shadow/renderScale/MSAA; không đổi gameplay.
- FarmData: desktop StreamingAssets, Android Resources. Configure đồng bộ3JSON.
- FarmServices: manual/chat/URL/mã/telemetry/control/ack; connection/history độc lập
  save22. Cloud chỉ trạm0 đã xây; loss/offline trở về local. FarmWaterSystem kiểm gate mới.
- FarmAR: ARSession/XROrigin/pose/plane/raycast/anchor tạo khi mở; layer30 tách
  rendering/raycast; pause/camera phục hồi; lightweight licensed models, cây snapshot.
- FarmTechnologyBuild: saved-scene-only Windows/Android; XR/renderer/config tự cấu hình
  không sửa scene/prefab. ARFoundation/Core6.3.5, AndroidOptional, ARM64 IL2CPP/API26–36.
- Backend: FastAPI0.115.12, MQTTpaho2.1TLS, BM25Vietnamese, Ollama0.12.3/Qwen3:1.7b.
  Model chọn câu hướng dẫn với JSON schema; trả nguyên câu và tên nguồn (extractive QA).
  Lease/revision/serverID bỏ lệnh cũ/trùng/phiên khác;3feed/20s, max20publishes/min.
- Docker Compose2services, healthchecks, volume model, nonroot API, key ngoài Git.
  .github/workflows/rubric-backend.yml kiểm checkout sạch/containers/model.

## Kiểm tra và thiết bị

FarmTechnologyChecks (-farmSmokeCheck -farmTechnologyOnly) kiểm touch edges, dữ liệu,
manual/pause/fallback/miniature và render ảnh. Thêm -farmTouch kiểm bố cục chạm;
-farmServicesLive cần APIlocalhost8000 với mã test local-unity-smoke-only (không ghi prefs).
Full smoke sửa thứ tự để FarmPolishChecks (cố tình bật Creative) chạy cuối, tránh làm
các kiểm tra normal Save sau đó sai. Không sửa quy tắc Creative/save để làm test qua.

Backend/tests/test_services.py:8unit tests + BM2530câu; evaluate_model.py gọi model thật,
raw response lưu Evidence/Rubric/chat-model-native.json, accuracy do đối chiếu riêng.
Xem Docs/TEST_RESULTS.md cho kết quả build/test cuối, không suy luận pass từ code có sẵn.

Máy PC i3-1115G4/UHD/8GB. Android SDK36, NDKr27c, JDK17 đã cài cho Editor.
Chưa có điện thoại kết nối ADB, chưa có tài khoản Adafruit. Docker tự cài bị auto-review
chặn "blocked by policy"; native Python/Ollama dùng để chẩn đoán, không tính là Docker.
Hướng dẫn manualDocker/ARCore/account nằm Docs/RUBRIC_INTEGRATION.md.

## Bàn giao và phụ thuộc

Save22, item IDs, gameplay/collider, farm scene và licensed assets giữ tương thích.
Knowledge backend và Unity Resources phải byte-identical. Không track .env, weights,
Library/Temp/Logs/cache hoặc tải trùng. Cập nhật gói/source/commit/hash ở DongGoi và
release rubric-preview-20261004. Gói cũ chỉ dọn sau khi bản thay thế được xác minh.
Xem Docs/ACCEPTANCE.md và DEMO.md cho phần cần chủ dự án tự nghiệm thu/video.
