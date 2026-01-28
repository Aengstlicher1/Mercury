using System.Windows;
using Mercury.Services;
using Mercury.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui;
using Wpf.Ui.Abstractions;


namespace Mercury
{
    public partial class App : Application
    {
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Wpf-UI Services
                services.AddSingleton<INavigationViewPageProvider, ServiceProviderPageProvider>();
                services.AddSingleton<INavigationService, NavigationService>();

                // Custom
                services.AddSingleton<ISearchService, SearchService>();
                services.AddSingleton<IAppService, AppService>();
                services.AddSingleton<IMediaPlayerService, MediaPlayerService>();

                // Views
                services.AddSingleton<MainWindow>();
                services.AddTransient<SearchView>();
                services.AddTransient<SongView>();

                // ViewModels
                services.AddSingleton<MainWindowModel>();
                services.AddTransient<SearchViewModel>();
                services.AddTransient<SongViewModel>();
            })
            .Build();

        public static T? GetService<T>() where T : class
        {
            return _host.Services.GetService<T>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var mainWindow = GetService<MainWindow>();
            mainWindow?.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            base.OnExit(e);
        }
    }
}
