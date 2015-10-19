using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;
using FieldInfo = TridionDesktopTools.Core.FieldInfo;

namespace TridionDesktopTools.ComponentImporter
{
    public partial class MainWindow
    {
        private List<string> _SourceDatabases;
        private List<string> _SourceTables;

        public TridionObjectInfo TargetTridionFolder { get; set; }
        public TridionSelectorMode TargetTridionSelectorMode { get; set; }

        public HistoryMappingInfo HistoryMapping { get; set; }

        public string Sql { get; set; }

        private string _FormatString;
        private List<ReplacementInfo> _Replacements;

        private List<ItemInfo> _Publications;
        private string _CurrentTargetPublication;

        private List<FieldInfo> _SourceSchemaFields;

        private List<ItemInfo> _TargetSchemas;
        private List<FieldInfo> _TargetSchemaFields;

        public CustomTransformerInfo CustomComponentImporter { get; set; }
        public CustomTransformerInfo CustomMetadataImporter { get; set; }

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

            this.txtDbHost.Text = Functions.GetFromIsolatedStorage("DbHost");
            this.txtDbUsername.Text = Functions.GetFromIsolatedStorage("DbUsername");
            this.txtDbPassword.Password = Functions.GetFromIsolatedStorage("DbPassword");

            this.txtHost.Text = Functions.GetFromIsolatedStorage("Host");
            this.txtUsername.Text = Functions.GetFromIsolatedStorage("Username");
            this.txtPassword.Password = Functions.GetFromIsolatedStorage("Password");

            this.TargetTridionSelectorMode = TridionSelectorMode.Folder;
        }

        #region Database functionality

