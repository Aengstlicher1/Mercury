using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using Mercury.Services;
using Mercury.Views.Pages;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;


namespace Mercury
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow(MainWindowModel viewModel, INavigationService navigationService, IAppService appService, IMediaPlayerService playerService)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Set Up Navigation
            navigationService.SetNavigationControl(NavView);
            Loaded += (s, e) => NavView.Navigate(typeof(SearchView));

            // Set up Vlc MediaPlayer and its service
            VideoView.MediaPlayer = playerService.MediaPlayer;

            // Store the media buttons for later use
            playerService.PlayButton = MediaPlayButton;

            SystemThemeWatcher.Watch(this);
        }
    }

    public partial class MainWindowModel : ObservableObject
    {
        private readonly ISearchService _searchService;
        private INavigationService? _navigationService;
        private IMediaPlayerService _playerService;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private double _songProgress = 0d;

        public MainWindowModel(ISearchService searchService, INavigationService navigationService, IMediaPlayerService playerService)
        {
            _searchService = searchService;
            _navigationService = navigationService;
            _playerService = playerService;
        }

        [RelayCommand]
        private void TogglePlay()
        {
            if (_playerService.CurrentSong != null)
            {
                if (!_playerService.MediaPlayer.IsPlaying)
                {
                    _playerService.StartSong();
                }
                else
                {
                    _playerService.PauseSong();
                }
            }
        }

        [RelayCommand]
        private void PlayPrevious() => _playerService.PreviousSong();
        [RelayCommand]
        private void PlayNext() => _playerService.NextSong();

        partial void OnSearchTextChanged(string value)
        {
            // Update the search service
            _searchService.SearchQuery = value;

            // Navigate to SearchView when user starts typing
            if (!string.IsNullOrWhiteSpace(value))
            {
                _navigationService?.Navigate(typeof(SearchView));
            }
        }
    }
}