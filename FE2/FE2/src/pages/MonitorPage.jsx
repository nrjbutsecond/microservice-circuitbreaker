import { useState } from 'react';
import useSignalR from '../hooks/useSignalR';
import ConnectionPanel from '../components/monitor/ConnectionPanel';
import StatsOverview from '../components/monitor/StatsOverview';
import ServiceCard from '../components/monitor/ServiceCard';
import './MonitorPage.css';

function MonitorPage() {
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
    <div className="monitor-app">
      <header className="monitor-app-header">
        <h1>🔴 Circuit Breaker Real-Time Monitor</h1>
        <p className="monitor-subtitle">Monitor your microservices circuit breaker status in real-time</p>
      </header>

      <div className="monitor-app-container">
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

            <div className="monitor-services-section">
              <h2 className="monitor-section-title">
                📡 Services Status ({data.totalServices})
              </h2>
              {getServices().length > 0 ? (
                <div className="monitor-services-grid">
                  {getServices().map((service, index) => (
                    <ServiceCard key={`${service.serviceName}-${index}`} service={service} />
                  ))}
                </div>
              ) : (
                <div className="monitor-no-services">
                  <p>No services registered yet. Services will appear here once they start making calls.</p>
                </div>
              )}
            </div>
          </>
        )}

        {!isConnected && !isConnecting && !data && (
          <div className="monitor-placeholder">
            <div className="monitor-placeholder-icon">🔌</div>
            <h2>Not Connected</h2>
            <p>Connect to a WebSocket endpoint to start monitoring circuit breaker statistics</p>
            <div className="monitor-placeholder-features">
              <div className="monitor-feature">
                <span className="monitor-feature-icon">📊</span>
                <span>Real-time statistics</span>
              </div>
              <div className="monitor-feature">
                <span className="monitor-feature-icon">🔄</span>
                <span>Auto-refresh every 2 seconds</span>
              </div>
              <div className="monitor-feature">
                <span className="monitor-feature-icon">📈</span>
                <span>Visual health indicators</span>
              </div>
              <div className="monitor-feature">
                <span className="monitor-feature-icon">🎯</span>
                <span>Circuit breaker state tracking</span>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default MonitorPage;
