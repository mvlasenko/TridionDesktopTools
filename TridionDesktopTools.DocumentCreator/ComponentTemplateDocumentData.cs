using System.Collections.Generic;

namespace TridionDesktopTools.DocumentCreator
{
    public class ComponentTemplateDocumentData
    {
        public ComponentTemplateDocumentData()
        {
            TBBs = new List<TbbDocumentData>();
        }

        public string Title { get; set; }
        public string TemplateType { get; set; }
        public List<SchemaDocumentData> LinkedSchemas { get; set; }
        public SchemaDocumentData MetadataSchema { get; set; }
        public string OutputFormat { get; set; }
        public int? Priority { get; set; }
        public bool? Dynamic { get; set; }
        public bool? InlineEditing { get; set; }

        public List<TbbDocumentData> TBBs { get; set; }
    }
}
