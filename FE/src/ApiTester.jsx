import { useState } from 'react'
import './App.css'

function ApiTester() {
  const [activeRoute, setActiveRoute] = useState('comics')
  const [apiResponse, setApiResponse] = useState(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [selectedEndpoint, setSelectedEndpoint] = useState(null)

  const routes = [
    { id: 'comics', label: 'Comics Service', api: 'http://localhost:5001' },
    { id: 'reading', label: 'Reading Service', api: 'http://localhost:5002' },
    { id: 'user', label: 'User Service', api: 'http://localhost:5003' },
    { id: 'stats', label: 'Stats Service', api: 'http://localhost:5002/stats' }
  ]

  const apiEndpoints = {
    comics: [
      {
        name: 'Get Comic Details',
        method: 'GET',
        endpoint: '/api/comics/{id}',
        url: 'https://localhost:7212/api/comics/1',
        description: '🔴 Circuit Breaker Point: Calls Reading-Service for stats',
        feature: 'Feature #1'
      },
      {
        name: 'Get Comics List (Paged)',
        method: 'GET',
        endpoint: '/api/comics?page=1&pageSize=10',
        url: 'https://localhost:7212/api/comics?page=1&pageSize=10',
        description: '🔴 Batch Circuit Breaker: Calls Reading-Service for trending stats',
        feature: 'Feature #3'
      },
      {
        name: 'Search Comics',
        method: 'GET',
        endpoint: '/api/comics/search?keyword={keyword}',
        url: 'http://localhost:5001/api/comics/search?keyword=one',
        description: 'Search comics by keyword',
        feature: 'Feature #5'
      },
      {
        name: 'Get Batch Comics',
        method: 'GET',
        endpoint: '/api/comics/batch?ids=1,2,3',
        url: 'http://localhost:5001/api/comics/batch?ids=1,2,3',
        description: 'Called from Reading-Service',
        feature: 'Internal'
      },
      {
        name: 'Get Chapter by Number',
        method: 'GET',
        endpoint: '/api/chapters/{chapterNumber}?comicId={comicId}',
        url: 'http://localhost:5001/api/chapters/1?comicId=1',
        description: 'Get chapter content to read',
        feature: 'Feature #2'
      },
      {
        name: 'Get All Chapters',
        method: 'GET',
        endpoint: '/api/chapters?comicId={comicId}',
        url: 'http://localhost:5001/api/chapters?comicId=1',
        description: 'Get all chapters for a comic',
        feature: 'Supporting'
      }
    ],
    reading: [
      {
        name: 'Track Reading',
        method: 'POST',
        endpoint: '/api/reading/track',
        url: 'http://localhost:5002/api/reading/track',
        description: '🔴 Circuit Breaker Point: Validates user via User-Service',
        feature: 'Feature #2',
        body: {
          userId: 1,
          comicId: 1,
          chapterNumber: 1
        }
      },
      {
        name: 'Get Reading History',
        method: 'GET',
        endpoint: '/api/reading/history?userId={userId}',
        url: 'http://localhost:5002/api/reading/history?userId=1',
        description: 'Get user reading history',
        feature: 'Feature #4'
      },
      {
        name: 'Get User Reading Stats',
        method: 'GET',
        endpoint: '/api/reading/stats/user/{userId}',
        url: 'http://localhost:5002/api/reading/stats/user/1',
        description: 'Get user reading statistics',
        feature: 'Supporting'
      }
    ],
    user: [
      {
        name: 'Validate User',
        method: 'GET',
        endpoint: '/api/users/validate/{userId}',
        url: 'http://localhost:5003/api/users/validate/1',
        description: '🔴 Called from Reading-Service (Circuit Breaker Point)',
        feature: 'Internal'
      },
      {
        name: 'Get User by ID',
        method: 'GET',
        endpoint: '/api/users/{id}',
        url: 'http://localhost:5003/api/users/1',
        description: 'Retrieve user details',
        feature: 'Supporting'
      },
      {
        name: 'Login',
        method: 'POST',
        endpoint: '/api/auth/login',
        url: 'http://localhost:5003/api/auth/login',
        description: 'User authentication',
        feature: 'Auth',
        body: {
          username: 'user1',
          password: 'password123'
        }
      }
    ],
    stats: [
      {
        name: 'Get Comic Stats',
        method: 'GET',
        endpoint: '/api/stats/comic/{comicId}',
        url: 'http://localhost:5002/api/stats/comic/1',
        description: '🔴 Called from Comic-Service (Circuit Breaker Point)',
        feature: 'Feature #1'
      },
      {
        name: 'Get Batch Stats',
        method: 'GET',
        endpoint: '/api/stats/comics/batch?ids=1,2,3',
        url: 'http://localhost:5002/api/stats/comics/batch?ids=1,2,3',
        description: '🔴 Called from Comic-Service (Batch Circuit Breaker)',
        feature: 'Feature #3'
      }
    ]
  }

  const handleRouteClick = (routeId) => {
    setActiveRoute(routeId)
    setApiResponse(null)
    setError(null)
    setSelectedEndpoint(null)
  }

  const handleApiCall = async (apiEndpoint) => {
    setSelectedEndpoint(apiEndpoint)
    setApiResponse(null)
    setError(null)
    setLoading(true)

    try {
      const options = {
        method: apiEndpoint.method,
        headers: {
          'Content-Type': 'application/json',
        }
      }

      if (apiEndpoint.body) {
        options.body = JSON.stringify(apiEndpoint.body)
      }

      const response = await fetch(apiEndpoint.url, options)
      const data = await response.json()
      
      setApiResponse({
        status: response.status,
        statusText: response.statusText,
        data: data
      })
    } catch (err) {
      setError({
        message: err.message,
        type: 'Network Error'
      })
    } finally {
      setLoading(false)
    }
  }

  const renderContent = () => {
    const endpoints = apiEndpoints[activeRoute] || []

    return (
      <div className="content-section">
        <h2>{routes.find(r => r.id === activeRoute)?.label}</h2>
        
        <div className="endpoints-list">
          <h3>Available Endpoints:</h3>
          {endpoints.map((api, index) => (
            <div key={index} className="api-card">
              <div className="api-header">
                <div className="api-title">
                  <span className={`method-badge ${api.method.toLowerCase()}`}>{api.method}</span>
                  <span className="api-name">{api.name}</span>
                  <span className="feature-badge">{api.feature}</span>
                </div>
                <button 
                  className="test-btn"
                  onClick={() => handleApiCall(api)}
                  disabled={loading}
                >
                  {loading && selectedEndpoint === api ? '⏳' : '▶️'} Test
                </button>
              </div>
              <code className="api-endpoint">{api.endpoint}</code>
              <p className="api-description">{api.description}</p>
              {api.body && (
                <details className="request-body">
                  <summary>Request Body</summary>
                  <pre>{JSON.stringify(api.body, null, 2)}</pre>
                </details>
              )}
            </div>
          ))}
        </div>

        {loading && selectedEndpoint && (
          <div className="response-container loading">
            <div className="spinner"></div>
            <p>Calling {selectedEndpoint.name}...</p>
          </div>
        )}

        {error && (
          <div className="response-container error">
            <h3>❌ Error</h3>
            <div className="error-details">
              <p><strong>Type:</strong> {error.type}</p>
              <p><strong>Message:</strong> {error.message}</p>
              <p className="error-hint">💡 Make sure the service is running on the expected port</p>
            </div>
          </div>
        )}

        {apiResponse && !loading && (
          <div className="response-container success">
            <div className="response-header">
              <h3>✅ Response from: {selectedEndpoint?.name}</h3>
              <span className={`status-badge status-${Math.floor(apiResponse.status / 100)}xx`}>
                {apiResponse.status} {apiResponse.statusText}
              </span>
            </div>
            <div className="response-body">
              <pre>{JSON.stringify(apiResponse.data, null, 2)}</pre>
            </div>
          </div>
        )}
      </div>
    )
  }

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>🔄 Microservice Circuit Breaker Testing</h1>
        <p className="subtitle">Comic Reader Demo - Service Testing Interface</p>
      </header>

      <nav className="route-buttons">
        {routes.map(route => (
          <button
            key={route.id}
            className={`route-btn ${activeRoute === route.id ? 'active' : ''}`}
            onClick={() => handleRouteClick(route.id)}
            disabled={loading}
          >
            <span className="route-label">{route.label}</span>
            <span className="route-api">{route.api}</span>
          </button>
        ))}
      </nav>

      <main className="main-content">
        {renderContent()}
      </main>

      <footer className="app-footer">
        <p>💡 Click on service buttons to view endpoints and features</p>
      </footer>
    </div>
  )
}

export default ApiTester
