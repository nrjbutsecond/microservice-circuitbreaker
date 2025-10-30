# Quick Start Guide

## Prerequisites

Make sure you have the following running:
- Node.js (v20+)
- All backend services (UserService, ComicService, ReadingService)

## Backend Services

Start all three backend services in separate terminals:

### Terminal 1 - UserService (Port 5001)
```bash
cd UserService.Api
dotnet run
```

### Terminal 2 - ComicService (Port 5003)
```bash
cd ComicService.Api
dotnet run
```

### Terminal 3 - ReadingService (Port 5002)
```bash
cd ReadingService.API
dotnet run
```

## Frontend Setup

### Terminal 4 - React Frontend (Port 5173)
```bash
cd FE2/FE2
npm install
npm run dev
```

## Access the Application

Open your browser and navigate to:
```
http://localhost:5173
```

## First Time Setup

1. **Register a new user**:
   - Click "Register" in the navigation
   - Fill in username, email, password, and full name
   - Submit the form

2. **Login**:
   - After registration, you'll be redirected to login
   - Enter your email and password
   - Click "Sign in"

3. **Browse Comics**:
   - You should see the comics list (if any comics exist in the database)
   - Use the search bar to find specific comics
   - Click on a comic to view details

4. **Read Chapters**:
   - From the comic detail page, click on any chapter to start reading
   - Your reading progress will be automatically tracked
   - Use Previous/Next buttons to navigate between chapters

5. **View Reading History**:
   - Click "History" in the navigation
   - See your reading statistics and recent activity

## Testing Circuit Breaker

To test the circuit breaker functionality:

1. **Stop ReadingService**:
   - Stop the ReadingService backend (Terminal 3)
   - The comic list and details should still work
   - Reading stats will fallback to default values

2. **Restart ReadingService**:
   - Start ReadingService again
   - Refresh the page to see stats updated

## Common Issues

### CORS Errors
- Make sure all backend services have CORS enabled for `http://localhost:5173`
- Check the backend Program.cs files for CORS configuration

### API Connection Errors
- Verify all backend services are running on correct ports
- Check browser console for specific error messages
- Verify API URLs in `src/services/api.js`

### Authentication Issues
- Clear browser localStorage and try again
- Check if UserService is running properly
- Verify database connections in backend services

## API Endpoints Used

### UserService (http://localhost:5001/api)
- POST /auth/login - User login
- POST /users - User registration
- GET /users/{id} - Get user details

### ComicService (http://localhost:5003/api)
- GET /comics - Get paginated comics list
- GET /comics/{id} - Get comic details
- GET /comics/search?keyword={keyword} - Search comics
- GET /chapters?comicId={id} - Get chapters for a comic
- GET /chapters/{number}?comicId={id} - Get specific chapter

### ReadingService (http://localhost:5002/api)
- POST /reading/track - Track reading progress
- GET /reading/history?userId={id} - Get user reading history
- GET /reading/stats/user/{id} - Get user reading statistics
- GET /stats/comic/{id} - Get comic reading stats (Circuit Breaker Point)

## Development Tips

- Hot reload is enabled - changes will reflect immediately
- Check browser console for any errors
- Use React Developer Tools for debugging
- Backend services have Swagger UI at `/swagger`

Enjoy reading! 📚
