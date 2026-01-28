using Wpf.Ui.Abstractions;

namespace Mercury.Services
{
    public class ServiceProviderPageProvider : INavigationViewPageProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, object> _pageCache = new();

        public ServiceProviderPageProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object? GetPage(Type pageType)
        {
            if (_pageCache.TryGetValue(pageType, out var cachedPage))
            {
                return cachedPage;
            }

            var page = _serviceProvider.GetService(pageType);

            if (page != null)
            {
                _pageCache[pageType] = page;
            }

            return page;
        }

        public void ClearCache()
        {
            _pageCache.Clear();
        }
    }
}
