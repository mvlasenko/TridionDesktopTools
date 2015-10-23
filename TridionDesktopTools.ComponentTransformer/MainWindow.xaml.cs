using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;
using FieldInfo = TridionDesktopTools.Core.FieldInfo;

namespace TridionDesktopTools.ComponentTransformer
{
    public partial class MainWindow
    {
        public TridionObjectInfo SourceTridionObject { get; set; }
        public TridionObjectInfo TargetTridionFolder { get; set; }
        public TridionSelectorMode SourceTridionSelectorMode { get; set; }
        public TridionSelectorMode TargetTridionSelectorMode { get; set; }

        public HistoryMappingInfo HistoryMapping { get; set; }

        private List<Criteria> _Criterias;
        private string _FormatString;
        private List<ReplacementInfo> _Replacements;

        private List<ItemInfo> _Publications1;
        private List<ItemInfo> _Publications2;

        private string _CurrentSourcePublication;
        private string _CurrentTargetPublication;

        private List<ItemInfo> _SourceSchemas;
        private List<ItemInfo> _TargetSchemas;

        private List<FieldInfo> _TargetSchemaFields;

        public CustomTransformerInfo CustomComponentTransformer { get; set; }
        public CustomTransformerInfo CustomMetadataTransformer { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException +=
                (oEx, eEx) =>
                {
                    var eventLogName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
                    if (!EventLog.SourceExists(eventLogName))
                    {
                        EventLog.CreateEventSource(eventLogName, "Application");
                    }
                    string strToAdd = ((Exception) eEx.ExceptionObject).StackTrace;
                    EventLog.WriteEntry(eventLogName, strToAdd, EventLogEntryType.Error);

                    MessageBox.Show("Unhadnled exception occured. Please look 'Application' event log for more details.", "Unhadnled exception", MessageBoxButton.OK, MessageBoxImage.Error);
                };

            this.cbBindingType.ItemsSource = Enum.GetNames(typeof(BindingType));
            this.cbBindingType.Text = Functions.GetFromIsolatedStorage("BindingType");
            if(String.IsNullOrEmpty(this.cbBindingType.Text))
                this.cbBindingType.SelectedIndex = 0;

            //get from isolated stoage
            this.txtHost.Text = Functions.GetFromIsolatedStorage("Host");
            this.txtUsername.Text = Functions.GetFromIsolatedStorage("Username");
            this.txtPassword.Password = Functions.GetFromIsolatedStorage("Password");

            this.SourceTridionSelectorMode = TridionSelectorMode.Folder | TridionSelectorMode.Component | TridionSelectorMode.StructureGroup | TridionSelectorMode.Page;
            this.TargetTridionSelectorMode = TridionSelectorMode.Folder;
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

            this.lblSourceTridionObject.Visibility = Visibility.Visible;
            this.treeSourceTridionObject.Visibility = Visibility.Visible;
            this.grdTargetTridionFolder.Visibility = Visibility.Visible;
            this.treeTargetTridionFolder.Visibility = Visibility.Visible;

            this.SourceTridionObject = new TridionObjectInfo();

            string sourceItemTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "SourceItemTcmId"));
            if (!String.IsNullOrEmpty(sourceItemTcmId))
            {
                this.SourceTridionObject.TcmId = sourceItemTcmId;
            }

