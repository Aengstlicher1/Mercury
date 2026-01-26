using Mercury.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using YouTubeApi;

namespace Mercury.Views.Pages
{
    public partial class SearchView : Page
    {
        public SearchView()
        {
            InitializeComponent();
            DataContext = new SearchViewModel();
        }
    }

    public class SearchViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Song> _searchResults = new();
        public ObservableCollection<Song> SearchResults
        {
            get => _searchResults;
            set
            {
                SetProperty(ref _searchResults, value);
            }
        }

        public SearchViewModel()
        {
            Task.Run(async () => 
            {
                SearchResults = new ObservableCollection<Song>(await SearchSongs("Love Me"));
            });
        }

        private async Task<List<Song>> SearchSongs(string query)
        {
            var test = await YouTube.SearchYouTubeMusic(query, YouTube.MusicSearchFilter.Songs);
            List<Song> songs = test.CurrentPage.ContentItems
                .Where(c => c.Content is YouTube.Video)
                .Select(v => v.Content as YouTube.Video)
                .Select(s => new Song() { Media = s! })
                .ToList()!;
            return songs;
        }



        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
