using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Models;
using Mercury.Services;
using Microsoft.Extensions.Primitives;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Input;

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
        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        private ObservableCollection<Song> _searchResults = new();

        [ObservableProperty]
        private string? _searchQuery = string.Empty;

        public SearchViewModel(ISearchService searchService, INavigationService navigationService, IAppService appService)
        {
            _navigationService = navigationService;
            _searchService = searchService;
            _appService = appService;
            _searchService.SearchQueryChanged += OnSearchQueryChanged;

            // Inititalize the search
            SearchQuery = _searchService.SearchQuery;
            _ = PerformSearchAsync(SearchQuery!);
        }

        [RelayCommand]
        private void EnterSongView(Song? song)
        {
            if (song == null) return;
            _appService.CurrentSong = song;
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
                    return;
                }

                var results = await SongTools.GetSongs(query);

                if (!token.IsCancellationRequested)
                {
                    SearchResults = new ObservableCollection<Song>(results);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }
    }
}
