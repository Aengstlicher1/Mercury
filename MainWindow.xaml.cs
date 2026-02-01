using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using Markdig.Extensions.Tables;
using Mercury.Services;
using Mercury.Views.Pages;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Converters;
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
            playerService.RepeatButton = MediaContinueButton;

            SystemThemeWatcher.Watch(this);
        }

        private void PosSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (DataContext is MainWindowModel vm) vm.StartScrubbing();
        }

        private void PosSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (DataContext is MainWindowModel vm) vm.StopScrubbing();
        }
    }

    public partial class MainWindowModel : ObservableObject
    {
        private readonly ISearchService _searchService;
        private INavigationService? _navigationService;
        private IMediaPlayerService _playerService;

        private bool _isUserDragging = false;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private float _songProgress = 0f;

        public MainWindowModel(ISearchService searchService, INavigationService navigationService, IMediaPlayerService playerService)
        {
            _searchService = searchService;
            _navigationService = navigationService;
            _playerService = playerService;

            _playerService.MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
        }

        private void MediaPlayer_PositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
        {
            SongProgress = e.Position;
        }

        partial void OnSongProgressChanged(float value)
        {
            if (_isUserDragging)
            {
                _playerService.MediaPlayer.Position = (float)value;
            }
        }

        public void StartScrubbing() 
        { 
            _isUserDragging = true;
            _playerService.PauseSong();
        }
        public void StopScrubbing() 
        { 
            _isUserDragging = false;
            _playerService.StartSong();
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

        [RelayCommand]
        private void SwtichRepeat()
        {
            var Icon = (_playerService.RepeatButton!.Icon as SymbolIcon)!;
            if (_playerService.RepeatingState == MediaPlayerService.RepeatState.RepeatSingle)
            {
                Icon.Symbol = SymbolRegular.ArrowRepeatAll24;
                _playerService.RepeatingState = MediaPlayerService.RepeatState.RepeatAll;
            }
            else if (_playerService.RepeatingState == MediaPlayerService.RepeatState.RepeatAll)
            {
                Icon.Symbol = SymbolRegular.ArrowRepeatAllOff24;
                _playerService.RepeatingState = MediaPlayerService.RepeatState.NoRepeat;
            }
            else if (_playerService.RepeatingState == MediaPlayerService.RepeatState.NoRepeat)
            {
                Icon.Symbol = SymbolRegular.ArrowRepeat124;
                _playerService.RepeatingState = MediaPlayerService.RepeatState.RepeatSingle;
            }
        }

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