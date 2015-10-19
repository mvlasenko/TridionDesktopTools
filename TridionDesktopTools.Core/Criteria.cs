using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
{
    public class Criteria
    {
        public ItemFieldDefinitionData Field { get; set; }

        public Operation Operation { get; set; }

        public object Value { get; set; }

        public ItemFieldDefinitionData FieldCompare { get; set; }
    }
}
