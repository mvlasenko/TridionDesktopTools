using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
{
    public partial class NameTransformOptionsWindow
    {
        public List<FieldInfo> SourceSchemaFields { private get; set; }
        public string DbHost { private get; set; }
        public string SourceDatabase { private get; set; }
        public string SourceTable { private get; set; }

        public string Host { private get; set; }
        public string SourceSchemaUri { private get; set; }
        
        public string FormatString { get; private set; }
        public List<ReplacementInfo> Replacements { get; private set; }

        public NameTransformOptionsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> sourceFields = this.GetSourceFieldNames();

            this.cbReplacement1.ItemsSource = sourceFields;
            this.cbReplacement2.ItemsSource = sourceFields;
            this.cbReplacement3.ItemsSource = sourceFields;

            this.txtFormatString.Text = Functions.GetFromIsolatedStorage(this.GetKey("FormatString"));
            if(string.IsNullOrEmpty(this.txtFormatString.Text))
                this.txtFormatString.Text = "{0}";

            string replacement1 = Functions.GetFromIsolatedStorage(this.GetKey("Replacement1"));
            if (!String.IsNullOrEmpty(replacement1))
            {
                this.cbReplacement1.SelectedIndex = sourceFields.FindIndex(x => x == replacement1);
            }
            if (this.cbReplacement1.SelectedIndex < 0) this.cbReplacement1.SelectedIndex = 0;

            this.txtRegex1.Text = Functions.GetFromIsolatedStorage(this.GetKey("Regex1"));
            if(string.IsNullOrEmpty(this.txtRegex1.Text))
                this.txtRegex1.Text = ".+";

            string replacement2 = Functions.GetFromIsolatedStorage(this.GetKey("Replacement2"));
            if (!String.IsNullOrEmpty(replacement2))
            {
                this.cbReplacement2.SelectedIndex = sourceFields.FindIndex(x => x == replacement2);
            }

            this.txtRegex2.Text = Functions.GetFromIsolatedStorage(this.GetKey("Regex2"));

            string replacement3 = Functions.GetFromIsolatedStorage(this.GetKey("Replacement3"));
            if (!String.IsNullOrEmpty(replacement3))
            {
                this.cbReplacement3.SelectedIndex = sourceFields.FindIndex(x => x == replacement3);
            }

            this.txtRegex3.Text = Functions.GetFromIsolatedStorage(this.GetKey("Regex3"));
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.FormatString = this.txtFormatString.Text;
            Functions.SaveToIsolatedStorage(this.GetKey("FormatString"), this.txtFormatString.Text);
            
            this.Replacements = new List<ReplacementInfo>();

            if (this.FormatString.Contains("{0}") && this.cbReplacement1.SelectedIndex >= 0)
            {
                this.Replacements.Add(this.GetReplacement(this.cbReplacement1, this.txtRegex1.Text));
                Functions.SaveToIsolatedStorage(this.GetKey("Replacement1"), this.cbReplacement1.Text);
                Functions.SaveToIsolatedStorage(this.GetKey("Regex1"), this.txtRegex1.Text);
            }

            if (this.FormatString.Contains("{1}") && this.cbReplacement2.SelectedIndex >= 0)
            {
                this.Replacements.Add(this.GetReplacement(this.cbReplacement2, this.txtRegex2.Text));
                Functions.SaveToIsolatedStorage(this.GetKey("Replacement2"), this.cbReplacement2.Text);
                Functions.SaveToIsolatedStorage(this.GetKey("Regex2"), this.txtRegex2.Text);
            }

            if (this.FormatString.Contains("{2}") && this.cbReplacement3.SelectedIndex >= 0)
            {
                this.Replacements.Add(this.GetReplacement(this.cbReplacement3, this.txtRegex3.Text));
                Functions.SaveToIsolatedStorage(this.GetKey("Replacement3"), this.cbReplacement3.Text);
                Functions.SaveToIsolatedStorage(this.GetKey("Regex3"), this.txtRegex3.Text);
            }

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private List<string> GetSourceFieldNames()
        {
            if (this.SourceSchemaFields != null && this.SourceSchemaFields.Any())
            {
                return this.SourceSchemaFields.Select(x => x.GetFieldFullName(false)).ToList();
            }

            List<string> sourceFields = new[] { "[Title]", "[TcmId]", "[ID]" }.ToList();

            if (!string.IsNullOrEmpty(this.SourceSchemaUri))
            {
                List<ItemFieldDefinitionData> sourceComponentFields = Functions.GetSchemaFields(this.SourceSchemaUri);
                List<ItemFieldDefinitionData> sourceMetadataFields = Functions.GetSchemaMetadataFields(this.SourceSchemaUri);
                this.SourceSchemaFields = Functions.GetAllFields(sourceComponentFields, sourceMetadataFields, false, false);

                sourceFields.AddRange(this.SourceSchemaFields.Select(x => x.GetFieldFullName(false)).ToList());
            }

            return sourceFields;
        }

        private ReplacementInfo GetReplacement(ComboBox cb, string regex)
        {
            ReplacementInfo replacement = new ReplacementInfo();
            replacement.Regex = regex;

            string fragment = cb.Text;
            if (fragment == "[Title]" || fragment == "[TcmId]" || fragment == "[ID]")
            {
                replacement.Fragment = fragment;
            }
            else
            {
                replacement.Field = this.SourceSchemaFields.FirstOrDefault(x => x.GetFieldFullName(false) == fragment);
            }

            return replacement;
        }

        private string GetKey(string suffix)
        {
            return !string.IsNullOrEmpty(this.SourceSchemaUri) ? 
                Functions.GetId(this.Host.GetDomainName(), this.SourceSchemaUri, suffix) : 
                Functions.GetId(this.DbHost, this.SourceDatabase, this.SourceTable, suffix);
        }
    }
}
