using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Models;
using Mercury.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

        [ObservableProperty]
        private Song? _currentSong;

        public SongViewModel(INavigationService navigationService, IAppService appService)
        {
            _navigationService = navigationService;
            _appService = appService;

            WeakReferenceMessenger.Default.Register<CurrentSongChangedMessage>(this);
            _currentSong = _appService.CurrentSong;
        }

        public void Receive(CurrentSongChangedMessage message)
        {
            // Update the local property, which triggers the UI
            CurrentSong = message.Value;
        }
    }
}
