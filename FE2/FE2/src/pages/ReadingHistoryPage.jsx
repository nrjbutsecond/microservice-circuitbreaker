import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { readingService } from '../services/readingService';
import { useAuth } from '../contexts/AuthContext';
import { Card } from '../components/Card';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { 
  Clock, 
  BookOpen, 
  TrendingUp, 
  Calendar,
  Eye 
} from 'lucide-react';

export const ReadingHistoryPage = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [history, setHistory] = useState([]);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (user) {
      loadHistoryAndStats();
    }
  }, [user]);

  const loadHistoryAndStats = async () => {
    setLoading(true);
    setError('');
    try {
      const [historyResponse, statsResponse] = await Promise.all([
        readingService.getUserHistory(user.id),
        readingService.getUserStats(user.id),
      ]);

      if (historyResponse.success) {
        setHistory(historyResponse.data || []);
      }
      if (statsResponse.success) {
        setStats(statsResponse.data);
      }
    } catch (err) {
      setError('Failed to load reading history. Please try again.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleHistoryClick = (item) => {
    navigate(`/comics/${item.comicId}/chapters/${item.chapterNumber}`);
  };

  const formatDuration = (seconds) => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    
    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    }
    return `${minutes}m`;
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">
          Reading History
        </h1>

        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
            {error}
          </div>
        )}

        {/* Stats Cards */}
        {stats && (
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
            <Card className="text-center">
              <BookOpen className="w-8 h-8 text-primary-600 mx-auto mb-2" />
              <p className="text-3xl font-bold text-gray-900 mb-1">
                {stats.totalComicsRead}
              </p>
              <p className="text-sm text-gray-600">Comics Read</p>
            </Card>

            <Card className="text-center">
              <Eye className="w-8 h-8 text-green-600 mx-auto mb-2" />
              <p className="text-3xl font-bold text-gray-900 mb-1">
                {stats.totalChaptersRead}
              </p>
              <p className="text-sm text-gray-600">Chapters Read</p>
            </Card>

            <Card className="text-center">
              <Clock className="w-8 h-8 text-orange-600 mx-auto mb-2" />
              <p className="text-3xl font-bold text-gray-900 mb-1">
                {stats.totalReadingTimeHours}
              </p>
              <p className="text-sm text-gray-600">Hours Reading</p>
            </Card>

            <Card className="text-center">
              <Calendar className="w-8 h-8 text-purple-600 mx-auto mb-2" />
              <p className="text-sm font-medium text-gray-900 mb-1">
                {stats.lastReadAt 
                  ? new Date(stats.lastReadAt).toLocaleDateString()
                  : 'Never'}
              </p>
              <p className="text-sm text-gray-600">Last Read</p>
            </Card>
          </div>
        )}

        {/* History List */}
        <Card>
          <h2 className="text-2xl font-bold text-gray-900 mb-6">
            Recent Reading Activity
          </h2>

          {history.length > 0 ? (
            <div className="space-y-3">
              {history.map((item) => (
                <div
                  key={item.id}
                  onClick={() => handleHistoryClick(item)}
                  className="flex items-center justify-between p-4 bg-gray-50 hover:bg-primary-50 rounded-lg cursor-pointer transition-colors group"
                >
                  <div className="flex-1">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <h3 className="font-semibold text-gray-900 group-hover:text-primary-700">
                          {item.comicTitle || `Comic #${item.comicId}`}
                        </h3>
                        <p className="text-sm text-gray-600">
                          Chapter {item.chapterNumber}
                        </p>
                      </div>
                      {item.completed && (
                        <span className="bg-green-100 text-green-800 text-xs font-medium px-2 py-1 rounded">
                          Completed
                        </span>
                      )}
                    </div>
                    <div className="flex items-center space-x-4 text-sm text-gray-500">
                      <span className="flex items-center space-x-1">
                        <Clock className="w-4 h-4" />
                        <span>{formatDuration(item.readingDurationSeconds)}</span>
                      </span>
                      <span className="flex items-center space-x-1">
                        <Calendar className="w-4 h-4" />
                        <span>{new Date(item.readAt).toLocaleString()}</span>
                      </span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-12">
              <BookOpen className="w-16 h-16 text-gray-400 mx-auto mb-4" />
              <h3 className="text-xl font-semibold text-gray-700 mb-2">
                No reading history yet
              </h3>
              <p className="text-gray-500 mb-6">
                Start reading some comics to see your history here
              </p>
              <button
                onClick={() => navigate('/')}
                className="btn-primary"
              >
                Browse Comics
              </button>
            </div>
          )}
        </Card>
      </div>
    </div>
  );
};