            string sourceItemTcmIdPath = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "SourceItemTcmIdPath"));
            if (!String.IsNullOrEmpty(sourceItemTcmIdPath))
            {
                this.SourceTridionObject.TcmIdPath = sourceItemTcmIdPath.Split(';').ToList();
            }

            this.TargetTridionFolder = new TridionObjectInfo();

            string targetItemTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetItemTcmId"));
            if (!String.IsNullOrEmpty(targetItemTcmId))
            {
                this.TargetTridionFolder.TcmId = targetItemTcmId;
            }

            string targetItemTcmIdPath = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetItemTcmIdPath"));
            if (!String.IsNullOrEmpty(targetItemTcmIdPath))
            {
                this.TargetTridionFolder.TcmIdPath = targetItemTcmIdPath.Split(';').ToList();
            }

            this._Publications1 = Functions.GetPublications();
            this.treeSourceTridionObject.ItemsSource = this._Publications1.Expand(this.SourceTridionSelectorMode, this.SourceTridionObject.TcmIdPath, this.SourceTridionObject.TcmId).MakeExpandable();

            this._Publications2 = Functions.GetPublications();
            this.treeTargetTridionFolder.ItemsSource = this._Publications2.Expand(this.TargetTridionSelectorMode, this.TargetTridionFolder.TcmIdPath, this.TargetTridionFolder.TcmId).MakeExpandable();

            this.chkSameFolder.IsChecked = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "SameFolder")) == "true";

            if (this._Publications1 != null && this._Publications1.Count > 0 && this._Publications2 != null && this._Publications2.Count > 0)
            {
                //save to isolated stoage
                Functions.SaveToIsolatedStorage("Host", this.txtHost.Text);
                Functions.SaveToIsolatedStorage("Username", this.txtUsername.Text);
                Functions.SaveToIsolatedStorage("Password", this.txtPassword.Password);
            }
        }

        private void treeSourceTridionObject_Expanded(object sender, RoutedEventArgs e)
        {
            ItemInfo item = ((TreeViewItem)e.OriginalSource).DataContext as ItemInfo;
            if (item == null)
                return;

            Functions.OnItemExpanded(item, this.SourceTridionSelectorMode);
        }

        private void treeTargetTridionFolder_Expanded(object sender, RoutedEventArgs e)
        {
            ItemInfo item = ((TreeViewItem)e.OriginalSource).DataContext as ItemInfo;
            if (item == null)
                return;

            Functions.OnItemExpanded(item, this.TargetTridionSelectorMode);
        }

        private void treeSourceTridionObject_Selected(object sender, RoutedEventArgs e)
        {
            ItemInfo item = ((TreeViewItem)e.OriginalSource).DataContext as ItemInfo;
            if (item == null)
                return;

            bool itemTypeChangedOrNew = Functions.GetItemType(this.SourceTridionObject.TcmId) != item.ItemType || this._SourceSchemas == null;

            this.SourceTridionObject.TcmId = item.TcmId;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "SourceItemTcmId"), item.TcmId);

            List<ItemInfo> list = new List<ItemInfo>();
            Functions.AddPathItem(list, item);

            this.SourceTridionObject.TcmIdPath = list.Select(x => x.TcmId).ToList();

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "SourceItemTcmIdPath"), String.Join(";", this.SourceTridionObject.TcmIdPath));

            list.Reverse();
            this.SourceTridionObject.NamedPath = string.Join("/", list.Select(x => x.Title));

            if (this.chkSameFolder.IsChecked == true)
                this.TargetTridionFolder.NamedPath = string.Join("/", list.Select(x => x.Title));
            
            if (this._CurrentSourcePublication != Functions.GetPublicationTcmId(item.TcmId))
            {
                this._CurrentSourcePublication = Functions.GetPublicationTcmId(item.TcmId);
                this._SourceSchemas = Functions.GetSchemas(this._CurrentSourcePublication);
                itemTypeChangedOrNew = true;
            }

            if (this._SourceSchemas != null && this._SourceSchemas.Count > 0 && itemTypeChangedOrNew)
            {
                if (item.ItemType == ItemType.Component || item.ItemType == ItemType.Folder)
                {
                    this._SourceSchemas = this._SourceSchemas.Where(x => x.SchemaType == SchemaType.Component || x.SchemaType == SchemaType.Multimedia).OrderBy(x => x.Title).ToList();
                }
                else if (item.ItemType == ItemType.Page || item.ItemType == ItemType.StructureGroup)
                {
                    this._SourceSchemas = this._SourceSchemas.Where(x => x.SchemaType == SchemaType.Metadata).OrderBy(x => x.Title).ToList();
                }
                else
                {
                    this._SourceSchemas = this._SourceSchemas.OrderBy(x => x.Title).ToList();
                }

                this.cbSourceSchema.ItemsSource = this._SourceSchemas;
                this.cbSourceSchema.DisplayMemberPath = "Title";

                string sourceSchemaTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "SourceSchemaTcmId"));
                if (!String.IsNullOrEmpty(sourceSchemaTcmId))
                {
                    this.cbSourceSchema.SelectedIndex = this._SourceSchemas.FindIndex(x => x.TcmId == sourceSchemaTcmId);
                }

                if (this.chkSameFolder.IsChecked == true)
                {
                    this.cbTargetSchema.ItemsSource = this._SourceSchemas;
                    this.cbTargetSchema.DisplayMemberPath = "Title";

                    string targetSchemaTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetSchemaTcmId"));
                    if (!String.IsNullOrEmpty(targetSchemaTcmId))
                    {
                        this.cbTargetSchema.SelectedIndex = this._SourceSchemas.FindIndex(x => x.TcmId == targetSchemaTcmId);
                    }
                }
            }

            this.lblSourceSchema.Visibility = Visibility.Visible;
            this.cbSourceSchema.Visibility = Visibility.Visible;

            if (this.chkSameFolder.IsChecked == true)
            {
                this.lblTargetSchema.Visibility = Visibility.Visible;
                this.cbTargetSchema.Visibility = Visibility.Visible;
            }
            
            this.ChangeButtonsVisibility();
        }

        private void treeTargetTridionFolder_Selected(object sender, RoutedEventArgs e)
        {
            ItemInfo item = ((TreeViewItem)e.OriginalSource).DataContext as ItemInfo;
            if (item == null)
                return;

            bool itemTypeChangedOrNew = Functions.GetItemType(this.TargetTridionFolder.TcmId) != item.ItemType || this._TargetSchemas == null;

            this.TargetTridionFolder.TcmId = item.TcmId;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetItemTcmId"), item.TcmId);

            List<ItemInfo> list = new List<ItemInfo>();
            Functions.AddPathItem(list, item);

            this.TargetTridionFolder.TcmIdPath = list.Select(x => x.TcmId).ToList();

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetItemTcmIdPath"), String.Join(";", this.TargetTridionFolder.TcmIdPath));

            list.Reverse();
            this.TargetTridionFolder.NamedPath = string.Join("/", list.Select(x => x.Title));
            
            if (this._CurrentTargetPublication != Functions.GetPublicationTcmId(item.TcmId))
            {
                this._CurrentTargetPublication = Functions.GetPublicationTcmId(item.TcmId);
                this._TargetSchemas = Functions.GetSchemas(this._CurrentTargetPublication);
                itemTypeChangedOrNew = true;
            }

            if (this._TargetSchemas != null && this._TargetSchemas.Count > 0 && itemTypeChangedOrNew)
            {
                if (item.ItemType == ItemType.Component || item.ItemType == ItemType.Folder)
                {
                    this._TargetSchemas = this._TargetSchemas.Where(x => x.SchemaType == SchemaType.Component || x.SchemaType == SchemaType.Multimedia).OrderBy(x => x.Title).ToList();
                }
                else if (item.ItemType == ItemType.Page || item.ItemType == ItemType.StructureGroup)
                {
                    this._TargetSchemas = this._TargetSchemas.Where(x => x.SchemaType == SchemaType.Metadata).OrderBy(x => x.Title).ToList();
                }
                else
                {
                    this._TargetSchemas = this._TargetSchemas.OrderBy(x => x.Title).ToList();
                }

                this.cbTargetSchema.ItemsSource = this._TargetSchemas;
                this.cbTargetSchema.DisplayMemberPath = "Title";

                string targetSchemaTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetSchemaTcmId"));
                if (!String.IsNullOrEmpty(targetSchemaTcmId))
                {
                    this.cbTargetSchema.SelectedIndex = this._TargetSchemas.FindIndex(x => x.TcmId == targetSchemaTcmId);
                }
            }

            this.lblTargetSchema.Visibility = Visibility.Visible;
            this.cbTargetSchema.Visibility = Visibility.Visible;

            this.ChangeButtonsVisibility();
        }
        
        private void treeSourceTridionObject_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ItemInfo item = this.treeSourceTridionObject.SelectedItem as ItemInfo;
            if (item == null)
                return;

            Process.Start(Functions.GetItemCmsUrl(this.txtHost.Text, item.TcmId, item.Title));
        }

        private void treeTargetTridionFolder_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ItemInfo item = this.treeTargetTridionFolder.SelectedItem as ItemInfo;
            if (item == null)
                return;

            Process.Start(Functions.GetItemCmsUrl(this.txtHost.Text, item.TcmId, item.Title));
        }

        private void cbSourceSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemInfo sourceSchema = this.cbSourceSchema.SelectedValue as ItemInfo;
            if (sourceSchema == null)
                return;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "SourceSchemaTcmId"), sourceSchema.TcmId);

            this._Criterias = null;

            if (this.cbSourceSchema.SelectedIndex > -1 && this.cbTargetSchema.SelectedIndex > -1)
            {
                this.spSettingButtons.Visibility = Visibility.Visible;
                this.spButtons.Visibility = Visibility.Visible;

                ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;
                if (targetSchema != null)
                {
                    this.HistoryMapping = Functions.GetHistoryMapping(Functions.GetId(this.txtHost.Text, sourceSchema.TcmId, targetSchema.TcmId));
                }

                this.SetCustomTransformers();

                this.SetCustomNameTransformers();

                this.chkLocalize.IsChecked = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), sourceSchema.TcmId, "Localize")).ToLower() == "true";
            }

            this.ChangeButtonsVisibility();
        }

        private void cbTargetSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;
            if (targetSchema == null)
                return;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetSchemaTcmId"), targetSchema.TcmId);

            List<ItemFieldDefinitionData> targetComponentFields = Functions.GetSchemaFields(targetSchema.TcmId) ?? new List<ItemFieldDefinitionData>();
            List<ItemFieldDefinitionData> targetMetadataFields = Functions.GetSchemaMetadataFields(targetSchema.TcmId);
            this._TargetSchemaFields = Functions.GetAllFields(targetComponentFields, targetMetadataFields, false, true);

            if (this.cbSourceSchema.SelectedIndex > -1 && this.cbTargetSchema.SelectedIndex > -1)
            {
                this.spSettingButtons.Visibility = Visibility.Visible;
                this.spButtons.Visibility = Visibility.Visible;
                
                ItemInfo sourceSchema = this.cbSourceSchema.SelectedValue as ItemInfo;
                if (sourceSchema != null)
                {
                    this.HistoryMapping = Functions.GetHistoryMapping(Functions.GetId(this.txtHost.Text, sourceSchema.TcmId, targetSchema.TcmId));

                    this.chkLocalize.IsChecked = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), sourceSchema.TcmId, "Localize")).ToLower() == "true";
                }

                this.SetCustomTransformers();

                this.SetCustomNameTransformers();
            }

            this.ChangeButtonsVisibility();
        }

        private void chkSameFolder_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (this.chkSameFolder.IsChecked == true)
            {
                Grid.SetColumnSpan(this.treeSourceTridionObject, 2);
                this.treeTargetTridionFolder.Visibility = Visibility.Collapsed;
                this.lblTargetTridionFolder.Visibility = Visibility.Hidden;

                if (this._SourceSchemas != null && this._SourceSchemas.Count > 0)
                {
                    this.cbTargetSchema.ItemsSource = this._SourceSchemas;
                    this.cbTargetSchema.DisplayMemberPath = "Title";

                    string targetSchemaTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetSchemaTcmId"));
                    if (!String.IsNullOrEmpty(targetSchemaTcmId))
                    {
                        this.cbTargetSchema.SelectedIndex = this._SourceSchemas.FindIndex(x => x.TcmId == targetSchemaTcmId);
                    }
                }
            }
            else
            {
                Grid.SetColumnSpan(this.treeSourceTridionObject, 1);
                this.treeTargetTridionFolder.Visibility = Visibility.Visible;
                this.lblTargetTridionFolder.Visibility = Visibility.Visible;

                if (this._TargetSchemas != null && this._TargetSchemas.Count > 0)
                {
                    this.cbTargetSchema.ItemsSource = this._TargetSchemas;
                    this.cbTargetSchema.DisplayMemberPath = "Title";

                    string targetSchemaTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetSchemaTcmId"));
                    if (!String.IsNullOrEmpty(targetSchemaTcmId))
                    {
                        this.cbTargetSchema.SelectedIndex = this._TargetSchemas.FindIndex(x => x.TcmId == targetSchemaTcmId);
                    }
                }
            }

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "SameFolder"), this.chkSameFolder.IsChecked.ToString().ToLower());

            this.ChangeButtonsVisibility();
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            ItemInfo sourceSchema = this.cbSourceSchema.SelectedValue as ItemInfo;
            if (sourceSchema == null)
                return;

            FilterWindow dialog = new FilterWindow();
            dialog.Host = this.txtHost.Text;
            dialog.SchemaUri = sourceSchema.TcmId;
            bool res = dialog.ShowDialog() == true;
            if (res)
            {
                this._Criterias = dialog.Criterias;
                this.ChangeButtonsVisibility();
            }
        }

        private void btnFieldMapping_Click(object sender, RoutedEventArgs e)
        {
            ItemInfo sourceSchema = this.cbSourceSchema.SelectedValue as ItemInfo;
            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;

            if (sourceSchema == null || targetSchema == null)
                return;

            FieldMappingWindow dialog = new FieldMappingWindow();
            dialog.Host = this.txtHost.Text;
            dialog.SourceSchemaUri = sourceSchema.TcmId;
            dialog.TargetSchemaUri = targetSchema.TcmId;

            bool res = dialog.ShowDialog() == true;
            if (res)
            {
                this.HistoryMapping = dialog.HistoryMapping;
                this.ChangeButtonsVisibility();
            }
        }

        private void btnNameTransform_Click(object sender, RoutedEventArgs e)
        {
            ItemInfo sourceSchema = this.cbSourceSchema.SelectedValue as ItemInfo;
            if (sourceSchema == null)
                return;

            NameTransformOptionsWindow dialog = new NameTransformOptionsWindow();
            dialog.Host = this.txtHost.Text;
            dialog.SourceSchemaUri = sourceSchema.TcmId;
            
            bool res = dialog.ShowDialog() == true;
            if (res)
            {
                this._FormatString = dialog.FormatString;
                this._Replacements = dialog.Replacements;

                this.ChangeButtonsVisibility();
            }
        }

        private void btnCustomTransform_Click(object sender, RoutedEventArgs e)
        {
            ItemInfo sourceSchema = this.cbSourceSchema.SelectedValue as ItemInfo;
            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;
            
            if (sourceSchema == null || targetSchema == null)
                return;

            this.SetCustomTransformers();

            CustomTransformWindow dialog = new CustomTransformWindow();
            dialog.CustomComponentTransformer = this.CustomComponentTransformer;
            dialog.CustomMetadataTransformer = this.CustomMetadataTransformer;
            dialog.SourceTridionObject = this.SourceTridionObject;
            dialog.SourceSchema = sourceSchema;
            dialog.TargetSchema = targetSchema;

            bool res = dialog.ShowDialog() == true;
            if (res)
            {
                this.CustomComponentTransformer = dialog.CustomComponentTransformer;
                this.CustomMetadataTransformer = dialog.CustomMetadataTransformer;

                Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "CustomComponentTransformer", sourceSchema.TcmId, targetSchema.TcmId), dialog.CustomComponentTransformer != null ? dialog.CustomComponentTransformer.TypeName : string.Empty);
                Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "CustomMetadataTransformer", sourceSchema.TcmId, targetSchema.TcmId), dialog.CustomMetadataTransformer != null ? dialog.CustomMetadataTransformer.TypeName : string.Empty);

                this.ChangeButtonsVisibility();
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ItemInfo sourceSchema = this.cbSourceSchema.SelectedValue as ItemInfo;
            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;

            if (sourceSchema == null || targetSchema == null)
                return;

            this.SetCustomTransformers();
            this.SetCustomNameTransformers();

            if (this.HistoryMapping == null)
            {
                this.HistoryMapping = Functions.GetHistoryMapping(Functions.GetId(this.txtHost.Text, sourceSchema.TcmId, targetSchema.TcmId));
            }

            if (this.HistoryMapping == null && (this.CustomComponentTransformer == null || this.CustomMetadataTransformer == null))
            {
                MessageBox.Show("Neither field mapping nor custom transformers is set. Please set mapping or select custom transformer and try again.", "Field Mapping", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //inform that custom transfromation is selected
            if (this.CustomComponentTransformer != null || this.CustomMetadataTransformer != null)
            {
                string messsage = "";
                if (this.CustomComponentTransformer != null && this.CustomMetadataTransformer != null)
                {
                    messsage = String.Format("Custom transformations {0} and {1} are used.\n\nContinue?", this.CustomComponentTransformer.Title, this.CustomMetadataTransformer.Title);
                }
                else if (this.CustomComponentTransformer != null)
                {
                    messsage = String.Format("Custom component transformation {0} is used.\n\nContinue?", this.CustomComponentTransformer.Title);
                }
                else if (this.CustomMetadataTransformer != null)
                {
                    messsage = String.Format("Custom metadata transformation {0} is used.\n\nContinue?", this.CustomMetadataTransformer.Title);
                }

                if (MessageBox.Show(messsage, "Custom Transformations", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }

            if (this.HistoryMapping != null)
            {
                foreach (HistoryItemMappingInfo historyMapping in this.HistoryMapping)
                {
                    string sourceSchemaVersionTcmId = historyMapping.TcmId;
                    if (historyMapping.Current && sourceSchemaVersionTcmId.Contains("-v"))
                    {
                        sourceSchemaVersionTcmId = sourceSchemaVersionTcmId.Replace("-" + sourceSchemaVersionTcmId.Split('-')[3], "");
                    }

                    List<ItemFieldDefinitionData> sourceComponentFields = Functions.GetSchemaFields(sourceSchemaVersionTcmId);
                    List<ItemFieldDefinitionData> sourceMetadataFields = Functions.GetSchemaMetadataFields(sourceSchemaVersionTcmId);
                    List<FieldInfo> sourceSchemaFields = Functions.GetAllFields(sourceComponentFields, sourceMetadataFields, true, false);

                    foreach (FieldMappingInfo mapping in historyMapping.Mapping)
                    {
                        mapping.SourceFields = sourceSchemaFields;
                        mapping.TargetFields = this._TargetSchemaFields;
                    }
                }

                Functions.SetHistoryMappingTree(this.HistoryMapping);
            }
            
            List<ResultInfo> results = new List<ResultInfo>();

            ItemType sourceItemType = Functions.GetItemType(this.SourceTridionObject.TcmId);

            string sourceSchemaTcmId = sourceSchema.TcmId;

            string targetFolderUri = this.chkSameFolder.IsChecked == true ? this.SourceTridionObject.TcmId : this.TargetTridionFolder.TcmId;
            bool sameFolder = this.chkSameFolder.IsChecked == true || this.SourceTridionObject.TcmId == this.TargetTridionFolder.TcmId;

            bool localize = this.chkLocalize.IsChecked == true;
            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), sourceSchema.TcmId, "Localize"), localize.ToString());

            // same folder selected: schema change and component fix functionality
            if (sameFolder)
            {
                // single component
                if (sourceItemType == ItemType.Component)
                {
                    if (sourceSchema.TcmId.GetId() == targetSchema.TcmId.GetId())
                    {
                        // fix single component
                        Functions.FixComponent(this.SourceTridionObject.TcmId, sourceSchemaTcmId, targetFolderUri, localize, this.HistoryMapping, this.CustomComponentTransformer, this.CustomMetadataTransformer, results);
                    }
                    else
                    {
                        // change schema for single component
                        Functions.ChangeSchemaForComponent(this.SourceTridionObject.TcmId, sourceSchemaTcmId, targetSchema.TcmId, targetFolderUri, localize, this.HistoryMapping, this.CustomComponentTransformer, this.CustomMetadataTransformer, results);
                    }
                }
                
                // folder
                if (sourceItemType == ItemType.Folder)
                {
                    if (sourceSchema.TcmId.GetId() == targetSchema.TcmId.GetId())
                    {
                        // fix all componnets in folder
                        Functions.FixComponentsInFolder(this.SourceTridionObject.TcmId, sourceSchemaTcmId, targetFolderUri, this._Criterias, localize, this.HistoryMapping, this.CustomComponentTransformer, this.CustomMetadataTransformer, results);
                    }
                    else
                    {
                        // change schema for all components in folder
                        Functions.ChangeSchemasForComponentsInFolder(this.SourceTridionObject.TcmId, sourceSchemaTcmId, targetFolderUri, targetSchema.TcmId, this._Criterias, localize, this.HistoryMapping, this.CustomComponentTransformer, this.CustomMetadataTransformer, results);
                    }
                }
                
                // single page
                if (sourceItemType == ItemType.Page)
                {
                    if (sourceSchema.TcmId.GetId() == targetSchema.TcmId.GetId())
                    {
                        // fix single page metadata
                        Functions.FixTridionObjectMetadata(this.SourceTridionObject.TcmId, sourceSchemaTcmId, targetFolderUri, localize, this.HistoryMapping, this.CustomMetadataTransformer, results);
                    }
                    else
                    {
                        // change metadata schema for single page
                        Functions.ChangeMetadataSchemaForTridionObject(this.SourceTridionObject.TcmId, sourceSchemaTcmId, targetFolderUri, targetSchema.TcmId, localize, this.HistoryMapping, this.CustomMetadataTransformer, results);
                    }
                }

                // structure group
                if (sourceItemType == ItemType.StructureGroup)
                {
                    if (sourceSchema.TcmId.GetId() == targetSchema.TcmId.GetId())
                    {
                        // fix metadata for all pages in structure group
                        Functions.FixMetadataForTridionObjectsInContainer(this.SourceTridionObject.TcmId, sourceSchemaTcmId, targetFolderUri, localize, this.HistoryMapping, this.CustomMetadataTransformer, results);
                    }
                    else
                    {
                        // change metadata schema for all pages in structure group
                        Functions.ChangeMetadataSchemasForTridionObjectsInContainer(this.SourceTridionObject.TcmId, sourceSchemaTcmId, targetFolderUri, targetSchema.TcmId, localize, this.HistoryMapping, this.CustomMetadataTransformer, results);
                    }
                }
            }

            // transform and copy to destination
            else
            {
                string targetTridionObjectContainerUri = this.chkSameFolder.IsChecked == true ? this.SourceTridionObject.TcmId : this.TargetTridionFolder.TcmId;

                // single component
                if (sourceItemType == ItemType.Component)
                {
                    // copy / transform component
                    Functions.TransformComponent(this.SourceTridionObject.TcmId, string.Empty, sourceSchemaTcmId, targetTridionObjectContainerUri, targetSchema.TcmId, this._FormatString, this._Replacements, localize, this.HistoryMapping, this.CustomComponentTransformer, this.CustomMetadataTransformer, results);
                }
                // folder
                if (sourceItemType == ItemType.Folder)
                {
                    // copy / transform all components in folder
                    Functions.TransformComponentsInFolder(this.SourceTridionObject.TcmId, sourceSchemaTcmId, targetTridionObjectContainerUri, targetSchema.TcmId, this._Criterias, this._FormatString, this._Replacements, localize, this.HistoryMapping, this.CustomComponentTransformer, this.CustomMetadataTransformer, results);
                }
                
                // single page
                if (sourceItemType == ItemType.Page)
                {
                    // copy / transform metadata
                    Functions.TransformTridionObjectMetadata(this.SourceTridionObject.TcmId, string.Empty, sourceSchemaTcmId, targetTridionObjectContainerUri, targetSchema.TcmId, this._FormatString, this._Replacements, localize, this.HistoryMapping, this.CustomComponentTransformer, this.CustomMetadataTransformer, results);
                }
                // structure group
                if (sourceItemType == ItemType.StructureGroup)
                {
                    // copy / transform all pages in structure group
                    Functions.TransformMetadataForTridionObjectsInContainer(this.SourceTridionObject.TcmId, sourceSchemaTcmId, targetTridionObjectContainerUri, targetSchema.TcmId, this._FormatString, this._Replacements, localize, this.HistoryMapping, this.CustomComponentTransformer, this.CustomMetadataTransformer, results);
                }
            }

            if (results.Any(x => x.Status == Status.Success))
            {
                ResultInfo resultFinish = new ResultInfo();
                resultFinish.Status = Status.Info;
                resultFinish.Message = "Finished";
                results.Add(resultFinish);
            }
            else
            {
                ResultInfo resultFinish = new ResultInfo();
                resultFinish.Status = Status.Info;
                resultFinish.Message = "No Actions Made";
                results.Add(resultFinish);
            }

            //show results
            ResultsWindow dialog = new ResultsWindow();
            dialog.Host = this.txtHost.Text;
            dialog.ListBoxReport.ItemsSource = results.Where(x => x != null && x.Status != Status.None);
            dialog.ListBoxReport.MouseDoubleClick += lbReport_OnMouseDoubleClick;
            dialog.Show();

            //reload source and target tree
            this._Publications1 = Functions.GetPublications();
            this.treeSourceTridionObject.ItemsSource = this._Publications1.Expand(this.SourceTridionSelectorMode, this.SourceTridionObject.TcmIdPath, this.SourceTridionObject.TcmId).MakeExpandable();

            this._Publications2 = Functions.GetPublications();
            this.treeTargetTridionFolder.ItemsSource = this._Publications2.Expand(this.TargetTridionSelectorMode, this.TargetTridionFolder.TcmIdPath, this.TargetTridionFolder.TcmId).MakeExpandable();
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

            Process.Start(Functions.GetItemCmsUrl(this.txtHost.Text, item.TcmId));
        }

        private void ChangeButtonsVisibility()
        {
            ItemType sourceItemType = Functions.GetItemType(this.SourceTridionObject.TcmId);
            ItemInfo sourceSchema = this.cbSourceSchema.SelectedValue as ItemInfo;
            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;

            if (sourceItemType != ItemType.Folder && sourceItemType != ItemType.StructureGroup)
                this.btnFilter.SetDisabled();
            else
            {
                if (this._Criterias != null && this._Criterias.Any())
                {
                    this.btnFilter.SetEnabledGreen();
                }
                else
                {
                    this.btnFilter.SetEnabled();
                }
            }

            if (this.HistoryMapping == null)
                this.btnFieldMapping.SetEnabledRed();
            else
                this.btnFieldMapping.SetEnabledGreen();

            bool sameFolder = this.chkSameFolder.IsChecked == true || this.SourceTridionObject.TcmId == this.TargetTridionFolder.TcmId;
            if (sameFolder)
            {
                this.btnNameTransform.SetDisabled();
            }
            else
            {
                if (!string.IsNullOrEmpty(this._FormatString) && this._Replacements != null && this._Replacements.Any())
                {
                    this.btnNameTransform.SetEnabledGreen();
                }
                else
                {
                    this.btnNameTransform.SetEnabled();
                }
            }

            if (this.SourceTridionObject == null || sourceSchema == null || targetSchema == null)
            {
                this.btnCustomTransform.SetDisabled();
            }
            else
            {
                if (this.CustomComponentTransformer != null || this.CustomMetadataTransformer != null)
                {
                    this.btnCustomTransform.SetEnabledGreen();
                }
                else
                {
                    this.btnCustomTransform.SetEnabled();
                }
            }

            this.chkLocalize.IsEnabled = !sameFolder;
            if (!this.chkLocalize.IsEnabled)
                this.chkLocalize.IsChecked = false;
        }

        private void SetCustomTransformers()
        {
            if (this.CustomComponentTransformer != null && this.CustomMetadataTransformer != null)
                return;

            ItemInfo sourceSchema = this.cbSourceSchema.SelectedValue as ItemInfo;
            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;

            if (sourceSchema == null || targetSchema == null)
                return;

            List<CustomTransformerInfo> customTransformers = Functions.GetCustomTransformers(sourceSchema.Title, sourceSchema.SchemaType, Functions.GetItemType(this.SourceTridionObject.TcmId), targetSchema.Title, targetSchema.SchemaType);

            if (this.CustomComponentTransformer == null)
                this.CustomComponentTransformer = customTransformers.FirstOrDefault(x => x.TypeName == Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "CustomComponentTransformer", sourceSchema.TcmId, targetSchema.TcmId)));

            if (this.CustomMetadataTransformer == null)
                this.CustomMetadataTransformer = customTransformers.FirstOrDefault(x => x.TypeName == Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "CustomMetadataTransformer", sourceSchema.TcmId, targetSchema.TcmId)));
        }

        private void SetCustomNameTransformers()
        {
            ItemInfo sourceSchema = this.cbSourceSchema.SelectedValue as ItemInfo;
            if (sourceSchema == null)
                return;

            List<ItemFieldDefinitionData> sourceComponentFields = Functions.GetSchemaFields(sourceSchema.TcmId);
            List<ItemFieldDefinitionData> sourceMetadataFields = Functions.GetSchemaMetadataFields(sourceSchema.TcmId);
            List<FieldInfo> sourceSchemaFields = Functions.GetAllFields(sourceComponentFields, sourceMetadataFields, true, false);

            if (string.IsNullOrEmpty(this._FormatString))
            {
                this._FormatString = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), sourceSchema.TcmId, "FormatString"));
            }

            if (this._Replacements == null)
            {
                string replacement1 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), sourceSchema.TcmId, "Replacement1"));
                string regex1 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), sourceSchema.TcmId, "Regex1"));
                if (!string.IsNullOrEmpty(replacement1) && replacement1 != "< ignore >" && !string.IsNullOrEmpty(regex1))
                {
                    this._Replacements = new List<ReplacementInfo>();
                    this._Replacements.Add(this.GetReplacement(replacement1, regex1, sourceSchemaFields));
                }

                string replacement2 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), sourceSchema.TcmId, "Replacement2"));
                string regex2 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), sourceSchema.TcmId, "Regex2"));
                if (!string.IsNullOrEmpty(replacement2) && replacement2 != "< ignore >" && !string.IsNullOrEmpty(regex2))
                {
                    if (this._Replacements == null)
                        this._Replacements = new List<ReplacementInfo>();
                    this._Replacements.Add(this.GetReplacement(replacement2, regex2, sourceSchemaFields));
                }

                string replacement3 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), sourceSchema.TcmId, "Replacement3"));
                string regex3 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), sourceSchema.TcmId, "Regex3"));
                if (!string.IsNullOrEmpty(replacement3) && replacement3 != "< ignore >" && !string.IsNullOrEmpty(regex3))
                {
                    if (this._Replacements == null)
                        this._Replacements = new List<ReplacementInfo>();
                    this._Replacements.Add(this.GetReplacement(replacement3, regex3, sourceSchemaFields));
                }
            }
        }

        private ReplacementInfo GetReplacement(string fragment, string regex, List<FieldInfo> sourceSchemaFields)
        {
            ReplacementInfo replacement = new ReplacementInfo();
            replacement.Regex = regex;

            if (fragment == "[Title]" || fragment == "[TcmId]" || fragment == "[ID]")
            {
                replacement.Fragment = fragment;
            }
            else if (sourceSchemaFields != null)
            {
                replacement.Field = sourceSchemaFields.FirstOrDefault(x => x.GetFieldFullName(false) == fragment);
            }

            return replacement;
        }
    }
}
