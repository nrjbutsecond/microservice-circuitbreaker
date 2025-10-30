# 🔐 API Key Authentication - Service-to-Service

## 📋 Overview

This project implements API Key authentication for service-to-service communication to ensure only authorized services can call each other's APIs.

## 🏗️ Architecture

```
┌─────────────────┐           ┌─────────────────┐
│  ReadingService │  ────────>│   UserService   │
│  (Port 5002)    │  API Key  │   (Port 5001)   │
└─────────────────┘           └─────────────────┘
        │
        │ API Key
        ↓
┌─────────────────┐           ┌─────────────────┐
│  ComicService   │  <────────│ ReadingService  │
│  (Port 5003)    │  API Key  │   (Port 5002)   │
└─────────────────┘           └─────────────────┘
```

### Service Communication:
- **ReadingService** → **UserService** (validate users)
- **ReadingService** → **ComicService** (get comic details)
- **ComicService** → **ReadingService** (get reading stats)

---

## 🔑 API Keys

### Generated Keys:

```
ReadingService: reading-3fb64a14667b08f9da13cd8f47230aea465cfb1085a75af89c49d1339d96d745
ComicService:   comic-419ad2718ae073e2eb04c0363787b0ff901e437ddf3e6f5593f3c35fa2a11a03
UserService:    user-3461081afe0b3838e672773b80e8566324356835cf5e89ee249b221a8135e556
```

### Generate New Keys:

```bash
# Linux/Mac
./generate-api-keys.sh

# Windows PowerShell
.\generate-api-keys.ps1
```

---

## 🔄 How It Works

### 1. **Service Sends Request (ReadingService → UserService)**

```
┌────────────────────────────────────────────────┐
│ ReadingService Startup                         │
└────────────────────────────────────────────────┘
    ↓
Load Configuration:
  appsettings.json:
    ServiceAuthentication:
      ApiKey: "reading-3fb64a14667b..."
    ↓
Register HttpClient with API Key:
  services.AddHttpClient<IUserServiceClient>((client) => {
      client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
  });
    ↓
┌────────────────────────────────────────────────┐
│ HTTP Request to UserService                    │
└────────────────────────────────────────────────┘
    ↓
GET http://localhost:5001/api/users/validate/123
Headers:
    X-API-Key: reading-3fb64a14667b...
```

### 2. **Service Validates Request (UserService)**

```
┌────────────────────────────────────────────────┐
│ UserService Receives Request                   │
└────────────────────────────────────────────────┘
    ↓
ServiceAuthenticationMiddleware intercepts
    ↓
Extract API Key from Header:
    X-API-Key: reading-3fb64a14667b...
    ↓
Load AllowedServices from appsettings.json:
    {
      "ReadingService": {
        "ApiKey": "reading-3fb64a14667b...",
        "Permissions": ["validate-user", "get-user"]
      }
    }
    ↓
Compare API Keys:
    Received: reading-3fb64a14667b...
    Expected: reading-3fb64a14667b...
    Match? ✅ YES
    ↓
Log: "✅ Service authenticated: ReadingService"
    ↓
Set HttpContext.Items["ServiceName"] = "ReadingService"
    ↓
Allow request to proceed to controller
```

---

## 📁 Configuration Files

### UserService.Api/appsettings.json

```json
{
  "AllowedServices": {
    "ReadingService": {
      "ApiKey": "reading-3fb64a14667b08f9da13cd8f47230aea465cfb1085a75af89c49d1339d96d745",
      "Permissions": ["validate-user", "get-user"]
    }
  }
}
```

### ReadingService.API/appsettings.json

```json
{
  "ServiceAuthentication": {
    "ApiKey": "reading-3fb64a14667b08f9da13cd8f47230aea465cfb1085a75af89c49d1339d96d745"
  },
  "AllowedServices": {
    "ComicService": {
      "ApiKey": "comic-419ad2718ae073e2eb04c0363787b0ff901e437ddf3e6f5593f3c35fa2a11a03",
      "Permissions": ["get-stats"]
    }
  }
}
```

### ComicService.Api/appsettings.json

```json
{
  "ServiceAuthentication": {
    "ApiKey": "comic-419ad2718ae073e2eb04c0363787b0ff901e437ddf3e6f5593f3c35fa2a11a03"
  },
  "AllowedServices": {
    "ReadingService": {
      "ApiKey": "reading-3fb64a14667b08f9da13cd8f47230aea465cfb1085a75af89c49d1339d96d745",
      "Permissions": ["get-comic", "batch-comics"]
    }
  }
}
```

---

## 💻 Code Implementation

### Middleware (All Services)

**File:** `{Service}.Api/Middleware/ServiceAuthenticationMiddleware.cs`

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Skip auth for health checks and swagger
    if (path.StartsWith("/health") || path.StartsWith("/swagger"))
    {
        await _next(context);
        return;
    }

    // Get API Key from header
    if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
    {
        context.Response.StatusCode = 401;
        return; // Missing API Key
    }

    // Validate against allowed services
    var allowedServices = _config.GetSection("AllowedServices").GetChildren();

    foreach (var service in allowedServices)
    {
        if (apiKey == service.GetValue<string>("ApiKey"))
        {
            // ✅ Valid API Key
            context.Items["ServiceName"] = service.Key;
            await _next(context);
            return;
        }
    }

    // ❌ Invalid API Key
    context.Response.StatusCode = 401;
}
```

### HttpClient Configuration

**File:** `Reading.Infrastructure/DependencyInjection.cs`

```csharp
// Get API Key from configuration
var apiKey = configuration["ServiceAuthentication:ApiKey"];

