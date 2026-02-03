using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LibVLCSharp.Shared;
using Mercury.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Windows;
using Windows.Media;
using Windows.Storage;
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

        int Volume { get; set; }

        Task SetSong(Song song);
        void StartSong();
        void PauseSong();
        void StopSong();
        void PreviousSong();
        void NextSong();

        SymbolIcon PlayButtonIcon { get; set;}
        SymbolIcon RepeatButtonIcon { get; set; }
    }

    public partial class MediaPlayerService : ObservableObject, IMediaPlayerService
    {
        public MediaPlayer MediaPlayer { get; private set; }
        public LibVLC LibVLC { get; } = new LibVLC("--no-video", "--quiet");

        [ObservableProperty]
        private Song? _currentSong;

        [ObservableProperty]
        private int _volume = 50;

        [ObservableProperty]
        private RepeatState _repeatingState = RepeatState.RepeatSingle;
        public enum RepeatState
        {
            NoRepeat = 0,
            RepeatSingle = 1,
            RepeatAll = 2,
        }
        public ObservableCollection<Song> Queue { get; } = new();

        [ObservableProperty]
        private SymbolIcon _playButtonIcon = new SymbolIcon(SymbolRegular.Play24, 24);
        [ObservableProperty]
        private SymbolIcon _repeatButtonIcon = new SymbolIcon(SymbolRegular.ArrowRepeat124, 24);

        private Windows.Media.Playback.MediaPlayer? _smtcMediaPlayer;
        private SystemMediaTransportControls? _smtc;
        public MediaPlayerService()
        {
            MediaPlayer = new MediaPlayer(LibVLC);
            MediaPlayer.Volume = Volume;

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

        partial void OnVolumeChanged(int value)
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;

            MediaPlayer.Volume = value;
        }

        public async Task SetSong(Song song)
        {
            _smtc!.IsEnabled = true;

            var streams = await YouTube.GetStreamInfo(song.Media.VideoId);  // Get streams
            var streamUri = new Uri(streams[0].Url);                        // make Uri from Stream
            var media = new Media(LibVLC, streamUri);                       // make LibVLC Media from Uri
            MediaPlayer.Play(media);                                        // Set and Play Song
            CurrentSong = song;
            PlayButtonIcon.Symbol = SymbolRegular.Pause24;

            UpdateNowPlaying(
                title: song.Title,
                artist: song.Artist,
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

        public void StartSong() { MediaPlayer.Play(); PlayButtonIcon.Symbol = SymbolRegular.Pause24; }
        public void PauseSong() { MediaPlayer.Pause(); PlayButtonIcon.Symbol = SymbolRegular.Play24; }
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
            _smtc.IsEnabled = false;
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
                    Application.Current.Dispatcher.Invoke(StartSong);
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    Application.Current.Dispatcher.Invoke(PauseSong);
                    break;

                case SystemMediaTransportControlsButton.Stop:
                    Application.Current.Dispatcher.Invoke(StopSong);
                    break;

                case SystemMediaTransportControlsButton.Previous:
                    Application.Current.Dispatcher.Invoke(PreviousSong);
                    break;

                case SystemMediaTransportControlsButton.Next:
                    Application.Current.Dispatcher.Invoke(NextSong);
                    break;
            }
        }

        private HttpClient _httpClient = new ();
        public async void UpdateNowPlaying(string title, string artist, string? album = null, string? thumbnailUrl = null)
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
                    var localPath = await DownloadThumbnailAsync(thumbnailUrl);
                    if (!string.IsNullOrEmpty(localPath))
                    {
                        var file = await StorageFile.GetFileFromPathAsync(localPath);
                        updater.Thumbnail = RandomAccessStreamReference.CreateFromFile(file);
                    }
                }
                catch { }
            }

            updater.Update();
        }

        private async Task<string?> DownloadThumbnailAsync(string url)
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), "Mercury", "Thumbnails");
                Directory.CreateDirectory(tempPath);

                var fileName = $"{Math.Abs(url.GetHashCode())}.jpg";
                var localPath = Path.Combine(tempPath, fileName);

                if (!File.Exists(localPath))
                {
                    var imageBytes = await _httpClient.GetByteArrayAsync(url);

                    var croppedBytes = CropImageToSquare(imageBytes);

                    await File.WriteAllBytesAsync(localPath, croppedBytes);
                }

                return localPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to download thumbnail: {ex.Message}");
                return null;
            }
        }

        private byte[] CropImageToSquare(byte[] imageBytes)
        {
            try
            {
                using var ms = new MemoryStream(imageBytes);
                using var originalImage = System.Drawing.Image.FromStream(ms);
         
                // Calculate square crop dimensions (center crop)
                int size = Math.Min(originalImage.Width, originalImage.Height);
                int x = (originalImage.Width - size) / 2;
                int y = (originalImage.Height - size) / 2;
         
                // Create cropped bitmap
                using var croppedBitmap = new Bitmap(size, size);
                using var graphics = Graphics.FromImage(croppedBitmap);
             
                // Set high quality rendering
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
         
                // Draw cropped portion
                graphics.DrawImage(
                    originalImage,
                    new Rectangle(0, 0, size, size),
                    new Rectangle(x, y, size, size),
                    GraphicsUnit.Pixel
                );
         
                // Save to memory stream
                using var outputStream = new MemoryStream();
                croppedBitmap.Save(outputStream, ImageFormat.Jpeg);
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to crop image: {ex.Message}");
                // Return original if cropping fails
                return imageBytes;
            }
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
