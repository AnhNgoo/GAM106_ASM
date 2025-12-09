# 🚀 Fly.io Deployment Guide

## 📋 Prerequisites

1. **Install Fly CLI**
   ```powershell
   # Windows (PowerShell)
   iwr https://fly.io/install.ps1 -useb | iex
   ```

2. **Login to Fly.io**
   ```powershell
   fly auth login
   ```

3. **Verify Installation**
   ```powershell
   fly version
   ```

---

## 🔧 Setup Database on Fly.io

### 1. Create PostgreSQL Database
```powershell
fly postgres create --name gam106asm-db --region sin
```

**Select options:**
- Development (256MB RAM, 1GB disk) - FREE
- Region: Singapore (sin)

### 2. Get Database Connection String
```powershell
fly postgres connect -a gam106asm-db
```

Copy the connection string from output.

---

## 📦 Deploy Application

### 1. Initialize Fly App
```powershell
cd "e:\FPT Polytechnic\GAM106 - Lập trình Game Back-End\Labs\GAM106ASM"
fly launch --no-deploy
```

**Configuration:**
- App name: `gam106asm` (or choose your own)
- Region: Singapore (sin)
- Database: No (we already created one)
- Deploy now: No

### 2. Set Environment Variables

**Database Connection String:**
```powershell
fly secrets set ConnectionStrings__DefaultConnection="Host=gam106asm-db.internal;Port=5432;Database=gam106asm;Username=postgres;Password=YOUR_PASSWORD"
```

**JWT Settings:**
```powershell
fly secrets set Jwt__Key="YourSuperSecretKeyThatIsAtLeast32CharactersLong123456"
fly secrets set Jwt__Issuer="GAM106ASM"
fly secrets set Jwt__Audience="UnityClient"
```

**Email Settings (if needed):**
```powershell
fly secrets set SmtpHost="smtp.gmail.com"
fly secrets set SmtpPort="587"
fly secrets set SmtpUsername="your-email@gmail.com"
fly secrets set SmtpPassword="your-app-password"
fly secrets set SmtpFromEmail="your-email@gmail.com"
```

### 3. Deploy!
```powershell
fly deploy
```

### 4. Open Your App
```powershell
fly open
```

Your app will be available at: `https://gam106asm.fly.dev`

---

## 🗄️ Database Migration

### Option 1: Run Migrations from Local
```powershell
# Get database connection string
fly postgres connect -a gam106asm-db

# Copy the connection string and update appsettings.json temporarily
# Then run migrations
cd GAM106ASM
dotnet ef database update

# Revert appsettings.json changes
```

### Option 2: SSH into Fly.io and Run
```powershell
fly ssh console -a gam106asm

# Inside the container
cd /app
dotnet ef database update
exit
```

### Option 3: Import Existing Database
```powershell
# Dump your local database
pg_dump -h localhost -U postgres -d gam106asm > backup.sql

# Connect to Fly.io database
fly postgres connect -a gam106asm-db

# Import
\i backup.sql
\q
```

---

## 🔍 Monitoring & Debugging

### View Logs
```powershell
fly logs -a gam106asm
```

### Check App Status
```powershell
fly status -a gam106asm
```

### SSH into Container
```powershell
fly ssh console -a gam106asm
```

### Restart App
```powershell
fly apps restart gam106asm
```

### Check Secrets
```powershell
fly secrets list -a gam106asm
```

---

## 🎮 Update Unity API Base URL

After deployment, update Unity code:

```csharp
// OLD (Local)
private const string BASE_URL = "http://localhost:5024/api";

// NEW (Production)
private const string BASE_URL = "https://gam106asm.fly.dev/api";
```

---

## 🔒 Security Checklist

### ✅ Before Going Live:

1. **Change Default Passwords**
   - Update JWT secret key
   - Use strong database password

2. **Environment Variables**
   - ✅ Database connection string in secrets
   - ✅ JWT keys in secrets
   - ✅ Email credentials in secrets

