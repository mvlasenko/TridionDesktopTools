using System.Collections.Generic;
using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
{
    public interface ICustomTransformer
    {
        string GetFixedContent(SchemaData sourceSchema, List<ItemFieldDefinitionData> sourceComponentFields, List<ItemFieldDefinitionData> sourceMetadataFields, string sourceComponentXml, string sourceMetadataXml, string sourceUri, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string targetFolderUri, List<ResultInfo> results);
    }
}