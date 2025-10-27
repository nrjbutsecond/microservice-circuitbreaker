import React from 'react';
import './ServiceCard.css';

const ServiceCard = ({ service }) => {
  const getStateColor = (state) => {
    switch (state) {
      case 'Closed':
        return 'success';
      case 'Open':
        return 'danger';
      case 'HalfOpen':
        return 'warning';
      default:
        return 'secondary';
    }
  };

  const getProgressColor = (rate) => {
    if (rate >= 90) return 'success';
    if (rate >= 70) return 'warning';
    return 'danger';
  };

  const formatUptime = (seconds) => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    if (hours > 0) {
      return `${hours}h ${minutes}m ${secs}s`;
    } else if (minutes > 0) {
      return `${minutes}m ${secs}s`;
    } else {
      return `${secs}s`;
    }
  };

  const formatTimestamp = (timestamp) => {
    if (!timestamp) return 'N/A';
    return new Date(timestamp).toLocaleString();
  };

  return (
    <div className="service-card">
      <div className="service-header">
        <div className="service-title">
          <span className="health-icon">{service.isHealthy ? '✅' : '❌'}</span>
          <h3>{service.ServiceName}</h3>
        </div>
        <span className={`badge badge-${getStateColor(service.state)}`}>
          {service.State}
        </span>
      </div>

      <div className="service-body">
        {/* Success Rate Progress */}
        <div className="metric-section">
          <div className="metric-header">
            <span className="metric-label">Success Rate</span>
            <span className="metric-value">{service.successRate.toFixed(2)}%</span>
          </div>
          <div className="progress-bar">
            <div
              className={`progress-fill progress-${getProgressColor(service.successRate)}`}
              style={{ width: `${service.successRate}%` }}
            >
              <span className="progress-text">{service.successRate.toFixed(1)}%</span>
            </div>
          </div>
        </div>

        {/* Stats Grid */}
        <div className="stats-grid">
          <div className="stat-item">
            <span className="stat-label">Total Calls</span>
            <span className="stat-value">{service.totalCalls}</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Success</span>
            <span className="stat-value success-text">{service.successCount}</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Failures</span>
            <span className="stat-value danger-text">{service.failureCount}</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Timeouts</span>
            <span className="stat-value warning-text">{service.timeoutCount}</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Rejected</span>
            <span className="stat-value">{service.rejectedCount}</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Circuit Opened</span>
            <span className="stat-value">{service.openCount}</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Half Open</span>
            <span className="stat-value">{service.halfOpenCount}</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Recoveries</span>
            <span className="stat-value success-text">{service.recoveryCount}</span>
          </div>
        </div>

        {/* Additional Info */}
        <div className="additional-info">
          <div className="info-item">
            <span className="info-label">Uptime:</span>
            <span className="info-value">{formatUptime(service.uptimeSeconds)}</span>
          </div>
          {service.lastSuccessTime && (
            <div className="info-item">
              <span className="info-label">Last Success:</span>
              <span className="info-value">{formatTimestamp(service.lastSuccessTime)}</span>
            </div>
          )}
          {service.lastFailureTime && (
            <div className="info-item">
              <span className="info-label">Last Failure:</span>
              <span className="info-value">{formatTimestamp(service.lastFailureTime)}</span>
            </div>
          )}
          {service.lastException && (
            <div className="info-item error-info">
              <span className="info-label">Last Error:</span>
              <span className="info-value">{service.lastException}</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ServiceCard;