        private void btnDbConnect_Click(object sender, RoutedEventArgs e)
        {
            this._SourceDatabases = new List<string>();
            this._SourceDatabases.Add("< Select Database >");

            System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.DataSource = this.txtDbHost.Text;
            builder.UserID = this.txtDbUsername.Text;
            builder.Password = this.txtDbPassword.Password;

            using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand();
                command.Connection = connection;
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "sp_databases";

                System.Data.SqlClient.SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    this._SourceDatabases.Add(reader.GetString(0));
                }
            }

            this.cbSourceDatabase.ItemsSource = this._SourceDatabases;

            string sourceDatabase = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, "SourceDatabase"));
            if (!String.IsNullOrEmpty(sourceDatabase))
            {
                this.cbSourceDatabase.SelectedIndex = this._SourceDatabases.FindIndex(x => x == sourceDatabase);
            }

            if (this.cbSourceDatabase.SelectedIndex < 0) this.cbSourceDatabase.SelectedIndex = 0;

            if (this._SourceDatabases.Count > 1)
            {
                this.lblSourceDatabase.Visibility = Visibility.Visible;
                this.cbSourceDatabase.Visibility = Visibility.Visible;
            }

            //save to isolated stoage
            Functions.SaveToIsolatedStorage("DbHost", this.txtDbHost.Text);
            Functions.SaveToIsolatedStorage("DbUsername", this.txtDbUsername.Text);
            Functions.SaveToIsolatedStorage("DbPassword", this.txtDbPassword.Password);
        }

        private void cbSourceDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sourceDatabase = this.cbSourceDatabase.SelectedValue as string;
            if (String.IsNullOrEmpty(sourceDatabase) || sourceDatabase == "< Select Database >")
            {
                this.lblSourceTable.Visibility = Visibility.Hidden;
                this.cbSourceTable.Visibility = Visibility.Hidden;

                this.lblSourceTableContent.Visibility = Visibility.Hidden;
                this.dgSourceTableContent.Visibility = Visibility.Hidden;

                this._SourceTables = null;

                return;
            }

            this._SourceTables = new List<string>();
            this._SourceTables.Add("< Select Table >");

            System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.DataSource = this.txtDbHost.Text;
            builder.InitialCatalog = this.cbSourceDatabase.SelectedValue.ToString();
            builder.UserID = this.txtDbUsername.Text;
            builder.Password = this.txtDbPassword.Password;

            using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                System.Data.DataTable schema = connection.GetSchema("Tables");
                foreach (System.Data.DataRow row in schema.Rows)
                {
                    this._SourceTables.Add(row[2].ToString());
                }
            }

            this.cbSourceTable.ItemsSource = this._SourceTables;

            string sourceTable = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, sourceDatabase, "SourceTable"));
            if (!String.IsNullOrEmpty(sourceTable))
            {
                this.cbSourceTable.SelectedIndex = this._SourceTables.FindIndex(x => x == sourceTable);
            }

            if (this.cbSourceTable.SelectedIndex < 0 || this.cbSourceTable.Visibility == Visibility.Visible)
            {
                this.cbSourceTable.SelectedIndex = 0;

                this.lblSourceTableContent.Visibility = Visibility.Hidden;
                this.dgSourceTableContent.Visibility = Visibility.Hidden;
            }
                
            if (this._SourceTables.Count > 1)
            {
                this.lblSourceTable.Visibility = Visibility.Visible;
                this.cbSourceTable.Visibility = Visibility.Visible;

                //save to isolated stoage
                Functions.SaveToIsolatedStorage(Functions.GetId(this.txtDbHost.Text, "SourceDatabase"), sourceDatabase);
            }

            this.ChangeButtonsVisibility();
        }

        private void cbSourceTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sourceTable = this.cbSourceTable.SelectedValue as string;
            if (String.IsNullOrEmpty(sourceTable) || sourceTable == "< Select Table >")
            {
                this.lblSourceTableContent.Visibility = Visibility.Hidden;
                this.dgSourceTableContent.Visibility = Visibility.Hidden;

                return;
            }

            System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.DataSource = this.txtDbHost.Text;
            builder.InitialCatalog = this.cbSourceDatabase.SelectedValue.ToString();
            builder.UserID = this.txtDbUsername.Text;
            builder.Password = this.txtDbPassword.Password;

            using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand();
                command.Connection = connection;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = string.Format("select * from [{0}]", sourceTable);

                System.Data.SqlClient.SqlDataAdapter sda = new System.Data.SqlClient.SqlDataAdapter(command);
                System.Data.DataTable dt = new System.Data.DataTable(sourceTable);
                sda.Fill(dt);
                dgSourceTableContent.ItemsSource = dt.DefaultView; 
            }

            this._SourceSchemaFields = Functions.GetDatabaseTableFields(this.txtDbHost.Text, this.txtDbUsername.Text, this.txtDbPassword.Password, this.cbSourceDatabase.SelectedValue.ToString(), sourceTable);

            this.lblSourceTableContent.Visibility = Visibility.Visible;
            this.dgSourceTableContent.Visibility = Visibility.Visible;

            this.ChangeButtonsVisibility();

            //save to isolated stoage
            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtDbHost.Text, this.cbSourceDatabase.SelectedValue.ToString(), "SourceTable"), sourceTable);

            if (this.cbSourceDatabase.SelectedIndex > 0 && this.cbSourceTable.SelectedIndex > 0 && this.cbTargetSchema.SelectedIndex > 0)
            {
                ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;
                if (targetSchema != null)
                {
                    this.HistoryMapping = Functions.GetHistoryMapping(Functions.GetId(this.txtHost.Text, this.cbSourceDatabase.SelectedValue.ToString(), sourceTable, targetSchema.TcmId));
                    this.btnFieldMapping.Foreground = this.HistoryMapping == null ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                }

                this.SetCustomImporters();
                if (this.CustomComponentImporter != null || this.CustomMetadataImporter != null)
                {
                    this.btnCustomImport.Foreground = new SolidColorBrush(Colors.Green);
                }

                this.SetCustomNameTransformers();
                if (!string.IsNullOrEmpty(this._FormatString) && this._Replacements != null && this._Replacements.Any())
                {
                    this.btnNameTransform.Foreground = new SolidColorBrush(Colors.Green);
                }

                this.chkLocalize.IsChecked = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, this.cbSourceDatabase.SelectedValue.ToString(), sourceTable, "Localize")).ToLower() == "true";
            }
        }

        #endregion

        #region Tridion functionality

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

            this._TargetSchemas = new List<ItemInfo>();
            this._TargetSchemas.Add(new ItemInfo { Title = "< Select Publication Below >" });
            this.cbTargetSchema.ItemsSource = this._TargetSchemas;
            this.cbTargetSchema.DisplayMemberPath = "Title";
            this.cbTargetSchema.SelectedIndex = 0;

            this._Publications = Functions.GetPublications();
            this.treeTargetTridionFolder.ItemsSource = this._Publications.Expand(this.TargetTridionSelectorMode, this.TargetTridionFolder.TcmIdPath, this.TargetTridionFolder.TcmId).MakeExpandable();

            if (this._Publications != null && this._Publications.Count > 0)
            {
                //save to isolated stoage
                Functions.SaveToIsolatedStorage("Host", this.txtHost.Text);
                Functions.SaveToIsolatedStorage("Username", this.txtUsername.Text);
                Functions.SaveToIsolatedStorage("Password", this.txtPassword.Password);

                this.lblTargetSchema.Visibility = Visibility.Visible;
                this.cbTargetSchema.Visibility = Visibility.Visible;

                this.lblTargetTridionFolder.Visibility = Visibility.Visible;
                this.treeTargetTridionFolder.Visibility = Visibility.Visible;
            }
        }

        private void treeTargetTridionFolder_Expanded(object sender, RoutedEventArgs e)
        {
            ItemInfo item = ((TreeViewItem)e.OriginalSource).DataContext as ItemInfo;
            if (item == null)
                return;

            Functions.OnItemExpanded(item, this.TargetTridionSelectorMode);
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

                this._TargetSchemas.Insert(0, new ItemInfo { Title = "< Select Schema >" });

                this.cbTargetSchema.ItemsSource = this._TargetSchemas;
                this.cbTargetSchema.DisplayMemberPath = "Title";

                string targetSchemaTcmId = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetSchemaTcmId"));
                if (!String.IsNullOrEmpty(targetSchemaTcmId))
                {
                    this.cbTargetSchema.SelectedIndex = this._TargetSchemas.FindIndex(x => x.TcmId == targetSchemaTcmId);
                }

                if (this.cbTargetSchema.SelectedIndex < 0) this.cbTargetSchema.SelectedIndex = 0;
            }

            this.ChangeButtonsVisibility();
        }
        
        private void treeTargetTridionFolder_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ItemInfo item = this.treeTargetTridionFolder.SelectedItem as ItemInfo;
            if (item == null)
                return;

            Process.Start(Functions.GetItemCmsUrl(this.txtHost.Text, item.TcmId, item.Title));
        }

        private void cbTargetSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;
            if (targetSchema == null || targetSchema.TcmId == null)
                return;

            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "TargetSchemaTcmId"), targetSchema.TcmId);

            List<ItemFieldDefinitionData> targetComponentFields = Functions.GetSchemaFields(targetSchema.TcmId) ?? new List<ItemFieldDefinitionData>();
            List<ItemFieldDefinitionData> targetMetadataFields = Functions.GetSchemaMetadataFields(targetSchema.TcmId);
            this._TargetSchemaFields = Functions.GetAllFields(targetComponentFields, targetMetadataFields, false, false);

            if (this.cbSourceDatabase.SelectedIndex > 0 && this.cbSourceTable.SelectedIndex > 0 && this.cbTargetSchema.SelectedIndex > 0)
            {
                string sourceDatabase = this.cbSourceDatabase.SelectedValue as string;
                string sourceTable = this.cbSourceTable.SelectedValue as string;
                if (sourceDatabase != null && sourceTable != null)
                {
                    this.HistoryMapping = Functions.GetHistoryMapping(Functions.GetId(this.txtHost.Text, sourceDatabase, sourceTable, targetSchema.TcmId));
                    this.btnFieldMapping.Foreground = this.HistoryMapping == null ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                }

                this.SetCustomImporters();
                if (this.CustomComponentImporter != null || this.CustomMetadataImporter != null)
                {
                    this.btnCustomImport.Foreground = new SolidColorBrush(Colors.Green);
                }

                this.SetCustomNameTransformers();
                if (!string.IsNullOrEmpty(this._FormatString) && this._Replacements != null && this._Replacements.Any())
                {
                    this.btnNameTransform.Foreground = new SolidColorBrush(Colors.Green);
                }

                this.chkLocalize.IsChecked = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, sourceDatabase, sourceTable, "Localize")).ToLower() == "true";
            }

            this.ChangeButtonsVisibility();
        }

        #endregion

        private void btnFieldMapping_Click(object sender, RoutedEventArgs e)
        {
            string sourceDatabase = this.cbSourceDatabase.SelectedValue as string;
            string sourceTable = this.cbSourceTable.SelectedValue as string;
            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;

            if (sourceDatabase == null || sourceTable == null || targetSchema == null)
                return;

            FieldMappingWindow dialog = new FieldMappingWindow();
            
            dialog.Host = this.txtHost.Text;

            dialog.TargetSchemaUri = targetSchema.TcmId;

            dialog.DbHost = this.txtDbHost.Text;
            dialog.DbUsername = this.txtDbUsername.Text;
            dialog.DbPassword = this.txtDbPassword.Password;

            dialog.SourceDatabase = sourceDatabase;
            dialog.SourceTable = sourceTable;

            bool res = dialog.ShowDialog() == true;
            if (res)
            {
                this.HistoryMapping = dialog.HistoryMapping;
                if (this.HistoryMapping != null)
                {
                    this.btnFieldMapping.Foreground = new SolidColorBrush(Colors.Green);
                }
            }
        }

        private void btnNameTransform_Click(object sender, RoutedEventArgs e)
        {
            NameTransformOptionsWindow dialog = new NameTransformOptionsWindow();
            dialog.Host = this.txtHost.Text;
            dialog.SourceSchemaFields = this._SourceSchemaFields;
            dialog.DbHost = this.txtDbHost.Text;
            dialog.SourceDatabase = this.cbSourceDatabase.SelectedValue as string;
            dialog.SourceTable = this.cbSourceTable.SelectedValue as string;

            bool res = dialog.ShowDialog() == true;
            if (res)
            {
                this._FormatString = dialog.FormatString;
                this._Replacements = dialog.Replacements;

                if (!string.IsNullOrEmpty(this._FormatString) && this._Replacements != null && this._Replacements.Any())
                {
                    this.btnNameTransform.Foreground = new SolidColorBrush(Colors.Green);
                }
            }
        }

        private void btnCustomImport_Click(object sender, RoutedEventArgs e)
        {
            string sourceTable = this.cbSourceTable.SelectedValue as string;
            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;
            
            if (sourceTable == null || targetSchema == null)
                return;

            this.SetCustomImporters();

            CustomImportWindow dialog = new CustomImportWindow();
            dialog.CustomComponentImporter = this.CustomComponentImporter;
            dialog.CustomMetadataImporter = this.CustomMetadataImporter;
            dialog.SourceTable = sourceTable;
            dialog.TargetSchema = targetSchema;
            dialog.Sql = string.Format("SELECT * FROM [{0}]", sourceTable);

            bool res = dialog.ShowDialog() == true;
            if (res)
            {
                this.Sql = dialog.Sql;
                this.CustomComponentImporter = dialog.CustomComponentImporter;
                this.CustomMetadataImporter = dialog.CustomMetadataImporter;

                Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "CustomComponentImporter", sourceTable, targetSchema.TcmId), dialog.CustomComponentImporter != null ? dialog.CustomComponentImporter.TypeName : string.Empty);
                
                Functions.SaveToIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "CustomMetadataImporter", sourceTable, targetSchema.TcmId), dialog.CustomMetadataImporter != null ? dialog.CustomMetadataImporter.TypeName : string.Empty);

                if (this.CustomComponentImporter == null && this.CustomMetadataImporter == null)
                {
                    this.btnCustomImport.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    this.btnCustomImport.Foreground = new SolidColorBrush(Colors.Green);
                }
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            string sourceTable = this.cbSourceTable.SelectedValue as string;
            string sourceDatabase = this.cbSourceDatabase.SelectedValue as string;
            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;

            if (sourceDatabase == null || sourceTable == null || targetSchema == null)
                return;

            this.SetCustomImporters();
            this.SetCustomNameTransformers();

            //inform that custom transfromation is selected
            if (this.CustomComponentImporter != null || this.CustomMetadataImporter != null)
            {
                string messsage = "";
                if (this.CustomComponentImporter != null && this.CustomMetadataImporter != null)
                {
                    messsage = String.Format("Custom imports {0} and {1} are used.\n\nContinue?", this.CustomComponentImporter.TypeName, this.CustomMetadataImporter.TypeName);
                }
                else if (this.CustomComponentImporter != null)
                {
                    messsage = String.Format("Custom component import {0} is used.\n\nContinue?", this.CustomComponentImporter.TypeName);
                }
                else if (this.CustomMetadataImporter != null)
                {
                    messsage = String.Format("Custom metadata import {0} is used.\n\nContinue?", this.CustomMetadataImporter.TypeName);
                }

                if(MessageBox.Show(messsage, "Custom import", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }

            if (this.HistoryMapping == null)
            {
                this.HistoryMapping = Functions.GetHistoryMapping(Functions.GetId(this.txtHost.Text, sourceDatabase, sourceTable, targetSchema.TcmId));
            }

            if (this.HistoryMapping == null)
            {
                MessageBox.Show("Field mapping is not set. Please set mapping and try again.", "Field Mapping", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (this.HistoryMapping != null)
            {
                foreach (HistoryItemMappingInfo historyMapping in this.HistoryMapping)
                {
                    foreach (FieldMappingInfo mapping in historyMapping.Mapping)
                    {
                        mapping.SourceFields = this._SourceSchemaFields;
                        mapping.TargetFields = this._TargetSchemaFields;
                    }
                }

                Functions.SetHistoryMappingTree(this.HistoryMapping);
            }
            
            List<ResultInfo> results = new List<ResultInfo>();

            bool localize = this.chkLocalize.IsChecked == true;
            Functions.SaveToIsolatedStorage(Functions.GetId(this.txtDbHost.Text, sourceDatabase, sourceTable, "Localize"), localize.ToString());

            // Import components from database
            Functions.ImportComponents(this.txtDbHost.Text, this.txtDbUsername.Text, this.txtDbPassword.Password, sourceDatabase, sourceTable, this.Sql, this.TargetTridionFolder.TcmId, targetSchema.TcmId, this._FormatString, this._Replacements, localize, this.HistoryMapping, this.CustomComponentImporter, this.CustomMetadataImporter, results);
                
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

            //reload target tree
            this._Publications = Functions.GetPublications();
            this.treeTargetTridionFolder.ItemsSource = this._Publications.Expand(this.TargetTridionSelectorMode, this.TargetTridionFolder.TcmIdPath, this.TargetTridionFolder.TcmId).MakeExpandable();
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

        private void ChangeButtonsVisibility()
        {
            if (this.cbSourceTable.SelectedIndex > 0 && this.cbTargetSchema.SelectedIndex > 0)
            {
                this.spSettingButtons.Visibility = Visibility.Visible;
                this.spButtons.Visibility = Visibility.Visible;
            }

            if (this.spSettingButtons.Visibility == Visibility.Visible)
            {
                string sourceTable = this.cbSourceTable.SelectedValue as string;
                ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;
                this.btnFieldMapping.IsEnabled = sourceTable != null && targetSchema != null && targetSchema.SchemaType == SchemaType.Component;
                this.btnCustomImport.IsEnabled = sourceTable != null && targetSchema != null;
            }
        }

        private void SetCustomImporters()
        {
            if (this.CustomComponentImporter != null && this.CustomMetadataImporter != null)
                return;

            string sourceTable = this.cbSourceTable.SelectedValue as string;
            if (string.IsNullOrEmpty(sourceTable))
                return;

            ItemInfo targetSchema = this.cbTargetSchema.SelectedValue as ItemInfo;
            if (targetSchema == null)
                return;

            List<CustomTransformerInfo> customImporters = Functions.GetCustomImporters(sourceTable, targetSchema.Title, targetSchema.SchemaType);

            if (this.CustomComponentImporter == null)
                this.CustomComponentImporter = customImporters.FirstOrDefault(x => x.TypeName == Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "CustomComponentImporter", sourceTable, targetSchema.TcmId)));

            if (this.CustomMetadataImporter == null)
                this.CustomMetadataImporter = customImporters.FirstOrDefault(x => x.TypeName == Functions.GetFromIsolatedStorage(Functions.GetId(this.txtHost.Text.GetDomainName(), "CustomMetadataImporter", sourceTable, targetSchema.TcmId)));
        }

        private void SetCustomNameTransformers()
        {
            if (string.IsNullOrEmpty(this._FormatString))
            {
                this._FormatString = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, this.cbSourceDatabase.SelectedValue as string, this.cbSourceTable.SelectedValue as string, "FormatString"));
            }

            if (this._Replacements == null)
            {
                string replacement1 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, this.cbSourceDatabase.SelectedValue as string, this.cbSourceTable.SelectedValue as string, "Replacement1"));
                string regex1 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, this.cbSourceDatabase.SelectedValue as string, this.cbSourceTable.SelectedValue as string, "Regex1"));
                if (!string.IsNullOrEmpty(replacement1) && !string.IsNullOrEmpty(regex1))
                {
                    this._Replacements = new List<ReplacementInfo>();
                    this._Replacements.Add(this.GetReplacement(replacement1, regex1));
                }

                string replacement2 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, this.cbSourceDatabase.SelectedValue as string, this.cbSourceTable.SelectedValue as string, "Replacement2"));
                string regex2 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, this.cbSourceDatabase.SelectedValue as string, this.cbSourceTable.SelectedValue as string, "Regex2"));
                if (!string.IsNullOrEmpty(replacement2) && !string.IsNullOrEmpty(regex2))
                {
                    if (this._Replacements == null)
                        this._Replacements = new List<ReplacementInfo>();
                    this._Replacements.Add(this.GetReplacement(replacement2, regex2));
                }

                string replacement3 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, this.cbSourceDatabase.SelectedValue as string, this.cbSourceTable.SelectedValue as string, "Replacement3"));
                string regex3 = Functions.GetFromIsolatedStorage(Functions.GetId(this.txtDbHost.Text, this.cbSourceDatabase.SelectedValue as string, this.cbSourceTable.SelectedValue as string, "Regex3"));
                if (!string.IsNullOrEmpty(replacement3) && !string.IsNullOrEmpty(regex3))
                {
                    if (this._Replacements == null)
                        this._Replacements = new List<ReplacementInfo>();
                    this._Replacements.Add(this.GetReplacement(replacement3, regex3));
                }
            }
        }

        private ReplacementInfo GetReplacement(string fragment, string regex)
        {
            ReplacementInfo replacement = new ReplacementInfo();
            replacement.Regex = regex;

            if (fragment == "[Title]" || fragment == "[TcmId]" || fragment == "[ID]")
            {
                replacement.Fragment = fragment;
            }
            else
            {
                replacement.Field = this._SourceSchemaFields.FirstOrDefault(x => x.GetFieldFullName(false) == fragment);
            }

            return replacement;
        }
    }
}