3. **CORS Configuration**
   - Update `AllowUnity` policy to restrict origins:
   ```csharp
   policy.WithOrigins("https://yourunity.game")
         .AllowAnyMethod()
         .AllowAnyHeader();
   ```

4. **Database Backup**
   ```powershell
   fly postgres backup create -a gam106asm-db
   ```

---

## 💰 Pricing

### Free Tier Includes:
- ✅ 3 shared-cpu VMs with 256MB RAM
- ✅ 160GB outbound data transfer
- ✅ PostgreSQL database (1GB storage)

**Your Configuration:**
- Web App: 1 VM (256MB RAM) - **FREE**
- Database: 1 VM (256MB RAM, 1GB disk) - **FREE**

Total: **$0/month** ✅

---

## 🐛 Common Issues

### Issue 1: App Won't Start
```powershell
# Check logs
fly logs -a gam106asm

# Restart
fly apps restart gam106asm
```

### Issue 2: Database Connection Failed
```powershell
# Verify connection string
fly secrets list -a gam106asm

# Test database connection
fly postgres connect -a gam106asm-db
```

### Issue 3: Static Files Not Loading
Check `wwwroot` is included in published output:
```xml
<!-- GAM106ASM.csproj -->
<ItemGroup>
  <Content Include="wwwroot\**" CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>
```

### Issue 4: HTTPS Certificate Error
Fly.io automatically provisions SSL. Wait 5-10 minutes after first deploy.

---

## 🔄 Update/Redeploy

### After Code Changes:
```powershell
# Commit to GitHub
git add .
git commit -m "Update features"
git push origin main

# Deploy to Fly.io
cd "e:\FPT Polytechnic\GAM106 - Lập trình Game Back-End\Labs\GAM106ASM"
fly deploy
```

---

## 📊 Scaling (If Needed)

### Increase Memory
```powershell
fly scale memory 512 -a gam106asm
```

### Add More Instances
```powershell
fly scale count 2 -a gam106asm
```

### Upgrade Database
```powershell
fly postgres update -a gam106asm-db
```

---

## 🎯 Quick Commands Reference

| Task | Command |
|------|---------|
| Deploy | `fly deploy` |
| View logs | `fly logs` |
| Open app | `fly open` |
| Restart | `fly apps restart gam106asm` |
| SSH console | `fly ssh console` |
| Check status | `fly status` |
| Database backup | `fly postgres backup create -a gam106asm-db` |

---

## 🌐 Your Live URLs

After deployment:
- **Web App**: https://gam106asm.fly.dev
- **API Base**: https://gam106asm.fly.dev/api
- **Admin Dashboard**: https://gam106asm.fly.dev/Admin/Dashboard
- **Test Unity API**: https://gam106asm.fly.dev/Admin/TestUnityAPI

---

## ✅ Final Checklist

Before sharing with users:

- [ ] Database migrated successfully
- [ ] All secrets configured
- [ ] App deploys without errors
- [ ] API endpoints working
- [ ] Admin login works
- [ ] Unity can connect to API
- [ ] Static files (images, CSS) loading
- [ ] HTTPS working (green padlock)
- [ ] Database backed up

---

## 🚀 Let's Deploy!

**Quick Start:**
```powershell
# 1. Install Fly CLI
iwr https://fly.io/install.ps1 -useb | iex

# 2. Login
fly auth login

# 3. Create database
fly postgres create --name gam106asm-db --region sin

# 4. Navigate to project
cd "e:\FPT Polytechnic\GAM106 - Lập trình Game Back-End\Labs\GAM106ASM"

# 5. Launch (don't deploy yet)
fly launch --no-deploy

# 6. Set secrets
fly secrets set ConnectionStrings__DefaultConnection="YOUR_DB_CONNECTION_STRING"
fly secrets set Jwt__Key="YourSuperSecretKey123456789012345678901234567890"
fly secrets set Jwt__Issuer="GAM106ASM"
fly secrets set Jwt__Audience="UnityClient"

# 7. Deploy!
fly deploy

# 8. Open app
fly open
```

🎉 **Done! Your game backend is now live!**
