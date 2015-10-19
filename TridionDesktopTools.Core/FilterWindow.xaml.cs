using System.Collections.Generic;
using System.Windows;

namespace TridionDesktopTools.Core
{
    public partial class FilterWindow
    {
        public string Host { private get; set; }

        public string SchemaUri { private get; set; }
        public List<Criteria> Criterias { get; private set; }

        public FilterWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SearchCriteriasControl1.Host = this.Host;

            this.SearchCriteriasControl1.InitSchema(this.SchemaUri);
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
