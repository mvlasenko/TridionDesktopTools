using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;

namespace TridionDesktopTools.Deleter
{
    public partial class MainWindow
    {
        public TridionObjectInfo TridionObject { get; set; }
        public TridionSelectorMode TridionSelectorMode { get; set; }

        private List<ResultInfo> _CheckResults;

        private List<Criteria> _Criterias;
        private string _SchemaUri;

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException +=
                (oEx, eEx) =>
                {
                    var eventLogName = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
                    if (!EventLog.SourceExists(eventLogName))
                    {
                        EventLog.CreateEventSource(eventLogName, "Application");
                    }
                    string strToAdd = ((Exception)eEx.ExceptionObject).StackTrace;
                    EventLog.WriteEntry(eventLogName, strToAdd, EventLogEntryType.Error);

                    MessageBox.Show("Unhadnled exception occured. Please look 'Application' event log for more details.", "Unhadnled exception", MessageBoxButton.OK, MessageBoxImage.Error);
                };

            this.cbBindingType.ItemsSource = Enum.GetNames(typeof(BindingType));
            this.cbBindingType.Text = Functions.GetFromIsolatedStorage("BindingType");
            if (String.IsNullOrEmpty(this.cbBindingType.Text))
                this.cbBindingType.SelectedIndex = 0;

            //get from isolated stoage
            this.txtHost.Text = Functions.GetFromIsolatedStorage("Host");
            this.txtUsername.Text = Functions.GetFromIsolatedStorage("Username");
            this.txtPassword.Password = Functions.GetFromIsolatedStorage("Password");

            this.TridionSelectorMode = TridionSelectorMode.Any;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            Functions.ResetClient();

            Functions.ClientBindingType = (BindingType)Enum.Parse(typeof(BindingType), this.cbBindingType.Text);
            Functions.SaveToIsolatedStorage("BindingType", this.cbBindingType.Text);

            if (!Functions.EnsureValidClient(this.txtHost.Text, this.txtUsername.Text, this.txtPassword.Password))
                return;

            if (!Functions.EnsureValidStreamDownloadClient(this.txtHost.Text, this.txtUsername.Text, this.txtPassword.Password))
                return;

            if (!Functions.EnsureValidStreamUploadClient(this.txtHost.Text, this.txtUsername.Text, this.txtPassword.Password))
                return;

            this.lblTridionObject.Visibility = Visibility.Visible;
            this.treeTridionObject.Visibility = Visibility.Visible;
            
            this.TridionObject = new TridionObjectInfo();
            
