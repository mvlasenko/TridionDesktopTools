using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace TridionDesktopTools.Core
{
    public partial class ResultsWindow
    {
        public ListBox ListBoxReport
        {
            get
            {
                return this.lbReport;
            }
        }

        public string Host { private get; set; }
        
        public ResultsWindow()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            string res = "";
            foreach (ResultInfo item in this.lbReport.ItemsSource)
            {
                if (string.IsNullOrEmpty(item.TcmId))
                    continue;
                res += Functions.GetItemCmsUrl(this.Host, item.TcmId) + "\r\n";
            }
            File.WriteAllText("C:\\export.txt", res);
            Process.Start("C:\\export.txt");
        }

    }
}