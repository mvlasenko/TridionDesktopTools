using System.Collections.Generic;
using System.Data;
using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
{
    public interface ICustomImporter
    {
        string GetContent(string sourceTable, DataRow sourceDataRow, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string targetFolderUri, List<ResultInfo> results);
    }
}