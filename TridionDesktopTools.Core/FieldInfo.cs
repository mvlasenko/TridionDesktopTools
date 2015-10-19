using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
{
    public class FieldInfo
    {
        public ItemFieldDefinitionData Field { get; set; }
        public bool IsMeta { get; set; }
        public FieldInfo Parent { get; set; }
        public int Level { get; set; }
    }
}
