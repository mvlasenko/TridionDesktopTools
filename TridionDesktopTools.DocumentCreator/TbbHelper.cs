using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;

namespace TridionDesktopTools.DocumentCreator
{
    public static class TBBHelper
    {
        public static TbbDocumentData GetTBBData(ILocalClient client, string id)
        {
            TemplateBuildingBlockData item = client.Read(id, new ReadOptions()) as TemplateBuildingBlockData;

            if (item == null)
                return null;

            TbbDocumentData tbb = new TbbDocumentData();
            tbb.Title = item.Title;
            tbb.TbbType = item.TemplateType;

            return tbb;
        }
    }
}