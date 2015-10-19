using System.Collections.Generic;

namespace TridionDesktopTools.DocumentCreator
{
    public class SchemaDocumentData
    {
        public SchemaDocumentData()
        {
            Fields = new List<SchemaFieldDocumentData>();
            MetadataFields = new List<SchemaFieldDocumentData>();
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string RootElementName { get; set; }
        public string NamespaceUri { get; set; }
        public string LocationInfo { get; set; }
        public string SchemaType { get; set; }

        public List<SchemaFieldDocumentData> Fields { get; set; }
        public List<SchemaFieldDocumentData> MetadataFields { get; set; }
    }
}
