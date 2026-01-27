using CommunityToolkit.Mvvm.Messaging;
using Mercury.Models;
using Mercury.Services;
using Microsoft.Extensions.Primitives;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Mercury.Views.Pages
{
    public partial class SearchView : Page
    {
        public SearchView(SearchViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }

    public class SearchViewModel : INotifyPropertyChanged
    {
        private readonly ISearchService _searchService;
        private CancellationTokenSource? _searchCts;

        private ObservableCollection<Song> _searchResults = new();
        public ObservableCollection<Song> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        private string? _searchQuery;
        public string? SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        public SearchViewModel(ISearchService searchService)
        {
            _searchService = searchService;
            _searchService.SearchQueryChanged += OnSearchQueryChanged;

            // Inititalize the search
            SearchQuery = _searchService.SearchQuery;
            _ = PerformSearchAsync(SearchQuery);
        }

        private void OnSearchQueryChanged(object? sender, string query)
        {
            SearchQuery = query;
            _ = PerformSearchAsync(SearchQuery);
        }

        private async Task PerformSearchAsync(string query)
        {
            // Cancel previous Search and renew token
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;


            try
            {
                await Task.Delay(300, token);

                // check for clear query
                if (string.IsNullOrWhiteSpace(query))
                {
                    SearchResults.Clear();
                    return;
                }

                var results = await SongTools.GetSongs(query);

                if (!token.IsCancellationRequested)
                {
                    SearchResults = new ObservableCollection<Song>(results);
                }
            }
            catch (TaskCanceledException)
            {

            }
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
