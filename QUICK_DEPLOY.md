# 🚀 Quick Deploy Commands

## Step 1: Install Fly CLI
```powershell
iwr https://fly.io/install.ps1 -useb | iex
```

## Step 2: Login
```powershell
fly auth login
```

## Step 3: Create Database
```powershell
fly postgres create --name gam106asm-db --region sin
```
**Select:** Development - 256MB RAM, 1GB disk (FREE)

Save the connection details shown!

## Step 4: Navigate to Project
```powershell
cd "e:\FPT Polytechnic\GAM106 - Lập trình Game Back-End\Labs\GAM106ASM"
```

## Step 5: Launch App (Don't Deploy Yet)
```powershell
fly launch --no-deploy
```
- App name: `gam106asm` (or your choice)
- Region: Singapore (sin)
- Deploy now: **No**

## Step 6: Set Database Secret
Replace `YOUR_PASSWORD` with password from Step 3:
```powershell
fly secrets set ConnectionStrings__DefaultConnection="Host=gam106asm-db.internal;Port=5432;Database=gam106asm;Username=postgres;Password=YOUR_PASSWORD"
```

## Step 7: Set JWT Secrets
```powershell
fly secrets set Jwt__Key="YourSuperSecretKey12345678901234567890123456789012"
fly secrets set Jwt__Issuer="GAM106ASM"
fly secrets set Jwt__Audience="UnityClient"
```

## Step 8: Set Email Secrets (Optional)
```powershell
fly secrets set SmtpHost="smtp.gmail.com"
fly secrets set SmtpPort="587"
fly secrets set SmtpUsername="your-email@gmail.com"
fly secrets set SmtpPassword="your-app-password"
fly secrets set SmtpFromEmail="your-email@gmail.com"
```

## Step 9: Deploy!
```powershell
fly deploy
```

## Step 10: Open Your App
```powershell
fly open
```

---

## 🗄️ Import Database Data

### Connect to Database
```powershell
fly postgres connect -a gam106asm-db
```

### Import from Local
```powershell
# Export local database first
pg_dump -h localhost -U postgres -d gam106asm -f backup.sql

# Then in Fly.io postgres console:
\i backup.sql
```

---

## 🎮 Update Unity

Change API URL in Unity:
```csharp
private const string BASE_URL = "https://gam106asm.fly.dev/api";
```

---

## 📝 Useful Commands

```powershell
# View logs
fly logs

# Restart app
fly apps restart gam106asm

# Check status
fly status

# SSH into container
fly ssh console

# List secrets
fly secrets list
```

---

## ✅ Your Live App

**Web**: https://gam106asm.fly.dev  
**API**: https://gam106asm.fly.dev/api  
**Admin**: https://gam106asm.fly.dev/Admin/Dashboard

---

## 🆘 If Something Goes Wrong

```powershell
# Check logs for errors
fly logs -a gam106asm

# Destroy and start over
fly apps destroy gam106asm
fly postgres destroy gam106asm-db

# Then start from Step 3 again
```

---

## 💰 Cost: $0 (FREE Tier)

Your configuration uses only free resources! 🎉
