using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;

namespace TridionDesktopTools.DocumentCreator
{
    public static class PageTemplateHelper
    {
        public static PageTemplateDocumentData GetPageTemplateData(ILocalClient client, string id)
        {
            PageTemplateData item = client.Read(id, new ReadOptions()) as PageTemplateData;
            
            if (item == null)
                return null;

            PageTemplateDocumentData pageTemplate = new PageTemplateDocumentData();
            pageTemplate.Title = item.Title;
            pageTemplate.TemplateType = item.TemplateType;
            pageTemplate.FileExtension = item.FileExtension;

            foreach (TbbInfo tbbInfo in Functions.GetTbbList(item.Content))
            {
                TbbDocumentData tbbDocument = TBBHelper.GetTBBData(client, tbbInfo.TcmId);
                pageTemplate.TBBs.Add(tbbDocument);
            }
            
            return pageTemplate;
        }
    }
}
