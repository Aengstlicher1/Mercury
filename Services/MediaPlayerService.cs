using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LibVLCSharp.Shared;
using Mercury.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using Windows.Media;
using Windows.Storage.Streams;
using Wpf.Ui.Controls;
using YouTubeApi;
using static Mercury.Services.MediaPlayerService;

namespace Mercury.Services
{
    public interface IMediaPlayerService
    {
        MediaPlayer MediaPlayer { get; }
        LibVLC LibVLC { get; }
        Song? CurrentSong { get; set; }
        RepeatState RepeatingState { get; set; }
        ObservableCollection<Song> Queue { get; }
        void UpdateNowPlaying(string title, string artist, string? album = null, string? thumbnailUrl = null);

        Task SetSong(Song song);
        void StartSong();
        void PauseSong();
        void StopSong();
        void PreviousSong();
        void NextSong();

        Wpf.Ui.Controls.Button? PlayButton { get; set; }
        Wpf.Ui.Controls.Button? RepeatButton { get; set; }
    }

    public partial class MediaPlayerService : ObservableObject, IMediaPlayerService
    {
        public MediaPlayer MediaPlayer { get; private set; }
        public LibVLC LibVLC { get; } = new LibVLC("--no-video", "--quiet");

        [ObservableProperty]
        private Song? _currentSong;

        [ObservableProperty]
        private RepeatState _repeatingState = RepeatState.RepeatSingle;
        public enum RepeatState
        {
            NoRepeat = 0,
            RepeatSingle = 1,
            RepeatAll = 2,
        }
        public ObservableCollection<Song> Queue { get; } = new();

        public Wpf.Ui.Controls.Button? PlayButton { get; set; }
        public Wpf.Ui.Controls.Button? RepeatButton { get; set; }
        public Slider? PositionSlider { get; set; }

