# TristanVu2k7-FastFoodCICD
sử dụng file Db "FastFood(NEW1).bacpac" trong thư mục Db chứa tất cả dữ liệu table, Migration
Để tạo database FastFood trên SQL Server Management Studio
mở appsettings.json trong project, đổi tên server trong chuỗi kết nối thành tên server của bạn
Ví dụ: 
```
"Server=DESKTOP-ABC123\SQLEXPRESS;Database=FastFood;Trusted_Connection=True;MultipleActiveResultSets=true"
```
chạy project
XONG!
Phần appsetings.json thì tự điền Server SQL của bạn sau đó tạo một Google OAuth API nhét ClientID và ClientSecret vào,
tiếp tục tạo trình gửi mã OTP Email lấy Port, điền Email của bạn vào SenderName là FastFood, SenderEmail và Username sử dụng Email thật và điền password đã tạo
Link repo gốc : [Google Drive](https://drive.google.com/file/d/1BSNtXwUBxSPg43liZRfzyiDn5wuYLeOW/view)
