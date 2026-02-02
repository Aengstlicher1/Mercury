using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Models;
using Mercury.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.Media.Protection.PlayReady;
using Wpf.Ui;
using YouTubeApi;

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
        private IMediaPlayerService _playerService;

        private CancellationTokenSource? _searchCts;

        [ObservableProperty]
        private ObservableCollection<Song> _songSearchResults = new();
        [ObservableProperty]
        private ObservableCollection<Playlist> _playlistSearchResults = new();

        [ObservableProperty]
        private string? _searchQuery = string.Empty;

        public SearchViewModel(ISearchService searchService, INavigationService navigationService, IAppService appService, IMediaPlayerService playerService)
        {
            _navigationService = navigationService;
            _searchService = searchService;
            _playerService = playerService;

            _searchService.SearchQueryChanged += OnSearchQueryChanged;
        }

        [RelayCommand]
        private async Task PlaySong(Song song) => _ = _playerService.SetSong(song);

        [RelayCommand]
        private void EnterSongView(Song song)
        {
            _playerService.CurrentSong = song;
            _navigationService.Navigate(typeof(SongView));
        }

        private void OnSearchQueryChanged(object? sender, string query)
        {
            SearchQuery = query;
            Task.Run(() => PerformSearchAsync(SearchQuery));
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

                List<Song> songResults = new();
                List<Playlist> playlistResults = new();

                if (_searchService.Filter is YouTubeApi.YouTube.MusicSearchFilter.Songs)
                {
                    songResults = await SongTools.SearchSongs(query, 2);
                }
                else if (_searchService.Filter is YouTubeApi.YouTube.MusicSearchFilter.CommunityPlaylists)
                {
                    playlistResults = await PlaylistTools.SearchPlaylists(query, 2);
                }

                if (!token.IsCancellationRequested)
                {
                    // empty old results
                    Application.Current.Dispatcher.Invoke(()=>
                    {
                        SongSearchResults.Clear();
                        PlaylistSearchResults.Clear();
                        _playerService.Queue.Clear();
                    });
                    

                    foreach (var item in songResults)
                    {
                        Application.Current.Dispatcher.Invoke(()=>
                        {
                            SongSearchResults.Add(item);
                            _playerService.Queue.Add(item);
                        });
                        await Task.Delay(20, token);
                    }
                    foreach (var item in playlistResults)
                    {
                        Application.Current.Dispatcher.Invoke(()=>
                        {
                            PlaylistSearchResults.Add(item);
                        });
                        await Task.Delay(20, token);
                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
        }
    }
}
