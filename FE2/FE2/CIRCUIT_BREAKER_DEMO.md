# Circuit Breaker Pattern Demonstration Guide

## 🎯 Overview

This Comic Reader application demonstrates the **Circuit Breaker Pattern** using **Polly** library. The circuit breaker prevents cascading failures when microservices are down or slow.

## 🔄 Circuit Breaker Flow

### Architecture:
```
Frontend (React)
    ↓
ComicService (Port 5003) ──[Circuit Breaker]──> ReadingService (Port 5002)
    ↓                                                    ↓
ReadingService (Port 5002) ──[Circuit Breaker]──> UserService (Port 5001)
                           └─[Circuit Breaker]──> ComicService (Port 5003)
```

### Key Circuit Breaker Points:

1. **ComicService → ReadingService**
   - When: Getting comic statistics (views, readers, trending)
   - Endpoint: `GET /api/stats/comic/{id}`
   - Fallback: Returns default stats (0 views, 0 readers)

2. **ReadingService → UserService**
   - When: Validating users before tracking reading
   - Endpoint: `GET /api/users/validate/{userId}`
   - Fallback: Allows reading to continue

3. **ReadingService → ComicService**
   - When: Enriching reading history with comic details
   - Endpoint: `GET /api/comics/batch`
   - Fallback: Shows reading history without comic titles

## ⚙️ Circuit Breaker Configuration

Location: `Share/Resilience/CircuitBreakerOptions.cs`

```csharp
SamplingDurationSeconds = 10     // Monitor failures for 10 seconds
FailureRatio = 0.5               // Open circuit if 50% fail
MinimumThroughput = 3            // Need at least 3 requests
BreakDurationSeconds = 30        // Stay open for 30 seconds
TimeoutSeconds = 3               // Request timeout
RetryCount = 2                   // Retry 2 times before failing
RetryDelaySeconds = 1            // Wait 1 second between retries
```

## 🧪 Demonstration Steps

### **Test 1: Normal Operation (All Services Running)**

1. **Start all services:**
   ```powershell
   # Terminal 1 - UserService
   cd UserService.Api
   dotnet run
   
   # Terminal 2 - ReadingService
   cd ReadingService.API
   dotnet run
   
   # Terminal 3 - ComicService
   cd ComicService.Api
   dotnet run
   
   # Terminal 4 - Frontend
   cd FE2/FE2
   npm run dev
   ```

2. **Open browser:** `http://localhost:5173`

3. **Register and login**

4. **Browse comics:**
   - Click on any comic to view details
   - **Observe:** Reading statistics are displayed (Total Reads, Unique Readers, Active 24h)
   - **Check logs:** ComicService successfully calls ReadingService

### **Test 2: ReadingService Failure - Circuit Breaker in Action**

1. **Stop ReadingService:**
   - Go to Terminal 2 (ReadingService)
   - Press `Ctrl+C` to stop the service

2. **Browse comics again:**
   - Refresh the comic list page
   - Click on a comic to view details
   - **Observe:** 
     - ✅ Comic details still load successfully
     - ✅ Stats show as 0 (fallback values)
     - ❌ No cascading failure
     - ❌ Frontend doesn't crash

3. **Check ComicService logs:**
   - You should see error messages like:
   ```
   🔴 CIRCUIT BREAKER OPEN: Reading-Service is unavailable
   ⚠️ Using fallback stats for comic {id}
   ```

4. **Try multiple requests:**
   - Click on different comics multiple times
   - After 3 failed requests in 10 seconds:
     - Circuit opens
     - Future requests fail immediately (no delay)
     - Service recovers faster

### **Test 3: Timeout Simulation**

1. **Edit ReadingService StatsController:**
   - File: `ReadingService.API/Controllers/StatsController.cs`
   - Line 32: **Uncomment** this line:
   ```csharp
   await Task.Delay(5000, ct);  // Simulates 5-second delay
   ```

2. **Keep all services running**

3. **Browse comics:**
   - Click on a comic
   - **Observe:**
     - Request times out after 3 seconds (configured timeout)
     - Circuit breaker kicks in after retries
     - Fallback stats are shown
     - User experience remains smooth

4. **Check logs:**
   ```
   ⏱️ TIMEOUT calling Reading-Service for comic {id}
   🔴 CIRCUIT BREAKER OPEN after multiple timeouts
   ```

### **Test 4: Error Response Simulation**

1. **Edit ReadingService StatsController:**
   - Line 35: **Uncomment** this line:
   ```csharp
   return StatusCode(500, "Service temporarily unavailable");
   ```

2. **Browse comics:**
   - Click on different comics
   - **Observe:**
     - After multiple 500 errors, circuit opens
     - Subsequent requests fail fast
     - Fallback values are used

### **Test 5: Circuit Recovery**

1. **After stopping ReadingService:**
   - Wait 30 seconds (BreakDurationSeconds)
   - Circuit moves to **Half-Open** state

2. **Restart ReadingService:**
   ```powershell
   cd ReadingService.API
   dotnet run
   ```

