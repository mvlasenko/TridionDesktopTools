using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
{
    public partial class FieldMappingWindow
    {
        public string Host { private get; set; }

        public string SourceSchemaUri { private get; set; }
        public string TargetSchemaUri { private get; set; }

        public string DbHost { private get; set; }
        public string DbUsername { private get; set; }
        public string DbPassword { private get; set; }
        
        public string SourceDatabase { private get; set; }
        public string SourceTable { private get; set; }

        public HistoryMappingInfo HistoryMapping { get; private set; }

        private List<FieldInfo> _TargetSchemaFields;

        private List<HistoryItemInfo> _History;

        public FieldMappingWindow()
        {
            InitializeComponent();
            this.dataGridFieldMapping.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SchemaData targetSchema = (SchemaData)Functions.ReadItem(this.TargetSchemaUri);
            List <ItemFieldDefinitionData> targetComponentFields = Functions.GetSchemaFields(this.TargetSchemaUri) ?? new List<ItemFieldDefinitionData>();
            List<ItemFieldDefinitionData> targetMetadataFields = Functions.GetSchemaMetadataFields(this.TargetSchemaUri);
            this.txtTargetSchema.Text = String.Format("Target Schema: {0} ({1})", targetSchema.Title, targetSchema.Purpose);

            if (!String.IsNullOrEmpty(this.SourceSchemaUri))
            {
                //schema 2 schema
                this._TargetSchemaFields = Functions.GetAllFields(targetComponentFields, targetMetadataFields, false, true);

                this.HistoryMapping = Functions.GetHistoryMapping(!String.IsNullOrEmpty(this.SourceSchemaUri) ? Functions.GetId(this.Host, this.SourceSchemaUri, this.TargetSchemaUri) : Functions.GetId(this.Host, this.SourceDatabase, this.SourceTable, this.TargetSchemaUri));
                if (this.HistoryMapping != null)
                {
                    foreach (HistoryItemMappingInfo historyMapping in this.HistoryMapping)
                    {
                        string sourceSchemaVersionTcmId = historyMapping.TcmId;
                        if (historyMapping.Current && sourceSchemaVersionTcmId.Contains("-v"))
                        {
                            sourceSchemaVersionTcmId = sourceSchemaVersionTcmId.Replace("-" + sourceSchemaVersionTcmId.Split('-')[3], "");
                        }

                        List<ItemFieldDefinitionData> sourceComponentFields = Functions.GetSchemaFields(sourceSchemaVersionTcmId);
                        List<ItemFieldDefinitionData> sourceMetadataFields = Functions.GetSchemaMetadataFields(sourceSchemaVersionTcmId);
                        List<FieldInfo> sourceSchemaFields = Functions.GetAllFields(sourceComponentFields, sourceMetadataFields, true, false);

                        foreach (FieldMappingInfo mapping in historyMapping.Mapping)
                        {
                            mapping.SourceFields = sourceSchemaFields;
                            mapping.TargetFields = this._TargetSchemaFields;
                        }
                    }
                }

                //schema 2 schema - init versions combo - grid binding is performs in combo selected_changed
                this._History = Functions.GetItemHistory(this.SourceSchemaUri);
                this.cbSourceSchemaVersion.ItemsSource = this._History;
                this.cbSourceSchemaVersion.DisplayMemberPath = "Version";

                this.cbSourceSchemaVersion.SelectedIndex = this._History.Count - 1;

                string detect = Functions.GetFromIsolatedStorage(Functions.GetId(this.Host, this.SourceSchemaUri, this.TargetSchemaUri, "Detect"));
                this.chkDetect.IsChecked = !String.IsNullOrEmpty(detect) && detect.ToLower() == "true";

            }
            else
            {
                //table 2 schema
                this._TargetSchemaFields = Functions.GetAllFields(targetComponentFields, targetMetadataFields, false, false);

                List<FieldInfo> sourceSchemaFields = Functions.GetDatabaseTableFields(this.DbHost, this.DbUsername, this.DbPassword, this.SourceDatabase, this.SourceTable);

                this.HistoryMapping = Functions.GetHistoryMapping(Functions.GetId(this.Host, this.SourceDatabase, this.SourceTable, this.TargetSchemaUri));

                List<FieldMappingInfo> fieldMapping = Functions.GetDefaultFieldMapping(this.Host, sourceSchemaFields, this._TargetSchemaFields, this.TargetSchemaUri);
                
                if (this.HistoryMapping == null)
                {
                    this.HistoryMapping = new HistoryMappingInfo();
                    this.HistoryMapping.Add(new HistoryItemMappingInfo { Mapping = fieldMapping, Current = true });
                }

                if (this.HistoryMapping != null)
                {
                    foreach (HistoryItemMappingInfo historyMapping in this.HistoryMapping)
                    {
                        foreach (FieldMappingInfo mapping in historyMapping.Mapping)
                        {
                            mapping.SourceFields = sourceSchemaFields;
                            mapping.TargetFields = this._TargetSchemaFields;
                        }
                    }
                }

                if (this.HistoryMapping != null)
                {
                    fieldMapping = this.HistoryMapping.Last().Mapping;
                }

                //disable versions combo
                this.chkDetect.Visibility = Visibility.Hidden;
                this.cbSourceSchemaVersion.Visibility = Visibility.Hidden;

                //grid binding

                this.dataGridFieldMapping.ItemsSource = fieldMapping;
                ((DataGridComboBoxColumn)this.dataGridFieldMapping.Columns[1]).ItemsSource = sourceSchemaFields.Select(x => x.GetFieldFullName());

                this.txtSourceSchema.Text = String.Format("Source Table: {1} (Source Database: {0})", this.SourceDatabase, this.SourceTable);

            }
        }

        private void cbSourceSchemaVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HistoryItemInfo sourceSchemaVersion = this.cbSourceSchemaVersion.SelectedValue as HistoryItemInfo;
            if (sourceSchemaVersion == null)
                return;

            string sourceSchemaVersionTcmId = sourceSchemaVersion.TcmId;
            if (sourceSchemaVersion.Current && sourceSchemaVersionTcmId.Contains("-v"))
            {
                sourceSchemaVersionTcmId = sourceSchemaVersionTcmId.Replace("-" + sourceSchemaVersionTcmId.Split('-')[3], "");
            }

            SchemaData sourceSchema = (SchemaData)Functions.ReadItem(sourceSchemaVersionTcmId);

            List<ItemFieldDefinitionData> sourceComponentFields = Functions.GetSchemaFields(sourceSchemaVersionTcmId);
            List<ItemFieldDefinitionData> sourceMetadataFields = Functions.GetSchemaMetadataFields(sourceSchemaVersionTcmId);
            List<FieldInfo> sourceSchemaFields = Functions.GetAllFields(sourceComponentFields, sourceMetadataFields, true, false);

            if(this.HistoryMapping == null)
                this.HistoryMapping = new HistoryMappingInfo();

            List<FieldMappingInfo> fieldMapping;
            if (this.HistoryMapping.Any(x => x.TcmId == sourceSchemaVersion.TcmId))
            {
                fieldMapping = this.HistoryMapping.First(x => x.TcmId == sourceSchemaVersion.TcmId).Mapping;
                foreach (FieldMappingInfo mapping in fieldMapping)
                {
                    mapping.SourceFields = sourceSchemaFields;
                }
            }
            else
            {
                fieldMapping = Functions.GetDefaultFieldMapping(this.Host, sourceSchemaFields, this._TargetSchemaFields, this.TargetSchemaUri);
                this.HistoryMapping.Add(new HistoryItemMappingInfo { TcmId = sourceSchemaVersion.TcmId, Version = sourceSchemaVersion.Version, Modified = sourceSchemaVersion.Modified, Mapping = fieldMapping });
            }

            this.dataGridFieldMapping.ItemsSource = fieldMapping;
            ((DataGridComboBoxColumn)this.dataGridFieldMapping.Columns[1]).ItemsSource = sourceSchemaFields.Select(x => x.GetFieldFullName());

            this.txtSourceSchema.Text = String.Format("Source Schema: {0} ({1})", sourceSchema.Title + (sourceSchemaVersion.Current ? "" : ", version " + sourceSchemaVersion.Version), sourceSchema.Purpose);
        }

        private void SourceFieldChanged(object sender, SelectionChangedEventArgs e)
        {
            List<FieldMappingInfo> fieldMappingList = (List<FieldMappingInfo>) this.dataGridFieldMapping.ItemsSource;
            if (fieldMappingList == null)
                return;

            FieldMappingInfo selectedFieldMappingItem = this.dataGridFieldMapping.CurrentItem as FieldMappingInfo;
            if (selectedFieldMappingItem == null)
                return;

            if (e.AddedItems == null || e.AddedItems.Count == 0 || e.AddedItems[0] == null)
                return;

            string selectedParentText = e.AddedItems[0].ToString();
            if (selectedParentText == "< ignore >")
                return;

            HistoryItemInfo sourceSchemaVersion = this.cbSourceSchemaVersion.SelectedValue as HistoryItemInfo;
            if (sourceSchemaVersion == null)
                return;

            string sourceSchemaVersionTcmId = sourceSchemaVersion.TcmId;
            if (sourceSchemaVersion.Current && sourceSchemaVersionTcmId.Contains("-v"))
            {
                sourceSchemaVersionTcmId = sourceSchemaVersionTcmId.Replace("-" + sourceSchemaVersionTcmId.Split('-')[3], "");
            }

            List<ItemFieldDefinitionData> sourceComponentFields = Functions.GetSchemaFields(sourceSchemaVersionTcmId);
            List<ItemFieldDefinitionData> sourceMetadataFields = Functions.GetSchemaMetadataFields(sourceSchemaVersionTcmId);
            List<FieldInfo> sourceSchemaFields = Functions.GetAllFields(sourceComponentFields, sourceMetadataFields, true, false);

            this.SetAutoChildrenMapping(fieldMappingList, selectedFieldMappingItem, sourceSchemaFields, selectedParentText);
            this.SetAutoParentMapping(fieldMappingList, selectedFieldMappingItem);
        }

        private void DefaultValueChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null)
                return;

            string newValue = tb.Text;
            if (string.IsNullOrEmpty(newValue))
                return;

            List<FieldMappingInfo> fieldMappingList = (List<FieldMappingInfo>)this.dataGridFieldMapping.ItemsSource;
            if (fieldMappingList == null)
                return;

            FieldMappingInfo selectedFieldMappingItem = this.dataGridFieldMapping.CurrentItem as FieldMappingInfo;
            if (selectedFieldMappingItem == null)
                return;

            if (!selectedFieldMappingItem.SourceFieldFullName.StartsWith("<"))
                return;

            string oldValue = selectedFieldMappingItem.DefaultValue;
            if (oldValue == newValue)
                return;

            selectedFieldMappingItem.SourceFieldFullName = "< new >";
            this.SetAutoParentMapping(fieldMappingList, selectedFieldMappingItem);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (this.Validate())
            {
                this.SaveHistoryMapping();
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                if (
                    MessageBox.Show(
                        "Possible errors:\n\n" +
                        " - field types do not match\n" +
                        " - mapping for mandatory field is not selected and default value is not set\n\n" +
                        "Save anyway?",
                        "Mapping Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    this.SaveHistoryMapping();
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.HistoryMapping = null;
            this.Close();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(this.SourceSchemaUri))
            {
                this.HistoryMapping = null;

                if (this.cbSourceSchemaVersion.SelectedIndex == this._History.Count - 1)
                {
                    this.cbSourceSchemaVersion_SelectionChanged(null, null);
                }
                else
                {
                    this.cbSourceSchemaVersion.SelectedIndex = this._History.Count - 1;
                }
            }
            else
            {
                List<FieldInfo> sourceSchemaFields = Functions.GetDatabaseTableFields(this.DbHost, this.DbUsername, this.DbPassword, this.SourceDatabase, this.SourceTable);

                this.HistoryMapping = new HistoryMappingInfo();
                List<FieldMappingInfo> fieldMapping = Functions.GetDefaultFieldMapping(this.Host, sourceSchemaFields, this._TargetSchemaFields, this.TargetSchemaUri);
                this.HistoryMapping.Add(new HistoryItemMappingInfo { Mapping = fieldMapping, Current = true });

                this.dataGridFieldMapping.ItemsSource = fieldMapping;
                ((DataGridComboBoxColumn)this.dataGridFieldMapping.Columns[1]).ItemsSource = sourceSchemaFields.Select(x => x.GetFieldFullName());
            }
        }

        private void SetAutoChildrenMapping(List<FieldMappingInfo> fieldMappingList, FieldMappingInfo selectedParentFieldMappingItem, List<FieldInfo> sourceSchemaFields, string selectedParentText)
        {
            selectedParentFieldMappingItem.SourceFieldFullName = selectedParentText;
            FieldInfo selectedParentSourceField = selectedParentFieldMappingItem.SourceField;
            string parentPath = selectedParentSourceField.GetFieldNamePath();

            foreach (FieldMappingInfo fieldMappingItem in fieldMappingList)
            {
                if (fieldMappingItem.TargetField != null && fieldMappingItem.TargetField.Parent == selectedParentFieldMappingItem.TargetField)
                {
                    string sourceFieldFullName = fieldMappingItem.TargetField.GetFieldFullName(false) + string.Format(" | ({0})", (parentPath + "/" + fieldMappingItem.TargetField.Field.Name).Trim('/'));
                    if (sourceSchemaFields.All(x => x.GetFieldFullName().Trim() != sourceFieldFullName.Trim()))
                    {
                        sourceFieldFullName = "< ignore >";
                    }
                    fieldMappingItem.SourceFieldFullName = sourceFieldFullName;

                    this.SetAutoChildrenMapping(fieldMappingList, fieldMappingItem, sourceSchemaFields, fieldMappingItem.SourceFieldFullName);
                }
            }
        }

        private void SetAutoParentMapping(List<FieldMappingInfo> fieldMappingList, FieldMappingInfo selectedChildFieldMappingItem)
        {
            string currentItemText = selectedChildFieldMappingItem.SourceFieldFullName;

            foreach (FieldMappingInfo fieldMappingItem in fieldMappingList)
            {
                if (fieldMappingItem.TargetField != null && fieldMappingItem.TargetField == selectedChildFieldMappingItem.TargetField.Parent)
                {
                    if (currentItemText == "< new >")
                        fieldMappingItem.SourceFieldFullName = "< new >";

                    this.SetAutoParentMapping(fieldMappingList, fieldMappingItem);
                }
            }
        }

        private void SaveHistoryMapping()
        {
            foreach (HistoryItemMappingInfo historyMappingItem in this.HistoryMapping)
            {
                if (this._History != null && !String.IsNullOrEmpty(historyMappingItem.TcmId))
                {
                    historyMappingItem.Current = this._History.First(x => x.TcmId == historyMappingItem.TcmId).Current;
                }
            }

            Functions.SaveHistoryMapping(!String.IsNullOrEmpty(this.SourceSchemaUri) ? Functions.GetId(this.Host, this.SourceSchemaUri, this.TargetSchemaUri) : Functions.GetId(this.Host, this.SourceDatabase, this.SourceTable, this.TargetSchemaUri), this.HistoryMapping);
        }

        private bool Validate()
        {
            return this.HistoryMapping.Aggregate(true, (currentHistoryRes, historyMapping) => currentHistoryRes && historyMapping.Mapping.Aggregate(true, (currentMappingRes, mapping) => currentMappingRes && mapping.Valid));
        }

    }
}
