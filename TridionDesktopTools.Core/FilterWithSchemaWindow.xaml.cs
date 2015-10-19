using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TridionDesktopTools.Core
{
    public partial class FilterWithSchemaWindow
    {
        public string Host { private get; set; }

        public string PublicationUri { private get; set; }
        public string SchemaUri { get; private set; }
        public List<Criteria> Criterias { get; private set; }

        public FilterWithSchemaWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SearchCriteriasControl1.Host = this.Host;

            List<ItemInfo> schemas = new List<ItemInfo>();
            schemas.Add(new ItemInfo {Title = "< Any >"});
            schemas.AddRange(Functions.GetSchemas(this.PublicationUri).OrderBy(x => x.Title));
            this.cbSchema.ItemsSource = schemas;
            this.cbSchema.DisplayMemberPath = "Title";

            string schemaToSearchTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.Host.GetDomainName(), "SchemaTcmId"));
            if (!String.IsNullOrEmpty(schemaToSearchTcmId))
            {
                this.cbSchema.SelectedIndex = schemas.FindIndex(x => x.TcmId == schemaToSearchTcmId);
            }
            if (this.cbSchema.SelectedIndex < 0) this.cbSchema.SelectedIndex = 0;
        }

        private void cbSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemInfo schema = this.cbSchema.SelectedValue as ItemInfo;
            if (schema == null)
                return;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.Host.GetDomainName(), "SchemaTcmId"), schema.TcmId ?? "tcm:0-0-8");

            this.SchemaUri = schema.TcmId;
            this.SearchCriteriasControl1.InitSchema(schema.TcmId);

            this.SearchCriteriasControl1.Visibility = Visibility.Visible;
            this.spButtons.Visibility = Visibility.Visible;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Criterias = this.SearchCriteriasControl1.GetCriterias();
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