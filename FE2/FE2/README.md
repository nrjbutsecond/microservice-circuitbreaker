# Comic Reader Frontend

A React frontend application for the Comic Reader microservices project with circuit breaker pattern.

## Features

- **Authentication**: Login and Register pages
- **Comic Browsing**: Browse comics with pagination and search
- **Comic Details**: View comic information with reading statistics (from Circuit Breaker)
- **Chapter Reading**: Read chapters with automatic reading tracking
- **Reading History**: View your reading history and statistics
- **Responsive Design**: Mobile-friendly UI with TailwindCSS

## Tech Stack

- **React** - UI library
- **Vite** - Build tool
- **React Router** - Navigation
- **Axios** - HTTP client
- **TanStack Query** - Data fetching and caching
- **TailwindCSS** - Styling
- **Lucide React** - Icons

## Backend Services

This frontend connects to three microservices:

- **UserService** (port 5001): Authentication and user management
- **ComicService** (port 5003): Comic and chapter management
- **ReadingService** (port 5002): Reading tracking and statistics

## Setup

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm run dev
```

3. Make sure all backend services are running:
   - UserService on http://localhost:5001
   - ComicService on http://localhost:5003
   - ReadingService on http://localhost:5002

## API Configuration

API endpoints are configured in `src/services/api.js`. Update the URLs if your backend services run on different ports.

## Project Structure

```
src/
├── components/       # Reusable UI components
├── contexts/        # React contexts (Auth)
├── pages/           # Page components
├── services/        # API service layers
├── App.jsx          # Main app component with routes
├── main.jsx         # App entry point
└── index.css        # Global styles
```

## Available Pages

- `/login` - Login page
- `/register` - Registration page
- `/` - Comics list (protected)
- `/comics/:id` - Comic detail page (protected)
- `/comics/:id/chapters/:chapterNumber` - Chapter reader (protected)
- `/history` - Reading history (protected)

## Circuit Breaker Demo

The application demonstrates the circuit breaker pattern:

- **ComicService** calls **ReadingService** to get reading statistics
- If ReadingService is down, the circuit breaker prevents cascading failures
- Comics still display even if reading stats are unavailable

## Building for Production

```bash
npm run build
```

The built files will be in the `dist/` directory.

