import { comicServiceApi } from './api';

export const comicService = {
  // Get comics list with pagination
  getComics: async (page = 1, pageSize = 20) => {
    const response = await comicServiceApi.get('/comics', {
      params: { page, pageSize },
    });
    return response;
  },

  // Get comic details
  getComicById: async (id) => {
    const response = await comicServiceApi.get(`/comics/${id}`);
    return response;
  },

  // Search comics
  searchComics: async (keyword) => {
    const response = await comicServiceApi.get('/comics/search', {
      params: { keyword },
    });
    return response;
  },

  // Get chapters by comic ID
  getChapters: async (comicId) => {
    const response = await comicServiceApi.get('/chapters', {
      params: { comicId },
    });
    return response;
  },

  // Get specific chapter
  getChapter: async (comicId, chapterNumber) => {
    const response = await comicServiceApi.get(`/chapters/${chapterNumber}`, {
      params: { comicId },
    });
    return response;
  },

  // Create comic (admin)
  createComic: async (comicData) => {
    const response = await comicServiceApi.post('/comics', comicData);
    return response;
  },

  // Create chapter (admin)
  createChapter: async (comicId, chapterData) => {
    const response = await comicServiceApi.post('/chapters', chapterData, {
      params: { comicId },
    });
    return response;
  },
};
