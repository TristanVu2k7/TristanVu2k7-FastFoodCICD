# TristanVu2k7-FastFoodCICD
sử dụng file Db "FastFood(NEW1).bacpac" trong thư mục Db chứa tất cả dữ liệu table, Migration
// để tạo database FastFood trên SQL Server Management Studio
mở appsettings.json trong project, đổi tên server trong chuỗi kết nối thành tên server của bạn
// ví dụ: "Server=DESKTOP-ABC123\SQLEXPRESS;Database=FastFood;Trusted_Connection=True;MultipleActiveResultSets=true"
chạy project
XONG!
Phần appsetings.json thì tự điền Server SQL của bạn sau đó tạo một Google OAuth API nhét ClientID và ClientSecret vào,
tiếp tục tạo trình gửi mã OTP Email lấy Port, điền Email của bạn vào SenderName là FastFood, SenderEmail và Username sử dụng Email thật và điền password đã tạo

Tạo appsetting.json 
```
lưu vào file Assignment_NET104/Assignment_NET104/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=FastFoodDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Authentication": {
    "Google": {
      "ClientId": "978342895546-dfhvkv6nffptaiapte90thpu7skhohpu.apps.googleusercontent.com",
      "ClientSecret": "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"
    }
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "FastFood",
    "SenderEmail": "namvutran20072@gmail.com",
    "Username": "namvutran20072@gmail.com",
    "Password": "evhk xhqx faxi cuxo" // App password
  },

  "AllowedHosts": "*"
}
```
lưu file thứ 2 ở /FastFood.IntergrationTest/appsettings.json
```
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "FastFood",
    "SenderEmail": "namvutran20072@gmail.com",
    "Username": "namvutran20072@gmail.com",
    "Password": "xlcl xfar snsz lbzw" // App password
  }

}
```
