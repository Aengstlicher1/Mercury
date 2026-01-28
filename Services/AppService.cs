using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Mercury.Models;
using System.Collections.ObjectModel;

namespace Mercury.Services
{
    public interface IAppService
    {
        Song? CurrentSong { get; set; }
        ObservableCollection<Song> Queue { get; }
    }

    public partial class AppService : ObservableObject, IAppService
    {
        [ObservableProperty]
        private Song? _currentSong;
        public ObservableCollection<Song> Queue { get; } = new();

        partial void OnCurrentSongChanged(Song? value)
        {
            if (value != null)
            {
                WeakReferenceMessenger.Default.Send(new CurrentSongChangedMessage(value));
            }
        }
    }

    public class CurrentSongChangedMessage : ValueChangedMessage<Song>
    {
        public CurrentSongChangedMessage(Song value) : base(value) { }
    }
}
