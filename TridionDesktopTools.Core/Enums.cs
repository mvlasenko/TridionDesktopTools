using System;

namespace TridionDesktopTools.Core
{
    [Flags]
    public enum TridionSelectorMode
    {
        None = 0,
        Publication = 1,
        Folder = 2,
        StructureGroup = 4,
        Schema = 8,
        Component = 16,
        ComponentTemplate = 32,
        Page = 64,
        PageTemplate = 128,
        TargetGroup = 256,
        Category = 512,
        Keyword = 1024,
        TemplateBuildingBlock = 2048,
        Any = 8192,
    }

    public enum Operation
    {
        Equal,
        Greater,
        Less,
        Like,
        EqualField,
        GreaterField,
        LessField,
        LikeField
    }

    public enum Status
    {
        Info,
        Success,
        Warning,
        Error,
        None
    }

    public enum FieldType
    {
        SingleLineText,
        MultiLineText,
        Xhtml,
        Date,
        Number,
        Keyword,
        Multimedia,
        ExternalLink,
        ComponentLink,
        EmbeddedSchema,
        None
    }

    public enum BindingType
    {
        HttpBinding,
        TcpBinding
    }

    public enum SchemaType
    {
        Any,
        Component,
        Metadata,
        Embedded,
        Multimedia,
        Parameters,
        Bundle,
        None
    }

    public enum ObjectType
    {
        Any,
        Component,
        Folder,
        ComponentOrFolder,
        Page,
        StructureGroup,
        PageOrStructureGroup
    }

    public enum LinkStatus
    {
        Found,
        NotFound,
        Mandatory,
        Error
    }
}