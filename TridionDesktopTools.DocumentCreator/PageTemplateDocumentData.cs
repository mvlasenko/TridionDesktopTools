using System.Collections.Generic;

namespace TridionDesktopTools.DocumentCreator
{
    public class PageTemplateDocumentData
    {
        public PageTemplateDocumentData()
        {
            TBBs = new List<TbbDocumentData>();
        }

        public string Title { get; set; }
        public string TemplateType { get; set; }
        public string FileExtension { get; set; }

        public List<TbbDocumentData> TBBs { get; set; }
    }
}
