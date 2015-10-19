using System.Collections.Generic;
using System.Windows;
using TridionDesktopTools.Core;

namespace TridionDesktopTools.ComponentImporter
{
    public partial class CustomImportWindow
    {
        public CustomTransformerInfo CustomComponentImporter { get; set; }
        public CustomTransformerInfo CustomMetadataImporter { get; set; }

        public string Sql
        {
            get
            {
                return this.txtSQL.Text;
            }
            set
            {
                this.txtSQL.Text = value;
            }
        }

        public string SourceTable { private get; set; }
        public ItemInfo TargetSchema { private get; set; }

        public CustomImportWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<CustomTransformerInfo> customImporters = Functions.GetCustomImporters(this.SourceTable, this.TargetSchema.Title, this.TargetSchema.SchemaType);
            customImporters.Insert(0, new CustomTransformerInfo { Title = "< none >" });

            this.cbCustomComponentImporter.ItemsSource = customImporters;
            this.cbCustomComponentImporter.DisplayMemberPath = "Title";
            this.cbCustomComponentImporter.SelectedIndex = this.CustomComponentImporter != null ? customImporters.FindIndex(x => x.TypeName == this.CustomComponentImporter.TypeName) : 0;

            this.cbCustomMetadataImporter.ItemsSource = customImporters;
            this.cbCustomMetadataImporter.DisplayMemberPath = "Title";
            this.cbCustomMetadataImporter.SelectedIndex = this.CustomMetadataImporter != null ? customImporters.FindIndex(x => x.TypeName == this.CustomMetadataImporter.TypeName) : 0;
        }
        
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.CustomComponentImporter = this.cbCustomComponentImporter.SelectedIndex == 0 ? null : this.cbCustomComponentImporter.SelectedValue as CustomTransformerInfo;
            this.CustomMetadataImporter = this.cbCustomMetadataImporter.SelectedIndex == 0 ? null : this.cbCustomMetadataImporter.SelectedValue as CustomTransformerInfo;

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

    }
}