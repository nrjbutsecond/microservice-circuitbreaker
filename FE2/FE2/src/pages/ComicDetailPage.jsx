import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { comicService } from '../services/comicService';
import { Card } from '../components/Card';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { 
  BookOpen, 
  TrendingUp, 
  Eye, 
  Users, 
  Activity,
  Calendar,
  ArrowLeft 
} from 'lucide-react';

export const ComicDetailPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [comic, setComic] = useState(null);
  const [chapters, setChapters] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadComicDetails();
  }, [id]);

  const loadComicDetails = async () => {
    setLoading(true);
    setError('');
    try {
      const [comicResponse, chaptersResponse] = await Promise.all([
        comicService.getComicById(id),
        comicService.getChapters(id),
      ]);

      if (comicResponse.success) {
        setComic(comicResponse.data);
      }
      if (chaptersResponse.success) {
        setChapters(chaptersResponse.data || []);
      }
    } catch (err) {
      setError('Failed to load comic details. Please try again.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleChapterClick = (chapterNumber) => {
    navigate(`/comics/${id}/chapters/${chapterNumber}`);
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (error || !comic) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="container mx-auto px-4 py-8">
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
            {error || 'Comic not found'}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto px-4 py-8">
        {/* Back Button */}
        <button
          onClick={() => navigate(-1)}
          className="flex items-center space-x-2 text-gray-600 hover:text-gray-900 mb-6"
        >
          <ArrowLeft className="w-5 h-5" />
          <span>Back</span>
        </button>

        {/* Comic Header */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 mb-8">
          {/* Cover Image */}
          <div className="lg:col-span-1">
            <Card className="overflow-hidden">
              <div className="aspect-[3/4] bg-gray-200 rounded-lg overflow-hidden">
                {comic.coverImageUrl ? (
                  <img
                    src={comic.coverImageUrl}
                    alt={comic.title}
                    className="w-full h-full object-cover"
                  />
                ) : (
                  <div className="flex items-center justify-center h-full">
                    <BookOpen className="w-24 h-24 text-gray-400" />
                  </div>
                )}
              </div>
            </Card>
          </div>

          {/* Comic Info */}
          <div className="lg:col-span-2">
            <Card>
              <div className="flex items-start justify-between mb-4">
                <h1 className="text-3xl font-bold text-gray-900">{comic.title}</h1>
                {comic.isTrending && (
                  <span className="bg-red-500 text-white px-3 py-1 rounded-full flex items-center space-x-1 text-sm font-semibold">
                    <TrendingUp className="w-4 h-4" />
                    <span>Trending</span>
                  </span>
                )}
              </div>

              {comic.author && (
                <p className="text-lg text-gray-600 mb-4">by {comic.author}</p>
              )}

              {/* Stats */}
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="flex items-center space-x-2 text-gray-600 mb-1">
                    <Eye className="w-4 h-4" />
                    <span className="text-sm">Total Reads</span>
                  </div>
                  <p className="text-2xl font-bold text-gray-900">
                    {comic.totalReads?.toLocaleString() || 0}
                  </p>
                </div>
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="flex items-center space-x-2 text-gray-600 mb-1">
                    <Users className="w-4 h-4" />
                    <span className="text-sm">Readers</span>
                  </div>
                  <p className="text-2xl font-bold text-gray-900">
                    {comic.uniqueReaders?.toLocaleString() || 0}
                  </p>
                </div>
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="flex items-center space-x-2 text-gray-600 mb-1">
                    <Activity className="w-4 h-4" />
                    <span className="text-sm">Active 24h</span>
                  </div>
                  <p className="text-2xl font-bold text-gray-900">
                    {comic.activeReaders24h || 0}
                  </p>
                </div>
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="flex items-center space-x-2 text-gray-600 mb-1">
                    <BookOpen className="w-4 h-4" />
                    <span className="text-sm">Chapters</span>
                  </div>
                  <p className="text-2xl font-bold text-gray-900">
                    {comic.totalChapters}
                  </p>
                </div>
              </div>

              {/* Status */}
              <div className="flex items-center space-x-4 text-sm text-gray-600 mb-6">
                <span className="capitalize font-medium">
                  Status: <span className="text-gray-900">{comic.status}</span>
                </span>
                <span className="flex items-center space-x-1">
                  <Calendar className="w-4 h-4" />
                  <span>
                    Updated: {new Date(comic.updatedAt).toLocaleDateString()}
                  </span>
                </span>
              </div>

              {/* Description */}
              {comic.description && (
                <div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">
                    Description
                  </h3>
                  <p className="text-gray-700 leading-relaxed">
                    {comic.description}
                  </p>
                </div>
              )}
            </Card>
          </div>
        </div>

        {/* Chapters List */}
        <Card>
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Chapters</h2>
          {chapters.length > 0 ? (
            <div className="space-y-2">
              {chapters.map((chapter) => (
                <div
                  key={chapter.id}
                  onClick={() => handleChapterClick(chapter.chapterNumber)}
                  className="flex items-center justify-between p-4 bg-gray-50 hover:bg-primary-50 rounded-lg cursor-pointer transition-colors group"
                >
                  <div className="flex items-center space-x-4">
                    <span className="text-sm font-medium text-gray-500 group-hover:text-primary-600">
                      #{chapter.chapterNumber}
                    </span>
                    <span className="font-medium text-gray-900 group-hover:text-primary-700">
                      {chapter.title || `Chapter ${chapter.chapterNumber}`}
                    </span>
                  </div>
                  <div className="flex items-center space-x-4 text-sm text-gray-500">
                    <span className="flex items-center space-x-1">
                      <Eye className="w-4 h-4" />
                      <span>{chapter.viewCount || 0}</span>
                    </span>
                    <span>
                      {new Date(chapter.createdAt).toLocaleDateString()}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-center text-gray-500 py-8">
              No chapters available yet
            </p>
          )}
        </Card>
      </div>
    </div>
  );
};
