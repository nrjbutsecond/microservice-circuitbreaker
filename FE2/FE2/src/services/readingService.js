import { readingServiceApi } from './api';

export const readingService = {
  // Track reading
  trackReading: async (trackingData) => {
    const response = await readingServiceApi.post('/reading/track', trackingData);
    return response;
  },

  // Get user reading history
  getUserHistory: async (userId) => {
    const response = await readingServiceApi.get('/reading/history', {
      params: { userId },
    });
    return response;
  },

  // Get user reading stats
  getUserStats: async (userId) => {
    const response = await readingServiceApi.get(`/reading/stats/user/${userId}`);
    return response;
  },

  // Get comic stats
  getComicStats: async (comicId) => {
    const response = await readingServiceApi.get(`/stats/comic/${comicId}`);
    return response;
  },
};
