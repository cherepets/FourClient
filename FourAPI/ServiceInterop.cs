using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FourAPI
{
    internal class ServiceInterop
    {
        public async Task<ObservableCollection<T>> CallAsync<T>(string method, string[] args = null) where T : new()
        {
            var localService = new LocalServiceClient.LocalServiceClient();
            if (localService.Implements(method))
                return await localService.CallAsync<T>(method, args);
            var webService = new WebServiceClient.WebServiceClient(Settings.Url);
            return await webService.CallAsync<T>(method, args);
        }
    }
}
