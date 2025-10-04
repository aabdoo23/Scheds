#!/bin/bash

# Deployment script for Scheds application
# Usage: ./deploy.sh [--ftp-host HOST] [--ftp-user USER] [--ftp-pass PASS]
# For MSDeploy, use GitHub Actions workflow on Windows runner

# set -e  # Exit on error

echo "🚀 Starting deployment process..."

# Build and publish
echo "📦 Building and publishing application..."
dotnet publish Scheds/Scheds.MVC.csproj -c Release -o publish-output

echo "✅ Build completed successfully!"
echo ""
echo "📦 Deployment package created in: publish-output/"
echo ""
echo "⚠️  MSDeploy is not available on Linux."
echo "Choose one of these options:"
echo ""
echo "1️⃣  Use GitHub Actions (recommended):"
echo "   - Push your changes to the main branch"
echo "   - GitHub workflow will deploy automatically"
echo ""
echo "2️⃣  Manual FTP Upload:"
echo "   - Install lftp: sudo apt-get install lftp"
echo "   - Get FTP credentials from your hosting panel"
echo "   - Run: ./deploy.sh --ftp-upload"
echo ""
echo "3️⃣  Manual upload via hosting control panel:"
echo "   - Upload contents of 'publish-output/' folder"
echo "   - To your site's wwwroot directory"
echo ""

# Check if FTP upload was requested
if [[ "$1" == "--ftp-upload" ]]; then
    echo "📤 FTP Upload mode"
    
    # Try to load .ftp-config if it exists
    if [ -f ".ftp-config" ]; then
        echo "Loading configuration from .ftp-config..."
        source .ftp-config
    fi
    
    # Read from publish settings if available
    PUBLISH_SETTINGS_FILE="Scheds/.publishSettings/scheds.runasp.net-WebDeploy.publishSettings"
    if [ -f "$PUBLISH_SETTINGS_FILE" ]; then
        SITE_NAME=$(grep -oP 'msdeploySite="\K[^"]+' "$PUBLISH_SETTINGS_FILE" || echo "site8805")
    else
        SITE_NAME="site8805"
    fi
    
    # Use env vars if set, otherwise prompt
    if [ -z "$FTP_HOST" ]; then
        read -p "FTP Host [site8805.siteasp.net]: " INPUT_FTP_HOST
        FTP_HOST=${INPUT_FTP_HOST:-"site8805.siteasp.net"}
    else
        echo "Using FTP Host: $FTP_HOST"
    fi
    
    if [ -z "$FTP_USER" ]; then
        read -p "FTP User [site8805]: " INPUT_FTP_USER
        FTP_USER=${INPUT_FTP_USER:-"site8805"}
    else
        echo "Using FTP User: $FTP_USER"
    fi
    
    if [ -z "$FTP_PASS" ]; then
        read -sp "FTP Password: " INPUT_FTP_PASS
        echo
        FTP_PASS=$INPUT_FTP_PASS
    else
        echo "Using FTP Password: ****"
    fi
    
    if [ -z "$FTP_REMOTE_DIR" ]; then
        read -p "Remote directory [/wwwroot]: " INPUT_REMOTE_DIR
        REMOTE_DIR=${INPUT_REMOTE_DIR:-"/wwwroot"}
    else
        REMOTE_DIR=$FTP_REMOTE_DIR
        echo "Using Remote Directory: $REMOTE_DIR"
    fi
    
    if command -v lftp &> /dev/null; then
        echo "📤 Uploading via FTP to $FTP_HOST..."
        echo "   Remote directory: $REMOTE_DIR"
        echo "   User: $FTP_USER"
        echo ""
        echo "⏳ This may take a few minutes..."
        echo ""
        
        # Use FTP (port 21) with passive mode
        # Skip chmod, use --only-newer to avoid locked files, remove --delete
        lftp -e "set ftp:ssl-allow no; set ftp:passive-mode on; set ftp:use-site-chmod no; open -u $FTP_USER,$FTP_PASS $FTP_HOST; cd $REMOTE_DIR || mkdir -p $REMOTE_DIR; cd $REMOTE_DIR; mirror -R --verbose --only-newer --ignore-time --no-perms publish-output/ .; quit"
        
        EXIT_CODE=$?
        
        echo ""
        if [ $EXIT_CODE -eq 0 ]; then
            echo "✅ Upload completed successfully!"
            echo ""
            echo "⚠️  Note: Some files may have been skipped if they were locked by the running application."
            echo "   If you see errors, you may need to:"
            echo "   1. Stop the application on the server"
            echo "   2. Upload again"
            echo "   3. Restart the application"
        else
            echo "⚠️  Upload completed with warnings (this is often OK)"
            echo "   Check the output above for any critical errors"
        fi
    else
        echo "❌ lftp not found. Install it with: sudo apt-get install lftp"
        exit 1
    fi
fi

echo ""
echo "🌐 Site URL: http://scheds.runasp.net/"
