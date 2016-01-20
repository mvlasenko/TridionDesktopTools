using System.Linq;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;
using TridionDesktopTools.Core.Client;

namespace TridionDesktopTools.DocumentCreator
{
    public static class ComponentTemplateHelper
    {
        public static ComponentTemplateDocumentData GetComponentTemplateData(ILocalClient client, string id)
        {
            ComponentTemplateData item = client.Read(id, new ReadOptions()) as ComponentTemplateData;

            if (item == null)
                return null;

            ComponentTemplateDocumentData componentTemplate = new ComponentTemplateDocumentData();
            componentTemplate.Title = item.Title;
            componentTemplate.TemplateType = item.TemplateType;
            componentTemplate.LinkedSchemas = item.RelatedSchemas.Select(x => new SchemaDocumentData {Title = x.Title}).ToList();
            componentTemplate.MetadataSchema = new SchemaDocumentData {Title = item.MetadataSchema.Title};
            componentTemplate.OutputFormat = item.OutputFormat;
            componentTemplate.Priority = item.Priority;
            componentTemplate.Dynamic = item.DynamicTemplate.ToLower() == "dynamic";
            componentTemplate.InlineEditing = item.IsEditable;

            foreach (TbbInfo tbbInfo in Functions.GetTbbList(item.Content))
            {
                TbbDocumentData tbbDocument = TBBHelper.GetTBBData(client, tbbInfo.TcmId);
                componentTemplate.TBBs.Add(tbbDocument);
            }

            return componentTemplate;
        }
    }
}