services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
{
    client.BaseAddress = new Uri(configuration["Services:User:Url"]);

    // Add API Key to all requests
    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }
});
```

### Middleware Registration

**File:** `UserService.Api/Program.cs`

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ServiceAuthenticationMiddleware>(); // 🔑 API Key auth
app.UseCors("AllowAll");
```

---

## 🧪 Testing

### Test Authentication Success

```bash
# Call UserService with valid API key
curl http://localhost:5001/api/users/validate/1 \
  -H "X-API-Key: reading-3fb64a14667b08f9da13cd8f47230aea465cfb1085a75af89c49d1339d96d745"

# Expected: 200 OK
```

### Test Authentication Failure

```bash
# Call without API key
curl http://localhost:5001/api/users/validate/1

# Expected: 401 Unauthorized
# Response: { "success": false, "message": "Missing X-API-Key header..." }
```

```bash
# Call with invalid API key
curl http://localhost:5001/api/users/validate/1 \
  -H "X-API-Key: invalid-key"

# Expected: 401 Unauthorized
# Response: { "success": false, "message": "Invalid API Key" }
```

### Test Service-to-Service Communication

```bash
# Call ReadingService (which will call UserService internally)
curl -X POST http://localhost:5002/api/reading/track \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "1",
    "comicId": "1",
    "chapterId": "1",
    "chapterNumber": 1
  }'

# ReadingService will automatically add API key when calling UserService
# Check logs for: "✅ Service authenticated: ReadingService"
```

---

## 📊 Logs

### UserService Logs (Successful Authentication)

```
[Information] ✅ Service authenticated: ReadingService
[Information] Validation requested by ReadingService
```

### UserService Logs (Failed Authentication)

```
[Warning] 🔴 Missing X-API-Key header from 127.0.0.1
```

```
[Warning] 🔴 Invalid API Key from 127.0.0.1
```

---

## 🔒 Security Best Practices

### ✅ DO:
1. **Use Environment Variables in Production**
   ```bash
   export ServiceAuthentication__ApiKey="your-key"
   ```

2. **Rotate Keys Regularly**
   - Development: Every 3 months
   - Production: Every 30 days

3. **Store Keys Securely**
   - Use Azure Key Vault / AWS Secrets Manager
   - Never commit `.env.apikeys` to Git

4. **Monitor Failed Authentications**
   - Set up alerts for 401 errors
   - Investigate suspicious patterns

5. **Use Different Keys Per Service**
   - If one service is compromised, only rotate that key

### ❌ DON'T:
1. **Hardcode API Keys in Code**
   ```csharp
   // ❌ BAD
   client.DefaultRequestHeaders.Add("X-API-Key", "abc123");

   // ✅ GOOD
   var apiKey = configuration["ServiceAuthentication:ApiKey"];
   client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
   ```

2. **Commit API Keys to Git**
   - Always add `.env.*` to `.gitignore`

3. **Share the Same Key Across All Services**
   - Each service should have unique keys

4. **Use Short/Weak Keys**
   - Minimum 32 bytes (64 hex characters)

---

## 🔧 Production Deployment

### Using Environment Variables

**docker-compose.yml:**
```yaml
services:
  user-service:
    environment:
      - AllowedServices__ReadingService__ApiKey=${READING_SERVICE_API_KEY}
    env_file:
      - .env.production

  reading-service:
    environment:
      - ServiceAuthentication__ApiKey=${READING_SERVICE_API_KEY}
    env_file:
      - .env.production
```

**.env.production:** (NOT in Git)
```bash
READING_SERVICE_API_KEY=prod-reading-very-long-secure-random-string
COMIC_SERVICE_API_KEY=prod-comic-very-long-secure-random-string
```

### Azure Key Vault Integration

```csharp
// Program.cs
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri("https://my-vault.vault.azure.net/"),
        new DefaultAzureCredential()
    );
}
```

---

## 🐛 Troubleshooting

### Issue: 401 Unauthorized

**Check:**
1. API Key is in request header: `X-API-Key`
2. API Key matches exactly (no extra spaces)
3. Middleware is registered in `Program.cs`
4. Configuration is loaded correctly

**Logs:**
```bash
# Check if middleware is intercepting requests
grep "Service authenticated" logs/*.log

# Check for authentication failures
grep "Invalid API Key" logs/*.log
```

### Issue: Services Can't Communicate

**Check:**
1. HttpClient has API Key in `DefaultRequestHeaders`
2. Service URLs are correct in `appsettings.json`
3. Both services are running
4. Firewall/network allows communication

---

## 📚 Additional Resources

- [ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware)
- [HttpClient Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)
- [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/)
- [API Security Best Practices](https://owasp.org/www-project-api-security/)

---

## 🔄 Key Rotation Procedure

1. Generate new API keys:
   ```bash
   ./generate-api-keys.sh
   ```

2. Update `appsettings.json` OR environment variables

3. Restart services in this order:
   - UserService (validates requests)
   - ReadingService (makes requests)
   - ComicService (makes and validates requests)

4. Verify communication:
   ```bash
   curl http://localhost:5002/api/reading/track -X POST ...
   ```

5. Check logs for authentication success

---

**Generated**: 2025-10-30
**Version**: 1.0
**Last Updated**: Initial API Key Authentication Implementation
