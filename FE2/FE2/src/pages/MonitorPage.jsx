import { useState, useEffect, useMemo } from 'react';
import useSignalR from '../hooks/useSignalR';
import ConnectionPanel from '../components/monitor/ConnectionPanel';
import StatsOverview from '../components/monitor/StatsOverview';
import ServiceCard from '../components/monitor/ServiceCard';
import './MonitorPage.css';

function MonitorPage() {
  // State for storing data from each service
  const [comicServiceData, setComicServiceData] = useState(null);
  const [readingServiceData, setReadingServiceData] = useState(null);

  // Comic Service Connection
  const [comicHubUrl] = useState('http://localhost:5003/hubs/circuitbreaker');
  const {
    isConnected: comicIsConnected,
    isConnecting: comicIsConnecting,
    data: comicData,
    error: comicError,
    connect: comicConnect,
    disconnect: comicDisconnect,
    toggleAutoReconnect: comicToggleAutoReconnect,
  } = useSignalR(comicHubUrl);

  // Reading Service Connection
  const [readingHubUrl] = useState('http://localhost:5002/hubs/circuitbreaker');
  const {
    isConnected: readingIsConnected,
    isConnecting: readingIsConnecting,
    data: readingData,
    error: readingError,
    connect: readingConnect,
    disconnect: readingDisconnect,
    toggleAutoReconnect: readingToggleAutoReconnect,
  } = useSignalR(readingHubUrl);

  // Update state when comic data changes
  useEffect(() => {
    if (comicData) {
      setComicServiceData(comicData);
    }
  }, [comicData]);

  // Update state when reading data changes
  useEffect(() => {
    if (readingData) {
      setReadingServiceData(readingData);
    }
  }, [readingData]);

  // Auto-connect on mount
  useEffect(() => {
    comicConnect();
    readingConnect();
  }, []);

  // Combine data from both services
  const getAllServices = useMemo(() => {
    const services = [];
    
    if (comicServiceData && comicServiceData.type === 'stats' && comicServiceData.services) {
      services.push(...comicServiceData.services);
    }
    
    if (readingServiceData && readingServiceData.type === 'stats' && readingServiceData.services) {
      services.push(...readingServiceData.services);
    }
    
    return services;
  }, [comicServiceData, readingServiceData]);

  // Combine stats from both services
  const combinedData = useMemo(() => {
    const services = getAllServices;
    
    if (services.length === 0) {
      return null;
    }

    const totalServices = services.length;
    const timestamp = new Date().toISOString();
    
    return {
      Type: 'stats',
      TotalServices: totalServices,
      Services: services,
      Timestamp: timestamp,
      totalServices: totalServices
    };
  }, [getAllServices]);

  return (
    <div className="monitor-app">
      <header className="monitor-app-header">
        <h1>🔴 Circuit Breaker Real-Time Monitor</h1>
        <p className="monitor-subtitle">Monitor your microservices circuit breaker status in real-time</p>
      </header>

      <div className="monitor-app-container">
        {/* Comic Service Connection Panel */}
        <ConnectionPanel
          url={comicHubUrl}
          onUrlChange={() => {}} // URL is fixed
          isConnected={comicIsConnected}
          isConnecting={comicIsConnecting}
          onConnect={comicConnect}
          onDisconnect={comicDisconnect}
          error={comicError}
          onAutoReconnectChange={comicToggleAutoReconnect}
          serviceName="Comic Service"
        />

        {/* Reading Service Connection Panel */}
        <ConnectionPanel
          url={readingHubUrl}
          onUrlChange={() => {}} // URL is fixed
          isConnected={readingIsConnected}
          isConnecting={readingIsConnecting}
          onConnect={readingConnect}
          onDisconnect={readingDisconnect}
          error={readingError}
          onAutoReconnectChange={readingToggleAutoReconnect}
          serviceName="Reading Service"
        />

        {combinedData && (
          <>
            <StatsOverview data={combinedData} />

            <div className="monitor-services-section">
              <h2 className="monitor-section-title">
                📡 Services Status ({combinedData.totalServices})
              </h2>
              {getAllServices.length > 0 ? (
                <div className="monitor-services-grid">
                  {getAllServices.map((service, index) => (
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

        {!comicIsConnected && !comicIsConnecting && !readingIsConnected && !readingIsConnecting && !combinedData && (
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
