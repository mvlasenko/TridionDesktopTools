using System;
using System.Windows;
using System.Windows.Controls;
using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
{
    public class CriteriaRow
    {
        public ComboBox ComboBoxField { get; set; }
        public TextBlock TextBlockType { get; set; }
        public ComboBox ComboBoxOperation { get; set; }
        public TextBox TextBoxValue { get; set; }
        public DatePicker DatePickerValue { get; set; }
        public ComboBox ComboBoxValue { get; set; }
        public ComboBox ComboBoxFieldCompare { get; set; }
        public Button ButtonRemove { get; set; }

        public int Index { get; set; }

        public ItemFieldDefinitionData Field
        {
            get
            {
                return this.ComboBoxField.SelectedValue as ItemFieldDefinitionData;
            }
            set
            {
                if (this.ComboBoxField != null)
                    this.ComboBoxField.SelectedValue = value;

                this.SetControlVisibility();
            }
        }

        private void SetControlVisibility()
        {
            if (this.ComboBoxOperation.SelectedValue != null &&
                ((Operation) this.ComboBoxOperation.SelectedValue == Operation.EqualField ||
                 (Operation) this.ComboBoxOperation.SelectedValue == Operation.GreaterField ||
                 (Operation) this.ComboBoxOperation.SelectedValue == Operation.LessField ||
                 (Operation) this.ComboBoxOperation.SelectedValue == Operation.LikeField))
            {
                this.TextBoxValue.Visibility = Visibility.Hidden;
                this.DatePickerValue.Visibility = Visibility.Hidden;
                this.ComboBoxValue.Visibility = Visibility.Hidden;
                this.ComboBoxFieldCompare.Visibility = Visibility.Visible;
            }
            else if (this.ComboBoxField.SelectedValue != null && ((ItemFieldDefinitionData)this.ComboBoxField.SelectedValue).IsDate())
            {
                this.TextBoxValue.Visibility = Visibility.Hidden;
                this.DatePickerValue.Visibility = Visibility.Visible;
                this.ComboBoxValue.Visibility = Visibility.Hidden;
                this.ComboBoxFieldCompare.Visibility = Visibility.Hidden;
            }
            else if (this.ComboBoxField.SelectedValue != null && (((ItemFieldDefinitionData)this.ComboBoxField.SelectedValue).IsKeyword() || ((ItemFieldDefinitionData)this.ComboBoxField.SelectedValue).IsTextSelect()))
            {
                this.TextBoxValue.Visibility = Visibility.Hidden;
                this.DatePickerValue.Visibility = Visibility.Hidden;
                this.ComboBoxValue.Visibility = Visibility.Visible;
                this.ComboBoxFieldCompare.Visibility = Visibility.Hidden;
            }
            else
            {
                this.TextBoxValue.Visibility = Visibility.Visible;
                this.DatePickerValue.Visibility = Visibility.Hidden;
                this.ComboBoxValue.Visibility = Visibility.Hidden;
                this.ComboBoxFieldCompare.Visibility = Visibility.Hidden;
            }
        }

        public Operation Operation
        {
            get
            {
                return (Operation)this.ComboBoxOperation.SelectedValue;
            }
            set
            {
                if (this.ComboBoxOperation != null)
                    this.ComboBoxOperation.SelectedValue = value;

                this.SetControlVisibility();
            }
        }

        public object Value
        {
            get
            {
                if (this.TextBoxValue != null && this.TextBoxValue.Visibility == Visibility.Visible)
                {
                    if (this.Field.IsNumber())
                    {
                        try
                        {
                            return double.Parse(this.TextBoxValue.Text);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    return this.TextBoxValue.Text;
                }
                if (this.DatePickerValue != null && this.DatePickerValue.Visibility == Visibility.Visible)
                {
                    return this.DatePickerValue.SelectedDate;
                }
                if (this.ComboBoxValue != null && this.ComboBoxValue.Visibility == Visibility.Visible)
                {
                    if (this.Field.IsKeyword())
                    {
                        ItemInfo item = this.ComboBoxValue.SelectedValue as ItemInfo;
                        return item == null ? null : item.Title;
                    }
                    if (this.Field.IsTextSelect())
                    {
                        return this.ComboBoxValue.SelectedValue.ToString();
                    }
                }
                if (this.ComboBoxFieldCompare != null && this.ComboBoxFieldCompare.Visibility == Visibility.Visible)
                {
                    ItemFieldDefinitionData item = this.ComboBoxFieldCompare.SelectedValue as ItemFieldDefinitionData;
                    return item == null ? null : item.Name;
                }
                return null;
            }
            set
            {
                this.TextBoxValue.Text = value.ToString();
                try
                {
                    this.DatePickerValue.SelectedDate = value as DateTime?;
                }
                catch (Exception)
                {
                    this.DatePickerValue.SelectedDate = null;
                }
                try
                {
                    this.ComboBoxValue.Text = value.ToString();
                }
                catch (Exception)
                {
                    this.ComboBoxValue.SelectedValue = null;
                }
                try
                {
                    this.ComboBoxFieldCompare.Text = value.ToString();
                }
                catch (Exception)
                {
                    this.ComboBoxFieldCompare.SelectedValue = null;
                }
            }
        }
    }
}