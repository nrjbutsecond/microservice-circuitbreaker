# API Key Generator Script for PowerShell
# Usage: .\generate-api-keys.ps1

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  API Key Generator for Microservices" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Function to generate a secure random API key
function Generate-ApiKey {
    param (
        [string]$ServiceName
    )

    # Generate 32 bytes random string
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($bytes)
    $randomHex = [System.BitConverter]::ToString($bytes).Replace("-", "").ToLower()

    return "$ServiceName-$randomHex"
}

Write-Host "Generating API Keys for services..." -ForegroundColor Yellow
Write-Host ""

# Generate keys for each service
$readingServiceKey = Generate-ApiKey -ServiceName "reading"
$comicServiceKey = Generate-ApiKey -ServiceName "comic"
$userServiceKey = Generate-ApiKey -ServiceName "user"

Write-Host "✅ API Keys Generated:" -ForegroundColor Green
Write-Host ""
Write-Host "ReadingService API Key:" -ForegroundColor Cyan
Write-Host $readingServiceKey -ForegroundColor White
Write-Host ""
Write-Host "ComicService API Key:" -ForegroundColor Cyan
Write-Host $comicServiceKey -ForegroundColor White
Write-Host ""
Write-Host "UserService API Key:" -ForegroundColor Cyan
Write-Host $userServiceKey -ForegroundColor White
Write-Host ""

# Create .env file for Docker Compose
$envContent = @"
# Generated API Keys - DO NOT COMMIT TO GIT
# Generated at: $(Get-Date)

# ReadingService API Key
READING_SERVICE_API_KEY=$readingServiceKey

# ComicService API Key
COMIC_SERVICE_API_KEY=$comicServiceKey

# UserService API Key
USER_SERVICE_API_KEY=$userServiceKey
"@

$envContent | Out-File -FilePath ".env.apikeys" -Encoding UTF8

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "✅ API Keys saved to: .env.apikeys" -ForegroundColor Green
Write-Host ""
Write-Host "⚠️  IMPORTANT SECURITY NOTES:" -ForegroundColor Yellow
Write-Host "1. Add .env.apikeys to .gitignore"
Write-Host "2. Share keys securely (use password manager)"
Write-Host "3. Rotate keys every 30 days in production"
Write-Host "4. Never commit keys to version control"
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Copy the keys above to your appsettings.json files"
Write-Host "2. Or use environment variables from .env.apikeys"
Write-Host "3. Restart all services"
Write-Host "==========================================" -ForegroundColor Cyan
