using Mercury.Services;
using Mercury.Views.Pages;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Input;


namespace Mercury
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow(MainWindowModel viewModel, INavigationService navigationService)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Set Up Navigation
            navigationService.SetNavigationControl(NavView);
            viewModel.SetNavigationService(navigationService);

            Loaded += (s, e) => NavView.Navigate(typeof(SearchView));
            SystemThemeWatcher.Watch(this);
        }
    }

    public class MainWindowModel : INotifyPropertyChanged
    {
        private readonly ISearchService _searchService;
        private INavigationService? _navigationService;

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    OnSearchTextChanged(value);
                }
            }
        }

        public MainWindowModel(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public ICommand SwtichHudVisibilityCommand => new RelayCommand<object>(SwitchHudVisibility);
        private void SwitchHudVisibility(object? parameter)
        {
            if (parameter is UIElement element)
            {
                element.Visibility = element.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
            }
        }

        public void SetNavigationService(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private void OnSearchTextChanged(string? value)
        {
            // Update the search service
            _searchService.SearchQuery = value;

            // Navigate to SearchView when user starts typing
            if (!string.IsNullOrWhiteSpace(value))
            {
                _navigationService?.Navigate(typeof(SearchView));
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