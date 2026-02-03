using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LibVLCSharp.Shared;
using Mercury.Models;
using Mercury.Services;
using Mercury.Views.Pages;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using YouTubeApi;
using static YouTubeApi.YouTube;


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

            SystemThemeWatcher.Watch(this);
            Focus();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Stop the window from actually closing to keep the Tray Icon
            e.Cancel = true;

            this.Hide();
        }

        private void PosSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (DataContext is MainWindowModel vm) vm.StartScrubbing();
        }

        private void PosSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (DataContext is MainWindowModel vm) vm.StopScrubbing();
        }

        private void MediaVolumeButton_Click(object sender, RoutedEventArgs e)
        {
            VolumePopup.IsOpen = !VolumePopup.IsOpen;
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var focusedElement = FocusManager.GetFocusedElement(this);
            if (focusedElement is not System.Windows.Controls.TextBox or not Wpf.Ui.Controls.TextBox)
            {
                if (e.Key == Key.Space)
                {
                    if (DataContext is MainWindowModel vm)
                    {
                        vm.TogglePlayCommand.Execute(null);
                        e.Handled = true;
                    }
                }
            }
        }
    }

    public partial class MainWindowModel : ObservableObject, IRecipient<CurrentSongChangedMessage>
    {
        private readonly ISearchService _searchService;
        private INavigationService? _navigationService;
        private IMediaPlayerService _playerService;

        private bool _isUserDragging = false;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private float _songProgress = 0f;

        public string? CurrentProgress => CurrentSong != null 
            ? CurrentSong!.Duration.TotalHours >= 1d 
                ? (CurrentSong.Duration * SongProgress).ToString(@"h\:mm\:ss") ?? "0:00:00"
                : (CurrentSong.Duration * SongProgress).ToString(@"m\:ss") ?? "0:00"
            : "0:00";


        public int Volume
        {
            get => _playerService.Volume;
            set
            {
                if (_playerService.Volume != value)
                {
                    _playerService.Volume = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(VolumeIcon));
                }
            }
        }

        public SymbolIcon VolumeIcon
        {
            get
            {
                if (Volume == 0)
                    return new SymbolIcon(SymbolRegular.SpeakerMute24, 24);
                else if (Volume < 33)
                    return new SymbolIcon(SymbolRegular.Speaker024, 24);
                else if (Volume < 66)
                    return new SymbolIcon(SymbolRegular.Speaker124, 24);
                else 
                    return new SymbolIcon(SymbolRegular.Speaker224, 24);
            }
        }

        public MusicSearchFilter SearchFilter
        {
            get => _searchService.Filter;
            set
            {
                if (_searchService.Filter != value)
                {
                    _searchService.Filter = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<MusicSearchFilter> SearchFilters => 
        Enum.GetValues(typeof(MusicSearchFilter)).Cast<MusicSearchFilter>();

        public SymbolIcon PlayButtonIcon => _playerService.PlayButtonIcon;
        public SymbolIcon RepeatButtonIcon => _playerService.RepeatButtonIcon;
        [ObservableProperty]
        private Song? _currentSong;

        public MainWindowModel(ISearchService searchService, INavigationService navigationService, IMediaPlayerService playerService)
        {
            _searchService = searchService;
            _navigationService = navigationService;
            _playerService = playerService;

            _playerService.MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
            WeakReferenceMessenger.Default.Register<CurrentSongChangedMessage>(this);
        }

        private void MediaPlayer_PositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
        {
            SongProgress = e.Position;
        }

        partial void OnSongProgressChanged(float value)
        {
            OnPropertyChanged(nameof(CurrentProgress));
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
        private void ShowWindow() => (Application.Current.MainWindow).Show();
        [RelayCommand]
        private void KillWindow() => Process.GetCurrentProcess().Kill();

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
            var Icon = _playerService.RepeatButtonIcon;
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

        [ObservableProperty]
        private Visibility _currentPlayingVisibility = Visibility.Collapsed;

        public void Receive(CurrentSongChangedMessage message)
        {
            // Update the local property, which triggers the UI
            CurrentSong = message.Value;

            CurrentPlayingVisibility = CurrentSong != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}