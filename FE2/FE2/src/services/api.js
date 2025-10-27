import axios from 'axios';

// API Base URLs
const API_URLS = {
  USER_SERVICE: 'http://localhost:5001/api',
  COMIC_SERVICE: 'http://localhost:5003/api',
  READING_SERVICE: 'http://localhost:5002/api',
};

// Create axios instance with default config
const createApiClient = (baseURL) => {
  const client = axios.create({
    baseURL,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  // Request interceptor to add auth token
  client.interceptors.request.use(
    (config) => {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );

  // Response interceptor for error handling
  client.interceptors.response.use(
    (response) => response.data,
    (error) => {
      if (error.response?.status === 401) {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/login';
      }
      return Promise.reject(error);
    }
  );

  return client;
};

export const userServiceApi = createApiClient(API_URLS.USER_SERVICE);
export const comicServiceApi = createApiClient(API_URLS.COMIC_SERVICE);
export const readingServiceApi = createApiClient(API_URLS.READING_SERVICE);
