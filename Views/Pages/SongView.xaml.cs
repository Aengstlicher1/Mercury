using Accessibility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibVLCSharp.Shared;
using Mercury.Models;
using Mercury.Services;
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
        private readonly IAppService _appService;
        private readonly IMediaPlayerService _mediaPlayerService;

        [ObservableProperty]
        private MediaPlayer _mediaPlayer;

        [ObservableProperty]
        private Song? _currentSong;

        public SongViewModel(INavigationService navigationService, IAppService appService, IMediaPlayerService mediaPlayerService)
        {
            _navigationService = navigationService;
            _appService = appService;
            _mediaPlayerService = mediaPlayerService;
            _mediaPlayer = mediaPlayerService.MediaPlayer;

            WeakReferenceMessenger.Default.Register<CurrentSongChangedMessage>(this);
            _currentSong = _mediaPlayerService.CurrentSong;
        }

        public void Receive(CurrentSongChangedMessage message)
        {
            // Update the local property, which triggers the UI
            CurrentSong = message.Value;
        }
    }
}
