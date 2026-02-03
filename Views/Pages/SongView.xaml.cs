using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibVLCSharp.Shared;
using Mercury.Models;
using Mercury.Services;
using System.Net.Http;
using System.Windows.Controls;
using Wpf.Ui;


namespace Mercury.Views.Pages
{
    public partial class SongView : Page
    {
        public SongView(SongViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }

    public partial class SongViewModel : ObservableObject, IRecipient<CurrentSongChangedMessage>
    {
        private readonly INavigationService _navigationService;
        private readonly IMediaPlayerService _mediaPlayerService;
        private readonly HttpClient _httpClient = new HttpClient();

        [ObservableProperty]
        private MediaPlayer _mediaPlayer;

        [ObservableProperty]
        private Song? _currentSong;

        [ObservableProperty]
        private string? _plainLyrics = string.Empty;
        public SongViewModel(INavigationService navigationService, IAppService appService, IMediaPlayerService mediaPlayerService)
        {
            _navigationService = navigationService;
            _mediaPlayerService = mediaPlayerService;
            _mediaPlayer = mediaPlayerService.MediaPlayer;

            WeakReferenceMessenger.Default.Register<CurrentSongChangedMessage>(this);
            _currentSong = _mediaPlayerService.CurrentSong;
        }

        public async void Receive(CurrentSongChangedMessage message)
        {
            // Update the local property, which triggers the UI
            CurrentSong = message.Value;
            PlainLyrics = (await LyricsService.GetLyricsAsync(message.Value))?.PlainLyrics;
        }
    }
}
