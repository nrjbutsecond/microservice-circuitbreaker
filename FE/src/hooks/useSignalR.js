import { useState, useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

const useSignalR = (url) => {
  const [isConnected, setIsConnected] = useState(false);
  const [isConnecting, setIsConnecting] = useState(false);
  const [data, setData] = useState(null);
  const [error, setError] = useState(null);
  const connectionRef = useRef(null);
  const [shouldReconnect, setShouldReconnect] = useState(false);

  const connect = useCallback(async () => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      console.log('Already connected');
      return;
    }

    setIsConnecting(true);
    setError(null);

    try {
      // Create SignalR connection
      const connection = new signalR.HubConnectionBuilder()
        .withUrl(url, {
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Custom retry delays: 0s, 2s, 5s, 10s, then 30s
            if (retryContext.previousRetryCount === 0) return 0;
            if (retryContext.previousRetryCount === 1) return 2000;
            if (retryContext.previousRetryCount === 2) return 5000;
            if (retryContext.previousRetryCount === 3) return 10000;
            return 30000;
          }
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Set up event handlers
      connection.on('Connected', (message) => {
        console.log('SignalR Connected:', message);
        setIsConnected(true);
        setIsConnecting(false);
        setError(null);
      });

      connection.on('ReceiveStats', (stats) => {
        setData(stats);
      });

      connection.on('ReceiveServiceStats', (stats) => {
        setData(stats);
      });

      connection.on('Error', (errorMessage) => {
        console.error('SignalR Error:', errorMessage);
        setError(errorMessage.Message || 'Unknown error');
      });

      // Handle connection state changes
      connection.onreconnecting((error) => {
        console.log('SignalR reconnecting...', error);
        setIsConnected(false);
        setIsConnecting(true);
        setError('Reconnecting...');
      });

      connection.onreconnected((connectionId) => {
        console.log('SignalR reconnected:', connectionId);
        setIsConnected(true);
        setIsConnecting(false);
        setError(null);
        // Re-subscribe to stats after reconnection
        if (connectionRef.current) {
          connectionRef.current.invoke('SubscribeToAllStats').catch(err => {
            console.error('Failed to re-subscribe:', err);
          });
        }
      });

      connection.onclose((error) => {
        console.log('SignalR connection closed', error);
        setIsConnected(false);
        setIsConnecting(false);
        connectionRef.current = null;
        
        if (error) {
          setError(error.message || 'Connection closed with error');
        }

        // Auto-reconnect if enabled
        if (shouldReconnect && !error?.message?.includes('manually')) {
          setTimeout(() => {
            console.log('Attempting to reconnect...');
            connect();
          }, 3000);
        }
      });

      // Start the connection
      await connection.start();
      console.log('SignalR connection started');
      
      // Subscribe to all stats
      await connection.invoke('SubscribeToAllStats');
      console.log('Subscribed to circuit breaker stats');
      
      connectionRef.current = connection;
      setIsConnected(true);
      setIsConnecting(false);
      
    } catch (err) {
      console.error('Failed to connect to SignalR:', err);
      setError(err.message || 'Failed to connect');
      setIsConnecting(false);
      
      // Auto-reconnect if enabled
      if (shouldReconnect) {
        setTimeout(() => {
          console.log('Retrying connection...');
          connect();
        }, 3000);
      }
    }
  }, [url, shouldReconnect]);

  const disconnect = useCallback(async () => {
    setShouldReconnect(false);
    if (connectionRef.current) {
      try {
        // Unsubscribe before disconnecting
        if (connectionRef.current.state === signalR.HubConnectionState.Connected) {
          await connectionRef.current.invoke('UnsubscribeFromAllStats');
        }
        await connectionRef.current.stop();
        console.log('SignalR disconnected');
      } catch (err) {
        console.error('Error disconnecting:', err);
      }
      connectionRef.current = null;
    }
  }, []);

  const toggleAutoReconnect = useCallback((enabled) => {
    setShouldReconnect(enabled);
  }, []);

  const subscribeToService = useCallback(async (serviceName) => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      try {
        await connectionRef.current.invoke('SubscribeToService', serviceName);
        console.log(`Subscribed to service: ${serviceName}`);
      } catch (err) {
        console.error(`Failed to subscribe to service ${serviceName}:`, err);
      }
    }
  }, []);

  const unsubscribeFromService = useCallback(async (serviceName) => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      try {
        await connectionRef.current.invoke('UnsubscribeFromService', serviceName);
        console.log(`Unsubscribed from service: ${serviceName}`);
      } catch (err) {
        console.error(`Failed to unsubscribe from service ${serviceName}:`, err);
      }
    }
  }, []);

  const getStats = useCallback(async () => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      try {
        await connectionRef.current.invoke('GetAllStats');
      } catch (err) {
        console.error('Failed to get stats:', err);
      }
    }
  }, []);

  useEffect(() => {
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop().catch(err => {
          console.error('Error stopping connection on cleanup:', err);
        });
      }
    };
  }, []);

  return {
    isConnected,
    isConnecting,
    data,
    error,
    connect,
    disconnect,
    toggleAutoReconnect,
    subscribeToService,
    unsubscribeFromService,
    getStats
  };
};

export default useSignalR;
