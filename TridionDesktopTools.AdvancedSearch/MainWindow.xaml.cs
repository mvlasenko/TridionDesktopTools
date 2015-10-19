using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TridionDesktopTools.Core;

namespace TridionDesktopTools.AdvancedSearch
{
    public partial class MainWindow
    {
        public TridionObjectInfo TridionFolder { get; set; }
        public TridionSelectorMode TridionSelectorMode { get; set; }

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

            this.TridionSelectorMode = TridionSelectorMode.Folder;
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

            this.lblTridionFolder.Visibility = Visibility.Visible;
            this.treeTridionFolder.Visibility = Visibility.Visible;
            this.lblSchema.Visibility = Visibility.Visible;
            this.cbSchema.Visibility = Visibility.Visible;

            this.TridionFolder = new TridionObjectInfo();
            
            string containerTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "ContainerTcmId"));
            if (!String.IsNullOrEmpty(containerTcmId))
            {
                this.TridionFolder.TcmId = containerTcmId;
            }

            string strContainerTcmIdPath = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "ContainerTcmIdPath"));
            if (!String.IsNullOrEmpty(strContainerTcmIdPath))
            {
                this.TridionFolder.TcmIdPath = strContainerTcmIdPath.Split(';').ToList();
            }

            List<ItemInfo> publications = Functions.GetPublications().Expand(TridionSelectorMode, this.TridionFolder.TcmIdPath, this.TridionFolder.TcmId).MakeExpandable();
            this.treeTridionFolder.ItemsSource = publications;

            if (publications != null && publications.Count > 0)
            {
                //save to isolated stoage
                Functions.SaveToIsolatedStorage("Host", this.txtHost.Text);
                Functions.SaveToIsolatedStorage("Username", this.txtUsername.Text);
                Functions.SaveToIsolatedStorage("Password", this.txtPassword.Password);
            }

            this.SearchCriteriasControl1.Host = this.txtHost.Text;
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

            this.TridionFolder.TcmId = item.TcmId;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "ContainerTcmId"), item.TcmId);

            List<ItemInfo> list = new List<ItemInfo>();
            Functions.AddPathItem(list, item);

            this.TridionFolder.TcmIdPath = list.Select(x => x.TcmId).ToList();
            
            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "ContainerTcmIdPath"), String.Join(";", this.TridionFolder.TcmIdPath));

            list.Reverse();
            this.TridionFolder.NamedPath = string.Join("/", list.Select(x => x.Title));

            List<ItemInfo> schemas = new List<ItemInfo>();
            schemas.Add(new ItemInfo { Title = "< Any >" });
            schemas.AddRange(Functions.GetSchemas(Functions.GetPublicationTcmId(item.TcmId)).OrderBy(x => x.Title));
            this.cbSchema.ItemsSource = schemas;
            this.cbSchema.DisplayMemberPath = "Title";

            string schemaToSearchTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "SchemaTcmId"));
            if (!String.IsNullOrEmpty(schemaToSearchTcmId))
            {
                this.cbSchema.SelectedIndex = schemas.FindIndex(x => x.TcmId == schemaToSearchTcmId);
            }
            if(this.cbSchema.SelectedIndex < 0) this.cbSchema.SelectedIndex = 0;
        }

        private void treeTridionFolder_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ItemInfo item = this.treeTridionFolder.SelectedItem as ItemInfo;
            if (item == null)
                return;

            Process.Start(Functions.GetItemCmsUrl(this.txtHost.Text, item.TcmId, item.Title));
        }

        private void cbSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemInfo schema = this.cbSchema.SelectedValue as ItemInfo;
            if (schema == null)
                return;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "SchemaTcmId"), schema.TcmId ?? "tcm:0-0-8");

            this.SearchCriteriasControl1.InitSchema(schema.TcmId);

            this.SearchCriteriasControl1.Visibility = Visibility.Visible;
            this.spButtons.Visibility = Visibility.Visible;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ItemInfo schema = cbSchema.SelectedValue as ItemInfo;
            if (schema == null)
                return;

            List<Criteria> criterias = this.SearchCriteriasControl1.GetCriterias();

            //get results
            List<ItemInfo> res = Functions.GetComponentsByCriterias(this.TridionFolder.TcmId, schema.TcmId, criterias);
            
            //show results
            ResultsWindow dialog = new ResultsWindow();
            dialog.Host = this.txtHost.Text;
            dialog.ListBoxReport.ItemsSource = this.CheckResults(res.Select(itemInfo => new ResultInfo { TcmId = itemInfo.TcmId, ItemType = itemInfo.ItemType, Status = Status.Info, Message = string.Format("{0} ({1})", itemInfo.Title, itemInfo.TcmId) }).ToList());
            dialog.ListBoxReport.MouseDoubleClick += lbReport_OnMouseDoubleClick;
            dialog.Show();
        }

        private void lbReport_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null)
                return;

            ResultInfo item = listBox.SelectedItem as ResultInfo;
            if (item == null || string.IsNullOrEmpty(item.TcmId))
                return;

            Process.Start(Functions.GetItemCmsUrl(this.txtHost.Text, item.TcmId));
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private List<ResultInfo> CheckResults(List<ResultInfo> results)
        {
            if (results != null && results.Any())
                return results;

            List<ResultInfo> newResults = new List<ResultInfo>();

            ResultInfo resultFinish = new ResultInfo();
            resultFinish.Status = Status.Info;
            resultFinish.Message = "No Results Found";
            newResults.Add(resultFinish);

            return newResults;
        }

    }
}