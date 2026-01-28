using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Services
{
    public interface IAppService
    {
        
    }

    public partial class AppService : ObservableObject, IAppService
    {
        public AppService()
        {
            
        }
    }
}
