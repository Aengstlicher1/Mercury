using Mercury.Views.Pages;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Wpf.Ui.Controls;


namespace Mercury
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowModel();
            Loaded += (s, e) => NavView.Navigate(typeof(SearchView));
        }

        private void TextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            NavView.Navigate(typeof(SearchView));
        }
    }

    public class MainWindowModel : INotifyPropertyChanged
    {
        
        public MainWindowModel()
        {
            
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