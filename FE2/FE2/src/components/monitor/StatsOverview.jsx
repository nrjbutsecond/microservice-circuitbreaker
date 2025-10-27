import React from 'react';
import './StatsOverview.css';

const StatsOverview = ({ data }) => {
  if (!data || data.Type !== 'stats') {
    return null;
  }

  const totalServices = data.TotalServices || 0;
  const services = data.Services || [];
  
  const healthyServices = services.filter(s => s.isHealthy).length;
  const unhealthyServices = totalServices - healthyServices;
  
  const closedCount = services.filter(s => s.state === 'Closed').length;
  const openCount = services.filter(s => s.state === 'Open').length;
  const halfOpenCount = services.filter(s => s.state === 'HalfOpen').length;

  const totalCalls = services.reduce((sum, s) => sum + s.totalCalls, 0);
  const totalSuccess = services.reduce((sum, s) => sum + s.successCount, 0);
  const totalFailures = services.reduce((sum, s) => sum + s.failureCount, 0);

  const overallSuccessRate = totalCalls > 0 ? (totalSuccess / totalCalls * 100) : 0;

  const formatTimestamp = (timestamp) => {
    return new Date(timestamp).toLocaleString();
  };

  return (
    <div className="stats-overview">
      <div className="overview-header">
        <h2>📊 System Overview</h2>
        <span className="last-update">
          Last Update: {formatTimestamp(data.Timestamp)}
        </span>
      </div>

      <div className="overview-grid">
        {/* Total Services */}
        <div className="overview-card">
          <div className="card-icon" style={{ background: 'linear-gradient(135deg, #667eea, #764ba2)' }}>
            🔧
          </div>
          <div className="card-content">
            <div className="card-value">{totalServices}</div>
            <div className="card-label">Total Services</div>
          </div>
        </div>

        {/* Health Status */}
        <div className="overview-card">
          <div className="card-icon" style={{ background: 'linear-gradient(135deg, #4caf50, #81c784)' }}>
            ✅
          </div>
          <div className="card-content">
            <div className="card-value">
              {healthyServices} / {totalServices}
            </div>
            <div className="card-label">Healthy Services</div>
          </div>
        </div>

        {/* Overall Success Rate */}
        <div className="overview-card">
          <div className="card-icon" style={{ background: 'linear-gradient(135deg, #2196f3, #64b5f6)' }}>
            📈
          </div>
          <div className="card-content">
            <div className="card-value">{overallSuccessRate.toFixed(1)}%</div>
            <div className="card-label">Success Rate</div>
          </div>
        </div>

        {/* Total Calls */}
        <div className="overview-card">
          <div className="card-icon" style={{ background: 'linear-gradient(135deg, #ff9800, #ffb74d)' }}>
            📞
          </div>
          <div className="card-content">
            <div className="card-value">{totalCalls.toLocaleString()}</div>
            <div className="card-label">Total Calls</div>
          </div>
        </div>
      </div>

      {/* Circuit Breaker States */}
      <div className="states-section">
        <h3>Circuit Breaker States</h3>
        <div className="states-grid">
          <div className="state-card state-closed">
            <div className="state-icon">🟢</div>
            <div className="state-value">{closedCount}</div>
            <div className="state-label">Closed</div>
          </div>
          <div className="state-card state-open">
            <div className="state-icon">🔴</div>
            <div className="state-value">{openCount}</div>
            <div className="state-label">Open</div>
          </div>
          <div className="state-card state-halfopen">
            <div className="state-icon">🟡</div>
            <div className="state-value">{halfOpenCount}</div>
            <div className="state-label">Half-Open</div>
          </div>
        </div>
      </div>

      {/* Success vs Failures */}
      <div className="chart-section">
        <h3>Success vs Failures</h3>
        <div className="bar-chart">
          <div className="bar-item">
            <div className="bar-label">Success</div>
            <div className="bar-container">
              <div 
                className="bar-fill bar-success" 
                style={{ width: `${totalCalls > 0 ? (totalSuccess / totalCalls * 100) : 0}%` }}
              >
                <span className="bar-value">{totalSuccess}</span>
              </div>
            </div>
          </div>
          <div className="bar-item">
            <div className="bar-label">Failures</div>
            <div className="bar-container">
              <div 
                className="bar-fill bar-failure" 
                style={{ width: `${totalCalls > 0 ? (totalFailures / totalCalls * 100) : 0}%` }}
              >
                <span className="bar-value">{totalFailures}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default StatsOverview;
