using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
{
    public class ComponentFieldData
    {
        public object Value { get; set; }
        
        public ItemFieldDefinitionData SchemaField { get; set; }

        public bool IsMultiValue
        {
            get
            {
                return SchemaField.IsMultiValue();
            }
        }

        public bool IsMandatory
        {
            get
            {
                return SchemaField.IsMandatory();
            }
        }
    }
}