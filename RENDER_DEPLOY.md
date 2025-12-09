# 🚀 Deploy lên Render.com - Hướng Dẫn Chi Tiết

## ✅ Ưu điểm Render.com
- ✅ **MIỄN PHÍ** - Không cần credit card
- ✅ Deploy từ GitHub tự động
- ✅ HTTPS miễn phí
- ✅ Free 750 giờ/tháng
- ✅ Dùng Supabase database hiện tại

## 📋 Bước 1: Tạo tài khoản Render.com

1. Truy cập: https://render.com
2. Click **"Get Started"**
3. Đăng nhập bằng **GitHub** (khuyến nghị)
4. Authorize Render truy cập GitHub repos

## 📦 Bước 2: Push code lên GitHub

```powershell
cd "e:\FPT Polytechnic\GAM106 - Lập trình Game Back-End\Labs\GAM106ASM"

# Add files
git add .

# Commit
git commit -m "Add Render.com deployment config"

# Push
git push origin main
```

## 🌐 Bước 3: Tạo Web Service trên Render

### 3.1. New Web Service
1. Vào Render Dashboard: https://dashboard.render.com
2. Click **"New +"** (góc trên bên phải)
3. Chọn **"Web Service"**

### 3.2. Connect Repository
1. Chọn **"Build and deploy from a Git repository"**
2. Click **"Next"**
3. Tìm và chọn repository: **`GAM106_ASM`**
4. Click **"Connect"**

### 3.3. Configure Service
Điền các thông tin sau:

**Name:** `gam106asm`

**Region:** `Singapore` (gần Việt Nam nhất)

**Branch:** `main`

**Root Directory:** (để trống)

**Environment:** `Docker`

**Dockerfile Path:** `./Dockerfile`

**Instance Type:** `Free`

### 3.4. Environment Variables
Click **"Advanced"** rồi thêm các environment variables sau:

| Key | Value |
|-----|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:10000` |
| `ConnectionStrings__DefaultConnection` | `Host=db.kdzpomsimonribvlrcuw.supabase.co;Database=postgres;Username=postgres;Password=Anh992k6ngo;SSL Mode=Require;Trust Server Certificate=true` |
| `Jwt__Key` | `YourSuperSecretKey12345678901234567890123456789012` |
| `Jwt__Issuer` | `GAM106ASM` |
| `Jwt__Audience` | `UnityClient` |
| `Jwt__ExpiryMinutes` | `60` |
| `SmtpHost` | `smtp.gmail.com` |
| `SmtpPort` | `587` |
| `SmtpUsername` | `nqa9926@gmail.com` |
| `SmtpPassword` | `xsbb ovzm cfcc sbwe` |
| `SmtpFromEmail` | `nqa9926@gmail.com` |

**Cách thêm:**
1. Click **"Add Environment Variable"**
2. Nhập **Key** và **Value**
3. Lặp lại cho tất cả các biến trên

### 3.5. Deploy!
1. Click **"Create Web Service"**
2. Render sẽ bắt đầu build và deploy
3. Quá trình mất khoảng **5-10 phút**

## 📊 Bước 4: Theo dõi Deploy

Bạn sẽ thấy:
```
==> Building...
==> Pulling Docker image
==> Building Docker container
==> Deploying...
==> Your service is live! 🎉
```

**Live URL:** `https://gam106asm.onrender.com`

## 🔍 Bước 5: Kiểm tra App

### Test các endpoints:

**1. Trang chủ:**
```
https://gam106asm.onrender.com
```

**2. Admin Dashboard:**
```
https://gam106asm.onrender.com/Admin/Dashboard
```

**3. Test API:**
```
https://gam106asm.onrender.com/Admin/TestUnityAPI
```

**4. Character API:**
```
GET https://gam106asm.onrender.com/api/Character/check/1
```

### Test với curl:
```powershell
curl https://gam106asm.onrender.com/api/Character/check/1
```

## 🎮 Bước 6: Cập nhật Unity

Thay đổi base URL trong Unity code:

```csharp
// CŨ (Local)
private const string BASE_URL = "http://localhost:5024/api";

// MỚI (Production - Render)
private const string BASE_URL = "https://gam106asm.onrender.com/api";
```

## ⚙️ Cấu hình Nâng cao

### Auto-Deploy từ GitHub

Render tự động deploy khi bạn push code:

```powershell
# Mỗi khi có thay đổi
git add .
git commit -m "Update feature"
git push origin main

# Render sẽ tự động deploy!
```