        private Windows.Media.Playback.MediaPlayer? _smtcMediaPlayer;
        private SystemMediaTransportControls? _smtc;
        public MediaPlayerService()
        {
            MediaPlayer = new MediaPlayer(LibVLC);
            MediaPlayer.Volume = 50;

            InititalizeSMTC();

            MediaPlayer.Playing += (s, e) => 
            {
                UpdateSMTCPlaybackStatus(MediaPlaybackStatus.Playing);
                WeakReferenceMessenger.Default.Send(new MediaPlayerStateChangedMessage(MediaPlaybackStatus.Playing));
            };
            MediaPlayer.Paused += (s, e) => 
            {
                UpdateSMTCPlaybackStatus(MediaPlaybackStatus.Paused);
                WeakReferenceMessenger.Default.Send(new MediaPlayerStateChangedMessage(MediaPlaybackStatus.Paused)); 
            };
            MediaPlayer.Stopped += (s, e) => 
            {
                UpdateSMTCPlaybackStatus(MediaPlaybackStatus.Stopped);
                WeakReferenceMessenger.Default.Send(new MediaPlayerStateChangedMessage(MediaPlaybackStatus.Stopped));
            };
            MediaPlayer.EndReached += async (s, e) => 
            {
                UpdateSMTCPlaybackStatus(MediaPlaybackStatus.Stopped);
                WeakReferenceMessenger.Default.Send(new MediaPlayerStateChangedMessage(MediaPlaybackStatus.Stopped));

                if (RepeatingState == RepeatState.RepeatSingle)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        MediaPlayer.Stop(); // properly stops the mediaplayer
                        MediaPlayer.Play(); // reinitializes the media and starts playing
                    });
                }
                else if (RepeatingState == RepeatState.RepeatAll)
                {
                    if (CurrentSong != null && Queue.Contains(CurrentSong))
                    {
                        if (Queue.IndexOf(CurrentSong) == Queue.Count - 1)
                        {
                            Thread.Sleep(500);
                            await SetSong(Queue.First());
                        }
                    }
                }
            };
        }


        public async Task SetSong(Song song)
        {
            var streams = await YouTube.GetStreamInfo(song.Media.VideoId);  // Get streams
            var streamUri = new Uri(streams[0].Url);                        // make Uri from Stream
            var media = new Media(LibVLC, streamUri);                       // make LibVLC Media from Uri
            MediaPlayer.Play(media);                                        // Set and Play Song
            CurrentSong = song;
            (PlayButton!.Icon as SymbolIcon)!.Symbol = SymbolRegular.Pause24;

            UpdateNowPlaying(
                title: song.Title,
                artist: song.Author,
                album: song.Album,
                thumbnailUrl: song.Media.Thumbnails.MediumResUrl
            );

            // Setting the availability of the buttons based on Queue
            if (Queue.Contains(song))
            {
                if (Queue.IndexOf(song) == 0)
                {
                    _smtc!.IsPreviousEnabled = false;
                }
                else { _smtc!.IsPreviousEnabled = true; }

                if (Queue.IndexOf(song) == Queue.Count - 1)
                {
                    _smtc!.IsNextEnabled = false;
                }
                else { _smtc!.IsNextEnabled = true; }
            }
        }

        public void StartSong() { MediaPlayer.Play(); (PlayButton!.Icon as SymbolIcon)!.Symbol = SymbolRegular.Pause24; }
        public void PauseSong() { MediaPlayer.Pause(); (PlayButton!.Icon as SymbolIcon)!.Symbol = SymbolRegular.Play24;}
        public void StopSong() => MediaPlayer.Stop();
        public void NextSong() => QueueHelper(1);
        public void PreviousSong() => QueueHelper(-1);

        private void QueueHelper(int index)
        {
            if (CurrentSong != null && Queue.Any())
            {
                if (Queue.Contains(CurrentSong))
                {
                    // check if current Song is within normal parameters
                    // (should never exceed those anyways)
                    var x = Queue.IndexOf(CurrentSong);
                    if (x + index >= 0 && x + index <= Queue.Count - 1) 
                    {
                        var nextSong = Queue.ElementAt(x + index);

                        if (nextSong != null)
                        {
                            _ = SetSong(nextSong);
                        }
                    }
                    
                }
            }
            else { Debug.WriteLine($"Unable to set new Song with argument ({index}); Empty Queue:{!Queue.Any()},, Empty CurrentSong:{CurrentSong == null}"); }
        }

        private void InititalizeSMTC()
        {
            // create instance of dummy player
            _smtcMediaPlayer = new Windows.Media.Playback.MediaPlayer();

            // diable auto command management
            _smtcMediaPlayer.CommandManager.IsEnabled = false;

            _smtc = _smtcMediaPlayer.SystemMediaTransportControls;

            // Enable/Disable buttons
            _smtc.IsEnabled = true;
            _smtc.IsPlayEnabled = true;
            _smtc.IsPauseEnabled = true;
            _smtc.IsStopEnabled = true;
            _smtc.IsNextEnabled = false;
            _smtc.IsPreviousEnabled = false;

            // Handle button presses
            _smtc.ButtonPressed += OnSMTCButtonPressed;
        }

        partial void OnCurrentSongChanged(Song? value)
        {
            if (value != null)
            {
                WeakReferenceMessenger.Default.Send(new CurrentSongChangedMessage(value));
            }
        }

        private void OnSMTCButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    StartSong();
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    PauseSong();
                    break;

                case SystemMediaTransportControlsButton.Stop:
                    StopSong();
                    break;

                case SystemMediaTransportControlsButton.Previous:
                    PreviousSong();
                    break;

                case SystemMediaTransportControlsButton.Next:
                    NextSong();
                    break;
            }
        }

        public void UpdateNowPlaying(string title, string artist, string? album = null, string? thumbnailUrl = null)
        {
            var updater = _smtc!.DisplayUpdater;

            updater.Type = MediaPlaybackType.Music;

            updater.MusicProperties.Title = title ?? "Unknown";
            updater.MusicProperties.Artist = artist ?? "Unknown";
            updater.MusicProperties.AlbumTitle = album ?? "";

            if (!string.IsNullOrEmpty(thumbnailUrl))
            {
                try
                {
                    updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(thumbnailUrl));
                }
                catch { }
            }

            updater.Update();
        }

        private void UpdateSMTCPlaybackStatus(MediaPlaybackStatus status)
        {
            if (_smtc != null)
            {
                _smtc.PlaybackStatus = status;
            }
        }

        public void Dispose()
        {
            MediaPlayer.Dispose();
            LibVLC.Dispose();
        }
    }

    public class CurrentSongChangedMessage : ValueChangedMessage<Song>
    {
        public CurrentSongChangedMessage(Song value) : base(value) { }
    }

    public class MediaPlayerStateChangedMessage : ValueChangedMessage<MediaPlaybackStatus>
    {
        public MediaPlayerStateChangedMessage(MediaPlaybackStatus value) : base(value) { }
    }
}
