import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { comicService } from '../services/comicService';
import { Card } from '../components/Card';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { Search, TrendingUp, BookOpen, ChevronLeft, ChevronRight } from 'lucide-react';

export const ComicsListPage = () => {
  const [comics, setComics] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const navigate = useNavigate();

  useEffect(() => {
    loadComics();
  }, [page]);

  const loadComics = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await comicService.getComics(page, 20);
      if (response.success) {
        setComics(response.data.items || []);
        setTotalPages(response.data.totalPages || 1);
      }
    } catch (err) {
      setError('Failed to load comics. Please try again.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = async (e) => {
    e.preventDefault();
    if (!searchQuery.trim()) {
      loadComics();
      return;
    }

    setLoading(true);
    setError('');
    try {
      const response = await comicService.searchComics(searchQuery);
      if (response.success) {
        setComics(response.data || []);
        setTotalPages(1);
      }
    } catch (err) {
      setError('Search failed. Please try again.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleComicClick = (comicId) => {
    navigate(`/comics/${comicId}`);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-4">
            Discover Comics
          </h1>

          {/* Search Bar */}
          <form onSubmit={handleSearch} className="max-w-2xl">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Search comics by title, author..."
                className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
            </div>
          </form>
        </div>

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
            {error}
          </div>
        )}

        {/* Loading State */}
        {loading ? (
          <LoadingSpinner size="lg" className="py-20" />
        ) : (
          <>
            {/* Comics Grid */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-8">
              {comics.map((comic) => (
                <Card
                  key={comic.id}
                  onClick={() => handleComicClick(comic.id)}
                  className="overflow-hidden hover:scale-105 transition-transform duration-200"
                >
                  {/* Cover Image */}
                  <div className="relative aspect-[3/4] bg-gray-200 mb-4 rounded-lg overflow-hidden">
                    {comic.coverImageUrl ? (
                      <img
                        src={comic.coverImageUrl}
                        alt={comic.title}
                        className="w-full h-full object-cover"
                      />
                    ) : (
                      <div className="flex items-center justify-center h-full">
                        <BookOpen className="w-16 h-16 text-gray-400" />
                      </div>
                    )}
                    {comic.isTrending && (
                      <div className="absolute top-2 right-2 bg-red-500 text-white px-2 py-1 rounded-full flex items-center space-x-1 text-xs font-semibold">
                        <TrendingUp className="w-3 h-3" />
                        <span>Trending</span>
                      </div>
                    )}
                  </div>

                  {/* Comic Info */}
                  <h3 className="font-bold text-lg text-gray-900 mb-1 line-clamp-2">
                    {comic.title}
                  </h3>
                  {comic.author && (
                    <p className="text-sm text-gray-600 mb-2">{comic.author}</p>
                  )}
                  <div className="flex items-center justify-between text-sm text-gray-500">
                    <span>{comic.totalChapters} chapters</span>
                    <span className="capitalize">{comic.status}</span>
                  </div>
                  {comic.totalReads > 0 && (
                    <p className="text-xs text-primary-600 mt-2">
                      {comic.totalReads.toLocaleString()} reads
                    </p>
                  )}
                </Card>
              ))}
            </div>

            {/* Empty State */}
            {comics.length === 0 && !loading && (
              <div className="text-center py-20">
                <BookOpen className="w-16 h-16 text-gray-400 mx-auto mb-4" />
                <h3 className="text-xl font-semibold text-gray-700 mb-2">
                  No comics found
                </h3>
                <p className="text-gray-500">
                  Try adjusting your search or browse all comics
                </p>
              </div>
            )}

            {/* Pagination */}
            {!searchQuery && totalPages > 1 && (
              <div className="flex items-center justify-center space-x-4">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="btn-secondary flex items-center space-x-2"
                >
                  <ChevronLeft className="w-4 h-4" />
                  <span>Previous</span>
                </button>
                <span className="text-gray-700 font-medium">
                  Page {page} of {totalPages}
                </span>
                <button
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                  className="btn-secondary flex items-center space-x-2"
                >
                  <span>Next</span>
                  <ChevronRight className="w-4 h-4" />
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};