### Xem Logs

1. Vào Render Dashboard
2. Click vào service **"gam106asm"**
3. Tab **"Logs"** để xem real-time logs

### Manual Deploy

1. Vào service dashboard
2. Click **"Manual Deploy"** → **"Clear build cache & deploy"**

## 🐛 Troubleshooting

### Lỗi 1: Service không start
**Kiểm tra:**
```
Logs tab → Tìm error message
```

**Thường gặp:**
- Database connection failed → Check connection string
- Port mismatch → Đảm bảo dùng port 10000

### Lỗi 2: Static files không load
**Giải pháp:**
Đảm bảo `wwwroot` được copy trong Dockerfile (đã OK)

### Lỗi 3: Database connection timeout
**Giải pháp:**
Supabase database đã allow public access, nên OK.

Nếu vẫn lỗi, thử whitelist Render IP:
1. Vào Supabase Dashboard
2. Project Settings → Database → Connection Pooling
3. Add IP: `0.0.0.0/0` (allow all)

### Lỗi 4: 502 Bad Gateway
**Nguyên nhân:** App đang build hoặc crashed

**Giải pháp:**
1. Đợi 5-10 phút cho build xong
2. Check logs để tìm lỗi
3. Redeploy nếu cần

## 📈 Performance & Limits

### Free Tier Limits:
- ✅ 750 giờ/tháng
- ✅ 512MB RAM
- ✅ Shared CPU
- ⚠️ **Sleep sau 15 phút không dùng** (wake up mất ~30s)
- ✅ Unlimited bandwidth
- ✅ Custom domain support

### Keep Alive (Tránh sleep):
**Option 1:** UptimeRobot (ping mỗi 5 phút)
1. Đăng ký: https://uptimerobot.com (free)
2. Add monitor: `https://gam106asm.onrender.com`
3. Interval: 5 phút

**Option 2:** Unity ping định kỳ
```csharp
IEnumerator KeepAlive() {
    while(true) {
        yield return new WaitForSeconds(300); // 5 minutes
        UnityWebRequest.Get("https://gam106asm.onrender.com/api/Character/check/1").SendWebRequest();
    }
}
```

## 🔒 Security Best Practices

### 1. Update CORS (sau khi test xong)
Sửa `Program.cs`:
```csharp
policy.WithOrigins("https://your-unity-game.com")
      .AllowAnyMethod()
      .AllowAnyHeader();
```

### 2. Rotate Secrets
Định kỳ đổi:
- Jwt__Key
- SmtpPassword
- Database password

### 3. Rate Limiting
Thêm middleware để chống spam API.

## 📊 Monitoring

### Render Dashboard
- **Metrics:** CPU, Memory, Request count
- **Logs:** Real-time application logs
- **Events:** Deploy history, crashes

### Setup Alerts
1. Vào service settings
2. Notifications → Add email/Slack
3. Alert when: Deploy fails, Service down

## 💰 Cost Comparison

| Feature | Render Free | Fly.io Free | Vercel Free |
|---------|-------------|-------------|-------------|
| Credit Card | ❌ No | ✅ Required | ❌ No |
| Sleep Policy | 15 min | No sleep | No |
| RAM | 512MB | 256MB | 1GB |
| Database | External | PostgreSQL | External |
| Best For | ASP.NET | Flexible | Next.js |

## 🎯 Checklist Deploy

- [x] Code pushed to GitHub
- [x] render.yaml created
- [x] Dockerfile updated (port 10000)
- [ ] Render account created
- [ ] Repository connected
- [ ] Environment variables set
- [ ] Service deployed
- [ ] URLs tested
- [ ] Unity updated with new URL

## 🚀 Quick Reference

**Your URLs:**
- **Web:** https://gam106asm.onrender.com
- **API:** https://gam106asm.onrender.com/api
- **Admin:** https://gam106asm.onrender.com/Admin/Dashboard
- **Test API:** https://gam106asm.onrender.com/Admin/TestUnityAPI

**Dashboard:** https://dashboard.render.com/web/srv-XXXXX

**Support:** https://render.com/docs

## ✅ Hoàn Tất!

Sau khi deploy xong:
1. Test tất cả API endpoints
2. Update Unity với URL mới
3. Setup UptimeRobot để tránh sleep
4. Monitor logs trong 24h đầu

**🎉 Chúc mừng! Backend đã live!**
