import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { Navbar } from './components/Navbar';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ComicsListPage } from './pages/ComicsListPage';
import { ComicDetailPage } from './pages/ComicDetailPage';
import { ChapterReaderPage } from './pages/ChapterReaderPage';
import { ReadingHistoryPage } from './pages/ReadingHistoryPage';
import MonitorPage from './pages/MonitorPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function AppRoutes() {
  const { user } = useAuth();

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <Routes>
        {/* Public Routes */}
        <Route 
          path="/login" 
          element={user ? <Navigate to="/" replace /> : <LoginPage />} 
        />
        <Route 
          path="/register" 
          element={user ? <Navigate to="/" replace /> : <RegisterPage />} 
        />

        {/* Protected Routes */}
        <Route 
          path="/" 
          element={
            <ProtectedRoute>
              <ComicsListPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/comics/:id" 
          element={
            <ProtectedRoute>
              <ComicDetailPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/comics/:id/chapters/:chapterNumber" 
          element={
            <ProtectedRoute>
              <ChapterReaderPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/history" 
          element={
            <ProtectedRoute>
              <ReadingHistoryPage />
            </ProtectedRoute>
          } 
        />
        
        {/* Monitor Route */}
        <Route 
          path="/monitor" 
          element={<MonitorPage />} 
        />

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </div>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </Router>
    </QueryClientProvider>
  );
}

export default App;
