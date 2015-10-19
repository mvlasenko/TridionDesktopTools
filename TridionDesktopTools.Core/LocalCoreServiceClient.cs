using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
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
