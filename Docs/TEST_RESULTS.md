# Kết quả kiểm tra nhánh rubric

© HThinh.yy. Cập nhật kết quả thực tế trước bàn giao; các mục chưa chạy giữ Pending.

| Kiểm tra | Trạng thái | Giới hạn |
|---|---|---|
| Unity Configure Android/AR | PASS | Compilation/config, chưa tracking thiết bị |
| Backend8tests + BM2530câu | PASS | Model contract có mock; không thay test model thật |
| Model Qwen tải/digest/license | PASS | Ollama native Windows; không là Docker |
| Windows build / full smoke / UI | Pending | Đang chạy bản source mới |
| Model thật30câu / độ chính xác / thời gian | Pending | Lưu raw response rồi đối chiếu source |
| Android APK ARM64 IL2CPP | Pending | Build không thay kiểm tra chơi trên máy thật |
| Android cảm ứng / 15phút30FPS | Pending | Chưa có thiết bị được chọn |
| AR tracking thật / camera / background | Pending | Preview desktop không tính tracking |
| Adafruit IO end-to-end | Pending | Chủ dự án chưa có tài khoản, sẽ tạo sau |
| Docker checkout sạch / điện thoại | Pending | Cài Docker tự động bị policy chặn |
| Chủ dự án tự test / merge | Pending | Không gộp main trước xác nhận |

Log kiểm tra được chọn lọc và ảnh ở `Evidence/Rubric`; Logs/cache đầy đủ chỉ lưu cục bộ.
Không đổi save schema22 hoặc scene Farm để chạy builder. Khi sửa code phải build/test
lại phần bị ảnh hưởng và ghi đúng artifact nguồn.
