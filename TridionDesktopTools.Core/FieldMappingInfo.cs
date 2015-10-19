using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core.Annotations;

namespace TridionDesktopTools.Core
{
    public class FieldMappingInfo : INotifyPropertyChanged
    {
        private string _SourceFieldFullName;
        private string _TargetFieldFullName;
        private string _DefaultValue;

        [XmlIgnore]
        public List<FieldInfo> SourceFields { private get; set; }

        [XmlIgnore]
        public List<FieldInfo> TargetFields { private get; set; }

        public string SourceFieldFullName
        {
            get { return _SourceFieldFullName; }
            set
            {
                if (Equals(value, _SourceFieldFullName)) return;
                _SourceFieldFullName = value;
                OnPropertyChanged("SourceField");
                OnPropertyChanged("SourceFieldFullName");
                OnPropertyChanged("SourceFieldSchemaUri");
                OnPropertyChanged("Valid");
            }
        }

        [XmlIgnore]
        public FieldInfo SourceField
        {
            get
            {
                return string.IsNullOrEmpty(this.SourceFieldFullName) || this.SourceFields == null ? null : this.SourceFields.FirstOrDefault(x => x.GetFieldFullName() == this.SourceFieldFullName);
            }
        }

        [XmlIgnore]
        public string SourceFieldSchemaUri
        {
            get
            {
                if (this.SourceField.Field.IsEmbedded())
                    return ((EmbeddedSchemaFieldDefinitionData)this.SourceField.Field).EmbeddedSchema.IdRef;

                if (this.SourceField.Field.IsComponentLink())
                {
                    ComponentLinkFieldDefinitionData field = ((ComponentLinkFieldDefinitionData)this.SourceField.Field);
                    if (field.AllowedTargetSchemas != null && field.AllowedTargetSchemas.Any())
                        return field.AllowedTargetSchemas[0].IdRef;
                }

                return string.Empty;
            }
        }

        public string TargetFieldFullName
        {
            get { return _TargetFieldFullName; }
            set
            {
                if (Equals(value, _TargetFieldFullName)) return;
                _TargetFieldFullName = value;
                OnPropertyChanged("TargetField");
                OnPropertyChanged("TargetFieldFullName");
                OnPropertyChanged("TargetFieldSchemaUri");
                OnPropertyChanged("Valid");
            }
        }

        [XmlIgnore]
        public List<FieldMappingInfo> ChildFieldMapping { get; set; }

        [XmlIgnore]
        public FieldInfo TargetField
        {
            get
            {
                return string.IsNullOrEmpty(this.TargetFieldFullName) || this.TargetFields == null ? null : this.TargetFields.FirstOrDefault(x => x.GetFieldFullName() == this.TargetFieldFullName);
            }
        }

        [XmlIgnore]
        public string TargetFieldSchemaUri
        {
            get
            {
                if (this.TargetField.Field.IsEmbedded())
                    return ((EmbeddedSchemaFieldDefinitionData)this.TargetField.Field).EmbeddedSchema.IdRef;

                if (this.TargetField.Field.IsComponentLink())
                {
                    ComponentLinkFieldDefinitionData field = ((ComponentLinkFieldDefinitionData)this.TargetField.Field);
                    if (field.AllowedTargetSchemas != null && field.AllowedTargetSchemas.Any())
                        return field.AllowedTargetSchemas[0].IdRef;
                }

                return string.Empty;
            }
        }

        public string DefaultValue
        {
            get { return _DefaultValue; }
            set
            {
                if (value == _DefaultValue) return;
                _DefaultValue = value;
                OnPropertyChanged("DefaultValue");
                OnPropertyChanged("Valid");
            }
        }

        [XmlIgnore]
        public bool Valid
        {
            get
            {
                if (this.TargetField.Field.IsMandatory())
                    return this.SourceField != null && this.SourceField.Field != null && !string.IsNullOrEmpty(this.SourceField.Field.Name) && (this.SourceField.Field.IsCastAllowed(this.TargetField.Field) || this.SourceField.Field.Name == "< new >")
                        || this.TargetField.Field.IsPrimitive() && !String.IsNullOrEmpty(this.DefaultValue);

                return this.SourceField == null || this.SourceField.Field == null || string.IsNullOrEmpty(this.SourceField.Field.Name) || this.SourceField.Field.IsCastAllowed(this.TargetField.Field) || this.SourceField.Field.Name == "< new >";
            }
        }

        [XmlIgnore]
        public bool Equals
        {
            get
            {
                if (this.SourceField.Field.GetFieldType() != this.TargetField.Field.GetFieldType())
                    return false;

                if (!string.IsNullOrEmpty(this.DefaultValue))
                    return false;

                if (this.ChildFieldMapping != null && this.ChildFieldMapping.Any(x => !x.Equals))
                    return false;
                
                if (this.SourceField.Field.GetFieldType() == FieldType.EmbeddedSchema)
                {
                    if (((EmbeddedSchemaFieldDefinitionData) this.SourceField.Field).EmbeddedSchema.IdRef != ((EmbeddedSchemaFieldDefinitionData) this.TargetField.Field).EmbeddedSchema.IdRef)
                        return false;

                    if (this.ChildFieldMapping != null && this.ChildFieldMapping.Any(childMapping => !childMapping.Equals))
                        return false;
                }

                if (this.SourceField.Field.GetFieldType() == FieldType.ComponentLink)
                {
                    if (((ComponentLinkFieldDefinitionData)this.SourceField.Field).AllowedTargetSchemas.Any() && ((ComponentLinkFieldDefinitionData)this.TargetField.Field).AllowedTargetSchemas.Any() && ((ComponentLinkFieldDefinitionData)this.SourceField.Field).AllowedTargetSchemas[0].IdRef != ((ComponentLinkFieldDefinitionData)this.TargetField.Field).AllowedTargetSchemas[0].IdRef)
                        return false;
                }

                return this.SourceField.Field.Name == this.TargetField.Field.Name;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
