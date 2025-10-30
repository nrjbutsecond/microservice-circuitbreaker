import { BrowserRouter, Routes, Route, Link, useLocation } from 'react-router-dom';
import Monitor from './Monitor';
import ApiTester from './ApiTester';
import './App.css';

// Home page component
function Home() {
  return (
    <div className="home-container">
      <div className="hero">
        <h1>Welcome to Microservice Monitoring</h1>
        <p>Monitor your microservices in real-time with circuit breaker insights</p>
      </div>
      <div className="features-grid">
        <div className="feature-card">
          <h3>📊 Real-time Monitoring</h3>
          <p>Track circuit breaker states and metrics across all your services</p>
        </div>
        <div className="feature-card">
          <h3>🔄 WebSocket Connection</h3>
          <p>Live updates without polling for efficient monitoring</p>
        </div>
        <div className="feature-card">
          <h3>⚡ Circuit Breaker Stats</h3>
          <p>Failure rates, success rates, and circuit states at a glance</p>
        </div>
      </div>
      <div className="cta">
        <Link to="/monitor" className="cta-button">
          Go to Circuit Breaker Monitor →
        </Link>
        <Link to="/api-tester" className="cta-button cta-button-secondary">
          Go to API Tester →
        </Link>
      </div>
    </div>
  );
}

// Navigation component
function Navigation() {
  const location = useLocation();
  
  return (
    <nav className="main-nav">
      <div className="nav-container">
        <div className="nav-brand">
          <span className="brand-icon">🔧</span>
          <span className="brand-text">Microservice Hub</span>
        </div>
        <div className="nav-links">
          <Link 
            to="/" 
            className={`nav-link ${location.pathname === '/' ? 'active' : ''}`}
          >
            Home
          </Link>
          <Link 
            to="/monitor" 
            className={`nav-link ${location.pathname === '/monitor' ? 'active' : ''}`}
          >
            Circuit Breaker Monitor
          </Link>
          <Link 
            to="/api-tester" 
            className={`nav-link ${location.pathname === '/api-tester' ? 'active' : ''}`}
          >
            API Tester
          </Link>
        </div>
      </div>
    </nav>
  );
}

function AppWithRouting() {
  return (
    <BrowserRouter>
      <div className="app-wrapper">
        <Navigation />
        <main className="main-content">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/monitor" element={<Monitor />} />
            <Route path="/api-tester" element={<ApiTester />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default AppWithRouting;
