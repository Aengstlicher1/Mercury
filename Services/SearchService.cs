using CommunityToolkit.Mvvm.ComponentModel;
using YouTubeApi;

namespace Mercury.Services
{
    public interface ISearchService
    {
        string? SearchQuery { get; set; }
        event EventHandler<string> SearchQueryChanged; 
        YouTube.MusicSearchFilter Filter { get; set;}
    }

    public partial class SearchService : ObservableObject, ISearchService
    {
        private string? _searchQuery;
        public string? SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    SearchQueryChanged?.Invoke(this, value ?? string.Empty);
                }
            }
        }
        [ObservableProperty]
        private YouTube.MusicSearchFilter _filter = YouTube.MusicSearchFilter.Songs;

        public event EventHandler<string>? SearchQueryChanged;
    }
}
