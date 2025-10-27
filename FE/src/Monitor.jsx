import { useState } from 'react';
import useSignalR from './hooks/useSignalR';
import ConnectionPanel from './components/ConnectionPanel';
import StatsOverview from './components/StatsOverview';
import ServiceCard from './components/ServiceCard';
import './NewApp.css';

function Monitor() {
  const [hubUrl, setHubUrl] = useState('http://localhost:5001/hubs/circuitbreaker');
  const {
    isConnected,
    isConnecting,
    data,
    error,
    connect,
    disconnect,
    toggleAutoReconnect,
  } = useSignalR(hubUrl);

  const handleConnect = () => {
    connect();
  };

  const handleDisconnect = () => {
    disconnect();
  };

  const handleUrlChange = (newUrl) => {
    if (!isConnected && !isConnecting) {
      setHubUrl(newUrl);
    }
  };

  const handleAutoReconnectChange = (enabled) => {
    toggleAutoReconnect(enabled);
  };

  const getServices = () => {
    if (data && data.Type === 'stats' && data.Services) {
      return data.Services;
    }
    return [];
  };

  return (
    <div className="app">
      <header className="app-header">
        <h1>🔴 Circuit Breaker Real-Time Monitor</h1>
        <p className="subtitle">Monitor your microservices circuit breaker status in real-time</p>
      </header>

      <div className="app-container">
        <ConnectionPanel
          url={hubUrl}
          onUrlChange={handleUrlChange}
          isConnected={isConnected}
          isConnecting={isConnecting}
          onConnect={handleConnect}
          onDisconnect={handleDisconnect}
          error={error}
          onAutoReconnectChange={handleAutoReconnectChange}
        />

        {data && data.Type === 'stats' && (
          <>
            <StatsOverview data={data} />

            <div className="services-section">
              <h2 className="section-title">
                📡 Services Status ({data.totalServices})
              </h2>
              {getServices().length > 0 ? (
                <div className="services-grid">
                  {getServices().map((service, index) => (
                    <ServiceCard key={`${service.serviceName}-${index}`} service={service} />
                  ))}
                </div>
              ) : (
                <div className="no-services">
                  <p>No services registered yet. Services will appear here once they start making calls.</p>
                </div>
              )}
            </div>
          </>
        )}

        {!isConnected && !isConnecting && !data && (
          <div className="placeholder">
            <div className="placeholder-icon">🔌</div>
            <h2>Not Connected</h2>
            <p>Connect to a WebSocket endpoint to start monitoring circuit breaker statistics</p>
            <div className="placeholder-features">
              <div className="feature">
                <span className="feature-icon">📊</span>
                <span>Real-time statistics</span>
              </div>
              <div className="feature">
                <span className="feature-icon">🔄</span>
                <span>Auto-refresh every 2 seconds</span>
              </div>
              <div className="feature">
                <span className="feature-icon">📈</span>
                <span>Visual health indicators</span>
              </div>
              <div className="feature">
                <span className="feature-icon">🎯</span>
                <span>Circuit breaker state tracking</span>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default Monitor;
