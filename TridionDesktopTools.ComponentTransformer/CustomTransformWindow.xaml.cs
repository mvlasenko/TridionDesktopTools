using System.Collections.Generic;
using System.Windows;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;

namespace TridionDesktopTools.ComponentTransformer
{
    public partial class CustomTransformWindow
    {
        public CustomTransformerInfo CustomComponentTransformer { get; set; }
        public CustomTransformerInfo CustomMetadataTransformer { get; set; }

        public TridionObjectInfo SourceTridionObject { private get; set; }
        public ItemInfo SourceSchema { private get; set; }
        public ItemInfo TargetSchema { private get; set; }

        public CustomTransformWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<CustomTransformerInfo> customTransformers = Functions.GetCustomTransformers(this.SourceSchema.Title, this.SourceSchema.SchemaType, Functions.GetItemType(this.SourceTridionObject.TcmId), this.TargetSchema.Title, this.TargetSchema.SchemaType);
            customTransformers.Insert(0, new CustomTransformerInfo { Title = "< none >" });

            this.cbCustomComponentTransformer.ItemsSource = customTransformers;
            this.cbCustomComponentTransformer.DisplayMemberPath = "Title";
            this.cbCustomComponentTransformer.SelectedIndex = this.CustomComponentTransformer != null ? customTransformers.FindIndex(x => x.TypeName == this.CustomComponentTransformer.TypeName) : 0;

            this.cbCustomMetadataTransformer.ItemsSource = customTransformers;
            this.cbCustomMetadataTransformer.DisplayMemberPath = "Title";
            this.cbCustomMetadataTransformer.SelectedIndex = this.CustomMetadataTransformer != null ? customTransformers.FindIndex(x => x.TypeName == this.CustomMetadataTransformer.TypeName) : 0;
        }
        
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.CustomComponentTransformer = this.cbCustomComponentTransformer.SelectedIndex == 0 ? null : this.cbCustomComponentTransformer.SelectedValue as CustomTransformerInfo;
            this.CustomMetadataTransformer = this.cbCustomMetadataTransformer.SelectedIndex == 0 ? null : this.cbCustomMetadataTransformer.SelectedValue as CustomTransformerInfo;

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
