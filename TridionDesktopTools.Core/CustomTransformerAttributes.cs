using System;

namespace TridionDesktopTools.Core
{
    public class SourceSchemaAttribute : Attribute
    {
        public SourceSchemaAttribute()
        {
            Type = SchemaType.Any;
        }
        public string Title { get; set; }
        public SchemaType Type { get; set; }
    }

    public class TargetSchemaAttribute : Attribute
    {
        public TargetSchemaAttribute()
        {
            Type = SchemaType.Any;
        }
        public string Title { get; set; }
        public SchemaType Type { get; set; }
    }

    public class SourceObjectAttribute : Attribute
    {
        public SourceObjectAttribute()
        {
            Type = ObjectType.Any;
        }
        public ObjectType Type { get; set; }
    }

    public class SourceTableAttribute : Attribute
    {
        public string Title { get; set; }
    }

}