            string containerTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "ContainerTcmId"));
            if (!String.IsNullOrEmpty(containerTcmId))
            {
                this.TridionObject.TcmId = containerTcmId;
            }

            string strContainerTcmIdPath = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "ContainerTcmIdPath"));
            if (!String.IsNullOrEmpty(strContainerTcmIdPath))
            {
                this.TridionObject.TcmIdPath = strContainerTcmIdPath.Split(';').ToList();
            }

            List<ItemInfo> publications = Functions.GetPublications().Expand(TridionSelectorMode, this.TridionObject.TcmIdPath, this.TridionObject.TcmId).MakeExpandable();
            this.treeTridionObject.ItemsSource = publications;

            if (publications != null && publications.Count > 0)
            {
                //save to isolated stoage
                Functions.SaveToIsolatedStorage("Host", this.txtHost.Text);
                Functions.SaveToIsolatedStorage("Username", this.txtUsername.Text);
                Functions.SaveToIsolatedStorage("Password", this.txtPassword.Password);
            }
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            ItemInfo item = ((TreeViewItem)e.OriginalSource).DataContext as ItemInfo;
            if (item == null)
                return;

            Functions.OnItemExpanded(item, this.TridionSelectorMode);
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            ItemInfo item = ((TreeViewItem)e.OriginalSource).DataContext as ItemInfo;
            if (item == null)
                return;

            this.TridionObject.TcmId = item.TcmId;

            ItemType itemType = Functions.GetItemType(item.TcmId);

            this.btnFilter.IsEnabled = itemType == ItemType.Folder;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "ContainerTcmId"), item.TcmId);

            List<ItemInfo> list = new List<ItemInfo>();
            Functions.AddPathItem(list, item);

            this.TridionObject.TcmIdPath = list.Select(x => x.TcmId).ToList();
            
            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "ContainerTcmIdPath"), String.Join(";", this.TridionObject.TcmIdPath));

            list.Reverse();
            this.TridionObject.NamedPath = string.Join("/", list.Select(x => x.Title));

            this._CheckResults = null;

            this.spButtons.Visibility = Visibility.Visible;
        }

        private void treeTridionObject_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ItemInfo item = this.treeTridionObject.SelectedItem as ItemInfo;
            if (item == null)
                return;

            Process.Start(Functions.GetItemCmsUrl(this.txtHost.Text, item.TcmId, item.Title));
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterWithSchemaWindow dialog = new FilterWithSchemaWindow();
            dialog.Host = this.txtHost.Text;
            dialog.PublicationUri = Functions.GetPublicationTcmId(this.TridionObject.TcmId);
            bool res = dialog.ShowDialog() == true;
            if (res)
            {
                this._Criterias = dialog.Criterias;
                this._SchemaUri = dialog.SchemaUri;

                if (!string.IsNullOrEmpty(this._SchemaUri) && this._Criterias != null && this._Criterias.Any())
                {
                    this.btnFilter.SetEnabledGreen();
                    this._CheckResults = null;
                }
            }
        }

        private void btnCheckDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!Functions.ExistsItem(this.TridionObject.TcmId))
                return;

            if (this._CheckResults == null)
            {
                this._CheckResults = new List<ResultInfo>();
                Functions.Delete(this.TridionObject.TcmId, false, this._SchemaUri, this._Criterias, this._CheckResults);
            }

            //show results
            ResultsWindow dialog = new ResultsWindow();
            dialog.Host = this.txtHost.Text;
            dialog.ListBoxReport.ItemsSource = this.CheckResults(this._CheckResults);
            dialog.ListBoxReport.MouseDoubleClick += lbReport_OnMouseDoubleClick;
            dialog.Show();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!Functions.ExistsItem(this.TridionObject.TcmId))
                return;
            
            if (MessageBox.Show("Delete selected item and all related items?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            if (this._CheckResults == null)
            {
                this._CheckResults = new List<ResultInfo>();
                Functions.Delete(this.TridionObject.TcmId, false, this._SchemaUri, this._Criterias, this._CheckResults);
            }

            ResultsWindow dialog;

            if (this._CheckResults.Any(x => x.Status == Status.Warning || x.Status == Status.Error))
            {
                if (MessageBox.Show("Some items probably won't be deleted. \n\nTry to delete anyway?", "Problem items detected", MessageBoxButton.YesNo, MessageBoxImage.Stop, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    //show results
                    dialog = new ResultsWindow();
                    dialog.Host = this.txtHost.Text;
                    dialog.ListBoxReport.ItemsSource = this.CheckResults(this._CheckResults);
                    dialog.ListBoxReport.MouseDoubleClick += lbReport_OnMouseDoubleClick;
                    dialog.Show();

                    return;
                }
            }

            List<ResultInfo> results = new List<ResultInfo>();
            Functions.Delete(this.TridionObject.TcmId, true, this._SchemaUri, this._Criterias, results);

            //show results
            dialog = new ResultsWindow();
            dialog.Host = this.txtHost.Text;
            dialog.ListBoxReport.ItemsSource = this.CheckResults(results);
            dialog.ListBoxReport.MouseDoubleClick += lbReport_OnMouseDoubleClick;
            dialog.Show();

            //reload tree
            List<ItemInfo> publications = Functions.GetPublications().Expand(TridionSelectorMode, this.TridionObject.TcmIdPath, this.TridionObject.TcmId).MakeExpandable();
            this.treeTridionObject.ItemsSource = publications;

            this._CheckResults = null;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lbReport_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null)
                return;

            ResultInfo item = listBox.SelectedItem as ResultInfo;
            if (item == null || string.IsNullOrEmpty(item.TcmId))
                return;

            string tcmFolder = Functions.GetItemContainer(item.TcmId);
            if (string.IsNullOrEmpty(tcmFolder))
                return;

            Process.Start(Functions.GetItemCmsUrl(this.txtHost.Text, item.TcmId));
        }

        private List<ResultInfo> CheckResults(List<ResultInfo> results)
        {
            if (results != null && results.Any())
                return results;

            List<ResultInfo> newResults = new List<ResultInfo>();

            ResultInfo resultFinish = new ResultInfo();
            resultFinish.Status = Status.Info;
            resultFinish.Message = "No Actions Made";
            newResults.Add(resultFinish);

            return newResults;
        }

    }
}