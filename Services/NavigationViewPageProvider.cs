using Wpf.Ui.Abstractions;

namespace Mercury.Services
{
    public class ServiceProviderPageProvider : INavigationViewPageProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderPageProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object? GetPage(Type pageType)
        {
            return _serviceProvider.GetService(pageType);
        }
    }
}
