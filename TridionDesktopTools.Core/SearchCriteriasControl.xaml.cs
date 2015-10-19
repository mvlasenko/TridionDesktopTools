using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
{
    public partial class SearchCriteriasControl
    {
        public List<CriteriaRow> CriteriaRows { get; set; }
        public string Host { private get; set; }
        private List<ItemFieldDefinitionData> _Fields;
        private string _SchemaUri;

        public SearchCriteriasControl()
        {
            InitializeComponent();
        }

        public void InitSchema(string schemaUri)
        {
            this._Fields = GetFields(schemaUri);

            this._SchemaUri = schemaUri;

            this.CriteriaRows = new List<CriteriaRow>();
            Button btn = this.btnAnd;
            this.grdCriteria.Children.Clear();
            this.grdCriteria.Children.Add(btn);
            Grid.SetRow(btn, 0);

            this.btnAnd_Click(null, null);
        }

        public List<Criteria> GetCriterias()
        {
            List<Criteria> criterias = new List<Criteria>();

            foreach (CriteriaRow criteriaRow in this.CriteriaRows)
            {
                if (criteriaRow.Value == null || criteriaRow.Value.ToString() == string.Empty)
                    continue;

                ItemFieldDefinitionData field = criteriaRow.ComboBoxField.SelectedValue as ItemFieldDefinitionData;
                if (field == null)
                    continue;

                if (criteriaRow.ComboBoxOperation.SelectedValue == null)
                    continue;

                Operation operation = (Operation)criteriaRow.ComboBoxOperation.SelectedValue;

                Criteria criteria = new Criteria();
                criteria.Field = field;
                criteria.Operation = operation;
                criteria.Value = criteriaRow.Value;
                criteria.FieldCompare = criteriaRow.ComboBoxFieldCompare.SelectedValue as ItemFieldDefinitionData;

                criterias.Add(criteria);

                //save values to isolated storage
                if (criteriaRow.Value != null)
                    Functions.SaveToIsolatedStorage(Functions.GetId(this.Host, field.Name, "Value"), criteriaRow.Value.ToString());
            }

            return criterias;
        }

        private void cbField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this._SchemaUri))
                return;

            CriteriaRow criteriaRow = this.CriteriaRows.FirstOrDefault(x => Equals(x.ComboBoxField, sender as ComboBox));
            if (criteriaRow == null)
                return;

            ItemFieldDefinitionData field = criteriaRow.ComboBoxField.SelectedValue as ItemFieldDefinitionData;
            if (field == null)
                return;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.Host, this._SchemaUri, criteriaRow.Index, "FieldName"), field.Name);

            //load type name
            criteriaRow.TextBlockType.Text = field.Name == "< Any >" ? "" : (field.GetFieldTypeName() + (field.IsMultiValue() ? " (MV)" : ""));

            //load operations
            List<Operation> operations = this.GetOperations(field);
            criteriaRow.ComboBoxOperation.ItemsSource = operations;

            string operationName = Functions.GetFromIsolatedStorage(Functions.GetId(this.Host, field.Name, "OperationName"));
            if (!String.IsNullOrEmpty(operationName))
            {
                criteriaRow.ComboBoxOperation.SelectedIndex = operations.FindIndex(x => x.ToString() == operationName);
            }

            //bind combobox values
            this.BindComboValues(field, criteriaRow.ComboBoxValue);

            //bind fields to compare
            criteriaRow.ComboBoxFieldCompare.ItemsSource = this._Fields;
            criteriaRow.ComboBoxFieldCompare.DisplayMemberPath = "Name";
            if (criteriaRow.ComboBoxFieldCompare.SelectedIndex < 0)
                criteriaRow.ComboBoxFieldCompare.SelectedIndex = 0;

            //load value
            criteriaRow.Value = Functions.GetFromIsolatedStorage(Functions.GetId(this.Host, field.Name, "Value"));
            criteriaRow.Field = field;
        }

        private void cbOperation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CriteriaRow criteriaRow = this.CriteriaRows.FirstOrDefault(x => Equals(x.ComboBoxOperation, sender as ComboBox));
            if (criteriaRow == null)
                return;

            ItemFieldDefinitionData field = criteriaRow.ComboBoxField.SelectedValue as ItemFieldDefinitionData;
            if (field == null)
                return;

            if (criteriaRow.ComboBoxOperation.SelectedValue == null)
                return;

            Operation operation = (Operation)criteriaRow.ComboBoxOperation.SelectedValue;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.Host, field.Name, "OperationName"), operation.ToString());

            criteriaRow.Operation = operation;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            CriteriaRow criteriaRow = this.CriteriaRows.FirstOrDefault(x => Equals(x.ButtonRemove, sender as Button));
            if (criteriaRow == null)
                return;

            this.grdCriteria.Children.Remove(criteriaRow.ComboBoxField);
            this.grdCriteria.Children.Remove(criteriaRow.TextBlockType);
            this.grdCriteria.Children.Remove(criteriaRow.ComboBoxOperation);
            this.grdCriteria.Children.Remove(criteriaRow.TextBoxValue);
            this.grdCriteria.Children.Remove(criteriaRow.DatePickerValue);
            this.grdCriteria.Children.Remove(criteriaRow.ComboBoxValue);
            this.grdCriteria.Children.Remove(criteriaRow.ComboBoxFieldCompare);
            this.grdCriteria.Children.Remove(criteriaRow.ButtonRemove);

            this.CriteriaRows.Remove(criteriaRow);

            int index = 0;
            foreach (CriteriaRow row in this.CriteriaRows)
            {
                row.Index = index;
                Grid.SetRow(row.ComboBoxField, index);
                Grid.SetRow(row.TextBlockType, index);
                Grid.SetRow(row.ComboBoxOperation, index);
                Grid.SetRow(row.TextBoxValue, index);
                Grid.SetRow(row.DatePickerValue, index);
                Grid.SetRow(row.ComboBoxValue, index);
                Grid.SetRow(row.ComboBoxFieldCompare, index);
                Grid.SetRow(row.ButtonRemove, index);
                row.ButtonRemove.Visibility = this.CriteriaRows.Count > 1 ? Visibility.Visible : Visibility.Hidden;
                index++;
            }

            Grid.SetRow(this.btnAnd, index);
        }

        private void btnAnd_Click(object sender, RoutedEventArgs e)
        {
            CriteriaRow criteriaRow = new CriteriaRow();

            int index = Grid.GetRow(this.btnAnd);
            criteriaRow.Index = index;

            //<ComboBox Name="cbField" Width="160" Height="23" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="0" Grid.Row="0" Margin="6" SelectionChanged="cbField_SelectionChanged" />
            ComboBox cbFieldNew = new ComboBox();
            cbFieldNew.Name = "cbField" + index;
            cbFieldNew.Width = 160;
            cbFieldNew.Height = 23;
            cbFieldNew.HorizontalAlignment = HorizontalAlignment.Left;
            cbFieldNew.VerticalAlignment = VerticalAlignment.Center;
            cbFieldNew.Margin = new Thickness(6);
            Grid.SetColumn(cbFieldNew, 0);
            Grid.SetRow(cbFieldNew, index);
            cbFieldNew.SelectionChanged += cbField_SelectionChanged;
            this.grdCriteria.Children.Add(cbFieldNew);

            //bind fields
            cbFieldNew.ItemsSource = this._Fields;
            cbFieldNew.DisplayMemberPath = "Name";

            string fieldName = Functions.GetFromIsolatedStorage(Functions.GetId(this.Host, this._SchemaUri, index, "FieldName"));
            if (!String.IsNullOrEmpty(fieldName))
            {
                cbFieldNew.SelectedIndex = this._Fields.FindIndex(x => x.Name == fieldName);
            }

            if (cbFieldNew.SelectedIndex < 0)
                cbFieldNew.SelectedIndex = 0;

            ItemFieldDefinitionData field = cbFieldNew.SelectedValue as ItemFieldDefinitionData ?? new ItemFieldDefinitionData { Name = "< Any Field >" };

            criteriaRow.ComboBoxField = cbFieldNew;

            //<TextBlock Name="txtType" Width="120" Height="16" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="1" Grid.Row="0" />
            TextBlock txtTypeNew = new TextBlock();
            txtTypeNew.Name = "txtType" + index;
            txtTypeNew.Width = 120;
            txtTypeNew.Height = 16;
            txtTypeNew.HorizontalAlignment = HorizontalAlignment.Left;
            txtTypeNew.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(txtTypeNew, 1);
            Grid.SetRow(txtTypeNew, index);
            this.grdCriteria.Children.Add(txtTypeNew);

            //load type name
            txtTypeNew.Text = field.Name == "< Any >" ? "" : (field.GetFieldTypeName() + (field.IsMultiValue() ? " (MV)" : ""));

            criteriaRow.TextBlockType = txtTypeNew;

            //<ComboBox Name="cbOperation" Width="80" Height="23" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="2" Grid.Row="0" Margin="6" SelectionChanged="cbOperation_SelectionChanged" />
            ComboBox cbOperationNew = new ComboBox();
            cbOperationNew.Name = "cbOperation" + index;
            cbOperationNew.Width = 80;
            cbOperationNew.Height = 23;
            cbOperationNew.HorizontalAlignment = HorizontalAlignment.Left;
            cbOperationNew.VerticalAlignment = VerticalAlignment.Center;
            cbOperationNew.Margin = new Thickness(6);
            Grid.SetColumn(cbOperationNew, 2);
            Grid.SetRow(cbOperationNew, index);
            cbOperationNew.SelectionChanged += cbOperation_SelectionChanged;
            this.grdCriteria.Children.Add(cbOperationNew);

            //load operations
            List<Operation> operations = this.GetOperations(field);
            cbOperationNew.ItemsSource = operations;

            string operationName = Functions.GetFromIsolatedStorage(Functions.GetId(this.Host, field.Name, "OperationName"));
            if (!String.IsNullOrEmpty(operationName))
            {
                cbOperationNew.SelectedIndex = operations.FindIndex(x => x.ToString() == operationName);
            }

            criteriaRow.ComboBoxOperation = cbOperationNew;

            //<TextBox Name="txtValue" Width="106" Height="23" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="3" Grid.Row="0" Margin="6" />
            TextBox txtValueNew = new TextBox();
            txtValueNew.Name = "txtValue" + index;
            txtValueNew.Width = 120;
            txtValueNew.Height = 23;
            txtValueNew.HorizontalAlignment = HorizontalAlignment.Left;
            txtValueNew.VerticalAlignment = VerticalAlignment.Center;
            txtValueNew.Margin = new Thickness(6);
            Grid.SetColumn(txtValueNew, 3);
            Grid.SetRow(txtValueNew, index);
            this.grdCriteria.Children.Add(txtValueNew);

            criteriaRow.TextBoxValue = txtValueNew;

            //<DatePicker Name="dateValue" Width="106" Height="23" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="3" Grid.Row="0" Margin="6"/>
            DatePicker dateValueNew = new DatePicker();
            dateValueNew.Name = "dateValue" + index;
            dateValueNew.Width = 120;
            dateValueNew.Height = 23;
            dateValueNew.HorizontalAlignment = HorizontalAlignment.Left;
            dateValueNew.VerticalAlignment = VerticalAlignment.Center;
            dateValueNew.Margin = new Thickness(6);
            Grid.SetColumn(dateValueNew, 3);
            Grid.SetRow(dateValueNew, index);
            this.grdCriteria.Children.Add(dateValueNew);

            criteriaRow.DatePickerValue = dateValueNew;

            //<ComboBox Name="cbValue" Width="106" Height="23" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="3" Grid.Row="0" Margin="6" />
            ComboBox cbValueNew = new ComboBox();
            cbValueNew.Name = "cbValue" + index;
            cbValueNew.Width = 120;
            cbValueNew.Height = 23;
            cbValueNew.HorizontalAlignment = HorizontalAlignment.Left;
            cbValueNew.VerticalAlignment = VerticalAlignment.Center;
            cbValueNew.Margin = new Thickness(6);
            Grid.SetColumn(cbValueNew, 3);
            Grid.SetRow(cbValueNew, index);
            this.grdCriteria.Children.Add(cbValueNew);

            criteriaRow.ComboBoxValue = cbValueNew;

            //bind combobox values
            this.BindComboValues(field, criteriaRow.ComboBoxValue);

            //<ComboBox Name="cbFieldCompare" Width="120" Height="23" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="3" Grid.Row="0" Margin="6" />
            ComboBox cbFieldCompareNew = new ComboBox();
            cbFieldCompareNew.Name = "cbFieldCompare" + index;
            cbFieldCompareNew.Width = 120;
            cbFieldCompareNew.Height = 23;
            cbFieldCompareNew.HorizontalAlignment = HorizontalAlignment.Left;
            cbFieldCompareNew.VerticalAlignment = VerticalAlignment.Center;
            cbFieldCompareNew.Margin = new Thickness(6);
            Grid.SetColumn(cbFieldCompareNew, 3);
            Grid.SetRow(cbFieldCompareNew, index);
            this.grdCriteria.Children.Add(cbFieldCompareNew);

            criteriaRow.ComboBoxFieldCompare = cbFieldCompareNew;

            //bind fields to compare
            cbFieldCompareNew.ItemsSource = this._Fields;
            cbFieldCompareNew.DisplayMemberPath = "Name";
            if (cbFieldCompareNew.SelectedIndex < 0)
                cbFieldCompareNew.SelectedIndex = 0;

            //load value
            criteriaRow.Value = Functions.GetFromIsolatedStorage(Functions.GetId(this.Host, field.Name, "Value"));

            //<Button Content="Remove" Name="btnRemove" Width="60" Height="23" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="4" Grid.Row="0" Margin="6" Click="btnRemove_Click" />
            Button btnRemoveNew = new Button();
            btnRemoveNew.Name = "btnRemove" + index;
            btnRemoveNew.Content = "Remove";
            btnRemoveNew.Width = 60;
            btnRemoveNew.Height = 23;
            btnRemoveNew.HorizontalAlignment = HorizontalAlignment.Left;
            btnRemoveNew.VerticalAlignment = VerticalAlignment.Center;
            btnRemoveNew.Margin = new Thickness(6);
            Grid.SetColumn(btnRemoveNew, 4);
            Grid.SetRow(btnRemoveNew, index);
            btnRemoveNew.Click += btnRemove_Click;
            this.grdCriteria.Children.Add(btnRemoveNew);

            criteriaRow.ButtonRemove = btnRemoveNew;

            criteriaRow.Field = field;

            //add row to collection
            this.CriteriaRows.Add(criteriaRow);

            if (index < 4)
            {
                Grid.SetRow(this.btnAnd, index + 1);
            }

            foreach (CriteriaRow row in this.CriteriaRows)
            {
                row.ButtonRemove.Visibility = this.CriteriaRows.Count > 1 ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private List<ItemFieldDefinitionData> GetFields(string schemaTcmId)
        {
            if(string.IsNullOrEmpty(schemaTcmId))
                return new List<ItemFieldDefinitionData> {new ItemFieldDefinitionData { Name = "< Any Field >" }};
            
            List<ItemFieldDefinitionData> fields = Functions.GetSchemaFields(schemaTcmId) ?? new List<ItemFieldDefinitionData>();
            List<ItemFieldDefinitionData> metadataFields = Functions.GetSchemaMetadataFields(schemaTcmId);
            if (metadataFields != null && metadataFields.Count > 0)
            {
                foreach (ItemFieldDefinitionData metadataField in metadataFields)
                {
                    metadataField.Name = metadataField.Name + " [meta]";
                }
                fields.AddRange(metadataFields);
            }

            return fields;
        }

        private List<Operation> GetOperations(ItemFieldDefinitionData field)
        {
            if (field.IsRichText() || field.IsEmbedded() || field.Name == "< Any Field >")
            {
                return new List<Operation> { Operation.Like };
            }
            if (field.IsText())
            {
                return new List<Operation> { Operation.Equal, Operation.Like, Operation.EqualField };
            }
            if (field.IsDate() || field.IsNumber())
            {
                return new List<Operation> { Operation.Equal, Operation.Greater, Operation.Less, Operation.EqualField, Operation.GreaterField, Operation.LessField };
            }
            if (field.IsMultimedia())
            {
                return new List<Operation> { Operation.Equal, Operation.Like, Operation.EqualField };
            }
            return new List<Operation> { Operation.Equal };
        }

        private void BindComboValues(ItemFieldDefinitionData field, ComboBox cbValueCurrent)
        {
            if (field.IsKeyword())
            {
                KeywordFieldDefinitionData keywordField = (KeywordFieldDefinitionData)field;
                List<ItemInfo> keywords = Functions.GetKeywordsByCategory(keywordField.Category.IdRef);
                cbValueCurrent.ItemsSource = keywords;
                cbValueCurrent.DisplayMemberPath = "Title";
            }
            if (field.IsTextSelect())
            {
                SingleLineTextFieldDefinitionData textField = (SingleLineTextFieldDefinitionData)field;
                cbValueCurrent.ItemsSource = textField.List.Entries;
            }
        }
    }
}
