import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { comicService } from '../services/comicService';
import { readingService } from '../services/readingService';
import { useAuth } from '../contexts/AuthContext';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { 
  ArrowLeft, 
  ChevronLeft, 
  ChevronRight, 
  BookOpen 
} from 'lucide-react';

export const ChapterReaderPage = () => {
  const { id: comicId, chapterNumber } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [chapter, setChapter] = useState(null);
  const [comic, setComic] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [readingStartTime] = useState(Date.now());

  useEffect(() => {
    loadChapter();
  }, [comicId, chapterNumber]);

  useEffect(() => {
    // Track reading when component unmounts
    return () => {
      if (chapter && user) {
        trackReading();
      }
    };
  }, [chapter, user]);

  const loadChapter = async () => {
    setLoading(true);
    setError('');
    try {
      const [chapterResponse, comicResponse] = await Promise.all([
        comicService.getChapter(comicId, parseInt(chapterNumber)),
        comicService.getComicById(comicId),
      ]);

      if (chapterResponse.success) {
        setChapter(chapterResponse.data);
      }
      if (comicResponse.success) {
        setComic(comicResponse.data);
      }
    } catch (err) {
      setError('Failed to load chapter. Please try again.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const trackReading = async () => {
    if (!chapter || !user) return;

    const readingDuration = Math.floor((Date.now() - readingStartTime) / 1000);
    
    try {
      await readingService.trackReading({
        userId: user.id,
        comicId: parseInt(comicId),
        chapterId: chapter.id,
        chapterNumber: chapter.chapterNumber,
        readingDurationSeconds: readingDuration,
        completed: true,
      });
    } catch (err) {
      console.error('Failed to track reading:', err);
    }
  };

  const goToChapter = (newChapterNumber) => {
    navigate(`/comics/${comicId}/chapters/${newChapterNumber}`);
  };

  const handlePrevious = () => {
    if (parseInt(chapterNumber) > 1) {
      goToChapter(parseInt(chapterNumber) - 1);
    }
  };

  const handleNext = () => {
    if (comic && parseInt(chapterNumber) < comic.totalChapters) {
      goToChapter(parseInt(chapterNumber) + 1);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (error || !chapter) {
    return (
      <div className="min-h-screen bg-gray-900">
        <div className="container mx-auto px-4 py-8">
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
            {error || 'Chapter not found'}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      {/* Header */}
      <div className="bg-gray-800 border-b border-gray-700 sticky top-0 z-10">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <button
                onClick={() => navigate(`/comics/${comicId}`)}
                className="flex items-center space-x-2 text-gray-300 hover:text-white"
              >
                <ArrowLeft className="w-5 h-5" />
                <span>Back to Comic</span>
              </button>
              <div className="border-l border-gray-600 pl-4">
                <div className="flex items-center space-x-2">
                  <BookOpen className="w-5 h-5 text-primary-400" />
                  <div>
                    <p className="text-sm text-gray-400">
                      {comic?.title || 'Loading...'}
                    </p>
                    <p className="font-semibold">
                      {chapter.title || `Chapter ${chapter.chapterNumber}`}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Navigation */}
            <div className="flex items-center space-x-2">
              <button
                onClick={handlePrevious}
                disabled={parseInt(chapterNumber) <= 1}
                className="flex items-center space-x-2 px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                <ChevronLeft className="w-5 h-5" />
                <span className="hidden sm:inline">Previous</span>
              </button>
              <span className="text-sm text-gray-400 px-4">
                {chapterNumber} / {comic?.totalChapters || '?'}
              </span>
              <button
                onClick={handleNext}
                disabled={parseInt(chapterNumber) >= (comic?.totalChapters || 0)}
                className="flex items-center space-x-2 px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                <span className="hidden sm:inline">Next</span>
                <ChevronRight className="w-5 h-5" />
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="container mx-auto px-4 py-8">
        <div className="max-w-4xl mx-auto">
          <div className="bg-gray-800 rounded-lg p-8">
            <h1 className="text-2xl font-bold mb-6 text-center">
              {chapter.title || `Chapter ${chapter.chapterNumber}`}
            </h1>
            <div className="prose prose-invert max-w-none">
              <div 
                className="whitespace-pre-wrap text-gray-200 leading-relaxed"
                style={{ fontSize: '1.1rem', lineHeight: '1.8' }}
              >
                {chapter.content}
              </div>
            </div>
          </div>

          {/* Bottom Navigation */}
          <div className="flex items-center justify-between mt-8">
            <button
              onClick={handlePrevious}
              disabled={parseInt(chapterNumber) <= 1}
              className="btn-secondary flex items-center space-x-2 disabled:opacity-50"
            >
              <ChevronLeft className="w-5 h-5" />
              <span>Previous Chapter</span>
            </button>
            <button
              onClick={handleNext}
              disabled={parseInt(chapterNumber) >= (comic?.totalChapters || 0)}
              className="btn-primary flex items-center space-x-2 disabled:opacity-50"
            >
              <span>Next Chapter</span>
              <ChevronRight className="w-5 h-5" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
