# Deployment Guide

## ⚠️ Important Note
MSDeploy (Web Deploy) is not available on Linux. For local Linux deployments, use the manual methods below. **The recommended approach is using GitHub Actions which runs on Windows and has MSDeploy built-in.**

---

## 🚀 GitHub Actions Deployment (Recommended)

### Setup

1. **Add Secrets to GitHub Repository:**
   ```
   Repository → Settings → Secrets and variables → Actions → New repository secret
   ```
   Add these secrets:
   - `DEPLOY_USERNAME`: `site8805`
   - `DEPLOY_PASSWORD`: `Xr9_7+sQ!B5p`

2. **Configure Branch:**
   - The workflow deploys on push to `main` branch
   - Edit `.github/workflows/deploy.yml` to change the branch

3. **Deploy:**
   - **Automatic:** Push to `main` branch
   - **Manual:** Go to Actions → Deploy to Production → Run workflow

---

## 🐧 Local Deployment (Linux/Mac)

### Build Package

```bash
./deploy.sh
```

This creates a deployment package in `publish-output/`

### Option 1: FTP Upload (Recommended for Linux)

Install lftp:
```bash
sudo apt-get install lftp
```

Run interactive FTP upload:
```bash
./deploy.sh --ftp-upload
```

Or manual FTP:
```bash
# Build first
dotnet publish Scheds/Scheds.MVC.csproj -c Release -o publish-output

# Upload using lftp
lftp -c "open -u USERNAME,PASSWORD ftp.runasp.net; mirror -R publish-output/ /site/wwwroot/"
```

### Option 2: Manual Upload via Control Panel

1. Run `./deploy.sh` to create the package
2. Login to your hosting control panel
3. Upload contents of `publish-output/` to the site root

---

## 🪟 Windows Deployment

On Windows, you can use the original publish settings:

```powershell
dotnet publish Scheds/Scheds.MVC.csproj /p:PublishProfile=path/to/profile.pubxml
```

Or use Visual Studio's built-in publish feature.

---

## Troubleshooting

### Build Warnings
The nullable reference warnings can be safely ignored or fixed:
- `CS8603`: Possible null reference return
- `CS8601`: Possible null reference assignment

### Connection Issues
- Verify credentials in publish settings
- Check firewall allows connection to `site8805.siteasp.net`
- Ensure Web Deploy is enabled on hosting

### GitHub Actions Fails
- Verify secrets are set correctly
- Check workflow logs for specific errors
- Ensure Windows runner has network access

---

## Security Notes

⚠️ **Important:** 
- `.publishSettings/` is in `.gitignore` 
- Never commit credentials to repository
- Use GitHub Secrets for CI/CD
- Rotate passwords regularly
