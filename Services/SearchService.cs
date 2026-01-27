using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Mercury.Services
{
    public interface ISearchService
    {
        string? SearchQuery { get; set; }
        event EventHandler<string> SearchQueryChanged; 
    }

    public class SearchService : ISearchService
    {
        private string? _searchQuery;
        public string? SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    SearchQueryChanged?.Invoke(this, value ?? string.Empty);
                }
            }
        }

        public event EventHandler<string>? SearchQueryChanged;
    }
}