3. **Browse comics:**
   - First request tests if service is back (Half-Open)
   - If successful, circuit **closes**
   - Normal operation resumes
   - Stats are displayed again

4. **Observe logs:**
   ```
   🔹 Calling Reading-Service: Get stats for comic {id}
   ✅ Stats fetched successfully
   Circuit breaker: CLOSED (healthy)
   ```

## 📊 Expected Behaviors

### ✅ **With Circuit Breaker (Current Implementation)**

| Scenario | Behavior | User Experience |
|----------|----------|-----------------|
| Service Down | Fails fast after circuit opens | Smooth, no delays |
| Service Slow | Timeout protection, retries | Some delay, then fallback |
| Service Recovered | Automatic recovery | Seamless transition |
| Multiple Failures | Circuit opens, prevents overload | Protected system |

### ❌ **Without Circuit Breaker (What Would Happen)**

| Scenario | Behavior | User Experience |
|----------|----------|-----------------|
| Service Down | Each request waits for timeout | Long delays, hanging |
| Service Slow | All requests queue up | System overload |
| Service Recovered | Slow to detect, hammers service | Poor recovery |
| Multiple Failures | Cascading failures across services | Total system failure |

## 🔍 Monitoring Circuit Breaker

### **Check Logs in Real-Time:**

**ComicService logs:**
```bash
# Watch for these messages:
🔹 Calling Reading-Service: Get stats for comic {id}
✅ Stats fetched successfully
⚠️ Reading-Service returned 500
🔴 CIRCUIT BREAKER OPEN
⏱️ TIMEOUT calling Reading-Service
```

**ReadingService logs:**
```bash
📊 Stats request for comic: {ComicId}
```

### **Browser Developer Tools:**

1. Open DevTools (F12)
2. Go to **Network** tab
3. Watch API calls to `/api/comics/{id}`
4. When circuit is open:
   - Response comes back quickly (not waiting for timeout)
   - Stats show as default values

## 🎓 Key Learning Points

### 1. **Circuit States:**
- **CLOSED** (Normal): Requests pass through normally
- **OPEN** (Failed): Requests fail immediately, no calls made
- **HALF-OPEN** (Testing): Single request tests if service recovered

### 2. **Benefits Demonstrated:**
- ✅ Prevents cascading failures
- ✅ Fast failure detection
- ✅ Automatic recovery
- ✅ Resource protection (no waiting threads)
- ✅ Better user experience

### 3. **Resilience Features:**
- **Retry**: Automatically retries failed requests (2 times)
- **Timeout**: Prevents hanging requests (3 seconds)
- **Circuit Breaker**: Stops calling failing services (30 seconds)
- **Fallback**: Provides default values when service unavailable

## 📝 Demo Script for Presentation

```markdown
1. "Let me show you all services running normally..."
   - Browse comics, show stats working

2. "Now watch what happens when ReadingService goes down..."
   - Stop ReadingService
   - Browse comics - still works!
   - Show logs with circuit breaker messages

3. "Notice the comic details still load, just without live stats..."
   - Point out 0 views instead of crash
   - Show fast response (no timeout delay)

4. "Let me restart the service..."
   - Start ReadingService
   - Browse comics - stats are back!
   - Circuit automatically closed

5. "This is the Circuit Breaker Pattern in action!"
   - Prevents cascading failures
   - Automatic recovery
   - Graceful degradation
```

## 🛠️ Advanced Testing

### **Load Testing with Circuit Breaker:**

```powershell
# Send multiple rapid requests to trigger circuit
for ($i=1; $i -le 10; $i++) {
    Invoke-RestMethod -Uri "http://localhost:5003/api/comics/1"
    Write-Host "Request $i completed"
}
```

### **Monitor Circuit State:**

Add logging in `ResilienceExtensions.cs` to see state changes:
```csharp
config.CircuitBreaker.OnOpened = args => {
    Console.WriteLine($"🔴 Circuit OPENED at {DateTime.Now}");
};

config.CircuitBreaker.OnClosed = args => {
    Console.WriteLine($"✅ Circuit CLOSED at {DateTime.Now}");
};

config.CircuitBreaker.OnHalfOpened = args => {
    Console.WriteLine($"🟡 Circuit HALF-OPEN at {DateTime.Now}");
};
```

## 🎯 Success Criteria

You've successfully demonstrated the Circuit Breaker pattern when:

- ✅ Comics load even when ReadingService is down
- ✅ Fallback stats (0 values) are shown instead of errors
- ✅ Circuit opens after configured number of failures
- ✅ Requests fail fast when circuit is open (no waiting)
- ✅ Circuit automatically recovers when service is back
- ✅ No cascading failures or system crashes
- ✅ Smooth user experience throughout

## 🎉 Conclusion

This demonstrates a production-ready resilience pattern that:
- Protects your system from cascading failures
- Provides graceful degradation
- Automatically recovers from failures
- Maintains great user experience

Perfect for microservices architecture! 🚀
