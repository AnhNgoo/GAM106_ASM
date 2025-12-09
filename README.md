# 🎮 Game Server - Hướng Dẫn Sử Dụng

## 🚀 Tính Năng Đã Triển Khai

### 1. 🔐 JWT Authentication
- **Login API**: `POST /api/auth/login`
- **Register API**: `POST /api/auth/register`
- **Get Current User**: `GET /api/auth/me`
- Token có vai trò: `admin` và `member`
- Token hết hạn sau 60 phút

### 2. 📧 OTP Password Reset
- **Request OTP**: `POST /api/player/request-password-reset`
  - Gửi OTP qua email từ: nqa9926@gmail.com
  - OTP có hiệu lực 5 phút
  
- **Reset Password**: `POST /api/player/reset-password`
  - Yêu cầu: Email, OTP, NewPassword

### 3. 🛡️ API Authorization
- Tất cả API đều yêu cầu JWT token (trừ login/register/password-reset)
- Admin endpoints yêu cầu role `admin`
- Một số endpoints public: `/api/item/weapons/expensive`

### 4. 🎨 Admin Dashboard
- **URL**: http://localhost:5024/Admin/Login
- Giao diện game-style đẹp mắt
- Quản lý đầy đủ CRUD cho:
  - Players
  - Items & Weapons
  - Transactions
  - Quests
  - Resources
  - Monsters

## 📝 API Endpoints

### Authentication
```
POST /api/auth/login
Body: { "email": "user@example.com", "password": "password" }

POST /api/auth/register
Body: { "email": "user@example.com", "password": "password" }

GET /api/auth/me
Header: Authorization: Bearer {token}
```

### Player Management
```
GET /api/player/by-game-mode/{modeName}
PUT /api/player/{playerId}/password
GET /api/player/purchase-count
POST /api/player/request-password-reset
POST /api/player/reset-password
```

### Item Management
```
GET /api/item/weapons/expensive
GET /api/item/affordable/{playerId}
GET /api/item/diamond-items
POST /api/item (Admin only)
GET /api/item/most-purchased
```

### Transaction
```
GET /api/transaction/player/{playerId}
GET /api/transaction
```

### Resource
```
GET /api/resource
```

## 🔧 Cấu Hình

### appsettings.json
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyForJWTTokenGeneration12345678",
    "Issuer": "GAM106ASM",
    "Audience": "GAM106ASMUsers",
    "ExpiryMinutes": 60
  },
  "Email": {
    "From": "nqa9926@gmail.com",
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "nqa9926@gmail.com",
    "Password": "xsbb ovzm cfcc sbwe"
  }
}
```

## 🎯 Test Flow

### 1. Đăng ký tài khoản
```bash
POST http://localhost:5024/api/auth/register
Content-Type: application/json

{
  "email": "test@game.com",
  "password": "Test123!"
}
```

### 2. Đăng nhập
```bash
POST http://localhost:5024/api/auth/login
Content-Type: application/json

{
  "email": "test@game.com",
  "password": "Test123!"
}
```

Kết quả: Nhận được token JWT

### 3. Sử dụng API với token
```bash
GET http://localhost:5024/api/player/purchase-count
Authorization: Bearer {your-token-here}
```

### 4. Test OTP Password Reset
```bash
# Bước 1: Request OTP
POST http://localhost:5024/api/player/request-password-reset
Content-Type: application/json

{
  "email": "test@game.com"
}

# Bước 2: Check email và lấy OTP

# Bước 3: Reset password
POST http://localhost:5024/api/player/reset-password
Content-Type: application/json

{
  "email": "test@game.com",
  "otp": "123456",
  "newPassword": "NewPassword123!"
}
```

## 👨‍💼 Admin Panel

### Đăng nhập Admin
1. Truy cập: http://localhost:5024/Admin/Login
2. Đăng nhập với tài khoản có `role = "admin"` trong database
3. Quản lý toàn bộ dữ liệu game

### Tạo Admin User
Cập nhật role trong database:
```sql
UPDATE players 
SET role = 'admin' 
WHERE email_account = 'your-admin-email@game.com';
```

## 🌟 Features Highlights

### JWT Security
- ✅ Token-based authentication
- ✅ Role-based authorization (admin/member)
- ✅ Secure API endpoints
- ✅ Token expiration handling

### OTP Email System
- ✅ Secure password reset
- ✅ Email with game-style template
- ✅ 5-minute OTP expiration
- ✅ One-time use OTP

### Admin Dashboard
- ✅ Game-style UI design
- ✅ Full CRUD operations
- ✅ Session-based authentication
- ✅ Real-time statistics
- ✅ Beautiful animations

## 📦 Dependencies
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
- Microsoft.EntityFrameworkCore 8.0.0
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0
- MailKit 4.14.1
- System.IdentityModel.Tokens.Jwt 8.15.0

## 🚀 Run Application
```bash
cd "E:\FPT Polytechnic\GAM106 - Lập trình Game Back-End\Labs\GAM106ASM\GAM106ASM"
dotnet run
```

Ứng dụng chạy tại: **http://localhost:5024**

## 📚 URLs
- Home: http://localhost:5024
- Admin Login: http://localhost:5024/Admin/Login
- Admin Dashboard: http://localhost:5024/Admin/Dashboard
- Swagger (nếu có): http://localhost:5024/swagger

---
**Developed with ❤️ for GAM106 Assignment**
