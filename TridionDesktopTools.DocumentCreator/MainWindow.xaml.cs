using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;
using System.Windows.Input;

namespace TridionDesktopTools.DocumentCreator
{
    public partial class MainWindow
    {
        public TridionObjectInfo TridionObject { get; set; }
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

            this.TridionSelectorMode = TridionSelectorMode.Folder | TridionSelectorMode.Schema | TridionSelectorMode.ComponentTemplate | TridionSelectorMode.PageTemplate | TridionSelectorMode.TemplateBuildingBlock;
            this.txtFolder.Text = Functions.GetFromIsolatedStorage("GeneratedDocuments");
            if (string.IsNullOrEmpty(this.txtFolder.Text))
                this.txtFolder.Text = "C:\\";
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

            this.lblFolder.Visibility = Visibility.Visible;
            this.spOptions.Visibility = Visibility.Visible;

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

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "ContainerTcmId"), item.TcmId);

            List<ItemInfo> list = new List<ItemInfo>();
            Functions.AddPathItem(list, item);

            this.TridionObject.TcmIdPath = list.Select(x => x.TcmId).ToList();

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "ContainerTcmIdPath"), String.Join(";", this.TridionObject.TcmIdPath));

            list.Reverse();
            this.TridionObject.NamedPath = string.Join("/", list.Select(x => x.Title));

            this.spButtons.Visibility = Visibility.Visible;
        }

        private void treeTridionObject_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ItemInfo item = this.treeTridionObject.SelectedItem as ItemInfo;
            if (item == null)
                return;

            Process.Start(Functions.GetItemCmsUrl(this.txtHost.Text, item.TcmId, item.Title));
        }

        private void txtFolder_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.txtFolder.Text = dialog.SelectedPath;
                Functions.SaveToIsolatedStorage("GeneratedDocuments", this.txtFolder.Text);
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string resultFileName = System.IO.Path.Combine(this.txtFolder.Text, "Item_" + this.TridionObject.TcmId.GetId() + ".docx");
                
                if (this.chkDependencies.IsChecked == true)
                {
                    List<string> chainIds = new List<string>();
                    FillUsingChain(this.TridionObject.TcmId, chainIds);
                    chainIds = chainIds.Distinct().ToList();

                    List<byte[]> chainFiles = new List<byte[]>();
                    chainFiles.AddRange(chainIds.Select(GetItemDocument));
                        
                    WordHelper wordHelper = new WordHelper();
                    wordHelper.JoinDocuments(chainFiles, resultFileName);
                }
                else
                {
                    byte[] resultFile = GetItemDocument(this.TridionObject.TcmId);
                    System.IO.File.WriteAllBytes(resultFileName, resultFile);
                }

                Process.Start(resultFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillUsingChain(string id, List<string> chainId)
        {
            chainId.Add(id);
            
            List<string> items = Functions.GetUsedItems(id, new[] { ItemType.Schema, ItemType.ComponentTemplate, ItemType.PageTemplate, ItemType.TemplateBuildingBlock });

            foreach (string childId in items)
            {
                FillUsingChain(childId, chainId);
            }
        }

        private byte[] GetItemDocument(string id)
        {
            WordHelper wordHelper = new WordHelper();

            ItemType itemType = Functions.GetItemType(id);

            byte[] binWordFile = null;
            
            if (itemType == ItemType.Schema)
            {
                SchemaDocumentData schemaData = SchemaHelper.GetSchemaData(Functions.Client, id);
                if (schemaData == null)
                {
                    throw new Exception("Schema is null");
                }
                binWordFile = wordHelper.CreateSchemaDocument(schemaData, System.IO.Path.Combine(".", "schema.docx"));
            }
            if (itemType == ItemType.PageTemplate)
            {
                PageTemplateDocumentData ptData = PageTemplateHelper.GetPageTemplateData(Functions.Client, id);
                if (ptData == null)
                {
                    throw new Exception("Page Template is null");
                }
                binWordFile = wordHelper.CreatePageTemplateDocument(ptData, System.IO.Path.Combine(".", "pt.docx"));
            }
            if (itemType == ItemType.ComponentTemplate)
            {
                ComponentTemplateDocumentData ctData = ComponentTemplateHelper.GetComponentTemplateData(Functions.Client, id);
                if (ctData == null)
                {
                    throw new Exception("Component Template is null");
                }
                binWordFile = wordHelper.CreateComponentTemplateDocument(ctData, System.IO.Path.Combine(".", "ct.docx"));
            }
            if (itemType == ItemType.TemplateBuildingBlock)
            {
                TbbDocumentData tbbData = TBBHelper.GetTBBData(Functions.Client, id);
                if (tbbData == null)
                {
                    throw new Exception("TBB is null");
                }
                binWordFile = wordHelper.CreateTbbDocument(tbbData, System.IO.Path.Combine(".", "tbb.docx"));
            }

            return binWordFile;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
