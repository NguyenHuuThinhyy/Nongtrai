# Chat AI offline — bản được chủ dự án cho phép phát hành

© TriForge. Chủ dự án đã yêu cầu đẩy và gộp bản hiện tại vào main ngày 08/10/2026.
[Tải Windows, APK và source Unity kèm model](https://github.com/NguyenHuuThinhyy/Nongtrai/releases/tag/localai-20261008-a13f41f).

## Chơi

- Windows: giải nén **toàn bộ** ZIP Windows LocalAI, mở `NongTrai.exe` hoặc `CHAY_GAME.bat`. Nhấn **C** để chat. Không cần Python, Ollama, server, mã kết nối hay Internet.
- Android: cài APK LocalAI ARM64, mở game → Trợ lý → Chat. Model nằm trong APK. Lần mở chat đầu tiên game lấy model từ APK ra vùng riêng của ứng dụng và kiểm SHA256; các lần mở chat sau nạp từ file đã chuẩn bị. Không cần PC/Wi-Fi.
- Chỉ nạp AI khi mở chat. Đóng chat, mở bảng khác hoặc đưa ứng dụng xuống nền sẽ hủy việc lấy/kiểm tra/nạp model và trả lời, rồi giải phóng model cùng bộ nhớ ngữ cảnh trên worker thread. Gameplay không nạp hoặc chạy AI trong nền. Mở lại nhanh sẽ đợi lần cũ dừng xong, tránh nạp hai model đồng thời. File model đã kiểm tra trên Android vẫn được giữ để không phải lấy lại từ APK. Gửi câu hỏi lúc đang chuẩn bị sẽ chờ; câu trả lời hiển thị dần. Nút Hủy trả lời dừng câu hiện tại nhưng giữ model khi chat còn mở; xem Hướng dẫn tĩnh cũng tắt model.
- Có thể chào hỏi/trò chuyện bằng tiếng Việt; câu hỏi về game có thêm những mục hướng dẫn liên quan. Nguồn hiển thị là hướng dẫn được đưa vào câu hỏi, không phải chứng nhận câu trả lời luôn chính xác.
- Mặc định **AI trên máy**. AI qua PC vẫn là lựa chọn riêng trong **AI / Cloud**, chỉ dùng khi bạn chủ động chọn và nhập địa chỉ/mã. Cloud tắt mặc định.

Model chủ dự án cung cấp: `Backend/models/Qwen3-1.7B-Q4_K_M.gguf`, 1.107.409.472 byte. Đường dẫn `Backend0/model` ban đầu không tồn tại trên máy này. Metadata xác nhận kiến trúc qwen3, model Qwen3 1.7B, Unsloth lượng tử hóa. SHA256 và giấy phép ở `Backend/LOCAL-MODEL.json` và `Backend/models/Qwen3-LICENSE`.

Model khiến gói Windows/APK lớn hơn khoảng 1,11 GB. Android cần thêm khoảng 1,2 GB trống cho bản model trong vùng ứng dụng, ngoài dung lượng cài APK. Khuyến nghị điện thoại ARM64 có ít nhất 6 GB RAM; không bảo đảm mọi điện thoại đủ bộ nhớ hoặc tốc độ. Không tải model khi chạy; chuẩn bị một lần hoàn toàn từ gói cài. Chưa nghiệm thu tốc độ/bộ nhớ/đa chạm trên điện thoại thật.
Máy PC 8 GB RAM nên đóng Unity Editor khi chơi EXE để dành bộ nhớ cho game và AI.

## Bản build local 08/10/2026

Windows và Android đã build thành công với 0 lỗi build, sau thay đổi chỉ nạp khi mở chat và giải phóng khi đóng. Windows runtime: 1.265.493.488 byte; APK: 1.161.734.829 byte. Chưa mở EXE để hỏi model, chưa cài/thử APK trên thiết bị thật và chưa đo CPU/RAM sau đóng chat. Không bật âm thanh trong quá trình sửa/build. Chủ dự án đã cho phép phát hành/gộp main. Gói bàn giao có commit nguồn và SHA256 trong MANIFEST.json/SHA256SUMS.txt; kiểm tra runtime vẫn chưa được xác nhận.

## Build / phát triển

Giữ GGUF gốc trong `Backend/models`, kèm license. Model không commit vào Git; các gói bàn giao LocalAI đóng kèm file này. Source ZIP LocalAI có model và các plugin đã biên dịch, có thể build bằng Unity 6000.3.22f1 mà không cần tải model lại.
Khi mở Source ZIP lần đầu, chọn **Nong Trai → Technology → Prepare local chat model** trước khi Play trong Editor. Build bằng các công cụ của dự án tự thực hiện bước này.

## Chủ dự án kiểm tra trước khi duyệt

- Tắt mạng trên PC, mở EXE → C → hỏi “Xin chào”, rồi hỏi cách lấy nước hoặc sửa cung. Kiểm tra có câu trả lời từ model, không chỉ bảng hướng dẫn.
- Hủy khi đang trả lời, gửi câu mới; đóng chat lúc lấy model/kiểm hash/nạp/trả lời, theo dõi CPU và RAM giảm sau khi tác vụ dừng; mở lại nhanh không nạp hai model; J vào mô hình → Hỏi trợ lý → đóng chat trở lại mô hình.
- Android bật chế độ máy bay, mở APK → Chat và chờ chuẩn bị lần đầu; hỏi bằng tiếng Việt, thoát/mở lại để xác nhận không lấy lại model mỗi lần. Thử chuyển ứng dụng khi đang trả lời.
- Ghi thiết bị/RAM, thời gian chuẩn bị, thời gian trả lời, lỗi/ảnh. Build thành công không thay thế những kiểm tra runtime này. Chủ dự án đã cho phép đẩy/gộp; các kiểm tra runtime này vẫn cần thực hiện.

- `Tools/Build-LocalChat.ps1`: biên dịch plugin Windows x64 và Android ARM64 từ llama.cpp b7199 (commit `8c32d9d96d9ae345a0150cae8572859e9aafea0b`), nguồn/ZIP kiểm SHA256. Dùng CMake/NDK của Unity; Windows dùng llvm-mingw tải riêng. Cache compiler/source nằm ngoài dự án.
- `FarmLocalModelBuild.Stage`: kiểm checksum GGUF, chuẩn bị StreamingAssets và giấy phép; chọn platform/CPU đúng cho plugin. Build Windows/Android gọi Stage, không dựng lại scene.
- `FarmLocalChat`: lớp C# gọi plugin C ABI trên worker thread. CPU, context 2048 token, tối đa 3 luồng, tối đa 320 token trả lời; không cần GPU hay dịch vụ mạng. Tắt thinking theo template Qwen3 để trả lời gọn.
- `Tools/Package-LocalChat.ps1`: đóng các gói riêng trong DongGoi; không ghi đè gói đã phát hành. Model không nhân đôi trong source ZIP.

Mô hình/nhận dạng ngôn ngữ có thể trả lời sai. Không thay đổi inventory/save/gameplay qua chat. Tư vấn game lấy dữ liệu đọc từ bộ hướng dẫn, cloud tùy chọn vẫn tách riêng.

## Nguồn và giấy phép

- Qwen3: Apache 2.0, [model gốc](https://huggingface.co/Qwen/Qwen3-1.7B), [bản GGUF của Unsloth](https://huggingface.co/unsloth/Qwen3-1.7B-GGUF). File hiện tại do chủ dự án cung cấp; checksum ghim đúng file, không suy đoán URL tải riêng.
- [llama.cpp b7199](https://github.com/ggml-org/llama.cpp/tree/8c32d9d96d9ae345a0150cae8572859e9aafea0b): MIT. [Build Android bằng NDK](https://github.com/ggml-org/llama.cpp/blob/master/docs/android.md).
- LLVM/libc++ runtime: Apache 2.0 với LLVM Exceptions. Các giấy phép đi kèm StreamingAssets/FarmAI trong game.

Việc khởi động Python/Ollama native trước đây từng bị auto-review chặn; bản offline này không khởi động các tiến trình/dịch vụ đó. Backend qua PC vẫn là tính năng tùy chọn cũ, chưa được xác nhận chạy thật lại trên máy.
