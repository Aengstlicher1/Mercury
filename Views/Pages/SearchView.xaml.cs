using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Models;
using Mercury.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using Wpf.Ui;

namespace Mercury.Views.Pages
{
    public partial class SearchView : Page
    {
        public SearchView(SearchViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }

    public partial class SearchViewModel : ObservableObject
    {
        private readonly ISearchService _searchService;
        private INavigationService _navigationService;
        private IAppService _appService;
        private IMediaPlayerService _playerService;

        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        private ObservableCollection<Song> _searchResults = new();

        [ObservableProperty]
        private string? _searchQuery = string.Empty;

        public SearchViewModel(ISearchService searchService, INavigationService navigationService, IAppService appService, IMediaPlayerService playerService)
        {
            _navigationService = navigationService;
            _searchService = searchService;
            _appService = appService;
            _playerService = playerService;

            _searchService.SearchQueryChanged += OnSearchQueryChanged;

            // Inititalize the search
            SearchQuery = _searchService.SearchQuery;
            _ = PerformSearchAsync(SearchQuery!);
        }

        [RelayCommand]
        private async Task PlaySong(Song song)
        {
            _ = _playerService.SetSong(song);

            Debug.WriteLine($"Now playing: {song!.ToString()}");
        }

        [RelayCommand]
        private void EnterSongView(Song song)
        {
            _playerService.CurrentSong = song;
            _navigationService.Navigate(typeof(SongView));
        }

        private void OnSearchQueryChanged(object? sender, string query)
        {
            SearchQuery = query;
            _ = PerformSearchAsync(SearchQuery);
        }

        private async Task PerformSearchAsync(string query)
        {
            // Cancel previous Search and renew token
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;


            try
            {
                await Task.Delay(100, token);

                // check for empty query
                if (string.IsNullOrWhiteSpace(query))
                {
                    SearchResults.Clear();
                    _playerService.Queue.Clear();
                    return;
                }

                var results = await SongTools.GetSongs(query);

                if (!token.IsCancellationRequested)
                {
                    foreach (var item in results)
                    {
                        SearchResults.Add(item);
                        _playerService.Queue.Add(item);
                        await Task.Delay(25);
                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
        }
    }
}
