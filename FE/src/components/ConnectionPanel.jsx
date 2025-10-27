import React from 'react';
import './ConnectionPanel.css';

const ConnectionPanel = ({
  url,
  onUrlChange,
  isConnected,
  isConnecting,
  onConnect,
  onDisconnect,
  error,
  onAutoReconnectChange,
}) => {
  const getStatusClass = () => {
    if (isConnecting) return 'connecting';
    if (isConnected) return 'connected';
    return 'disconnected';
  };

  const getStatusText = () => {
    if (isConnecting) return 'Connecting...';
    if (isConnected) return 'Connected';
    return 'Disconnected';
  };

  return (
    <div className="connection-panel">
      <div className="connection-header">
        <h2>🔌 SignalR Connection</h2>
        <span className={`status-badge ${getStatusClass()}`}>
          {getStatusText()}
        </span>
      </div>

      <div className="connection-form">
        <div className="form-group">
          <label htmlFor="hubUrl">SignalR Hub URL:</label>
          <input
            type="text"
            id="hubUrl"
            value={url}
            onChange={(e) => onUrlChange(e.target.value)}
            placeholder="http://localhost:5001/hubs/circuitbreaker"
            disabled={isConnected || isConnecting}
            className="url-input"
          />
        </div>

        <div className="button-group">
          <button
            onClick={onConnect}
            disabled={isConnected || isConnecting}
            className="btn btn-connect"
          >
            {isConnecting ? 'Connecting...' : 'Connect'}
          </button>
          <button
            onClick={onDisconnect}
            disabled={!isConnected && !isConnecting}
            className="btn btn-disconnect"
          >
            Disconnect
          </button>
        </div>

        <div className="checkbox-group">
          <label className="checkbox-label">
            <input
              type="checkbox"
              defaultChecked={true}
              onChange={(e) => onAutoReconnectChange(e.target.checked)}
            />
            <span>Auto-reconnect on disconnect</span>
          </label>
        </div>

        {error && (
          <div className="error-message">
            <span className="error-icon">⚠️</span>
            <span>{error}</span>
          </div>
        )}

        <div className="connection-info">
          <div className="info-box">
            <h4>Available Hubs:</h4>
            <ul>
              <li>
                <code>http://localhost:5001/hubs/circuitbreaker</code>
                <span className="endpoint-desc">Comic Service</span>
              </li>
              <li>
                <code>http://localhost:5002/hubs/circuitbreaker</code>
                <span className="endpoint-desc">Reading Service</span>
              </li>
              <li>
                <code>http://localhost:5003/hubs/circuitbreaker</code>
                <span className="endpoint-desc">User Service</span>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ConnectionPanel;
