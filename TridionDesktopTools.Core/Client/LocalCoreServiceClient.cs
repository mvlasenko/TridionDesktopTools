using System.ServiceModel;
using System.ServiceModel.Channels;
using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core.Client
{
    public class LocalCoreServiceClient : CoreServiceClient, ILocalClient
    {
        public LocalCoreServiceClient() : base()
        {
        }

        public LocalCoreServiceClient(Binding binding, EndpointAddress endpointAddress) : base(binding, endpointAddress)
        {
        }
    }
}
