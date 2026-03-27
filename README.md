# AppLauncher

AppLauncher là ứng dụng WPF giúp mở nhanh một bộ workspace trên Windows, gồm nhiều ứng dụng desktop và một tác vụ backend trong cùng một profile.

Giao diện được thiết kế theo phong cách Fluent Design, phù hợp với Windows 11 và phù hợp cho các nhóm công việc như:

- Frontend + backend
- Coding + browser + chat
- Development + database tool + terminal

## Tính năng chính

- Tạo nhiều profile cho từng ngữ cảnh làm việc
- Lưu danh sách ứng dụng theo từng profile
- Tái sử dụng ứng dụng đã có từ app library
- Chạy backend bằng Windows Terminal thay vì `cmd.exe`
- Tự động bỏ qua ứng dụng nếu ứng dụng đó đang chạy
- Tự động bỏ qua backend nếu cửa sổ Windows Terminal của profile đó đang mở
- Tự động lưu cấu hình vào file JSON trong thư mục `config`

## Giao diện và icon

- Giao diện Fluent Design theo hướng Windows 11
- Nút bấm có hiệu ứng hover và press
- Ứng dụng đã được gán icon từ file `image/icon-app.ico`

## Cách sử dụng

### 1. Tạo profile

Nhấn `Add Profile`, sau đó đổi tên profile trong khung `Profile Details`.

Ví dụ:

- `Work`
- `Client A`
- `Study`

### 2. Thêm ứng dụng vào profile

Bạn có 2 cách:

- `Add Existing`: thêm lại ứng dụng đã xuất hiện ở profile khác
- `Add New App`: thêm ứng dụng mới bằng tên hiển thị và đường dẫn `.exe`

Ví dụ đường dẫn ứng dụng:

```text
C:\Program Files\Google\Chrome\Application\chrome.exe
C:\Users\phuoc\AppData\Local\Programs\Microsoft VS Code\Code.exe
C:\Users\phuoc\AppData\Local\Discord\Update.exe
D:\Tools\Postman\Postman.exe
```

Ví dụ tên hiển thị:

```text
Chrome
VS Code
Discord
Postman
```

### 3. Cấu hình backend task

Nếu profile cần chạy backend, điền 2 trường:

- `Folder`: thư mục gốc của project backend
- `Command`: lệnh cần chạy

Ví dụ:

```text
Folder:
D:\Workspace\recruitment-website-backend

Command:
gradlew.bat bootRun
```

Ví dụ khác:

```text
Folder:
C:\Projects\my-api

Command:
npm run dev
```

Ví dụ cho Python:

```text
Folder:
D:\Code\fastapi-server

Command:
uvicorn main:app --reload
```

Khi bấm `Start`, AppLauncher sẽ mở backend bằng Windows Terminal (`wt.exe`) và gán một tiêu đề cửa sổ riêng theo profile.

## Logic tránh mở trùng

AppLauncher không mở lặp lại những tiến trình đã đang chạy:

- Nếu file `.exe` của ứng dụng đã chạy, ứng dụng đó sẽ bị bỏ qua
- Nếu backend của profile đã có cửa sổ Windows Terminal đang mở, backend sẽ bị bỏ qua

Ví dụ:

- Profile `Work` có `Code.exe`, `chrome.exe` và backend `npm run dev`
- Nếu `Code.exe` đang chạy rồi, AppLauncher sẽ không mở thêm VS Code nữa
- Nếu cửa sổ Terminal của profile `Work` đang tồn tại, AppLauncher sẽ không mở thêm backend nữa

## Cấu trúc cấu hình

Cấu hình được lưu tại:

```text
config\profiles.json
```

Ví dụ dữ liệu:

```json
{
  "Profiles": [
    {
      "Name": "Work",
      "Apps": [
        {
          "Name": "Chrome",
          "Path": "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
        },
        {
          "Name": "VS Code",
          "Path": "C:\\Users\\phuoc\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe"
        }
      ],
      "Backend": {
        "FolderPath": "D:\\Workspace\\recruitment-website-backend",
        "Command": "gradlew.bat bootRun"
      }
    }
  ]
}
```

## Chạy project

Yêu cầu:

- Windows
- .NET 10 SDK
- Windows Terminal nếu bạn muốn dùng backend task

Build:

```powershell
dotnet build
```

Run:

```powershell
dotnet run
```

## Cấu trúc thư mục

```text
AppLauncher/
|-- App.xaml
|-- AppLauncher.csproj
|-- MainWindow.xaml
|-- MainWindow.xaml.cs
|-- ConfigStore.cs
|-- config/
|   |-- profiles.json
|-- image/
|   |-- icon-app.ico
```

## Lưu ý

- Đường dẫn ứng dụng nên là đường dẫn đầy đủ tới file `.exe`
- Trường `Folder` của backend nên trỏ tới thư mục gốc của project cần chạy
- Trường `Command` là lệnh bạn thường gõ trong terminal
- Nếu máy không có Windows Terminal, backend sẽ không mở được

## Gợi ý cấu hình thực tế

### Profile Frontend

```text
Apps:
C:\Users\phuoc\AppData\Local\Programs\Microsoft VS Code\Code.exe
C:\Program Files\Google\Chrome\Application\chrome.exe
D:\Tools\Postman\Postman.exe

Backend Folder:
D:\Workspace\web-client

Backend Command:
npm run dev
```

### Profile Java Backend

```text
Apps:
C:\Users\phuoc\AppData\Local\Programs\Microsoft VS Code\Code.exe
C:\Program Files\JetBrains\IntelliJ IDEA Community Edition 2024.3\bin\idea64.exe

Backend Folder:
D:\Workspace\spring-api

Backend Command:
gradlew.bat bootRun
```
