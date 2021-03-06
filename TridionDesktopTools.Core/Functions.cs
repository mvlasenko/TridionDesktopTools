﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using Tridion.ContentManager.CoreService.Client;

namespace TridionDesktopTools.Core
{
    public static class Functions
    {
        #region Fields

        public static ILocalClient Client;
        public static StreamDownloadClient StreamDownloadClient;
        public static StreamUploadClient StreamUploadClient;
        public static BindingType ClientBindingType = BindingType.HttpBinding;
        public const string ClientVersion = "2013";

        #endregion

        #region Tridion CoreService

        private static NetTcpBinding GetBinding()
        {
            var binding = new NetTcpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxStringContentLength = 2097152,
                    MaxArrayLength = 819200,
                    MaxBytesPerRead = 5120,
                    MaxDepth = 32,
                    MaxNameTableCharCount = 81920
                },
                CloseTimeout = TimeSpan.FromMinutes(10),
                OpenTimeout = TimeSpan.FromMinutes(10),
                ReceiveTimeout = TimeSpan.FromMinutes(10),
                SendTimeout = TimeSpan.FromMinutes(10),
                TransactionFlow = true,
                TransactionProtocol = TransactionProtocol.WSAtomicTransaction11
            };
            return binding;
        }

        private static BasicHttpBinding GetHttpBinding()
        {
            var binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxStringContentLength = 2097152,
                    MaxArrayLength = 819200,
                    MaxBytesPerRead = 5120,
                    MaxDepth = 32,
                    MaxNameTableCharCount = 81920
                },
                CloseTimeout = TimeSpan.FromMinutes(10),
                OpenTimeout = TimeSpan.FromMinutes(10),
                ReceiveTimeout = TimeSpan.FromMinutes(10),
                SendTimeout = TimeSpan.FromMinutes(10),
                MessageEncoding = WSMessageEncoding.Mtom,
                TransferMode = TransferMode.Streamed
            };
            return binding;
        }

        private static BasicHttpBinding GetHttpBinding2()
        {
            var binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxStringContentLength = 2097152,
                    MaxArrayLength = 819200,
                    MaxBytesPerRead = 5120,
                    MaxDepth = 32,
                    MaxNameTableCharCount = 81920
                },
                CloseTimeout = TimeSpan.FromMinutes(10),
                OpenTimeout = TimeSpan.FromMinutes(10),
                ReceiveTimeout = TimeSpan.FromMinutes(10),
                SendTimeout = TimeSpan.FromMinutes(10),
                Security = new BasicHttpSecurity
                {
                    Mode = BasicHttpSecurityMode.TransportCredentialOnly,
                    Transport = new HttpTransportSecurity
                    {
                        ClientCredentialType = HttpClientCredentialType.Windows
                    },
                },
                MessageEncoding = WSMessageEncoding.Mtom
            };
            return binding;
        }

        private static BasicHttpBinding GetHttpBinding3()
        {
            var binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxStringContentLength = 2097152,
                    MaxArrayLength = 819200,
                    MaxBytesPerRead = 5120,
                    MaxDepth = 32,
                    MaxNameTableCharCount = 81920
                },
                CloseTimeout = TimeSpan.FromMinutes(10),
                OpenTimeout = TimeSpan.FromMinutes(10),
                ReceiveTimeout = TimeSpan.FromMinutes(10),
                SendTimeout = TimeSpan.FromMinutes(10),
                Security = new BasicHttpSecurity
                {
                    Mode = BasicHttpSecurityMode.TransportCredentialOnly,
                    Transport = new HttpTransportSecurity
                    {
                        ClientCredentialType = HttpClientCredentialType.Windows
                    },
                },
            };
            return binding;
        }

        public static LocalSessionAwareCoreServiceClient GetTcpClient(string host, string username, string password)
        {
            if (String.IsNullOrEmpty(host))
                host = "localhost";

            host = host.GetDomainName();

            var binding = GetBinding();

            var endpoint = new EndpointAddress(String.Format("net.tcp://{0}:2660/CoreService/{1}/netTcp", host, ClientVersion));

            var client = new LocalSessionAwareCoreServiceClient(binding, endpoint);

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                client.ChannelFactory.Credentials.Windows.ClientCredential = new NetworkCredential(username, password);
            }

            return client;
        }

        public static LocalCoreServiceClient GetHttpClient(string host, string username, string password)
        {
            if (String.IsNullOrEmpty(host))
                host = "localhost";

            host = host.GetDomainName();

            var binding = GetHttpBinding3();

            var endpoint = new EndpointAddress(String.Format("http://{0}/webservices/CoreService{1}.svc/basicHttp", host, ClientVersion));

            var client = new LocalCoreServiceClient(binding, endpoint);

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
                client.ClientCredentials.Windows.ClientCredential = new NetworkCredential(username, password);
            }

            return client;
        }

        public static bool EnsureValidClient(string host, string username, string password)
        {
            if (Client == null || Client is SessionAwareCoreServiceClient && ((SessionAwareCoreServiceClient)Client).InnerChannel.State == CommunicationState.Faulted)
            {
                if (ClientBindingType == BindingType.HttpBinding)
                    Client = GetHttpClient(host, username, password);
                else
                    Client = GetTcpClient(host, username, password);

                try
                {
                    var publications = Client.GetSystemWideListXml(new PublicationsFilterData());
                }
                catch
                {
                    MessageBox.Show("Not able to connect to TCM. Check your credentials and try again.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Client = null;
                    return false;
                }
            }
            return true;
        }

        public static void ResetClient()
        {
            Client = null;
        }

        public static StreamDownloadClient GetStreamDownloadClient(string host, string username, string password)
        {
            if (String.IsNullOrEmpty(host))
                host = "localhost";

            host = host.GetDomainName();

            var binding = GetHttpBinding2();

            var endpoint = new EndpointAddress(String.Format("http://{0}/webservices/CoreService{1}.svc/streamDownload_basicHttp", host, ClientVersion));

            StreamDownloadClient client = new StreamDownloadClient(binding, endpoint);

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
                client.ClientCredentials.Windows.ClientCredential = new NetworkCredential(username, password);
            }

            return client;
        }

        public static bool EnsureValidStreamDownloadClient(string host, string username, string password)
        {
            if (StreamDownloadClient == null || StreamDownloadClient.InnerChannel.State == CommunicationState.Faulted)
            {
                StreamDownloadClient = GetStreamDownloadClient(host, username, password);
                return true;
            }
            return false;
        }
        
        public static StreamUploadClient GetStreamUploadClient(string host, string username, string password)
        {
            if (String.IsNullOrEmpty(host))
                host = "localhost";

            host = host.GetDomainName();

            var binding = GetHttpBinding();

            var endpoint = new EndpointAddress(String.Format("http://{0}/webservices/CoreService{1}.svc/streamUpload_basicHttp", host, ClientVersion));

            StreamUploadClient client = new StreamUploadClient(binding, endpoint);

            return client;
        }

        public static bool EnsureValidStreamUploadClient(string host, string username, string password)
        {
            if (StreamUploadClient == null || StreamUploadClient.InnerChannel.State == CommunicationState.Faulted)
            {
                StreamUploadClient = GetStreamUploadClient(host, username, password);
                return true;
            }
            return false;
        }

        #endregion

        #region Tridion items access

        public static string CreateFolder(string title, string tcmContainer)
        {
            try
            {
                FolderData folderData = new FolderData
                {
                    Title = title,
                    LocationInfo = new LocationInfo { OrganizationalItem = new LinkToOrganizationalItemData { IdRef = tcmContainer } },
                    Id = "tcm:0-0-0"
                };

                folderData = Client.Save(folderData, new ReadOptions()) as FolderData;
                if (folderData == null)
                    return string.Empty;

                return folderData.Id;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string CreateFolderChain(List<string> folderChain, string tcmContainer)
        {
            if (folderChain == null || folderChain.Count == 0 || String.IsNullOrEmpty(tcmContainer))
                return tcmContainer;

            string topFolder = folderChain[0];
            List<ItemInfo> items = GetFoldersByParentFolder(tcmContainer);
            if (items.All(x => x.Title != topFolder))
            {
                CreateFolder(topFolder, tcmContainer);
                items = GetFoldersByParentFolder(tcmContainer);
            }

            string tcmTopFolder = items.First(x => x.Title == topFolder).TcmId;

            return CreateFolderChain(folderChain.Skip(1).ToList(), tcmTopFolder);
        }

        public static string CreateStructureGroup(string title, string tcmContainer)
        {
            try
            {
                StructureGroupData sgData = new StructureGroupData
                {
                    Title = title,
                    Directory = title,
                    LocationInfo = new LocationInfo { OrganizationalItem = new LinkToOrganizationalItemData { IdRef = tcmContainer } },
                    Id = "tcm:0-0-0"
                };

                sgData = Client.Save(sgData, new ReadOptions()) as StructureGroupData;
                if (sgData == null)
                    return string.Empty;

                return sgData.Id;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static ComponentData GetComponent(string id)
        {
            if (String.IsNullOrEmpty(id))
                return null;

            return ReadItem(id) as ComponentData;
        }

        public static bool ExistsItem(string tcmItem)
        {
            return (ReadItem(tcmItem) != null);
        }

        public static bool ExistsItem(string tcmContainer, string itemTitle)
        {
            if (String.IsNullOrEmpty(tcmContainer))
                return false;

            OrganizationalItemItemsFilterData filter = new OrganizationalItemItemsFilterData();
            return Client.GetList(tcmContainer, filter).Any(x => x.Title == itemTitle);
        }

        public static string GetItemTcmId(string tcmContainer, string itemTitle)
        {
            if (String.IsNullOrEmpty(tcmContainer))
                return string.Empty;

            OrganizationalItemItemsFilterData filter = new OrganizationalItemItemsFilterData();
            foreach (XElement element in Client.GetListXml(tcmContainer, filter).Elements())
            {
                if (element.Attribute("Title").Value == itemTitle)
                    return element.Attribute("ID").Value;
            }

            return string.Empty;
        }

        public static IdentifiableObjectData ReadItem(string id)
        {
            try
            {
                return Client.Read(id, null);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region Tridion hierarchy

        public static List<ItemInfo> GetItemsByParentContainer(string tcmContainer)
        {
            return Client.GetListXml(tcmContainer, new OrganizationalItemItemsFilterData()).ToList();
        }

        public static List<ItemInfo> GetItemsByParentContainer(string tcmContainer, bool recursive)
        {
            return Client.GetListXml(tcmContainer, new OrganizationalItemItemsFilterData { Recursive = recursive }).ToList();
        }

        public static List<ItemInfo> GetItemsByParentContainer(string tcmContainer, ItemType[] itemTypes)
        {
            return Client.GetListXml(tcmContainer, new OrganizationalItemItemsFilterData { ItemTypes = itemTypes }).ToList();
        }

        public static List<ItemInfo> GetFoldersByParentFolder(string tcmFolder)
        {
            return Client.GetListXml(tcmFolder, new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Folder } }).ToList(ItemType.Folder);
        }

        public static List<ItemInfo> GetFolders(string tcmFolder, bool recursive)
        {
            return Client.GetListXml(tcmFolder, new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Folder }, Recursive = recursive }).ToList(ItemType.Folder);
        }

        public static List<ItemInfo> GetTbbsByParentFolder(string tcmFolder)
        {
            return Client.GetListXml(tcmFolder, new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.TemplateBuildingBlock } }).ToList(ItemType.TemplateBuildingBlock);
        }

        public static List<ItemInfo> GetStructureGroupsByParentStructureGroup(string tcmSG)
        {
            return Client.GetListXml(tcmSG, new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.StructureGroup } }).ToList(ItemType.StructureGroup);
        }

        public static List<ItemInfo> GetFoldersByPublication(string tcmPublication)
        {
            return Client.GetListXml(tcmPublication, new RepositoryItemsFilterData { ItemTypes = new[] { ItemType.Folder } }).ToList(ItemType.Folder);
        }

        public static List<ItemInfo> GetStructureGroupsByPublication(string tcmPublication)
        {
            return Client.GetListXml(tcmPublication, new RepositoryItemsFilterData { ItemTypes = new[] { ItemType.StructureGroup } }).ToList(ItemType.StructureGroup);
        }

        public static List<ItemInfo> GetContainersByPublication(string tcmPublication)
        {
            return Client.GetListXml(tcmPublication, new RepositoryItemsFilterData { ItemTypes = new[] { ItemType.Folder, ItemType.StructureGroup } }).ToList();
        }

        public static List<ItemInfo> GetCategoriesByPublication(string tcmPublication)
        {
            return Client.GetListXml(tcmPublication, new RepositoryItemsFilterData { ItemTypes = new[] { ItemType.Category } }).ToList();
        }

        public static List<ItemInfo> GetKeywordsByCategory(string tcmCategory)
        {
            return Client.GetListXml(tcmCategory, new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Keyword } }).ToList(ItemType.Keyword);
        }

        public static List<ItemInfo> GetProcessDefinitionsByPublication(string tcmPublication)
        {
            return Client.GetSystemWideListXml(new ProcessDefinitionsFilterData { ContextRepository = new LinkToRepositoryData { IdRef = tcmPublication } }).ToList(ItemType.ProcessDefinition);
        }

        public static List<ItemInfo> GetItemsByPublication(string tcmPublication)
        {
            List<ItemInfo> list = new List<ItemInfo>();

            list.AddRange(GetContainersByPublication(tcmPublication));

            if (GetCategoriesByPublication(tcmPublication).Any())
                list.Add(new ItemInfo { Title = "Categories and Keywords", TcmId = "catman-" + tcmPublication, ItemType = ItemType.Folder });

            if (GetProcessDefinitionsByPublication(tcmPublication).Any())
                list.Add(new ItemInfo { Title = "Process Definitions", TcmId = "proc-" + tcmPublication, ItemType = ItemType.Folder });

            return list;
        }

        public static List<ItemInfo> GetItemsByPublication(string tcmPublication, bool recursive)
        {
            if (!recursive)
                return GetItemsByPublication(tcmPublication);

            List<ItemInfo> list = new List<ItemInfo>();

            foreach (ItemInfo container in GetContainersByPublication(tcmPublication))
            {
                list.Add(container);
                list.AddRange(GetItemsByParentContainer(container.TcmId, true));
            }

            List<ItemInfo> categories = GetCategoriesByPublication(tcmPublication);
            if (categories.Any())
            {
                list.Add(new ItemInfo { Title = "Categories and Keywords", TcmId = "catman-" + tcmPublication, ItemType = ItemType.Folder });
                list.AddRange(categories);
            }

            List<ItemInfo> processDefinitions = GetProcessDefinitionsByPublication(tcmPublication);
            if (processDefinitions.Any())
            {
                list.Add(new ItemInfo { Title = "Process Definitions", TcmId = "proc-" + tcmPublication, ItemType = ItemType.Folder });
                list.AddRange(processDefinitions);
            }

            return list;
        }

        public static List<ItemInfo> GetPublications()
        {
            return Client.GetSystemWideListXml(new PublicationsFilterData()).ToList(ItemType.Publication);
        }

        public static List<ItemInfo> GetPublications(string filterItemId)
        {
            List<ItemInfo> publications = GetPublications();
            var allowedPublications = Client.GetSystemWideList(new BluePrintFilterData { ForItem = new LinkToRepositoryLocalObjectData { IdRef = filterItemId } }).Cast<BluePrintNodeData>().Where(x => x.Item != null).Select(x => GetPublicationTcmId(x.Item.Id)).ToList();
            return publications.Where(x => allowedPublications.Any(y => y == x.TcmId)).ToList();
        }

        public static string GetWebDav(this RepositoryLocalObjectData item)
        {
            if (item.LocationInfo == null || string.IsNullOrEmpty(item.LocationInfo.WebDavUrl))
                return string.Empty;

            string webDav = HttpUtility.UrlDecode(item.LocationInfo.WebDavUrl.Replace("/webdav/", string.Empty));
            if (string.IsNullOrEmpty(webDav))
                return string.Empty;

            int dotIndex = webDav.LastIndexOf(".", StringComparison.Ordinal);
            int slashIndex = webDav.LastIndexOf("/", StringComparison.Ordinal);

            return dotIndex >= 0 && dotIndex > slashIndex ? webDav.Substring(0, dotIndex) : webDav;
        }

        public static List<string> GetWebDavChain(this RepositoryLocalObjectData item)
        {
            List<string> res = item.LocationInfo.WebDavUrl.Replace("/webdav/", string.Empty).Split('/').Select(HttpUtility.UrlDecode).ToList();

            if (res.Last().Contains("."))
                res[res.Count - 1] = res.Last().Substring(0, res.Last().LastIndexOf(".", StringComparison.Ordinal));
            
            return res;
        }

        public static List<string> Substract(this List<string> input, List<string> toSubstract)
        {
            if (input == null || toSubstract == null)
                return input;

            return String.Join("|||", input).Replace(String.Join("|||", toSubstract), string.Empty).Split(new [] { "|||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static List<string> GetUsingItems(string tcmItem, bool current = false, ItemType[] itemTypes = null)
        {
            UsingItemsFilterData filter = new UsingItemsFilterData();
            filter.IncludedVersions = current ? VersionCondition.OnlyLatestVersions : VersionCondition.AllVersions;
            filter.BaseColumns = ListBaseColumns.Id;
            if (itemTypes != null)
                filter.ItemTypes = itemTypes;

            List<string> items = Client.GetListXml(tcmItem, filter).ToList().Select(x => x.TcmId).ToList();
            return items;
        }

        private static List<string> GetUsingCurrentItems(string tcmItem)
        {
            return GetUsingItems(tcmItem, true);
        }

        public static List<string> GetUsedItems(string tcmItem, ItemType[] itemTypes = null)
        {
            UsedItemsFilterData filter = new UsedItemsFilterData();
            filter.BaseColumns = ListBaseColumns.Id;
            if (itemTypes != null)
                filter.ItemTypes = itemTypes;

            List<string> items = Client.GetListXml(tcmItem, filter).ToList().Select(x => x.TcmId).ToList();
            return items;
        }

        private static List<string> GetUsedItemsChain(string itemId, ItemType[] itemTypes = null)
        {
            List<string> usedItems = GetUsedItems(itemId, itemTypes);
            if (!usedItems.Any())
                return null;

            usedItems = usedItems.Where(x => x != itemId).ToList(); //avoid deadlock
            if (usedItems.Count == 0)
                return null;

            List<string> res = new List<string>();

            foreach (string usedItemId in usedItems)
            {
                List<string> innerUsedItems = GetUsedItemsChain(usedItemId, itemTypes);
                if (innerUsedItems == null)
                    continue;

                innerUsedItems = innerUsedItems.Where(x => x != itemId).ToList(); //avoid deadlock
                res.AddRange(innerUsedItems);
            }

            res.AddRange(usedItems);

            //avoid buplications
            res = res.Distinct().ToList();
            
            return res;
        }

        public static List<HistoryItemInfo> GetItemHistory(string tcmItem)
        {
            VersionsFilterData versionsFilter = new VersionsFilterData();
            XElement listOfVersions = Client.GetListXml(tcmItem, versionsFilter);

            List<HistoryItemInfo> res = new List<HistoryItemInfo>();

            if (listOfVersions != null && listOfVersions.HasElements)
            {
                foreach (XElement element in listOfVersions.Descendants())
                {
                    HistoryItemInfo item = new HistoryItemInfo();
                    item.TcmId = element.Attribute("ID").Value;
                    item.ItemType = element.Attributes().Any(x => x.Name == "Type") ? (ItemType)Int32.Parse(element.Attribute("Type").Value) : GetItemType(item.TcmId);
                    item.Title = element.Attributes().Any(x => x.Name == "Title") ? element.Attribute("Title").Value : item.TcmId;
                    item.Version = int.Parse(element.Attribute("Version").Value.Replace("v", ""));
                    item.Modified = DateTime.Parse(element.Attribute("Modified").Value);

                    res.Add(item);
                }
            }

            res.Last().Current = true;

            return res;
        }

        public static string GetItemContainer(string tcmItem)
        {
            RepositoryLocalObjectData item = Client.Read(tcmItem, new ReadOptions()) as RepositoryLocalObjectData;
            if (item == null)
                return string.Empty;

            return item.LocationInfo.OrganizationalItem.IdRef;
        }

        #endregion

        #region Tridion schemas

        public static List<ItemInfo> GetSchemas(string tcmPublication)
        {
            ItemInfo folder0 = GetFoldersByPublication(tcmPublication)[0];
            return Client.GetListXml(folder0.TcmId, new OrganizationalItemItemsFilterData { Recursive = true, ItemTypes = new[] { ItemType.Schema }, SchemaPurposes = new[] { SchemaPurpose.Component, SchemaPurpose.Metadata } }).ToList(ItemType.Schema);
        }

        public static List<ItemInfo> GetSchemas(string tcmFolder, bool recursive)
        {
            return Client.GetListXml(tcmFolder, new OrganizationalItemItemsFilterData { Recursive = recursive, ItemTypes = new[] { ItemType.Schema } }).ToList(ItemType.Schema);
        }

        public static List<ItemFieldDefinitionData> GetSchemaFields(string tcmSchema)
        {
            SchemaFieldsData schemaFieldsData;

            //todo: hot fix - find better solution
            if (tcmSchema.Contains("-v"))
            {
                string version = tcmSchema.Split('-')[3];

                SchemaData schema = (SchemaData)ReadItem(tcmSchema);

                string versionSchemaUri = GetItemTcmId(schema.LocationInfo.OrganizationalItem.IdRef, schema.Title + "_" + version);
                if (String.IsNullOrEmpty(versionSchemaUri))
                {
                    versionSchemaUri = DublicateSchema(schema, schema.Title + "_" + version);
                }

                schemaFieldsData = Client.ReadSchemaFields(versionSchemaUri, false, null);
                if (schemaFieldsData == null || schemaFieldsData.Fields == null)
                    return null;

                return schemaFieldsData.Fields.ToList();
            }

            schemaFieldsData = Client.ReadSchemaFields(tcmSchema, false, null);
            if (schemaFieldsData == null || schemaFieldsData.Fields == null)
                return null;

            return schemaFieldsData.Fields.ToList();
        }

        public static List<ItemFieldDefinitionData> GetSchemaMetadataFields(string tcmSchema)
        {
            SchemaFieldsData schemaFieldsData;

            //todo: hot fix - find better solution
            if (tcmSchema.Contains("-v"))
            {
                string version = tcmSchema.Split('-')[3];

                SchemaData schema = (SchemaData)ReadItem(tcmSchema);

                string versionSchemaUri = GetItemTcmId(schema.LocationInfo.OrganizationalItem.IdRef, schema.Title + "_" + version);
                if (String.IsNullOrEmpty(versionSchemaUri))
                {
                    versionSchemaUri = DublicateSchema(schema, schema.Title + "_" + version);
                }

                schemaFieldsData = Client.ReadSchemaFields(versionSchemaUri, false, null);
                if (schemaFieldsData == null || schemaFieldsData.MetadataFields == null)
                    return null;

                return schemaFieldsData.MetadataFields.ToList();
            }

            schemaFieldsData = Client.ReadSchemaFields(tcmSchema, false, null);
            if (schemaFieldsData == null || schemaFieldsData.MetadataFields == null)
                return null;

            return schemaFieldsData.MetadataFields.ToList();
        }

        //todo: hot fix - find better solution
        public static string DublicateSchema(SchemaData schema, string newName)
        {
            SchemaData newSchema = new SchemaData
            {
                Title = newName,
                Description = schema.Description,
                RootElementName = schema.RootElementName,
                Purpose = schema.Purpose,
                LocationInfo = new LocationInfo { OrganizationalItem = new LinkToOrganizationalItemData { IdRef = schema.LocationInfo.OrganizationalItem.IdRef } },
                Id = "tcm:0-0-0",
                Xsd = schema.Xsd,
                AllowedMultimediaTypes = schema.AllowedMultimediaTypes,
            };

            newSchema = Client.Save(newSchema, new ReadOptions()) as SchemaData;
            if (newSchema == null)
                return "";

            var res = Client.CheckIn(newSchema.Id, new ReadOptions());
            return res.Id;
        }

        public static List<T> GetSchemaFields<T>(string tcmSchema) where T : ItemFieldDefinitionData
        {
            SchemaFieldsData schemaFieldsData = Client.ReadSchemaFields(tcmSchema, false, null);
            return schemaFieldsData.Fields.Where(x => x is T).Cast<T>().ToList();
        }

        public static List<T> GetSchemaMetadataFields<T>(string tcmSchema) where T : ItemFieldDefinitionData
        {
            SchemaFieldsData schemaFieldsData = Client.ReadSchemaFields(tcmSchema, false, null);
            return schemaFieldsData.MetadataFields.Where(x => x is T).Cast<T>().ToList();
        }

        #endregion

        #region Tridion components

        public static List<ItemInfo> GetComponents(string tcmSchema)
        {
            return Client.GetListXml(tcmSchema, new UsingItemsFilterData { ItemTypes = new[] { ItemType.Component } }).ToList(ItemType.Component);
        }

        public static List<ItemInfo> GetComponents(string tcmFolder, bool recursive)
        {
            return Client.GetListXml(tcmFolder, new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Component }, Recursive = recursive }).ToList(ItemType.Component);
        }

        public static List<ItemInfo> GetComponents(string tcmFolder, string tcmSchema)
        {
            if (String.IsNullOrEmpty(tcmFolder) && String.IsNullOrEmpty(tcmFolder))
                return new List<ItemInfo>();

            if (String.IsNullOrEmpty(tcmFolder))
                return GetComponents(tcmSchema);

            if (String.IsNullOrEmpty(tcmSchema))
                return GetComponents(tcmFolder, true);

            return Client.GetListXml(tcmFolder, new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Component }, Recursive = true, BasedOnSchemas = new [] { new LinkToSchemaData { IdRef = tcmSchema} } }).ToList(ItemType.Component);
        }

        public static List<ItemInfo> GetComponentsByTextCriterias(string tcmFolder, string text, string tcmSchema, List<Criteria> textCriterias)
        {
            if ((textCriterias == null || textCriterias.Count == 0) && String.IsNullOrEmpty(text))
                return new List<ItemInfo>();

            SearchQueryData filter = new SearchQueryData();
            filter.ItemTypes = new[] { ItemType.Component };

            if (!String.IsNullOrEmpty(tcmFolder))
                filter.SearchIn = new LinkToIdentifiableObjectData { IdRef = tcmFolder };

            if (!String.IsNullOrEmpty(text))
                filter.FullTextQuery = text;

            if (!String.IsNullOrEmpty(tcmSchema) && textCriterias != null && textCriterias.Count > 0)
            {
                List<BasedOnSchemaData> basedOnSchemas = new List<BasedOnSchemaData>();
                foreach (Criteria criteria in textCriterias)
                {
                    BasedOnSchemaData basedSchema = new BasedOnSchemaData();
                    basedSchema.Schema = new LinkToSchemaData { IdRef = tcmSchema };
                    basedSchema.Field = criteria.Field.Name;

                    if (criteria.Operation == Operation.Equal)
                        basedSchema.FieldValue = criteria.Value.ToString();
                    
                    if (criteria.Operation == Operation.Like)
                        basedSchema.FieldValue = "*" + criteria.Value + "*";

                    basedOnSchemas.Add(basedSchema);
                }

                filter.BasedOnSchemas = basedOnSchemas.ToArray();
            }

            return Client.GetSearchResultsXml(filter).ToList(ItemType.Component);
        }

        public static List<ItemInfo> GetComponentsByDateOrNumberCriterias(string tcmFolder, string tcmSchema, List<Criteria> criterias)
        {
            if (criterias == null || criterias.Count == 0)
                return new List<ItemInfo>();

            List<List<ItemInfo>> arrRes = new List<List<ItemInfo>>();

            foreach (Criteria criteria in criterias)
            {
                List<ItemInfo> criteriaRes = GetComponentsByDateOrNumberSingleCriteria(tcmFolder, tcmSchema, criteria);
                arrRes.Add(criteriaRes);
            }

            return Intersect(arrRes.ToArray(), true);
        }

        private static List<ItemInfo> GetComponentsByDateOrNumberSingleCriteria(string tcmFolder, string schemaUri, Criteria criteria)
        {
            List<ItemInfo> res = new List<ItemInfo>();

            if (!criteria.Field.IsDate() && !criteria.Field.IsNumber() || criteria.Value == null)
                return res;

            SchemaData schema = Client.Read(schemaUri, null) as SchemaData;
            if (schema == null)
                return res;

            XNamespace ns = schema.NamespaceUri;

            List<ItemInfo> components = GetComponents(tcmFolder, schemaUri);

            foreach (ItemInfo item in components)
            {
                ComponentData component = Client.Read(item.TcmId, new ReadOptions()) as ComponentData;
                if (component == null)
                    continue;

                object value = GetComponentSingleValue(component, criteria.Field, ns);
                if (value == null)
                    continue;

                if (criteria.Field.IsDate() && criteria.Value != null)
                {
                    DateTime date = (DateTime) value;
                    DateTime compareDate = (DateTime) criteria.Value;

                    if (criteria.Operation == Operation.Equal && date == compareDate ||
                        criteria.Operation == Operation.Greater && date > compareDate ||
                        criteria.Operation == Operation.Less && date < compareDate)
                        res.Add(item);
                }

                if (criteria.Field.IsNumber() && criteria.Value != null)
                {
                    double num = (double) value;
                    double compareNum = (double) criteria.Value;

                    if (criteria.Operation == Operation.Equal && Math.Abs(num - compareNum) < 0.001 ||
                        criteria.Operation == Operation.Greater && num > compareNum ||
                        criteria.Operation == Operation.Less && num < compareNum)
                        res.Add(item);
                }
            }

            return res;
        }

        public static List<ItemInfo> GetComponentsByMultimediaCriterias(string tcmFolder, string tcmSchema, List<Criteria> criterias)
        {
            if (criterias == null || criterias.Count == 0)
                return new List<ItemInfo>();

            List<List<ItemInfo>> arrRes = new List<List<ItemInfo>>();

            foreach (Criteria criteria in criterias)
            {
                List<ItemInfo> criteriaRes = GetComponentsByMultimediaSingleCriteria(tcmFolder, tcmSchema, criteria);
                arrRes.Add(criteriaRes);
            }

            return Intersect(arrRes.ToArray(), true);
        }

        private static List<ItemInfo> GetComponentsByMultimediaSingleCriteria(string tcmFolder, string schemaUri, Criteria criteria)
        {
            List<ItemInfo> res = new List<ItemInfo>();

            if (!criteria.Field.IsMultimedia() || criteria.Value == null)
                return res;

            SchemaData schema = Client.Read(schemaUri, null) as SchemaData;
            if (schema == null)
                return res;

            XNamespace ns = schema.NamespaceUri;
            XNamespace imgNs = "http://www.w3.org/1999/xlink";

            List<ItemInfo> components = GetComponents(tcmFolder, schemaUri);

            foreach (ItemInfo item in components)
            {
                ComponentData component = Client.Read(item.TcmId, new ReadOptions()) as ComponentData;
                if (component == null)
                    continue;

                XElement value = GetComponentSingleValue(component, criteria.Field, ns) as XElement;
                if (value == null)
                    continue;

                string tcmImage = value.Attribute(imgNs + "href").Value;
                ComponentData imageComponent = GetComponent(tcmImage);
                string filename = imageComponent.BinaryContent.Filename;

                if (criteria.Operation == Operation.Equal && filename == criteria.Value.ToString() ||
                    criteria.Operation == Operation.Like && filename.Contains(criteria.Value.ToString()))
                    res.Add(item);
            }

            return res;
        }

        public static List<ItemInfo> GetComponentsByField2FieldCriterias(string tcmFolder, string tcmSchema, List<Criteria> criterias)
        {
            if (criterias == null || criterias.Count == 0)
                return new List<ItemInfo>();

            List<List<ItemInfo>> arrRes = new List<List<ItemInfo>>();

            foreach (Criteria criteria in criterias)
            {
                List<ItemInfo> criteriaRes = GetComponentsByField2FieldSingleCriteria(tcmFolder, tcmSchema, criteria);
                arrRes.Add(criteriaRes);
            }

            return Intersect(arrRes.ToArray(), true);
        }

        private static List<ItemInfo> GetComponentsByField2FieldSingleCriteria(string tcmFolder, string schemaUri, Criteria criteria)
        {
            List<ItemInfo> res = new List<ItemInfo>();

            if (criteria.Operation != Operation.EqualField && criteria.Operation != Operation.GreaterField && criteria.Operation != Operation.LessField && criteria.Operation != Operation.LikeField || criteria.Field == null || criteria.FieldCompare == null)
                return res;

            SchemaData schema = Client.Read(schemaUri, null) as SchemaData;
            if (schema == null)
                return res;

            XNamespace ns = schema.NamespaceUri;
            XNamespace imgNs = "http://www.w3.org/1999/xlink";

            List<ItemInfo> components = GetComponents(tcmFolder, schemaUri);

            foreach (ItemInfo item in components)
            {
                ComponentData component = Client.Read(item.TcmId, new ReadOptions()) as ComponentData;
                if (component == null)
                    continue;

                object value = GetComponentSingleValue(component, criteria.Field, ns);
                object valueCompare = GetComponentSingleValue(component, criteria.FieldCompare, ns);
                if (value == null || valueCompare == null)
                    continue;

                if (criteria.Field.IsDate() && criteria.FieldCompare.IsDate())
                {
                    DateTime date = (DateTime)value;
                    DateTime compareDate = (DateTime)valueCompare;

                    if (criteria.Operation == Operation.EqualField && date == compareDate ||
                        criteria.Operation == Operation.GreaterField && date > compareDate ||
                        criteria.Operation == Operation.LessField && date < compareDate)
                        res.Add(item);
                }

                else if (criteria.Field.IsNumber() && criteria.FieldCompare.IsNumber())
                {
                    double num = (double)value;
                    double compareNum = (double)valueCompare;

                    if (criteria.Operation == Operation.EqualField && Math.Abs(num - compareNum) < 0.001 ||
                        criteria.Operation == Operation.GreaterField && num > compareNum ||
                        criteria.Operation == Operation.LessField && num < compareNum)
                        res.Add(item);
                }

                else if (criteria.Field.IsMultimedia() && criteria.FieldCompare.IsMultimedia())
                {
                    string tcmImage = ((XElement)value).Attribute(imgNs + "href").Value;
                    ComponentData imageComponent = GetComponent(tcmImage);
                    string filename = imageComponent.BinaryContent.Filename;

                    string tcmImageCompare = ((XElement)valueCompare).Attribute(imgNs + "href").Value;
                    ComponentData imageComponentCompare = GetComponent(tcmImageCompare);
                    string filenameCompare = imageComponentCompare.BinaryContent.Filename;

                    if (criteria.Operation == Operation.EqualField && filename == filenameCompare)
                        res.Add(item);
                }

                else
                {
                    if (criteria.Operation == Operation.EqualField && value.ToString() == valueCompare.ToString())
                        res.Add(item);
                }
            }

            return res;
        }

        public static List<ItemInfo> GetComponentsByCriterias(string tcmContainer, string tcmSchema, List<Criteria> criterias)
        {
            string tcmFolder;
            ItemType containerType = GetItemType(tcmContainer);
            if (containerType == ItemType.Publication)
            {
                ItemInfo folder0 = GetFoldersByPublication(tcmContainer)[0];
                tcmFolder = folder0.TcmId;
            }
            else
            {
                tcmFolder = tcmContainer;
            }

            if (criterias == null || criterias.Count == 0)
                return GetComponents(tcmFolder, tcmSchema);

            List<List<ItemInfo>> res = new List<List<ItemInfo>>();

            List<Criteria> textCriterias = criterias.Where(x => (x.Field.IsText() || x.Field.IsRichText() || x.Field.IsTextSelect() || x.Field.IsKeyword() || x.Field.IsEmbedded()) && (x.Operation == Operation.Equal || x.Operation == Operation.Like)).ToList();
            List<ItemInfo> resText = GetComponentsByTextCriterias(tcmFolder, string.Empty, tcmSchema, textCriterias);
            res.Add(resText);

            List<Criteria> deteNumCriterias = criterias.Where(x => (x.Field.IsDate() || x.Field.IsNumber()) && (x.Operation == Operation.Equal || x.Operation == Operation.Greater || x.Operation == Operation.Less)).ToList();
            List<ItemInfo> resDateNum = GetComponentsByDateOrNumberCriterias(tcmFolder, tcmSchema, deteNumCriterias);
            res.Add(resDateNum);

            List<Criteria> multimediaCriterias = criterias.Where(x => (x.Field.IsMultimedia()) && (x.Operation == Operation.Equal || x.Operation == Operation.Like)).ToList();
            List<ItemInfo> resMultimedia = GetComponentsByMultimediaCriterias(tcmFolder, tcmSchema, multimediaCriterias);
            res.Add(resMultimedia);

            List<Criteria> anyCriterias = criterias.Where(x => (x.Field.Name == "< Any Field >") && x.Operation == Operation.Like).ToList();
            foreach (Criteria anyCriteria in anyCriterias)
            {
                List<ItemInfo> resAny = GetComponentsByTextCriterias(tcmFolder, anyCriteria.Value.ToString(), tcmSchema, null);
                res.Add(resAny);
            }

            List<Criteria> field2FieldCriterias = criterias.Where(x => x.Operation == Operation.EqualField || x.Operation == Operation.GreaterField || x.Operation == Operation.LessField || x.Operation == Operation.LikeField).ToList();
            List<ItemInfo> resField2Field = GetComponentsByField2FieldCriterias(tcmFolder, tcmSchema, field2FieldCriterias);
            res.Add(resField2Field);

            return Intersect(res.ToArray(), false);
        }

        //todo: use it
        public static string DetectComponentSchemaVersion(ComponentData component)
        {
            string schemaUri = component.Schema.IdRef;

            SchemaData schema = Client.Read(schemaUri, null) as SchemaData;
            if (schema == null)
                return string.Empty;

            if (component.VersionInfo.RevisionDate > schema.VersionInfo.RevisionDate)
                return schemaUri;

            List<HistoryItemInfo> schemaHistory = GetItemHistory(schemaUri);
            schemaHistory.Reverse();

            HistoryItemInfo historyItem = schemaHistory.FirstOrDefault(x => x.Modified < component.VersionInfo.RevisionDate);

            if (historyItem == null)
                return string.Empty;

            return historyItem.TcmId;
        }

        //todo: use it
        public static string DetectMetadataSchemaVersion(RepositoryLocalObjectData tridionObject)
        {
            string metadataSchemaUri = tridionObject.MetadataSchema.IdRef;

            SchemaData metadataSchema = Client.Read(metadataSchemaUri, null) as SchemaData;
            if (metadataSchema == null)
                return string.Empty;

            if (tridionObject.VersionInfo.RevisionDate > metadataSchema.VersionInfo.RevisionDate)
                return metadataSchemaUri;

            List<HistoryItemInfo> metadataSchemaHistory = GetItemHistory(metadataSchemaUri);
            metadataSchemaHistory.Reverse();

            HistoryItemInfo historyItem = metadataSchemaHistory.FirstOrDefault(x => x.Modified < tridionObject.VersionInfo.RevisionDate);

            if (historyItem == null)
                return string.Empty;

            return historyItem.TcmId;
        }

        public static XElement GetComponentXml(XNamespace ns, string rootElementName, List<ComponentFieldData> componentFieldValues)
        {
            if (string.IsNullOrEmpty(rootElementName) || componentFieldValues.Count == 0)
                return null;
            
            XElement contentXml = new XElement(ns + rootElementName);

            foreach (ComponentFieldData fieldValue in componentFieldValues)
            {
                if (fieldValue.IsMultiValue)
                {
                    IList values = fieldValue.Value as IList;
                    if (values != null)
                    {
                        foreach (object value in values)
                        {
                            if (value is XElement)
                            {
                                contentXml.Add(((XElement)value).Clone(fieldValue.SchemaField.Name));
                            }
                            else
                            {
                                contentXml.Add(new XElement(ns + fieldValue.SchemaField.Name, value));
                            }
                        }
                    }
                }
                else
                {
                    if (fieldValue.Value is XElement)
                    {
                        contentXml.Add(((XElement)fieldValue.Value).Clone(fieldValue.SchemaField.Name));
                    }
                    else
                    {
                        contentXml.Add(new XElement(ns + fieldValue.SchemaField.Name, fieldValue.Value));
                    }
                }
            }

            return contentXml;
        }

        public static object GetComponentFieldData(this XElement element, ItemFieldDefinitionData schemaField)
        {
            if (element == null)
                return null;

            string value = element.GetInnerXml();

            if (schemaField.IsNumber())
                return String.IsNullOrEmpty(value) ? null : (double?)double.Parse(value);

            if (schemaField.IsDate())
                return String.IsNullOrEmpty(value) ? null : (DateTime?)DateTime.Parse(value);

            if (schemaField.IsText() || schemaField.IsRichText() || schemaField.IsTextSelect())
                return value;

            return element.Clone(schemaField.Name);
        }

        public static object GetComponentFieldData(string value, ItemFieldDefinitionData schemaField)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            if (schemaField.IsNumber())
                return String.IsNullOrEmpty(value) ? null : (double?)double.Parse(value);

            if (schemaField.IsDate())
                return String.IsNullOrEmpty(value) ? null : (DateTime?)DateTime.Parse(value);

            return value;
        }

        public static object GetComponentSingleValue(ComponentData componentData, ItemFieldDefinitionData schemaField, XNamespace ns)
        {
            if (componentData == null || string.IsNullOrEmpty(componentData.Content))
                return null;

            XDocument doc = XDocument.Parse(componentData.Content);
            XElement element = null;
            
            if (doc.Root != null)
            {
                element = doc.Root.Element(ns + schemaField.Name);
            }

            if (element == null && !String.IsNullOrEmpty(componentData.Metadata))
            {
                XDocument docMeta = XDocument.Parse(componentData.Metadata);
                if (docMeta.Root != null)
                {
                    element = docMeta.Root.Element(ns + schemaField.Name);
                }
            }

            return element.GetComponentFieldData(schemaField);
        }

        public static List<FieldMappingInfo> GetDefaultFieldMapping(string host, List<FieldInfo> sourceFields, List<FieldInfo> targetFields, string targetSchemaUri)
        {
            List<FieldMappingInfo> fieldMapping = new List<FieldMappingInfo>();

            foreach (FieldInfo targetSchemaLink in targetFields)
            {
                string sourceFieldFullName = GetFromIsolatedStorage(GetId(host.GetDomainName(), targetSchemaUri, targetSchemaLink.Field.Name));
                if (String.IsNullOrEmpty(sourceFieldFullName))
                    sourceFieldFullName = targetSchemaLink.GetFieldFullName();

                FieldInfo sourceField = sourceFieldFullName == "< ignore >" ? null : sourceFields.FirstOrDefault(x => x.GetFieldFullName() == sourceFieldFullName);

                string defaultValue = GetFromIsolatedStorage(GetId(host.GetDomainName(), targetSchemaUri, targetSchemaLink.Field.Name, "DefaultValue"));

                fieldMapping.Add(new FieldMappingInfo { SourceFields = sourceFields, SourceFieldFullName = sourceField.GetFieldFullName(), TargetFields = targetFields, TargetFieldFullName = targetSchemaLink.GetFieldFullName(), DefaultValue = defaultValue });
            }

            return fieldMapping;
        }

        public static List<FieldMappingInfo> GetDefaultFieldMapping(List<ItemFieldDefinitionData> targetFields, string targetSchemaUri)
        {
            List<FieldMappingInfo> fieldMapping = new List<FieldMappingInfo>();
            foreach (FieldInfo targetSchemaLink in GetAllFields(targetFields, null, false, true))
            {
                fieldMapping.Add(new FieldMappingInfo { SourceFields = targetFields.Select(x => new FieldInfo { Field = x }).ToList(), SourceFieldFullName = targetSchemaLink.GetFieldFullName(), TargetFields = targetFields.Select(x => new FieldInfo { Field = x }).ToList(), TargetFieldFullName = targetSchemaLink.GetFieldFullName() });
            }

            return fieldMapping;
        }

        public static List<FieldInfo> ExpandChildFields(this List<FieldInfo> list)
        {
            if (list == null)
                return null;

            List<FieldInfo> res = new List<FieldInfo>();
            foreach (FieldInfo field in list)
            {
                if (field.Field.IsComponentLink() && ((ComponentLinkFieldDefinitionData)field.Field).AllowedTargetSchemas.Any())
                {
                    string childSchemaId = ((ComponentLinkFieldDefinitionData)field.Field).AllowedTargetSchemas[0].IdRef;
                    SchemaData childSchema = Client.Read(childSchemaId, null) as SchemaData;
                    if (childSchema != null)
                    {
                        field.RootElementName = childSchema.RootElementName;
                    }
                }

                res.Add(field);

                if (field.Level < 3 && (field.Field.IsEmbedded() || field.Field.IsComponentLink() && !field.Field.IsMultimediaComponentLink() && ((ComponentLinkFieldDefinitionData)field.Field).AllowedTargetSchemas.Any()))
                {
                    string childSchemaId = field.Field.IsEmbedded() ?
                        ((EmbeddedSchemaFieldDefinitionData) field.Field).EmbeddedSchema.IdRef :
                        ((ComponentLinkFieldDefinitionData) field.Field).AllowedTargetSchemas[0].IdRef;

                    var schemaFields = GetSchemaFields(childSchemaId);
                    var childFields = schemaFields.Select(x => new FieldInfo { Field = x }).ToList();

                    foreach (FieldInfo childField in childFields)
                    {
                        childField.Parent = field;
                        childField.Level = field.Level + 1;
                    }

                    foreach (FieldInfo childField in ExpandChildFields(childFields))
                    {
                        res.Add(childField);
                    }
                }
            }

            return res;
        }

        public static List<FieldInfo> GetAllFields(List<ItemFieldDefinitionData> schemaFields, List<ItemFieldDefinitionData> metadataFields, bool includeSourceSystemItems, bool includeTargetSystemItems)
        {
            List<FieldInfo> res = new List<FieldInfo>();

            if (schemaFields != null)
            {
                foreach (ItemFieldDefinitionData item in schemaFields)
                {
                    FieldInfo field = new FieldInfo();
                    field.IsMeta = false;
                    field.Field = item;
                    field.Level = 0;
                    res.Add(field);
                }
            }

            if (metadataFields != null)
            {
                foreach (ItemFieldDefinitionData item in metadataFields)
                {
                    FieldInfo field = new FieldInfo();
                    field.IsMeta = true;
                    field.Field = item;
                    field.Level = 0;
                    res.Add(field);
                }
            }

            res = res.ExpandChildFields();

            if (includeSourceSystemItems)
            {
                //link to source component
                res.Insert(0, new FieldInfo { Field = new ComponentLinkFieldDefinitionData { Name = "< this component link >" } });
                //new empty embedded schema, with possible child values
                res.Insert(0, new FieldInfo { Field = new ItemFieldDefinitionData { Name = "< new >" } });
                //ignore item
                res.Insert(0, new FieldInfo { Field = new ItemFieldDefinitionData() });
            }
            else if (includeTargetSystemItems)
            {
                //link to target component
                res.Insert(0, new FieldInfo { Field = new ComponentLinkFieldDefinitionData { Name = "< target component link >" } });
            }

            return res;
        }

        public static void SetHistoryMappingTree(List<HistoryItemMappingInfo> HistoryMapping)
        {
            if (HistoryMapping == null)
                return;
            
            foreach (HistoryItemMappingInfo historyMapping in HistoryMapping)
            {
                //back to tree mapping - set ChildFieldMapping property from plain mapping collection
                foreach (FieldMappingInfo mapping in historyMapping.Mapping)
                {
                    List<FieldMappingInfo> childMapping = historyMapping.Mapping.Where(x => x.TargetField.Parent.GetFieldFullName().Trim() == mapping.TargetField.GetFieldFullName().Trim()).ToList();
                    if (childMapping.Count > 0)
                        mapping.ChildFieldMapping = childMapping;
                }

                //back to tree mapping - remove > 0 level items
                historyMapping.Mapping = historyMapping.Mapping.Where(x => x.TargetField.Level == 0).ToList();
            }
        }

        public static List<FieldMappingInfo> GetCurrentMapping(this HistoryMappingInfo historyMapping)
        {
            return historyMapping.First(x => x.Current).Mapping;
        }

        public static List<FieldMappingInfo> GetDetectedMapping(this HistoryMappingInfo historyMapping, DateTime? tridionObjectDate)
        {
            if (tridionObjectDate == null)
                return GetCurrentMapping(historyMapping);

            List<HistoryItemMappingInfo> sortedHistoryItemMapping = new List<HistoryItemMappingInfo>();
            sortedHistoryItemMapping.AddRange(historyMapping.Select(x => x).OrderBy(x => x.Modified));
            sortedHistoryItemMapping.Reverse();

            HistoryItemMappingInfo historyMappingItem = sortedHistoryItemMapping.FirstOrDefault(x => x.Modified < tridionObjectDate);

            return historyMappingItem == null ? GetCurrentMapping(historyMapping) : historyMappingItem.Mapping;
        }

        public static object GetDefaultValue(this FieldMappingInfo mapping)
        {
            return GetComponentFieldData(mapping.DefaultValue, mapping.TargetField.Field);
        }

        public static XElement GetDefaultXmlValue(this FieldMappingInfo mapping, XNamespace ns)
        {
            object defaultValue = mapping.GetDefaultValue();

            if (defaultValue == null)
                return null;

            if (defaultValue is XElement)
                return defaultValue as XElement;

            return new XElement(ns + mapping.TargetField.Field.Name, defaultValue);
        }

        public static XElement GetByXPath(this XElement root, string xPath, XNamespace ns)
        {
            if (root == null || String.IsNullOrEmpty(xPath))
                return null;

            xPath = xPath.Trim('/');
            if (String.IsNullOrEmpty(xPath))
                return null;

            if (xPath.Contains("/"))
            {
                xPath = "/xhtml:" + xPath.Replace("/", "/xhtml:");
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
                namespaceManager.AddNamespace("xhtml", ns.ToString());
                return root.XPathSelectElement(xPath, namespaceManager);
            }

            return root.Element(ns + xPath);
        }

        public static List<XElement> GetListByXPath(this XElement root, string xPath, XNamespace ns)
        {
            if (root == null || String.IsNullOrEmpty(xPath))
                return null;

            xPath = xPath.Trim('/');

            if (String.IsNullOrEmpty(xPath))
                return null;

            if (xPath.Contains("/"))
            {
                return root.Elements(ns + xPath.Split('/')[0]).ToList().SelectMany(x => x.GetListByXPath(xPath.Substring(xPath.IndexOf('/')), ns)).ToList();
            }

            return root.Elements(ns + xPath).ToList();
        }

        public static XElement Clone(this XElement node, string name)
        {
            XNamespace linkNs = "http://www.w3.org/1999/xlink";
            if (node.Attribute(linkNs + "href") != null)
                return GetComponentLink(node.Attribute(linkNs + "href").Value, node.Attribute(linkNs + "title") == null ? null : node.Attribute(linkNs + "title").Value, name);

            XNamespace ns = node.GetDefaultNamespace();
            if (node.Elements().Any())
                return new XElement(ns + name, node.Attributes(), node.Elements());

            return new XElement(ns + name, node.Attributes(), node.Value);
        }

        public static XElement GetComponentLink(string id, string title, string fieldName)
        {
            XNamespace ns = "http://www.w3.org/1999/xlink";

            if (string.IsNullOrEmpty(title))
                return new XElement(fieldName,
                    new XAttribute(XNamespace.Xmlns + "xlink", ns),
                    new XAttribute(ns + "href", id));

            return new XElement(fieldName,
                new XAttribute(XNamespace.Xmlns + "xlink", ns),
                new XAttribute(ns + "href", id),
                new XAttribute(ns + "title", title));
        }

        public static XElement GetKeywordLink(string id, string title, string fieldName)
        {
            XElement res = GetComponentLink(id, title, fieldName);
            res.Add(title);
            return res;
        }

        private static XElement GetSourceMappedValue(FieldMappingInfo mapping, XElement root, XNamespace sourceNs)
        {
            XElement value;

            List<FieldMappingInfo> childFieldMapping = mapping.ChildFieldMapping != null ? mapping.ChildFieldMapping.Where(x => x.SourceField != null && x.SourceField.Field != null && x.SourceField.Field.Name != null && x.SourceFieldFullName != "< ignore >").ToList() : null;

            if (!mapping.Equals && childFieldMapping != null && childFieldMapping.Count > 0)
            {
                XNamespace ns = string.Empty;
                value = new XElement(ns + mapping.TargetField.Field.Name);

                foreach (FieldMappingInfo childMapping in childFieldMapping)
                {
                    XElement child = GetSourceMappedValue(childMapping, root, sourceNs);
                    if (child != null)
                    {
                        value.Add(child);
                    }
                }
            }
            else
            {
                XElement defaultValue = mapping.GetDefaultXmlValue(sourceNs);

                if (root == null || mapping.SourceFieldFullName == "< new >")
                {
                    value = defaultValue;
                }
                else
                {
                    value = root.GetByXPath(mapping.SourceField.GetFieldNamePath(true), sourceNs);
                    value = value != null ? value.Clone(mapping.TargetField.Field.Name) : defaultValue;
                }
            }

            return value;
        }

        private static List<XElement> GetSourceMappedValues(FieldMappingInfo mapping, XElement root, XNamespace sourceNs)
        {
            if (root == null)
                return new List<XElement>();

            List<XElement> values;

            List<FieldMappingInfo> childFieldMapping = mapping.ChildFieldMapping != null ? mapping.ChildFieldMapping.Where(x => x.SourceField != null && x.SourceField.Field != null && x.SourceField.Field.Name != null && x.SourceFieldFullName != "< ignore >").ToList() : null;

            if (!mapping.Equals && childFieldMapping != null && childFieldMapping.Count > 0)
            {
                values = new List<XElement>();

                int maxCount = childFieldMapping.Max(x => root.GetListByXPath(x.SourceField.GetFieldNamePath(true), sourceNs).Count);
                
                for (int i = 0; i < maxCount; i++)
                {
                    XNamespace ns = string.Empty;
                    XElement value = new XElement(ns + mapping.TargetField.Field.Name);
                    values.Add(value);
                }

                foreach (FieldMappingInfo childMapping in childFieldMapping)
                {
                    XElement childDefaultValue = childMapping.GetDefaultXmlValue(sourceNs);

                    if (childMapping.SourceFieldFullName == "< new >")
                    {
                        foreach (XElement value in values)
                        {
                            value.Add(childDefaultValue);
                        }
                    }
                    else
                    {
                        List<XElement> children = GetSourceMappedValues(childMapping, root, sourceNs);
                        int i = 0;
                        foreach (XElement child in children)
                        {
                            XElement value = values[i];
                            value.Add(child);
                            i++;
                        }
                    }
                }
            }
            else
            {
                XElement defaultValue = mapping.GetDefaultXmlValue(sourceNs);

                if (mapping.SourceFieldFullName == "< new >")
                {
                    values = defaultValue != null ? new List<XElement> { defaultValue } : new List<XElement>();
                }
                else
                {
                    values = root.GetListByXPath(mapping.SourceField.GetFieldNamePath(true), sourceNs);

                    if (values != null && values.Count > 0)
                    {
                        values = values.Select(x => x.Clone(mapping.TargetField.Field.Name)).ToList();
                    }
                    else
                    {
                        values = defaultValue != null ? new List<XElement> { defaultValue } : new List<XElement>();
                    }
                }
            }
            
            return values;
        }

        public static XElement EmbeddedSchemaToComponentLink(this XElement sourceElement, ComponentLinkFieldDefinitionData targetField, string sourceTcmId, List<FieldMappingInfo> childFieldMapping, string targetFolderUri, int index, List<ResultInfo> results)
        {
            if (sourceElement == null || targetField == null)
                return null;

            if (!targetField.AllowedTargetSchemas.Any())
                return null;

            string targetSchemaUri = targetField.AllowedTargetSchemas[0].IdRef;

            SchemaData targetSchema = Client.Read(targetSchemaUri, null) as SchemaData;
            if (targetSchema == null)
                return null;

            List<ItemFieldDefinitionData> targetFields = GetSchemaFields(targetSchemaUri);

            string xml = sourceElement.ToString();

            XNamespace ns = XDocument.Parse(xml).Root.GetDefaultNamespace();

            //get fixed xml
            string newXml = GetFixedContent(xml, ns, sourceTcmId, targetSchema, targetSchema.RootElementName, targetFields, targetFolderUri, childFieldMapping, results);

            if (String.IsNullOrEmpty(newXml))
                return null;

            ComponentData sourceComponent = GetComponent(sourceTcmId);

            string title = String.Format("[{0:00}0] {1}", index, sourceComponent.Title);

            ResultInfo result = SaveComponent(targetSchema, title, newXml, string.Empty, targetFolderUri, false);
            if (result == null)
                return null;

            results.Add(result);

            return GetComponentLink(result.TcmId, title, targetField.Name);
        }

        public static XElement ComponentLinkToEmbeddedSchema(this XElement sourceElement, EmbeddedSchemaFieldDefinitionData targetField, List<FieldMappingInfo> childFieldMapping, string targetFolderUri, List<ResultInfo> results)
        {
            XNamespace ns = "http://www.w3.org/1999/xlink";
            string sourceComponentUri = sourceElement.Attribute(ns + "href").Value;

            ComponentData sourceComponent = Client.Read(sourceComponentUri, new ReadOptions()) as ComponentData;
            if (sourceComponent == null || String.IsNullOrEmpty(sourceComponent.Content))
                return null;

            string sourceSchemaUri = sourceComponent.Schema.IdRef;
            SchemaData sourceSchema = Client.Read(sourceSchemaUri, null) as SchemaData;
            if (sourceSchema == null)
                return null;
           
            string targetEmbeddedSchemaUri = targetField.EmbeddedSchema.IdRef;
            SchemaData targetEmbeddedSchema = Client.Read(targetEmbeddedSchemaUri, null) as SchemaData;
            if (targetEmbeddedSchema == null)
                return null;

            List<ItemFieldDefinitionData> targetEmbeddedSchemaFields = GetSchemaFields(targetEmbeddedSchemaUri);

            string xml = sourceComponent.Content;

            //get fixed xml
            string newXml = GetFixedContent(xml, sourceSchema.NamespaceUri, sourceComponentUri, targetEmbeddedSchema, targetField.Name, targetEmbeddedSchemaFields, targetFolderUri, childFieldMapping, results);

            if (String.IsNullOrEmpty(newXml))
                return null;

            return XElement.Parse(newXml);
        }

        public static List<ComponentFieldData> GetValues(XNamespace schemaNs, List<ItemFieldDefinitionData> schemaFields, XElement parent)
        {
            List<ComponentFieldData> res = new List<ComponentFieldData>();

            if (schemaFields == null)
                return res;
            
            foreach (ItemFieldDefinitionData field in schemaFields)
            {
                if (!String.IsNullOrEmpty(field.Name) && parent.Element(schemaNs + field.Name) != null)
                {
                    List<XElement> elements = parent.Elements(schemaNs + field.Name).ToList();

                    ComponentFieldData item = new ComponentFieldData();
                    item.SchemaField = field;

                    item.Value = !field.IsMultiValue() ? elements.FirstOrDefault().GetComponentFieldData(field) : elements.Select(x => x.GetComponentFieldData(field)).ToList();

                    res.Add(item);
                }
            }

            return res;
        }

        public static List<ComponentFieldData> GetValues(XNamespace schemaNs, List<ItemFieldDefinitionData> schemaFields, string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            if (doc.Root == null)
                return null;

            List<ComponentFieldData> res = new List<ComponentFieldData>();

            foreach (ItemFieldDefinitionData field in schemaFields)
            {
                if (!String.IsNullOrEmpty(field.Name) && doc.Root.Element(schemaNs + field.Name) != null)
                {
                    List<XElement> elements = doc.Root.Elements(schemaNs + field.Name).ToList();

                    ComponentFieldData item = new ComponentFieldData();
                    item.SchemaField = field;

                    item.Value = !field.IsMultiValue() ? elements.FirstOrDefault().GetComponentFieldData(field) : elements.Select(x => x.GetComponentFieldData(field)).ToList();

                    res.Add(item);
                }
            }

            return res;
        }

        public static List<ComponentFieldData> GetValues(DataRow dataRow)
        {
            List<ComponentFieldData> res = new List<ComponentFieldData>();

            foreach (DataColumn col in dataRow.Table.Columns)
            {
                if (!String.IsNullOrEmpty(col.ColumnName) && dataRow[col.ColumnName] != null)
                {
                    ComponentFieldData item = new ComponentFieldData();
                    item.SchemaField = new ItemFieldDefinitionData();
                    item.SchemaField.Name = col.ColumnName;
                    item.Value = dataRow[col.ColumnName];

                    res.Add(item);
                }
            }

            return res;
        }

        public static XElement ToXhtml(string html, string rootName)
        {
            XElement x = XElement.Parse(string.Format("<{0}>{1}</{0}>", rootName, HttpUtility.HtmlDecode(html)));

            XNamespace ns = string.Empty;
            XElement res = new XElement(ns + rootName);

            XNamespace nsXhtml = "http://www.w3.org/1999/xhtml";

            if (x.Elements().Any())
            {
                foreach (XElement p in x.Elements())
                {
                    res.Add(new XElement(nsXhtml + p.Name.LocalName, p.Nodes(), p.Attributes()));
                }
            }
            else
            {
                res.Add(x.Value);    
            }

            return res;
        }

        public static string ToXml(this DataRow dataRow)
        {
            XElement root = new XElement(dataRow.Table.TableName);

            foreach (DataColumn column in dataRow.Table.Columns)
            {
                object value = dataRow[column.ColumnName];

                if (dataRow.Table.Columns[column.ColumnName].DataType == typeof(string) && value.ToString().IsHtml())
                {
                    try
                    {
                        value = ToXhtml(value.ToString(), column.ColumnName);
                        root.Add(value);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    root.Add(new XElement(column.ColumnName, value));
                }
            }

            return root.ToString();
        }

        public static List<ComponentFieldData> GetFixedValues(string sourceXml, XNamespace sourceNs, string sourceTcmId, SchemaData targetSchema, string targetRootElementName, List<ItemFieldDefinitionData> targetFields, string targetFolderUri, List<FieldMappingInfo> fieldMapping, List<ResultInfo> results)
        {
            if (fieldMapping == null || string.IsNullOrEmpty(sourceXml))
                return null;

            XDocument doc = XDocument.Parse(sourceXml);
            if (doc.Root == null)
                return null;

            List<ComponentFieldData> res = new List<ComponentFieldData>();

            foreach (ItemFieldDefinitionData targetField in targetFields)
            {
                FieldMappingInfo mapping = fieldMapping.FirstOrDefault(x => x.TargetField != null && x.TargetField.Field.Name == targetField.Name && x.TargetField.Field.GetFieldType() == targetField.GetFieldType());
                if (mapping == null)
                    continue;

                ComponentFieldData item = new ComponentFieldData();
                item.SchemaField = targetField;

                ItemFieldDefinitionData sourceField = mapping.SourceField.Field;

                if (sourceField == null)
                    continue;

                //construct Component Link to the source component
                if (mapping.SourceField.GetFieldFullName() == "< this component link >")
                {
                    ComponentData component = GetComponent(sourceTcmId);
                    item.Value = GetComponentLink(sourceTcmId, component.Title, targetField.Name);
                }

                //construct new Embedded Schema
                else if (mapping.SourceField.GetFieldFullName() == "< new >")
                {
                    //create XElement if embedded schema, for primitive types default values might be used
                    if (mapping.TargetField.Field.IsEmbedded())
                    {
                        if (!targetField.IsMultiValue())
                        {
                            item.Value = GetSourceMappedValue(mapping, doc.Root, sourceNs);
                        }
                        else
                        {
                            item.Value = GetSourceMappedValues(mapping, doc.Root, sourceNs);
                        }
                    }
                }

                else if (!String.IsNullOrEmpty(sourceField.Name) && doc.Root.Element(sourceNs + sourceField.Name) != null)
                {
                    List<XElement> elements = doc.Root.Elements(sourceNs + sourceField.Name).ToList();

                    //transform Embedded Schema into Component Link
                    if (mapping.SourceField.Field.IsEmbedded() && mapping.TargetField.Field.IsComponentLink())
                    {
                        if (!targetField.IsMultiValue())
                        {
                            item.Value = elements.FirstOrDefault().EmbeddedSchemaToComponentLink(targetField as ComponentLinkFieldDefinitionData, sourceTcmId, mapping.ChildFieldMapping, targetFolderUri, 0, results);
                        }
                        else
                        {
                            List<XElement> values = new List<XElement>();
                            int index = 0;
                            foreach (XElement element in elements)
                            {
                                index ++;
                                values.Add(element.EmbeddedSchemaToComponentLink(targetField as ComponentLinkFieldDefinitionData, sourceTcmId, mapping.ChildFieldMapping, targetFolderUri, index, results));
                            }
                            item.Value = values;
                        }
                    }

                    //transform Component Link into Embedded Schema
                    else if (mapping.SourceField.Field.IsComponentLink() && mapping.TargetField.Field.IsEmbedded())
                    {
                        if (!targetField.IsMultiValue())
                        {
                            item.Value = (elements.FirstOrDefault().GetComponentFieldData(sourceField) as XElement).ComponentLinkToEmbeddedSchema(targetField as EmbeddedSchemaFieldDefinitionData, mapping.ChildFieldMapping, targetFolderUri, results);
                        }
                        else
                        {
                            item.Value = elements.Select(x => (x.GetComponentFieldData(sourceField) as XElement).ComponentLinkToEmbeddedSchema(targetField as EmbeddedSchemaFieldDefinitionData, mapping.ChildFieldMapping, targetFolderUri, results)).ToList();
                        }
                    }

                    //Embedded Schema or any mapped XElement
                    else
                    {
                        if (!targetField.IsMultiValue())
                        {
                            item.Value = GetSourceMappedValue(mapping, doc.Root, sourceNs);
                        }
                        else
                        {
                            item.Value = GetSourceMappedValues(mapping, doc.Root, sourceNs);
                        }
                    }
                }

                //construct primitive or emebedded from default values
                else
                {
                    if (!targetField.IsMultiValue())
                    {
                        item.Value = GetSourceMappedValue(mapping, doc.Root, sourceNs);
                    }
                    else
                    {
                        item.Value = GetSourceMappedValues(mapping, doc.Root, sourceNs);
                    }
                }

                if (item.Value == null && !string.IsNullOrEmpty(mapping.DefaultValue))
                {
                    item.Value = GetDefaultValue(mapping);
                }

                if (item.Value != null)
                {
                    res.Add(item);
                }
            }

            return res;
        }

        private static string GetFixedContent(string sourceXml, XNamespace sourceNs, string sourceTcmId, SchemaData targetSchema, string targetRootElementName, List<ItemFieldDefinitionData> targetFields, string targetFolderUri, List<FieldMappingInfo> fieldMapping, List<ResultInfo> results)
        {
            if (targetFields == null || targetFields.Count == 0)
                return string.Empty;

            if (results == null)
                results = new List<ResultInfo>();
            
            if (String.IsNullOrEmpty(targetRootElementName))
                targetRootElementName = targetSchema.RootElementName;

            if (fieldMapping == null)
                fieldMapping = GetDefaultFieldMapping(targetFields, targetSchema.Id);

            List<ComponentFieldData> fixedValues = GetFixedValues(sourceXml, sourceNs, sourceTcmId, targetSchema, targetRootElementName, targetFields, targetFolderUri, fieldMapping, results);

            if (fixedValues == null || fixedValues.Count == 0)
                return string.Format("<{0} xmlns=\"{1}\" />", targetRootElementName, targetSchema.NamespaceUri);

            bool success = true;

            //check mandatory and empty items
            foreach (ItemFieldDefinitionData schemaField in targetFields)
            {
                if (schemaField.IsMandatory())
                {
                    //stop processing and show message if component contains mandatory empty field
                    ComponentFieldData componentFieldDataValue = fixedValues.FirstOrDefault(x => x.SchemaField.Name == schemaField.Name);
                    if (componentFieldDataValue == null || componentFieldDataValue.Value == null)
                    {
                        success = false;

                        RepositoryLocalObjectData itemData = ReadItem(sourceTcmId) as RepositoryLocalObjectData;

                        ResultInfo result = new ResultInfo();
                        result.Item = itemData.ToItem();
                        result.Status = Status.Error;
                        result.Message = String.Format("Item \"{0}\" contains mandatory empty fields. Please change mapping.", itemData == null ? sourceTcmId : itemData.GetWebDav().CutPath("/", 90, true));
                        results.Add(result);
                    }
                }
            }
            if (!success)
                return string.Format("<{0} xmlns=\"{1}\" />", targetRootElementName, targetSchema.NamespaceUri);

            string res = GetComponentXml(targetSchema.NamespaceUri, targetRootElementName, fixedValues).ToString();

            //replace to local publication ids
            List<string> ids = Regex.Matches(res, "tcm:(\\d)+-(\\d)+(-(\\d)+)?").Cast<Match>().Select(x => x.Value).ToList();
            foreach (string id in ids)
            {
                string newId = Regex.Replace(id, "tcm:(\\d)+-", targetSchema.Id.Split('-')[0] + "-");
                if (ExistsItem(newId))
                {
                    res = res.Replace(id, newId);
                }
                else
                {
                    RepositoryLocalObjectData itemData = ReadItem(id) as RepositoryLocalObjectData;

                    ResultInfo result = new ResultInfo();
                    result.Item = itemData.ToItem();
                    result.Status = Status.Error;
                    result.Message = String.Format("Item \"{0}\" doesn't exist in target publication", itemData == null ? id : itemData.GetWebDav().CutPath("/", 90, true));
                    results.Add(result);

                    throw new Exception(result.Message);
                }
            }

            //clear unnecessary namespaces
            res = res.Replace(String.Format("xmlns=\"{0}\"", sourceNs), String.Format("xmlns=\"{0}\"", targetSchema.NamespaceUri));
            res = res.Replace(" xmlns=\"\"", string.Empty);
            res = res.Replace(String.Format(" xmlns=\"{0}\"", targetSchema.NamespaceUri), string.Empty);
            res = res.Replace(String.Format("<{0}", targetRootElementName), String.Format("<{0} xmlns=\"{1}\"", targetRootElementName, targetSchema.NamespaceUri));
            
            return res;
        }

        private static string GetFixedContent(string sourceXml, string sourceMetadataXml, SchemaData sourceSchema, List<ItemFieldDefinitionData> sourceComponentFields, List<ItemFieldDefinitionData> sourceMetadataFields, string sourceTcmId, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string targetFolderUri, CustomTransformerInfo customTransformer, List<ResultInfo> results)
        {
            if (targetComponentFields == null || targetComponentFields.Count == 0)
                return string.Empty;

            if (customTransformer == null)
                return string.Empty;

            if(results == null)
                results = new List<ResultInfo>();

            try
            {
                Type type = Type.GetType(customTransformer.AssemblyQualifiedName);
                if (type != null)
                {
                    ICustomTransformer transformer = (ICustomTransformer)Activator.CreateInstance(type);
                    return transformer.GetFixedContent(sourceSchema, sourceComponentFields, sourceMetadataFields, sourceXml, sourceMetadataXml, sourceTcmId, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, results);
                }
            }
            catch (Exception ex)
            {
                RepositoryLocalObjectData itemData = GetComponent(sourceTcmId);

                ResultInfo result = new ResultInfo();
                result.Item = itemData.ToItem();
                result.Status = Status.Error;
                result.Message = String.Format("Item \"{0}\" contains broken data.", itemData == null ? sourceTcmId : itemData.GetWebDav().CutPath("/", 90, true));
                result.StackTrace = ex.StackTrace;
                results.Add(result);
            }

            return string.Empty;
        }

        private static string GetFixedContent(string sourceTable, DataRow sourceDataRow, SchemaData targetSchema, string targetRootElementName, List<ItemFieldDefinitionData> targetFields, List<FieldMappingInfo> fieldMapping, List<ResultInfo> results)
        {
            if (targetFields == null || targetFields.Count == 0)
                return string.Empty;

            if (results == null)
                results = new List<ResultInfo>();

            if (String.IsNullOrEmpty(targetRootElementName))
                targetRootElementName = targetSchema.RootElementName;

            if (fieldMapping == null)
                fieldMapping = GetDefaultFieldMapping(targetFields, targetSchema.Id);

            List<ComponentFieldData> fixedValues = GetFixedValues(sourceDataRow.ToXml(), string.Empty, string.Empty, targetSchema, targetRootElementName, targetFields, string.Empty, fieldMapping, results);

            if (fixedValues == null || fixedValues.Count == 0)
                return string.Format("<{0} xmlns=\"{1}\" />", targetSchema.RootElementName, targetSchema.NamespaceUri);

            bool success = true;

            //check mandatory and empty items
            foreach (ItemFieldDefinitionData schemaField in targetFields)
            {
                if (schemaField.IsMandatory())
                {
                    //stop processing and show message if component contains mandatory empty field
                    ComponentFieldData componentFieldDataValue = fixedValues.FirstOrDefault(x => x.SchemaField.Name == schemaField.Name);
                    if (componentFieldDataValue == null || componentFieldDataValue.Value == null)
                    {
                        success = false;

                        ResultInfo result = new ResultInfo();
                        result.Status = Status.Error;
                        result.Item = new ItemInfo { ItemType = ItemType.Component };
                        result.Message = String.Format("Table \"{0}\" contains mandatory empty field \"{1}\". Please change mapping.", sourceTable, schemaField.Name);
                        results.Add(result);
                    }
                }
            }
            if (!success)
                return string.Format("<{0} xmlns=\"{1}\" />", targetSchema.RootElementName, targetSchema.NamespaceUri);

            string res = GetComponentXml(targetSchema.NamespaceUri, targetRootElementName, fixedValues).ToString();

            //clear unnecessary namespaces
            res = res.Replace(" xmlns=\"\"", string.Empty);
            res = res.Replace(String.Format(" xmlns=\"{0}\"", targetSchema.NamespaceUri), string.Empty);
            res = res.Replace(String.Format("<{0}", targetRootElementName), String.Format("<{0} xmlns=\"{1}\"", targetRootElementName, targetSchema.NamespaceUri));

            return res;
        }

        private static string GetFixedContent(string sourceTable, DataRow sourceDataRow, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string targetFolderUri, CustomTransformerInfo customImporter, List<ResultInfo> results)
        {
            if (targetComponentFields == null || targetComponentFields.Count == 0)
                return string.Empty;

            if (customImporter == null)
                return string.Empty;

            if (results == null)
                results = new List<ResultInfo>();

            try
            {
                Type type = Type.GetType(customImporter.AssemblyQualifiedName);
                if (type != null)
                {
                    ICustomImporter importer = (ICustomImporter)Activator.CreateInstance(type);
                    return importer.GetContent(sourceTable, sourceDataRow, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, results);
                }
            }
            catch (Exception ex)
            {
                ResultInfo result = new ResultInfo();
                result.Status = Status.Error;
                result.Item = new ItemInfo { ItemType = ItemType.Component };
                result.Message = String.Format("Table \"{0}\" contains wrong data.", sourceTable);
                result.StackTrace = ex.StackTrace;
                results.Add(result);
            }

            return string.Empty;
        }

        private static ResultInfo SaveComponent(SchemaData schema, string title, string contentXml, string metadataXml, string folderUri, bool localize)
        {
            ResultInfo result = new ResultInfo();

            if (String.IsNullOrEmpty(title))
            {
                result.Status = Status.Error;
                result.Message = "Component title is not defined";
            }

            //check existing item
            List<ItemInfo> targetFolderItems = GetItemsByParentContainer(folderUri);
            if (targetFolderItems.All(x => x.Title != title))
            {
                //create new component
                try
                {
                    ComponentData component = new ComponentData
                    {
                        Title = title,
                        LocationInfo = new LocationInfo { OrganizationalItem = new LinkToOrganizationalItemData { IdRef = folderUri } },
                        Id = "tcm:0-0-0",
                        Schema = new LinkToSchemaData { IdRef = schema.Id },
                        Content = contentXml,
                        Metadata = metadataXml,
                        IsBasedOnMandatorySchema = false,
                        IsBasedOnTridionWebSchema = true,
                        ApprovalStatus = new LinkToApprovalStatusData { IdRef = "tcm:0-0-0" }
                    };

                    component = (ComponentData)Client.Save(component, new ReadOptions());
                    string componentUri = Client.CheckIn(component.Id, new ReadOptions()).Id;
                    component = GetComponent(componentUri);

                    result.Item = component.ToItem();
                    result.Status = Status.Success;
                    result.Message = String.Format("Component \"{0}\" was created", component.GetWebDav().CutPath("/", 80, true));
                }
                catch (Exception ex)
                {
                    result.Status = Status.Error;
                    result.Item = new ItemInfo { ItemType = ItemType.Component };
                    result.StackTrace = ex.StackTrace;
                    result.Message = String.Format("Error creating component \"{0}\"", title);
                }
            }
            else
            {
                //update existing component
                string componentUri = targetFolderItems.First(x => x.Title == title).TcmId;

                ComponentData component = GetComponent(componentUri);

                //only component of same name and title
                if (component != null && component.Schema.IdRef.GetId() == schema.Id.GetId())
                {
                    if ((component.Content.PrettyXml() == contentXml.PrettyXml() && component.Metadata.PrettyXml() == metadataXml.PrettyXml()))
                        return null;

                    //localize if item is shared
                    if (component.BluePrintInfo.IsShared == true)
                    {
                        if (localize)
                        {
                            Localize(component.ToItem());
                        }
                        else
                        {
                            componentUri = GetBluePrintTopTcmId(componentUri);
                        }
                    }

                    result.Item = component.ToItem();

                    try
                    {
                        component = (ComponentData)Client.CheckOut(componentUri, true, new ReadOptions());
                    }
                    catch
                    {
                    }

                    component.Content = contentXml;
                    component.Metadata = metadataXml;

                    try
                    {
                        Client.Update(component, new ReadOptions());
                        Client.CheckIn(componentUri, new ReadOptions());

                        result.Status = Status.Success;
                        result.Message = String.Format("Updated component \"{0}\"", component.GetWebDav().CutPath("/", 80, true));
                    }
                    catch (Exception ex)
                    {
                        Client.UndoCheckOut(componentUri, true, new ReadOptions());

                        result.Status = Status.Error;
                        result.Item = new ItemInfo { ItemType = ItemType.Component, TcmId = componentUri };
                        result.StackTrace = ex.StackTrace;
                        result.Message = String.Format("Error updating component \"{0}\"", component.GetWebDav().CutPath("/", 80, true));
                    }
                }
                else
                {
                    result.Status = Status.Error;
                    result.Item = new ItemInfo { ItemType = ItemType.Component, TcmId = componentUri };
                    result.Message = String.Format("Error updating component \"{0}\"", title);
                }
            }

            return result;
        }

        private static ResultInfo SaveTridionObjectMetadata(SchemaData metadataSchema, string title, string metadataXml, string containerUri, bool localize)
        {
            //check existing item
            List<ItemInfo> targetContainerItems = GetItemsByParentContainer(containerUri);
            if (targetContainerItems.All(x => x.Title != title))
                return null;

            //update existing tridionObject
            string tridionObjectUri = targetContainerItems.First(x => x.Title == title).TcmId;

            ResultInfo result = new ResultInfo();

            RepositoryLocalObjectData tridionObject = ReadItem(tridionObjectUri) as RepositoryLocalObjectData;
            result.Item = tridionObject.ToItem();

            //only tridionObject of same name and title
            if (tridionObject != null && tridionObject.MetadataSchema.IdRef.GetId() == metadataSchema.Id.GetId())
            {
                if ((tridionObject.Metadata.PrettyXml() == metadataXml.PrettyXml()))
                    return null;

                //localize if item is shared
                if (tridionObject.BluePrintInfo.IsShared == true)
                {
                    if (localize)
                    {
                        Localize(tridionObject.ToItem());
                    }
                    else
                    {
                        tridionObjectUri = GetBluePrintTopTcmId(tridionObjectUri);
                    }
                }

                try
                {
                    tridionObject = Client.CheckOut(tridionObjectUri, true, new ReadOptions());
                }
                catch
                {
                }

                tridionObject.Metadata = metadataXml;

                try
                {
                    Client.Update(tridionObject, new ReadOptions());
                    Client.CheckIn(tridionObjectUri, new ReadOptions());

                    result.Status = Status.Success;
                    result.Message = String.Format("Updated item \"{0}\"", tridionObject.GetWebDav().CutPath("/", 80, true));
                }
                catch (Exception ex)
                {
                    Client.UndoCheckOut(tridionObjectUri, true, new ReadOptions());

                    result.Status = Status.Error;
                    result.Item = new ItemInfo { ItemType = GetItemType(tridionObjectUri), TcmId = tridionObjectUri };
                    result.StackTrace = ex.StackTrace;
                    result.Message = String.Format("Error updating item \"{0}\"", tridionObject.GetWebDav().CutPath("/", 80, true));
                }
            }
            else
            {
                result.Status = Status.Error;
                result.Item = new ItemInfo { ItemType = GetItemType(tridionObjectUri), TcmId = tridionObjectUri };
                result.Message = String.Format("Error updating page \"{0}\"", title);
            }

            return result;
        }

        public static void ChangeSchemaForComponent(string componentUri, string sourceSchemaUri, string targetSchemaUri, string targetFolderUri, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            sourceSchemaUri = sourceSchemaUri.GetCurrentVersionTcmId();

            // Open up the source component schema 
            SchemaData sourceSchema = Client.Read(sourceSchemaUri, null) as SchemaData;
            if (sourceSchema == null)
                return;

            List<ItemFieldDefinitionData> sourceComponentFields = GetSchemaFields(sourceSchemaUri);
            List<ItemFieldDefinitionData> sourceMetadataFields = GetSchemaMetadataFields(sourceSchemaUri);

            targetSchemaUri = targetSchemaUri.GetCurrentVersionTcmId();

            // Open up the target component schema
            SchemaData targetSchema = Client.Read(targetSchemaUri, null) as SchemaData;
            if (targetSchema == null)
                return;

            List<ItemFieldDefinitionData> targetComponentFields = GetSchemaFields(targetSchemaUri);
            List<ItemFieldDefinitionData> targetMetadataFields = GetSchemaMetadataFields(targetSchemaUri);

            // Change schema for component
            ChangeSchemaForComponent(componentUri, sourceSchema, sourceComponentFields, sourceMetadataFields, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, localize, historyMapping, customComponentTransformer, customMetadataTransformer, results);
        }

        private static void ChangeSchemaForComponent(string componentUri, SchemaData sourceSchema, List<ItemFieldDefinitionData> sourceComponentFields, List<ItemFieldDefinitionData> sourceMetadataFields, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string targetFolderUri, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            if (String.IsNullOrEmpty(componentUri))
                return;

            if (results == null)
                results = new List<ResultInfo>();

            ComponentData component = GetComponent(componentUri);
            if (component == null)
                return;

            if (!component.Schema.IdRef.GetId().Equals(sourceSchema.Id.GetId()))
            {
                // If the component is not of the schmea that we want to change from, do nothing...
                return;
            }

            if (component.Schema.IdRef.GetId().Equals(targetSchema.Id.GetId()))
            {
                // If the component already has this schema, don't do anything.
                return;
            }

            //detect mapping by date
            List<FieldMappingInfo> fieldMapping = GetDetectedMapping(historyMapping, component.VersionInfo.RevisionDate);

            ResultInfo result = new ResultInfo();
            result.Item = component.ToItem();

            //get fixed xml
            string newContent = customComponentTransformer == null ?
                GetFixedContent(component.Content, sourceSchema.NamespaceUri, componentUri, targetSchema, targetSchema.RootElementName, targetComponentFields, targetFolderUri, fieldMapping, results) :
                GetFixedContent(component.Content, component.Metadata, sourceSchema, sourceComponentFields, sourceMetadataFields, componentUri, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, customComponentTransformer, results);

            //get fixed metadata
            string newMetadata = customMetadataTransformer == null ?
                GetFixedContent(component.Metadata, sourceSchema.NamespaceUri, componentUri, targetSchema, "Metadata", targetMetadataFields, targetFolderUri, fieldMapping, results) :
                GetFixedContent(component.Content, component.Metadata, sourceSchema, sourceComponentFields, sourceMetadataFields, componentUri, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, customMetadataTransformer, results);

            if (String.IsNullOrEmpty(newContent) && String.IsNullOrEmpty(newMetadata))
                return;

            //localize if item is shared
            if (component.BluePrintInfo.IsShared == true)
            {
                if (localize)
                {
                    Localize(component.ToItem());
                }
                else
                {
                    componentUri = GetBluePrintTopTcmId(componentUri);
                }
            }

            component = Client.TryCheckOut(componentUri, new ReadOptions()) as ComponentData;

            if (component != null && component.IsEditable == true)
            {
                try
                {
                    //rebild component xml
                    component.Content = newContent;

                    //rebuild metadata
                    component.Metadata = newMetadata;

                    //change schema id
                    component.Schema.IdRef = targetSchema.Id;

                    Client.Save(component, new ReadOptions());
                    Client.CheckIn(componentUri, new ReadOptions());

                    result.Status = Status.Success;
                    result.Message = String.Format("Changed schema for \"{0}\"", component.GetWebDav().CutPath("/", 80, true));
                }
                catch (Exception ex)
                {
                    Client.UndoCheckOut(componentUri, true, new ReadOptions());

                    result.Status = Status.Error;
                    result.Item = new ItemInfo {ItemType = ItemType.Component, TcmId = componentUri};
                    result.StackTrace = ex.StackTrace;
                    result.Message = String.Format("Error for \"{0}\"", component.GetWebDav().CutPath("/", 90, true));
                }
            }
            else
            {
                Client.UndoCheckOut(componentUri, true, new ReadOptions());

                result.Status = Status.Error;
                result.Item = new ItemInfo { ItemType = ItemType.Component, TcmId = componentUri };
                result.Message = String.Format("Error for \"{0}\"", component.GetWebDav().CutPath("/", 90, true));
            }

            results.Add(result);
        }

        public static void ChangeSchemasForComponentsInFolder(string folderUri, string sourceSchemaUri, string targetFolderUri, string targetSchemaUri, List<Criteria> criterias, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            sourceSchemaUri = sourceSchemaUri.GetCurrentVersionTcmId();

            // Open up the source component schema 
            SchemaData sourceSchema = Client.Read(sourceSchemaUri, null) as SchemaData;
            if (sourceSchema == null)
                return;

            List<ItemFieldDefinitionData> sourceComponentFields = GetSchemaFields(sourceSchemaUri);
            List<ItemFieldDefinitionData> sourceMetadataFields = GetSchemaMetadataFields(sourceSchemaUri);

            targetSchemaUri = targetSchemaUri.GetCurrentVersionTcmId();

            // Open up the target component schema
            SchemaData targetSchema = Client.Read(targetSchemaUri, null) as SchemaData;
            if (targetSchema == null)
                return;

            List<ItemFieldDefinitionData> targetComponentFields = GetSchemaFields(targetSchemaUri);
            List<ItemFieldDefinitionData> targetMetadataFields = GetSchemaMetadataFields(targetSchemaUri);

            // Change container folder linked schema
            ChangeFolderLinkedSchema(folderUri, sourceSchemaUri, targetSchemaUri, results);

            // Change inner folder linked schemas
            foreach (ItemInfo item in GetFolders(folderUri, true))
            {
                ChangeFolderLinkedSchema(item.TcmId, sourceSchemaUri, targetSchemaUri, results);
            }

            // Change schema for components
            foreach (ItemInfo item in GetComponentsByCriterias(folderUri, sourceSchemaUri, criterias))
            {
                ChangeSchemaForComponent(item.TcmId, sourceSchema, sourceComponentFields, sourceMetadataFields, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, localize, historyMapping, customComponentTransformer, customMetadataTransformer, results);
            }
        }

        public static void FixComponent(string componentUri, string schemaUri, string targetFolderUri, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            schemaUri = schemaUri.GetCurrentVersionTcmId();

            // Open up the schema
            SchemaData schema = Client.Read(schemaUri, null) as SchemaData;
            if (schema == null)
                return;

            List<ItemFieldDefinitionData> componentFields = GetSchemaFields(schemaUri);
            List<ItemFieldDefinitionData> metadataFields = GetSchemaMetadataFields(schemaUri);

            // Fix component
            FixComponent(componentUri, schema, componentFields, metadataFields, targetFolderUri, localize, historyMapping, customComponentTransformer, customMetadataTransformer, results);
        }

        private static void FixComponent(string componentUri, SchemaData schema, List<ItemFieldDefinitionData> componentFields, List<ItemFieldDefinitionData> metadataFields, string targetFolderUri, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            if (String.IsNullOrEmpty(componentUri))
                return;

            if (results == null)
                results = new List<ResultInfo>();

            ComponentData component = GetComponent(componentUri);
            if (component == null || String.IsNullOrEmpty(component.Content))
                return;

            if (!component.Schema.IdRef.GetId().Equals(schema.Id.GetId()))
            {
                // If the component is not of the schema, do nothing...
                return;
            }

            //detect mapping by date
            List<FieldMappingInfo> fieldMapping = GetDetectedMapping(historyMapping, component.VersionInfo.RevisionDate);

            //get fixed xml
            string newContent = customComponentTransformer == null ?
                GetFixedContent(component.Content, schema.NamespaceUri, componentUri, schema, schema.RootElementName, componentFields, targetFolderUri, fieldMapping, results) :
                GetFixedContent(component.Content, component.Metadata, schema, componentFields, metadataFields, componentUri, schema, componentFields, metadataFields, targetFolderUri, customComponentTransformer, results);

            //get fixed metadata
            string newMetadata = customMetadataTransformer == null ?
                GetFixedContent(component.Metadata, schema.NamespaceUri, componentUri, schema, "Metadata", metadataFields, targetFolderUri, fieldMapping, results) :
                GetFixedContent(component.Content, component.Metadata, schema, componentFields, metadataFields, componentUri, schema, componentFields, metadataFields, targetFolderUri, customMetadataTransformer, results);

            if (String.IsNullOrEmpty(newContent))
                return;

            if (component.Content.PrettyXml() == newContent.PrettyXml())
                return;

            ResultInfo result = SaveComponent(schema, component.Title, newContent, newMetadata, component.LocationInfo.OrganizationalItem.IdRef, localize);
            if (result != null)
                results.Add(result);
        }

        public static void FixComponentsInFolder(string folderUri, string schemaUri, string targetFolderUri, List<Criteria> criterias, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            schemaUri = schemaUri.GetCurrentVersionTcmId();

            // Open up the schema
            SchemaData schema = Client.Read(schemaUri, null) as SchemaData;
            if (schema == null)
                return;

            List<ItemFieldDefinitionData> componentFields = GetSchemaFields(schemaUri);
            List<ItemFieldDefinitionData> metadataFields = GetSchemaMetadataFields(schemaUri);

            // Fix components
            foreach (ItemInfo item in GetComponentsByCriterias(folderUri, schemaUri, criterias))
            {
                FixComponent(item.TcmId, schema, componentFields, metadataFields, targetFolderUri, localize, historyMapping, customComponentTransformer, customMetadataTransformer, results);
            }
        }

        public static void TransformComponent(string sourceComponentUri, string sourceFolderUri, string sourceSchemaUri, string targetFolderUri, string targetSchemaUri, string formatString, List<ReplacementInfo> replacements, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            if (String.IsNullOrEmpty(sourceComponentUri))
                return;

            sourceSchemaUri = sourceSchemaUri.GetCurrentVersionTcmId();

            // Open up the source component schema 
            SchemaData sourceSchema = Client.Read(sourceSchemaUri, null) as SchemaData;
            if (sourceSchema == null)
                return;

            List<ItemFieldDefinitionData> sourceComponentFields = GetSchemaFields(sourceSchemaUri);
            List<ItemFieldDefinitionData> sourceMetadataFields = GetSchemaMetadataFields(sourceSchemaUri);

            targetSchemaUri = targetSchemaUri.GetCurrentVersionTcmId();

            // Open up the target component schema
            SchemaData targetSchema = Client.Read(targetSchemaUri, null) as SchemaData;
            if (targetSchema == null)
                return;

            List<ItemFieldDefinitionData> targetComponentFields = GetSchemaFields(targetSchemaUri);
            List<ItemFieldDefinitionData> targetMetadataFields = GetSchemaMetadataFields(targetSchemaUri);

            // Change schema for component
            TransformComponent(sourceComponentUri, sourceFolderUri, sourceSchema, sourceComponentFields, sourceMetadataFields, targetFolderUri, targetSchema, targetComponentFields, targetMetadataFields, formatString, replacements, localize, historyMapping, customComponentTransformer, customMetadataTransformer, results);
        }

        private static void TransformComponent(string sourceComponentUri, string sourceFolderUri, SchemaData sourceSchema, List<ItemFieldDefinitionData> sourceComponentFields, List<ItemFieldDefinitionData> sourceMetadataFields, string targetFolderUri, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string formatString, List<ReplacementInfo> replacements, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            if (String.IsNullOrEmpty(sourceComponentUri))
                return;

            if (results == null)
                results = new List<ResultInfo>();

            ComponentData component = GetComponent(sourceComponentUri);
            if (component == null || String.IsNullOrEmpty(component.Content))
                return;

            if (!component.Schema.IdRef.GetId().Equals(sourceSchema.Id.GetId()))
            {
                // If the component is not of the schema, do nothing...
                return;
            }

            //detect mapping by date or get current schema mapping
            List<FieldMappingInfo> fieldMapping = historyMapping.GetDetectedMapping(component.VersionInfo.RevisionDate);

            // create folder chain
            if (!String.IsNullOrEmpty(sourceFolderUri))
            {
                FolderData sourceFolder = Client.Read(sourceFolderUri, new ReadOptions()) as FolderData;
                if (sourceFolder != null)
                {
                    List<string> componentWebDavChain = component.GetWebDavChain();
                    List<string> folderChain = componentWebDavChain.Take(componentWebDavChain.Count - 1).ToList().Substract(sourceFolder.GetWebDavChain());
                    targetFolderUri = CreateFolderChain(folderChain, targetFolderUri);
                }
            }

            //get fixed xml
            string newContent = customComponentTransformer == null ?
                GetFixedContent(component.Content, sourceSchema.NamespaceUri, sourceComponentUri, targetSchema, targetSchema.RootElementName, targetComponentFields, targetFolderUri, fieldMapping, results) :
                GetFixedContent(component.Content, component.Metadata, sourceSchema, sourceComponentFields, sourceMetadataFields, sourceComponentUri, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, customComponentTransformer, results);

            //get fixed metadata
            string newMetadata = customMetadataTransformer == null ?
                GetFixedContent(component.Metadata, sourceSchema.NamespaceUri, sourceComponentUri, targetSchema, "Metadata", targetMetadataFields, targetFolderUri, fieldMapping, results) :
                GetFixedContent(component.Content, component.Metadata, sourceSchema, sourceComponentFields, sourceMetadataFields, sourceComponentUri, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, customMetadataTransformer, results);

            if (String.IsNullOrEmpty(newContent))
                return;

            List<ComponentFieldData> sourceValues = GetValues(sourceSchema.NamespaceUri, sourceComponentFields, component.Content);
            List<ComponentFieldData> metadataValues = GetValues(sourceSchema.NamespaceUri, sourceMetadataFields, component.Metadata);
            
            string newTitle = GetTransformedName(component.Title, sourceComponentUri, sourceValues, metadataValues, formatString, replacements);

            ResultInfo result = SaveComponent(targetSchema, newTitle, newContent, newMetadata, targetFolderUri, localize);
            if (result != null)
                results.Add(result);

            FieldMappingInfo targetComponentLinkMapping = fieldMapping.FirstOrDefault(x => x.TargetFieldFullName == "< target component link >" && x.SourceField != null && x.SourceField.Field != null && !String.IsNullOrEmpty(x.SourceField.Field.Name) && !x.SourceField.Field.Name.StartsWith("<"));
            ComponentLinkFieldDefinitionData targetComponentLink = targetComponentLinkMapping == null ? null : targetComponentLinkMapping.SourceField.Field as ComponentLinkFieldDefinitionData;

            // save component link back to source component
            if (result != null && result.Status == Status.Success && !String.IsNullOrEmpty(result.TcmId) && targetComponentLink != null)
            {
                string pubId = GetPublicationTcmId(component.Id);
                string linkId = GetBluePrintItemTcmId(result.TcmId, pubId);
                XElement cl = GetComponentLink(linkId, newTitle, targetComponentLink.Name);

                ComponentFieldData sourceValue = sourceValues.FirstOrDefault(x => x.SchemaField.Name == targetComponentLink.Name && x.SchemaField.GetFieldType() == targetComponentLink.GetFieldType());
                if (sourceValue == null && !targetComponentLinkMapping.SourceField.IsMeta)
                {
                    sourceValue = new ComponentFieldData();
                    sourceValue.SchemaField = targetComponentLink;
                    sourceValue.Value = cl;
                    sourceValues.Add(sourceValue);
                }

                string newSourceContent = GetComponentXml(sourceSchema.NamespaceUri, sourceSchema.RootElementName, sourceValues).ToString();
                newSourceContent = newSourceContent.Replace(" xmlns=\"\"", string.Empty);

                ComponentFieldData metadataValue = metadataValues.FirstOrDefault(x => x.SchemaField.Name == targetComponentLink.Name && x.SchemaField.GetFieldType() == targetComponentLink.GetFieldType());
                if (metadataValue == null && targetComponentLinkMapping.SourceField.IsMeta)
                {
                    metadataValue = new ComponentFieldData();
                    metadataValue.SchemaField = targetComponentLink;
                    metadataValue.Value = cl;
                    metadataValues.Add(metadataValue);
                }

                string newSourceMetadata = string.Empty;
                XElement newXmlSourceMetadata = GetComponentXml(sourceSchema.NamespaceUri, "Metadata", metadataValues);
                if (newXmlSourceMetadata != null)
                {
                    newSourceMetadata = newXmlSourceMetadata.ToString();
                    newSourceMetadata = newSourceMetadata.Replace(" xmlns=\"\"", string.Empty);
                }
                if (newSourceMetadata == string.Empty && sourceMetadataFields != null && sourceMetadataFields.Count > 0)
                {
                    newSourceMetadata = string.Format("<Metadata xmlns=\"{0}\" />", sourceSchema.NamespaceUri);
                }

                ResultInfo result1 = SaveComponent(sourceSchema, component.Title, newSourceContent, newSourceMetadata, component.LocationInfo.OrganizationalItem.IdRef, false);
                if (result1 != null)
                    results.Add(result1);
            }
        }

        public static void TransformComponentsInFolder(string sourceFolderUri, string sourceSchemaUri, string targetFolderUri, string targetSchemaUri, List<Criteria> criterias, string formatString, List<ReplacementInfo> replacements, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            sourceSchemaUri = sourceSchemaUri.GetCurrentVersionTcmId();

            // Open up the source component schema 
            SchemaData sourceSchema = Client.Read(sourceSchemaUri, null) as SchemaData;
            if (sourceSchema == null)
                return;

            List<ItemFieldDefinitionData> sourceComponentFields = GetSchemaFields(sourceSchemaUri);
            List<ItemFieldDefinitionData> sourceMetadataFields = GetSchemaMetadataFields(sourceSchemaUri);

            targetSchemaUri = targetSchemaUri.GetCurrentVersionTcmId();

            // Open up the target component schema
            SchemaData targetSchema = Client.Read(targetSchemaUri, null) as SchemaData;
            if (targetSchema == null)
                return;

            List<ItemFieldDefinitionData> targetComponentFields = GetSchemaFields(targetSchemaUri);
            List<ItemFieldDefinitionData> targetMetadataFields = GetSchemaMetadataFields(targetSchemaUri);

            // Transform components
            foreach (ItemInfo item in GetComponentsByCriterias(sourceFolderUri, sourceSchemaUri, criterias))
            {
                TransformComponent(item.TcmId, sourceFolderUri, sourceSchema, sourceComponentFields, sourceMetadataFields, targetFolderUri, targetSchema, targetComponentFields, targetMetadataFields, formatString, replacements, localize, historyMapping, customComponentTransformer, customMetadataTransformer, results);
            }
        }

        public static void ChangeMetadataSchemaForTridionObject(string sourceTridionObjectUri, string sourceMetadataSchemaUri, string targetContainerUri, string targetMetadataSchemaUri, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            sourceMetadataSchemaUri = sourceMetadataSchemaUri.GetCurrentVersionTcmId();

            // Open up the source metadata schema 
            SchemaData sourceMetadataSchema = Client.Read(sourceMetadataSchemaUri, null) as SchemaData;
            if (sourceMetadataSchema == null)
                return;

            List<ItemFieldDefinitionData> sourceMetadataFields = GetSchemaMetadataFields(sourceMetadataSchemaUri);

            targetMetadataSchemaUri = targetMetadataSchemaUri.GetCurrentVersionTcmId();

            // Open up the target metadata schema
            SchemaData targetMetadataSchema = Client.Read(targetMetadataSchemaUri, null) as SchemaData;
            if (targetMetadataSchema == null)
                return;

            List<ItemFieldDefinitionData> targetMetadataFields = GetSchemaMetadataFields(targetMetadataSchemaUri);

            // Change schema for tridion object
            ChangeMetadataSchemaForTridionObject(sourceTridionObjectUri, sourceMetadataSchema, sourceMetadataFields, targetContainerUri, targetMetadataSchema, targetMetadataFields, localize, historyMapping, customMetadataTransformer, results);
        }

        private static void ChangeMetadataSchemaForTridionObject(string sourceTridionObjectUri, SchemaData sourceMetadataSchema, List<ItemFieldDefinitionData> sourceMetadataFields, string targetContainerUri, SchemaData targetMetadataSchema, List<ItemFieldDefinitionData> targetMetadataFields, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            if (String.IsNullOrEmpty(sourceTridionObjectUri))
                return;

            if (results == null)
                results = new List<ResultInfo>();

            RepositoryLocalObjectData tridionObject = ReadItem(sourceTridionObjectUri) as RepositoryLocalObjectData;
            if (tridionObject == null)
                return;

            if (!tridionObject.MetadataSchema.IdRef.GetId().Equals(sourceMetadataSchema.Id.GetId()))
            {
                // If the object is not of the metadata schema that we want to change from, do nothing...
                return;
            }

            if (tridionObject.MetadataSchema.IdRef.GetId().Equals(targetMetadataSchema.Id.GetId()))
            {
                // If the object already has this metadata schema, don't do anything.
                return;
            }

            //detect mapping by date
            List<FieldMappingInfo> fieldMapping = GetDetectedMapping(historyMapping, tridionObject.VersionInfo.RevisionDate);

            ResultInfo result = new ResultInfo();
            result.Item = tridionObject.ToItem();

            //get fixed metadata
            string newMetadata = customMetadataTransformer == null ?
                GetFixedContent(tridionObject.Metadata, sourceMetadataSchema.NamespaceUri, sourceTridionObjectUri, targetMetadataSchema, "Metadata", targetMetadataFields, targetContainerUri, fieldMapping, results) :
                GetFixedContent(null, tridionObject.Metadata, sourceMetadataSchema, null, sourceMetadataFields, sourceTridionObjectUri, targetMetadataSchema, null, targetMetadataFields, targetContainerUri, customMetadataTransformer, results);

            if (String.IsNullOrEmpty(newMetadata))
                return;

            //localize if item is shared
            if (tridionObject.BluePrintInfo.IsShared == true)
            {
                if (localize)
                {
                    Localize(tridionObject.ToItem());
                }
                else
                {
                    sourceTridionObjectUri = GetBluePrintTopTcmId(sourceTridionObjectUri);
                }
            }

            tridionObject = Client.TryCheckOut(sourceTridionObjectUri, new ReadOptions()) as RepositoryLocalObjectData;

            if (tridionObject != null && tridionObject.IsEditable == true)
            {
                try
                {
                    //rebuild metadata
                    tridionObject.Metadata = newMetadata;

                    //change schema id
                    tridionObject.MetadataSchema.IdRef = targetMetadataSchema.Id;

                    Client.Save(tridionObject, new ReadOptions());
                    Client.CheckIn(sourceTridionObjectUri, new ReadOptions());

                    result.Status = Status.Success;
                    result.Message = String.Format("Changed metadata schema for \"{0}\"", tridionObject.GetWebDav().CutPath("/", 80, true));
                }
                catch (Exception ex)
                {
                    Client.UndoCheckOut(sourceTridionObjectUri, true, new ReadOptions());

                    result.Status = Status.Error;
                    result.Item = new ItemInfo { ItemType = GetItemType(sourceTridionObjectUri), TcmId = sourceTridionObjectUri };
                    result.StackTrace = ex.StackTrace;
                    result.Message = String.Format("Error for \"{0}\"", tridionObject.GetWebDav().CutPath("/", 90, true));
                }
            }
            else
            {
                Client.UndoCheckOut(sourceTridionObjectUri, true, new ReadOptions());

                result.Status = Status.Error;
                result.Item = new ItemInfo { ItemType = GetItemType(sourceTridionObjectUri), TcmId = sourceTridionObjectUri };
                result.Message = String.Format("Error for \"{0}\"", tridionObject.GetWebDav().CutPath("/", 90, true));
            }

            results.Add(result);
        }

        public static void ChangeMetadataSchemasForTridionObjectsInContainer(string sourceContainerUri, string sourceMetadataSchemaUri, string targetContainerUri, string targetMetadataSchemaUri, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            sourceMetadataSchemaUri = sourceMetadataSchemaUri.GetCurrentVersionTcmId();

            // Open up the source metadata schema 
            SchemaData sourceMetadataSchema = Client.Read(sourceMetadataSchemaUri, null) as SchemaData;
            if (sourceMetadataSchema == null)
                return;

            List<ItemFieldDefinitionData> sourceMetadataFields = GetSchemaMetadataFields(sourceMetadataSchemaUri);

            targetMetadataSchemaUri = targetMetadataSchemaUri.GetCurrentVersionTcmId();

            // Open up the target metadata schema
            SchemaData targetMetadataSchema = Client.Read(targetMetadataSchemaUri, null) as SchemaData;
            if (targetMetadataSchema == null)
                return;

            List<ItemFieldDefinitionData> targetMetadataFields = GetSchemaMetadataFields(targetMetadataSchemaUri);

            // Change metadata schema for objects
            foreach (ItemInfo item in GetItemsByParentContainer(sourceContainerUri, true))
            {
                ChangeMetadataSchemaForTridionObject(item.TcmId, sourceMetadataSchema, sourceMetadataFields, targetContainerUri, targetMetadataSchema, targetMetadataFields, localize, historyMapping, customMetadataTransformer, results);
            }
        }

        public static void FixTridionObjectMetadata(string sourceTridionObjectUri, string sourceMetadataSchemaUri, string targetContainerUri, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            sourceMetadataSchemaUri = sourceMetadataSchemaUri.GetCurrentVersionTcmId();

            // Open up the schema
            SchemaData metadataSchema = Client.Read(sourceMetadataSchemaUri, null) as SchemaData;
            if (metadataSchema == null)
                return;

            List<ItemFieldDefinitionData> metadataFields = GetSchemaMetadataFields(sourceMetadataSchemaUri);

            // Fix metadata for tridion object
            FixTridionObjectMetadata(sourceTridionObjectUri, metadataSchema, metadataFields, targetContainerUri, localize, historyMapping, customMetadataTransformer, results);
        }

        private static void FixTridionObjectMetadata(string sourceTridionObjectUri, SchemaData sourceMetadataSchema, List<ItemFieldDefinitionData> sourceMetadataFields, string targetContainerUri, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            if (String.IsNullOrEmpty(sourceTridionObjectUri))
                return;

            if (results == null)
                results = new List<ResultInfo>();

            RepositoryLocalObjectData tridionObject = ReadItem(sourceTridionObjectUri) as RepositoryLocalObjectData;
            if (tridionObject == null || String.IsNullOrEmpty(tridionObject.Metadata))
                return;

            if (!tridionObject.MetadataSchema.IdRef.GetId().Equals(sourceMetadataSchema.Id.GetId()))
            {
                // If the tridion object is not of the metadata schema, do nothing...
                return;
            }

            //detect mapping by date
            List<FieldMappingInfo> fieldMapping = GetDetectedMapping(historyMapping, tridionObject.VersionInfo.RevisionDate);

            //get fixed metadata
            string newMetadata = customMetadataTransformer == null ?
                GetFixedContent(tridionObject.Metadata, sourceMetadataSchema.NamespaceUri, sourceTridionObjectUri, sourceMetadataSchema, "Metadata", sourceMetadataFields, targetContainerUri, fieldMapping, results) :
                GetFixedContent(null, tridionObject.Metadata, sourceMetadataSchema, null, sourceMetadataFields, sourceTridionObjectUri, sourceMetadataSchema, null, sourceMetadataFields, targetContainerUri, customMetadataTransformer, results);

            if (String.IsNullOrEmpty(newMetadata))
                return;

            if (tridionObject.Metadata.PrettyXml() == newMetadata.PrettyXml())
                return;

            ResultInfo result = SaveTridionObjectMetadata(sourceMetadataSchema, tridionObject.Title, newMetadata, tridionObject.LocationInfo.OrganizationalItem.IdRef, localize);
            if (result != null)
                results.Add(result);
        }

        public static void FixMetadataForTridionObjectsInContainer(string sourceContainerUri, string sourceMetadataSchemaUri, string targetFolderUri, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            sourceMetadataSchemaUri = sourceMetadataSchemaUri.GetCurrentVersionTcmId();

            // Open up the schema
            SchemaData metadataSchema = Client.Read(sourceMetadataSchemaUri, null) as SchemaData;
            if (metadataSchema == null)
                return;

            List<ItemFieldDefinitionData> metadataFields = GetSchemaMetadataFields(sourceMetadataSchemaUri);

            // Fix metadata for tridion objects
            foreach (ItemInfo item in GetItemsByParentContainer(sourceContainerUri, true))
            {
                FixTridionObjectMetadata(item.TcmId, metadataSchema, metadataFields, targetFolderUri, localize, historyMapping, customMetadataTransformer, results);
            }
        }

        public static void TransformTridionObjectMetadata(string sourceTridionObjectUri, string sourceContainerUri, string sourceMetadataSchemaUri, string targetFolderUri, string targetSchemaUri, string formatString, List<ReplacementInfo> replacements, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            if (String.IsNullOrEmpty(sourceTridionObjectUri))
                return;

            sourceMetadataSchemaUri = sourceMetadataSchemaUri.GetCurrentVersionTcmId();

            // Open up the source metadata schema 
            SchemaData sourceMetadataSchema = Client.Read(sourceMetadataSchemaUri, null) as SchemaData;
            if (sourceMetadataSchema == null)
                return;

            List<ItemFieldDefinitionData> sourceMetadataFields = GetSchemaMetadataFields(sourceMetadataSchemaUri);

            targetSchemaUri = targetSchemaUri.GetCurrentVersionTcmId();

            // Open up the target component schema
            SchemaData targetSchema = Client.Read(targetSchemaUri, null) as SchemaData;
            if (targetSchema == null)
                return;

            List<ItemFieldDefinitionData> targetComponentFields = GetSchemaFields(targetSchemaUri);
            List<ItemFieldDefinitionData> targetMetadataFields = GetSchemaMetadataFields(targetSchemaUri);

            // Change schema for component
            TransformTridionObjectMetadata(sourceTridionObjectUri, sourceContainerUri, sourceMetadataSchema, sourceMetadataFields, targetFolderUri, targetSchema, targetComponentFields, targetMetadataFields, formatString, replacements, localize, historyMapping, customComponentTransformer, customMetadataTransformer, results);
        }

        private static void TransformTridionObjectMetadata(string sourceTridionObjectUri, string sourceContainerUri, SchemaData sourceMetadataSchema, List<ItemFieldDefinitionData> sourceMetadataFields, string targetFolderUri, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string formatString, List<ReplacementInfo> replacements, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            if (String.IsNullOrEmpty(sourceTridionObjectUri))
                return;

            if (results == null)
                results = new List<ResultInfo>();

            RepositoryLocalObjectData tridionObject = ReadItem(sourceTridionObjectUri) as RepositoryLocalObjectData;
            if (tridionObject == null || String.IsNullOrEmpty(tridionObject.Metadata))
                return;

            if (!tridionObject.MetadataSchema.IdRef.GetId().Equals(sourceMetadataSchema.Id.GetId()))
            {
                // If the tridion object is not of the metadata schema, do nothing...
                return;
            }

            //detect mapping by date
            List<FieldMappingInfo> fieldMapping = GetDetectedMapping(historyMapping, tridionObject.VersionInfo.RevisionDate);

            // create folder chain
            if (!String.IsNullOrEmpty(sourceContainerUri))
            {
                FolderData sourceFolder = Client.Read(sourceContainerUri, new ReadOptions()) as FolderData;
                if (sourceFolder != null)
                {
                    List<string> tridionObjectWebDavChain = tridionObject.GetWebDavChain();
                    List<string> folderChain = tridionObjectWebDavChain.Take(tridionObjectWebDavChain.Count - 1).ToList().Substract(sourceFolder.GetWebDavChain());
                    targetFolderUri = CreateFolderChain(folderChain, targetFolderUri);
                }
            }

            //get fixed xml
            string newContent = customComponentTransformer == null ?
                GetFixedContent(tridionObject.Metadata, sourceMetadataSchema.NamespaceUri, sourceTridionObjectUri, targetSchema, targetSchema.RootElementName, targetComponentFields, targetFolderUri, fieldMapping, results) :
                GetFixedContent(null, tridionObject.Metadata, sourceMetadataSchema, null, sourceMetadataFields, sourceTridionObjectUri, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, customComponentTransformer, results);

            //get fixed metadata
            string newMetadata = customMetadataTransformer == null ?
                GetFixedContent(tridionObject.Metadata, sourceMetadataSchema.NamespaceUri, sourceTridionObjectUri, targetSchema, "Metadata", targetMetadataFields, targetFolderUri, fieldMapping, results) :
                GetFixedContent(null, tridionObject.Metadata, sourceMetadataSchema, null, sourceMetadataFields, sourceTridionObjectUri, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, customMetadataTransformer, results);

            if (String.IsNullOrEmpty(newContent))
                return;

            List<ComponentFieldData> metadataValues = GetValues(sourceMetadataSchema.NamespaceUri, sourceMetadataFields, tridionObject.Metadata);

            string newTitle = GetTransformedName(tridionObject.Title, sourceTridionObjectUri, null, metadataValues, formatString, replacements);

            ResultInfo result = SaveComponent(targetSchema, newTitle, newContent, newMetadata, targetFolderUri, localize);
            if (result != null)
                results.Add(result);

            FieldMappingInfo targetComponentLinkMapping = fieldMapping.FirstOrDefault(x => x.TargetFieldFullName == "< target component link >" && x.SourceField != null && x.SourceField.Field != null && !String.IsNullOrEmpty(x.SourceField.Field.Name) && !x.SourceField.Field.Name.StartsWith("<"));
            ComponentLinkFieldDefinitionData targetComponentLink = targetComponentLinkMapping == null ? null : targetComponentLinkMapping.SourceField.Field as ComponentLinkFieldDefinitionData;

            // save component link back to source metadata
            if (result != null && result.Status == Status.Success && !String.IsNullOrEmpty(result.TcmId) && targetComponentLink != null)
            {
                string pubId = GetPublicationTcmId(tridionObject.Id);
                string linkId = GetBluePrintItemTcmId(result.TcmId, pubId);
                XElement cl = GetComponentLink(linkId, newTitle, targetComponentLink.Name);

                ComponentFieldData metadataValue = metadataValues.FirstOrDefault(x => x.SchemaField.Name == targetComponentLink.Name && x.SchemaField.GetFieldType() == targetComponentLink.GetFieldType());
                if (metadataValue == null && targetComponentLinkMapping.SourceField.IsMeta)
                {
                    metadataValue = new ComponentFieldData();
                    metadataValue.SchemaField = targetComponentLink;
                    metadataValue.Value = cl;
                    metadataValues.Add(metadataValue);
                }

                string newSourceMetadata = string.Empty;
                XElement newXmlSourceMetadata = GetComponentXml(sourceMetadataSchema.NamespaceUri, "Metadata", metadataValues);
                if (newXmlSourceMetadata != null)
                {
                    newSourceMetadata = newXmlSourceMetadata.ToString();
                    newSourceMetadata = newSourceMetadata.Replace(" xmlns=\"\"", string.Empty);
                }
                if (newSourceMetadata == string.Empty && sourceMetadataFields != null && sourceMetadataFields.Count > 0)
                {
                    newSourceMetadata = string.Format("<Metadata xmlns=\"{0}\" />", sourceMetadataSchema.NamespaceUri);
                }

                ResultInfo result1 = SaveTridionObjectMetadata(sourceMetadataSchema, tridionObject.Title, newSourceMetadata, tridionObject.LocationInfo.OrganizationalItem.IdRef, false);
                if (result1 != null)
                    results.Add(result1);
            }
        }

        public static void TransformMetadataForTridionObjectsInContainer(string sourceContainerUri, string sourceMetadataSchemaUri, string targetFolderUri, string targetSchemaUri, string formatString, List<ReplacementInfo> replacements, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentTransformer, CustomTransformerInfo customMetadataTransformer, List<ResultInfo> results)
        {
            sourceMetadataSchemaUri = sourceMetadataSchemaUri.GetCurrentVersionTcmId();

            // Open up the source component schema 
            SchemaData sourceMetadataSchema = Client.Read(sourceMetadataSchemaUri, null) as SchemaData;
            if (sourceMetadataSchema == null)
                return;

            List<ItemFieldDefinitionData> sourceMetadataFields = GetSchemaMetadataFields(sourceMetadataSchemaUri);

            targetSchemaUri = targetSchemaUri.GetCurrentVersionTcmId();

            // Open up the target component schema
            SchemaData targetSchema = Client.Read(targetSchemaUri, null) as SchemaData;
            if (targetSchema == null)
                return;

            List<ItemFieldDefinitionData> targetComponentFields = GetSchemaFields(targetSchemaUri);
            List<ItemFieldDefinitionData> targetMetadataFields = GetSchemaMetadataFields(targetSchemaUri);

            // Transform tridion object metadata into components
            foreach (ItemInfo item in GetItemsByParentContainer(sourceContainerUri, true))
            {
                TransformTridionObjectMetadata(item.TcmId, sourceContainerUri, sourceMetadataSchema, sourceMetadataFields, targetFolderUri, targetSchema, targetComponentFields, targetMetadataFields, formatString, replacements, localize, historyMapping, customComponentTransformer, customMetadataTransformer, results);
            }
        }

        private static void ChangeFolderLinkedSchema(string folderUri, string sourceSchemaUri, string targetSchemaUri, List<ResultInfo> results)
        {
            if (results == null)
                results = new List<ResultInfo>();

            FolderData innerFolder = Client.Read(folderUri, new ReadOptions()) as FolderData;

            if (innerFolder == null)
                return;

            if (innerFolder.LinkedSchema == null)
                return;

            if (!innerFolder.LinkedSchema.IdRef.Equals(sourceSchemaUri))
            {
                // If the component is not of the schmea that we want to change from, do nothing...
                return;
            }

            if (innerFolder.LinkedSchema.IdRef.Equals(targetSchemaUri))
            {
                // If the component already has this schema, don't do anything.
                return;
            }

            ResultInfo result = new ResultInfo();
            result.Item = innerFolder.ToItem();

            if (innerFolder.IsEditable == true)
            {
                try
                {
                    //change schema id
                    innerFolder.LinkedSchema.IdRef = targetSchemaUri;

                    //make non-mandatory to aviod conflicts with inner components
                    innerFolder.IsLinkedSchemaMandatory = false;

                    Client.Save(innerFolder, new ReadOptions());

                    result.Status = Status.Success;
                    result.Message = String.Format("Changed schema for folder \"{0}\"", innerFolder.GetWebDav().CutPath("/", 80, true));
                }
                catch (Exception ex)
                {
                    result.Status = Status.Error;
                    result.Item = new ItemInfo { ItemType = ItemType.Folder, TcmId = folderUri };
                    result.StackTrace = ex.StackTrace;
                    result.Message = String.Format("Error for folder \"{0}\"", innerFolder.GetWebDav().CutPath("/", 80, true));
                }
            }
            else
            {
                result.Status = Status.Error;
                result.Item = new ItemInfo { ItemType = ItemType.Folder, TcmId = folderUri };
                result.Message = String.Format("Error for folder \"{0}\"", innerFolder.GetWebDav().CutPath("/", 80, true));
            }

            results.Add(result);
        }

        public static List<CustomTransformerInfo> GetCustomTransformers(string sourceSchemaTitle, SchemaType sourceSchemaType, ItemType sourceObjectType, string targetSchemaTitle, SchemaType targetSchemaType)
        {
            List<CustomTransformerInfo> transformers = new List<CustomTransformerInfo>();
            Assembly currAssembly = Assembly.GetExecutingAssembly();
            string dir = Path.GetDirectoryName(currAssembly.CodeBase.Replace("file:///", "").Replace("/", "\\"));
            if (dir != null)
            {
                foreach (string file in Directory.GetFiles(dir, "*.dll"))
                {
                    Assembly assembly = Assembly.LoadFrom(file);
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.GetInterfaces().Contains(typeof(ICustomTransformer)))
                        {
                            object[] attributes = type.GetCustomAttributes(false);
                            if (!attributes.Any(x => x is SourceSchemaAttribute || x is TargetSchemaAttribute || x is SourceObjectAttribute))
                            {
                                CustomTransformerInfo transformer = new CustomTransformerInfo();
                                transformer.Title = type.Name;
                                transformer.TypeName = type.FullName;
                                transformer.AssemblyQualifiedName = type.AssemblyQualifiedName;
                                transformers.Add(transformer);
                            }
                            else
                            {
                                bool include = true;
                                foreach (Attribute attribute in attributes)
                                {
                                    if (attribute is SourceSchemaAttribute)
                                    {
                                        SourceSchemaAttribute sourceSchemaAttribute = (SourceSchemaAttribute)attribute;
                                        include = include && (String.IsNullOrEmpty(sourceSchemaAttribute.Title) || sourceSchemaAttribute.Title == sourceSchemaTitle);
                                        include = include && (sourceSchemaAttribute.Type == SchemaType.Any || sourceSchemaAttribute.Type == sourceSchemaType || sourceSchemaAttribute.Type == SchemaType.Metadata && sourceSchemaType == SchemaType.Component);
                                    }
                                    if (attribute is SourceObjectAttribute)
                                    {
                                        SourceObjectAttribute sourceObjectAttribute = (SourceObjectAttribute)attribute;
                                        include = include && (
                                            sourceObjectAttribute.Type == ObjectType.Any || 
                                            sourceObjectAttribute.Type == ObjectType.ComponentOrFolder && sourceObjectType == ItemType.Component ||
                                            sourceObjectAttribute.Type == ObjectType.ComponentOrFolder && sourceObjectType == ItemType.Folder ||
                                            sourceObjectAttribute.Type == ObjectType.Component && sourceObjectType == ItemType.Component ||
                                            sourceObjectAttribute.Type == ObjectType.Folder && sourceObjectType == ItemType.Folder ||
                                            sourceObjectAttribute.Type == ObjectType.PageOrStructureGroup && sourceObjectType == ItemType.Page ||
                                            sourceObjectAttribute.Type == ObjectType.PageOrStructureGroup && sourceObjectType == ItemType.StructureGroup ||
                                            sourceObjectAttribute.Type == ObjectType.Page && sourceObjectType == ItemType.Page ||
                                            sourceObjectAttribute.Type == ObjectType.StructureGroup && sourceObjectType == ItemType.StructureGroup
                                            );
                                    }
                                    if (attribute is TargetSchemaAttribute)
                                    {
                                        TargetSchemaAttribute targetSchemaAttribute = (TargetSchemaAttribute)attribute;
                                        include = include && (String.IsNullOrEmpty(targetSchemaAttribute.Title) || targetSchemaAttribute.Title == targetSchemaTitle);
                                        include = include && (targetSchemaAttribute.Type == SchemaType.Any || targetSchemaAttribute.Type == targetSchemaType || targetSchemaAttribute.Type == SchemaType.Metadata && targetSchemaType == SchemaType.Component);
                                    }
                                }
                                if (include)
                                {
                                    CustomTransformerInfo transformer = new CustomTransformerInfo();
                                    transformer.Title = type.Name;
                                    transformer.TypeName = type.FullName;
                                    transformer.AssemblyQualifiedName = type.AssemblyQualifiedName;
                                    transformers.Add(transformer);
                                }
                            }
                        }
                    }
                }
            }

            return transformers;
        }

        public static List<CustomTransformerInfo> GetCustomImporters(string sourceTable, string targetSchemaTitle, SchemaType targetSchemaType)
        {
            List<CustomTransformerInfo> transformers = new List<CustomTransformerInfo>();
            Assembly currAssembly = Assembly.GetExecutingAssembly();
            string dir = Path.GetDirectoryName(currAssembly.CodeBase.Replace("file:///", "").Replace("/", "\\"));
            if (dir != null)
            {
                foreach (string file in Directory.GetFiles(dir, "*.dll"))
                {
                    Assembly assembly = Assembly.LoadFrom(file);
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.GetInterfaces().Contains(typeof(ICustomImporter)))
                        {
                            object[] attributes = type.GetCustomAttributes(false);
                            if (!attributes.Any(x => x is SourceTableAttribute || x is TargetSchemaAttribute))
                            {
                                CustomTransformerInfo transformer = new CustomTransformerInfo();
                                transformer.Title = type.Name;
                                transformer.TypeName = type.FullName;
                                transformer.AssemblyQualifiedName = type.AssemblyQualifiedName;
                                transformers.Add(transformer);
                            }
                            else
                            {
                                bool include = true;
                                foreach (Attribute attribute in attributes)
                                {
                                    if (attribute is SourceTableAttribute)
                                    {
                                        SourceTableAttribute sourceTableAttribute = (SourceTableAttribute)attribute;
                                        include = include && (String.IsNullOrEmpty(sourceTableAttribute.Title) || sourceTableAttribute.Title == sourceTable);
                                    }
                                    if (attribute is TargetSchemaAttribute)
                                    {
                                        TargetSchemaAttribute targetSchemaAttribute = (TargetSchemaAttribute)attribute;
                                        include = include && (String.IsNullOrEmpty(targetSchemaAttribute.Title) || targetSchemaAttribute.Title == targetSchemaTitle);
                                        include = include && (targetSchemaAttribute.Type == SchemaType.Any || targetSchemaAttribute.Type == targetSchemaType || targetSchemaAttribute.Type == SchemaType.Metadata && targetSchemaType == SchemaType.Component);
                                    }
                                }
                                if (include)
                                {
                                    CustomTransformerInfo transformer = new CustomTransformerInfo();
                                    transformer.Title = type.Name;
                                    transformer.TypeName = type.FullName;
                                    transformer.AssemblyQualifiedName = type.AssemblyQualifiedName;
                                    transformers.Add(transformer);
                                }
                            }
                        }
                    }
                }
            }

            return transformers;
        }

        #endregion

        #region Multimedia components

        public static void SaveBinaryFromMultimediaComponent(string id, string targetDir)
        {
            ComponentData multimediaComponent = Client.Read(id, new ReadOptions()) as ComponentData;
            if (multimediaComponent == null)
                return;

            string path = Path.Combine(targetDir, Path.GetFileName(multimediaComponent.BinaryContent.Filename));

            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            using (Stream tempStream = StreamDownloadClient.DownloadBinaryContent(id))
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    byte[] binaryContent = null;

                    if (multimediaComponent.BinaryContent.FileSize != -1)
                    {
                        var memoryStream = new MemoryStream();
                        tempStream.CopyTo(memoryStream);
                        binaryContent = memoryStream.ToArray();
                    }
                    if (binaryContent == null)
                        return;

                    fs.Write(binaryContent, 0, binaryContent.Length);
                }
            }
        }

        private static BinaryContentData GetBinaryData(string filePath)
        {
            string title = Path.GetFileName(filePath);

            string tempLocation;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                tempLocation = StreamUploadClient.UploadBinaryContent(title, fs);
            }
            if (String.IsNullOrEmpty(tempLocation))
                return null;

            BinaryContentData binaryContent = new BinaryContentData();
            binaryContent.UploadFromFile = tempLocation;
            binaryContent.Filename = title;
            binaryContent.MultimediaType = new LinkToMultimediaTypeData { IdRef = GetMimeTypeId(filePath) };
            binaryContent.FileSize = (int)(new FileInfo(filePath)).Length;

            return binaryContent;
        }

        public static ResultInfo SaveMultimediaComponentFromBinary(string filePath, string title, string metadata, string tcmContainer, string multimediaSchemaTitle)
        {
            ResultInfo result = new ResultInfo();

            if (!File.Exists(filePath))
            {
                result.Status = Status.Error;
                result.Message = String.Format("File {0} does not exist", filePath);

                return result;
            }

            if (String.IsNullOrEmpty(title))
                title = Path.GetFileName(filePath);

            if (ExistsItem(tcmContainer, title) || ExistsItem(tcmContainer, Path.GetFileNameWithoutExtension(filePath)) || ExistsItem(tcmContainer, Path.GetFileName(filePath)))
            {
                string id = GetItemTcmId(tcmContainer, title);
                if (String.IsNullOrEmpty(id))
                    id = GetItemTcmId(tcmContainer, Path.GetFileNameWithoutExtension(filePath));
                if (String.IsNullOrEmpty(id))
                    id = GetItemTcmId(tcmContainer, Path.GetFileName(filePath));

                ComponentData multimediaComponent = ReadItem(id) as ComponentData;
                if (multimediaComponent == null)
                {
                    result.Status = Status.Error;
                    result.Item = new ItemInfo { ItemType = ItemType.Component, TcmId = id };
                    result.Message = String.Format("Multimedia component \"{0}\" not found.", id);
                    return result;
                }

                BinaryContentData binaryContent = GetBinaryData(filePath);
                if (binaryContent == null)
                {
                    result.Status = Status.Error;
                    result.Item = new ItemInfo { ItemType = ItemType.Component, TcmId = id };
                    result.Message = String.Format("File {0} does not exist", filePath);
                    return result;
                }

                if (multimediaComponent.BinaryContent.Filename == Path.GetFileName(filePath) && multimediaComponent.BinaryContent.FileSize == binaryContent.FileSize)
                {
                    result.Status = Status.None;
                    result.Message = String.Format("No changes made for multimedia component \"{0}\"", id);
                    return result;
                }

                if (multimediaComponent.BluePrintInfo.IsShared == true)
                {
                    id = GetBluePrintTopTcmId(id);

                    multimediaComponent = ReadItem(id) as ComponentData;
                    if (multimediaComponent == null)
                    {
                        result.Status = Status.Error;
                        result.Item = new ItemInfo { ItemType = ItemType.Component, TcmId = id };
                        result.Message = String.Format("Multimedia component \"{0}\" not found.", id);
                        return result;
                    }
                }

                try
                {
                    multimediaComponent = Client.CheckOut(id, true, new ReadOptions()) as ComponentData;
                }
                catch (Exception ex)
                {
                    result.Status = Status.Error;
                    result.Item = new ItemInfo { ItemType = ItemType.Component, TcmId = id };
                    result.StackTrace = ex.StackTrace;
                    result.Message = String.Format("Multimedia component \"{0}\" is checked out.", id);
                    return result;
                }

                if (multimediaComponent == null)
                {
                    result.Status = Status.Error;
                    result.Item = new ItemInfo { ItemType = ItemType.Component, TcmId = id };
                    result.Message = String.Format("Multimedia component \"{0}\" not found.", id);
                    return result;
                }

                multimediaComponent.BinaryContent = binaryContent;
                multimediaComponent.ComponentType = ComponentType.Multimedia;
                multimediaComponent.Metadata = metadata;

                try
                {
                    multimediaComponent = (ComponentData)Client.Update(multimediaComponent, new ReadOptions());
                    Client.CheckIn(id, new ReadOptions());

                    result.Item = multimediaComponent.ToItem();
                    result.Status = Status.Success;
                    result.Message = String.Format("Multimedia component {0} was updated", title);

                    return result;
                }
                catch (Exception ex)
                {
                    Client.UndoCheckOut(multimediaComponent.Id, true, new ReadOptions());

                    result.Status = Status.Error;
                    result.Item = new ItemInfo { ItemType = ItemType.Component, TcmId = id };
                    result.Message = String.Format("Error creating multimedia component {0}", title);
                    result.StackTrace = ex.Message;

                    return result;
                }
            }

            try
            {
                BinaryContentData binaryContent = GetBinaryData(filePath);
                if (binaryContent == null)
                {
                    result.Status = Status.Error;
                    result.Message = String.Format("File {0} does not exist", filePath);

                    return result;
                }

                if (string.IsNullOrEmpty(multimediaSchemaTitle))
                    multimediaSchemaTitle = "Default Multimedia Schema";
                
                string tcmPublication = GetPublicationTcmId(tcmContainer);
                string schemaId = GetSchemas(tcmPublication).Single(x => x.Title == multimediaSchemaTitle).TcmId;

                ComponentData multimediaComponent = new ComponentData
                {
                    Title = title,
                    LocationInfo = new LocationInfo { OrganizationalItem = new LinkToOrganizationalItemData { IdRef = tcmContainer } },
                    Id = "tcm:0-0-0",
                    BinaryContent = binaryContent,
                    ComponentType = ComponentType.Multimedia,
                    Metadata = metadata,
                    Schema = new LinkToSchemaData { IdRef = schemaId },
                    IsBasedOnMandatorySchema = false,
                    IsBasedOnTridionWebSchema = true,
                    ApprovalStatus = new LinkToApprovalStatusData
                    {
                        IdRef = "tcm:0-0-0"
                    }
                };

                multimediaComponent = (ComponentData)Client.Save(multimediaComponent, new ReadOptions());
                string componentUri = Client.CheckIn(multimediaComponent.Id, new ReadOptions()).Id;
                multimediaComponent = GetComponent(componentUri);

                result.Item = multimediaComponent.ToItem();
                result.Status = Status.Success;
                result.Message = String.Format("Multimedia component \"{0}\" was created", multimediaComponent.GetWebDav().CutPath("/", 80, true));
            }
            catch (Exception ex)
            {
                result.Status = Status.Error;
                result.Item = new ItemInfo { ItemType = ItemType.Component };
                result.StackTrace = ex.StackTrace;
                result.Message = String.Format("Error creating multimedia component \"{0}\"", title);
            }

            return result;
        }

        public static ResultInfo SaveMultimediaComponentFromBinary(string filePath, string title, string tcmContainer)
        {
            return SaveMultimediaComponentFromBinary(filePath, title, null, tcmContainer, null);
        }

        #endregion

        #region SQL database

        public static List<FieldInfo> GetDatabaseTableFields(string dbHost, string dbUsername, string dbPassword, string database, string table)
        {
            List<FieldInfo> res = new List<FieldInfo>();

            //new empty embedded schema, with possible child values
            res.Insert(0, new FieldInfo { Field = new ItemFieldDefinitionData { Name = "< new >" } });
            //ignore item
            res.Insert(0, new FieldInfo { Field = new ItemFieldDefinitionData() });

            DataTable dt = new DataTable(table);

            System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.DataSource = dbHost;
            builder.InitialCatalog = database;
            builder.UserID = dbUsername;
            builder.Password = dbPassword;

            using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = string.Format("select * from INFORMATION_SCHEMA.COLUMNS where TABLE_Name='{0}' order by ORDINAL_POSITION", table);

                System.Data.SqlClient.SqlDataAdapter sda = new System.Data.SqlClient.SqlDataAdapter(command);
                sda.Fill(dt);
            }

            foreach (DataRow row in dt.Rows)
            {
                ItemFieldDefinitionData item;

                if (row["DATA_TYPE"].ToString().ToLower().Contains("varchar"))
                {
                    item = new SingleLineTextFieldDefinitionData();
                }
                else if (row["DATA_TYPE"].ToString().ToLower().Contains("text"))
                {
                    item = new XhtmlFieldDefinitionData();
                }
                else if (row["DATA_TYPE"].ToString().ToLower().Contains("date"))
                {
                    item = new DateFieldDefinitionData();
                }
                else if (row["DATA_TYPE"].ToString().ToLower().Contains("int") || row["DATA_TYPE"].ToString().ToLower().Contains("decimal") || row["DATA_TYPE"].ToString().ToLower().Contains("float"))
                {
                    item = new NumberFieldDefinitionData();
                }
                
                //KeywordFieldDefinitionData
                //MultimediaLinkFieldDefinitionData
                //ExternalLinkFieldDefinitionData
                //ComponentLinkFieldDefinitionData
                //EmbeddedSchemaFieldDefinitionData
                
                else
                {
                    item = new ItemFieldDefinitionData();
                }
                
                item.Name = row["COLUMN_NAME"].ToString();
                
                FieldInfo field = new FieldInfo();
                field.Field = item;
                field.Level = 0;
                res.Add(field);
            }

            return res;
        }

        public static void ImportComponents(string dbHost, string dbUsername, string dbPassword, string sourceDatabase, string sourceTable, string sql, string targetFolderUri, string targetSchemaUri, string formatString, List<ReplacementInfo> replacements, bool localize, HistoryMappingInfo historyMapping, CustomTransformerInfo customComponentImporter, CustomTransformerInfo customMetadataImporter, List<ResultInfo> results)
        {
            if (String.IsNullOrEmpty(sourceTable))
                return;

            // Open up the target component schema
            SchemaData targetSchema = Client.Read(targetSchemaUri, null) as SchemaData;
            if (targetSchema == null)
                return;

            List<ItemFieldDefinitionData> targetComponentFields = GetSchemaFields(targetSchemaUri);
            List<ItemFieldDefinitionData> targetMetadataFields = GetSchemaMetadataFields(targetSchemaUri);

            DataTable dt = new DataTable(sourceTable);

            System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.DataSource = dbHost;
            builder.InitialCatalog = sourceDatabase;
            builder.UserID = dbUsername;
            builder.Password = dbPassword;

            using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = string.IsNullOrEmpty(sql) ? string.Format("SELECT * FROM [{0}]", sourceTable) : sql;

                System.Data.SqlClient.SqlDataAdapter sda = new System.Data.SqlClient.SqlDataAdapter(command);
                sda.Fill(dt);
            }

            int i = 0;

            foreach (DataRow dataRow in dt.Rows)
            {
                //get fixed xml
                string newContent = customComponentImporter == null ?
                    GetFixedContent(sourceTable, dataRow, targetSchema, targetSchema.RootElementName, targetComponentFields, historyMapping.Last().Mapping, results) :
                    GetFixedContent(sourceTable, dataRow, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, customComponentImporter, results);

                //get fixed metadata
                string newMetadata = customMetadataImporter == null ?
                    GetFixedContent(sourceTable, dataRow, targetSchema, "Metadata", targetMetadataFields, historyMapping.Last().Mapping, results) :
                    GetFixedContent(sourceTable, dataRow, targetSchema, targetComponentFields, targetMetadataFields, targetFolderUri, customMetadataImporter, results);

                if (String.IsNullOrEmpty(newContent))
                    return;

                i++;

                List<ComponentFieldData> sourceValues = GetValues(dataRow);

                string title = GetTransformedName(null, null, sourceValues, null, formatString, replacements);
                if (string.IsNullOrEmpty(title))
                    title = string.Format("Component {0}", i);

                ResultInfo result = SaveComponent(targetSchema, title, newContent, newMetadata, targetFolderUri, localize);
                if (result != null)
                    results.Add(result);
            }
        }

        #endregion

        #region Tridion pages

        public static List<ItemInfo> GetPages(string tcmComponent)
        {
            return Client.GetListXml(tcmComponent, new UsingItemsFilterData { ItemTypes = new[] { ItemType.Page } }).ToList(ItemType.Page);
        }

        #endregion

        #region Broken data

        public static List<ResultInfo> CheckBrokenComponentsAndMetadata(string tcmItem)
        {
            List<ResultInfo> results = new List<ResultInfo>();

            ItemType itemType = GetItemType(tcmItem);
            if (itemType == ItemType.Publication)
            {
                List<ItemInfo> schemas = GetSchemas(tcmItem);
                CheckBrokenComponentsAndMetadata(schemas, results);
            }
            if (itemType == ItemType.Folder)
            {
                List<ItemInfo> schemas = GetSchemas(tcmItem, false);
                CheckBrokenComponentsAndMetadata(schemas, results);
            }
            if (itemType == ItemType.Schema)
            {
                List<ItemInfo> schemas = new List<ItemInfo> { new ItemInfo { TcmId =  tcmItem }};
                CheckBrokenComponentsAndMetadata(schemas, results);
            }

            results = results.Distinct(new ResultInfoComparer()).ToList();

            ResultInfo resultFinish = new ResultInfo();
            resultFinish.Status = Status.Info;
            resultFinish.Message = "Finished";
            results.Add(resultFinish);

            return results;
        }

        private static void CheckBrokenComponentsAndMetadata(List<ItemInfo> schemaItems, List<ResultInfo> results)
        {
            foreach (ItemInfo schemaItem in schemaItems)
            {
                SchemaData schema = ReadItem(schemaItem.TcmId) as SchemaData;
                if (schema == null)
                    continue;

                List<ItemFieldDefinitionData> schemaFields = GetSchemaFields(schemaItem.TcmId);
                List<ItemFieldDefinitionData> schemaMetadataFields = GetSchemaMetadataFields(schemaItem.TcmId);

                if (schema.Purpose == SchemaPurpose.Component)
                    CheckBrokenComponents(schema, schemaFields, schemaMetadataFields, results);

                if (schema.Purpose == SchemaPurpose.Metadata || schema.Purpose == SchemaPurpose.Multimedia)
                    CheckBrokenMetadata(schema, schemaMetadataFields, results);
            }
        }

        private static void CheckBrokenComponents(SchemaData schema, List<ItemFieldDefinitionData> schemaFields, List<ItemFieldDefinitionData> schemaMetadataFields, List<ResultInfo> results)
        {
            List<string> components = GetUsingItems(schema.Id, true, new[] { ItemType.Component });

            foreach (string componentUri in components)
            {
                ComponentData component = GetComponent(componentUri);

                CheckBrokenContent(schema, schema.RootElementName, schemaFields, component.Content, schema.NamespaceUri, component, results);
                CheckBrokenContent(schema, "Metadata", schemaMetadataFields, component.Metadata, schema.NamespaceUri, component, results);
            }
        }

        private static void CheckBrokenMetadata(SchemaData schema, List<ItemFieldDefinitionData> schemaFields, List<ResultInfo> results)
        {
            List<string> metadataObjects = GetUsingCurrentItems(schema.Id);
            foreach (string itemUri in metadataObjects)
            {
                IdentifiableObjectData item = ReadItem(itemUri);

                if (item is RepositoryLocalObjectData || item is PublicationData)
                {
                    string metadata = null;
                    if (item is RepositoryLocalObjectData && ((RepositoryLocalObjectData)item).MetadataSchema != null && ((RepositoryLocalObjectData)item).MetadataSchema.IdRef == schema.Id)
                        metadata = ((RepositoryLocalObjectData)item).Metadata;
                    if (item is PublicationData && ((PublicationData)item).MetadataSchema != null && ((PublicationData)item).MetadataSchema.IdRef == schema.Id)
                        metadata = ((PublicationData)item).Metadata;

                    if (metadata != null)
                        CheckBrokenContent(schema, "Metadata", schemaFields, metadata, schema.NamespaceUri, item, results);
                }
            }
        }

        public static void CheckBrokenContent(SchemaData schema, string rootElementName, List<ItemFieldDefinitionData> schemaFields, string xml, XNamespace ns, IdentifiableObjectData item, List<ResultInfo> results)
        {
            List<FieldMappingInfo> fieldMapping = GetDefaultFieldMapping(schemaFields, schema.Id);

            List<ComponentFieldData> componentFieldDataValues = GetFixedValues(xml, ns, item.Id, schema, rootElementName, schemaFields, string.Empty, fieldMapping, results);

            //check mandatory and empty items
            foreach (ItemFieldDefinitionData schemaField in schemaFields)
            {
                if (schemaField.IsMandatory())
                {
                    ComponentFieldData componentFieldDataValue = componentFieldDataValues.FirstOrDefault(x => x.SchemaField.Name == schemaField.Name);

                    if (!schemaField.IsMultiValue() && (componentFieldDataValue == null || componentFieldDataValue.Value == null) || 
                        schemaField.IsMultiValue() && (componentFieldDataValue == null || componentFieldDataValue.Value == null || !((IEnumerable<object>)componentFieldDataValue.Value).Any()))
                    {
                        ResultInfo result = new ResultInfo();

                        if (item is RepositoryLocalObjectData)
                            result.Item = ((RepositoryLocalObjectData)item).ToItem();

                        if (item is PublicationData)
                            result.Item = ((PublicationData)item).ToItem();

                        result.Status = Status.Error;
                        result.Message = String.Format("Element \"{0}\" contains mandatory empty fields", item is RepositoryLocalObjectData ? ((RepositoryLocalObjectData)item).GetWebDav().CutPath("/", 90, true) : item.Title);
                        results.Add(result);
                    }
                }
                if (schemaField.IsEmbedded())
                {
                    try
                    {
                        ComponentFieldData componentFieldDataValue = componentFieldDataValues.FirstOrDefault(x => x.SchemaField.Name == schemaField.Name);

                        if (componentFieldDataValue != null)
                        {
                            EmbeddedSchemaFieldDefinitionData embeddedSchemaField = (EmbeddedSchemaFieldDefinitionData)schemaField;
                            SchemaData embeddedSchema = ReadItem(embeddedSchemaField.EmbeddedSchema.IdRef) as SchemaData;
                            List<ItemFieldDefinitionData> embeddedSchemaFields = GetSchemaFields(embeddedSchemaField.EmbeddedSchema.IdRef);

                            if (!schemaField.IsMultiValue())
                            {
                                string value = componentFieldDataValue.Value.ToString();
                                if (embeddedSchema != null)
                                {
                                    CheckBrokenContent(embeddedSchema, schemaField.Name, embeddedSchemaFields, value, ns, item, results);
                                }
                            }
                            else
                            {
                                foreach (XElement element in (IEnumerable<object>)componentFieldDataValue.Value)
                                {
                                    string value = element.ToString();
                                    if (embeddedSchema != null)
                                    {
                                        CheckBrokenContent(embeddedSchema, schemaField.Name, embeddedSchemaFields, value, ns, item, results);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }
        }

        #endregion

        #region Tridion delete

        public static void Delete(string tcmItem, bool delete, string schemaUri, List<Criteria> criterias, List<ResultInfo> results)
        {
            ItemType itemType = GetItemType(tcmItem);

            if (itemType == ItemType.Folder && !string.IsNullOrEmpty(schemaUri) && criterias != null && criterias.Any())
            {
                foreach (ItemInfo component in GetComponentsByCriterias(tcmItem, schemaUri, criterias))
                {
                    DeleteTridionObject(component.TcmId, delete, results);
                }
            }
            else if (itemType == ItemType.Publication)
            {
                DeletePublication(tcmItem, delete, results);
            }
            else if (itemType == ItemType.Folder || itemType == ItemType.StructureGroup || itemType == ItemType.Category)
            {
                DeleteFolderOrStructureGroup(tcmItem, delete, results);
            }
            else
            {
                DeleteTridionObject(tcmItem, delete, results);
            }
        }

        private static LinkStatus RemoveDependency(string tcmItem, string tcmDependentItem, bool delete, List<ResultInfo> results)
        {
            if (results.Any(x => x.Status == Status.Error))
                return LinkStatus.Error;

            if (results.Count > 50)
            {
                results.Insert(0, new ResultInfo
                {
                    Message = "Delete stack exceeds 50 items. Please select other item",
                    Item = new ItemInfo { Title = "Delete stack exceeds 50 items" },
                    Status = Status.Error
                });

                return LinkStatus.Error;
            }

            ResultInfo result = new ResultInfo();

            ItemType itemType = GetItemType(tcmItem);
            ItemType dependentItemType = GetItemType(tcmDependentItem);
            LinkStatus status = LinkStatus.NotFound;
            string stackTraceMessage = "";

            RepositoryLocalObjectData dependentItemData = ReadItem(tcmDependentItem) as RepositoryLocalObjectData;

            //publication properies are not handled
            if (itemType == ItemType.Publication)
            {
                PublicationData publicationData = (PublicationData)ReadItem(tcmItem);
                result.Item = publicationData.ToItem();

                result.Status = Status.Error;
                result.Message = string.Format("Not able to unlink \"{1}\" from publication \"{0}\".", publicationData.Title, dependentItemData == null ? tcmDependentItem : dependentItemData.GetWebDav());
                results.Add(result);

                return LinkStatus.Error;
            }

            RepositoryLocalObjectData itemData = ReadItem(tcmItem) as RepositoryLocalObjectData;
            if (itemData == null)
                return LinkStatus.NotFound;

            if (itemData.BluePrintInfo.IsShared == true)
            {
                tcmItem = GetBluePrintTopTcmId(tcmItem);

                itemData = ReadItem(tcmItem) as RepositoryLocalObjectData;
                if (itemData == null)
                    return LinkStatus.NotFound;
            }
            
            result.Item = itemData.ToItem();

            if (delete)
            {
                //remove linked schema
                if (itemType == ItemType.Folder && dependentItemType == ItemType.Schema)
                {
                    status = RemoveFolderLinkedSchema(tcmItem, out stackTraceMessage);
                }

                //remove metadata schema
                if (dependentItemType == ItemType.Schema)
                {
                    status = RemoveMetadataSchema(tcmItem, tcmDependentItem, out stackTraceMessage);
                }

                //remove parameters schema
                if (dependentItemType == ItemType.Schema && itemData is TemplateData)
                {
                    status = RemoveParameterSchema(tcmItem, tcmDependentItem, out stackTraceMessage);
                }

                //remove component template linked schema
                if (itemType == ItemType.ComponentTemplate && dependentItemType == ItemType.Schema)
                {
                    status = RemoveCTLinkedSchema(tcmItem, tcmDependentItem, out stackTraceMessage);
                }

                //remove CP
                if (itemType == ItemType.Page && (dependentItemType == ItemType.Component || dependentItemType == ItemType.ComponentTemplate))
                {
                    status = RemoveComponentPresentation(tcmItem, tcmDependentItem, out stackTraceMessage);
                }
                //remove TBB from page template
                else if (itemType == ItemType.PageTemplate && dependentItemType == ItemType.TemplateBuildingBlock)
                {
                    status = RemoveTbbFromPageTemplate(tcmItem, tcmDependentItem, out stackTraceMessage);
                }
                //remove TBB from component template
                else if (itemType == ItemType.ComponentTemplate && dependentItemType == ItemType.TemplateBuildingBlock)
                {
                    status = RemoveTbbFromComponentTemplate(tcmItem, tcmDependentItem, out stackTraceMessage);
                }
                //remove TBB from compound TBB
                else if (itemType == ItemType.TemplateBuildingBlock && dependentItemType == ItemType.TemplateBuildingBlock)
                {
                    status = RemoveTbbFromCompoundTbb(tcmItem, tcmDependentItem, out stackTraceMessage);
                }
                //change schema keyword field to text field
                else if (itemType == ItemType.Schema && dependentItemType == ItemType.Category)
                {
                    status = RemoveKeywordField(tcmItem, tcmDependentItem, out stackTraceMessage);
                }
                //remove component or keyword link from component
                else if (itemType == ItemType.Component && (dependentItemType == ItemType.Component || dependentItemType == ItemType.Keyword))
                {
                    status = CheckRemoveLinkFromComponent(tcmItem, tcmDependentItem);

                    //component link is mandatory - schema field change
                    if (status == LinkStatus.Mandatory)
                    {
                        RemoveSchemaMandatoryLinkFields(itemData, tcmDependentItem, results);
                    }

                    status = RemoveLinkFromComponent(tcmItem, tcmDependentItem, out stackTraceMessage);
                }
                //remove component or keyword link from metadata
                else if (dependentItemType == ItemType.Component || dependentItemType == ItemType.Keyword)
                {
                    status = CheckRemoveLinkFromMetadata(tcmItem, tcmDependentItem);

                    //component link is mandatory - schema field change
                    if (status == LinkStatus.Mandatory)
                    {
                        RemoveSchemaMandatoryLinkFields(itemData, tcmDependentItem, results);
                    }

                    status = RemoveLinkFromMetadata(tcmItem, tcmDependentItem, out stackTraceMessage);
                }

                if (status == LinkStatus.Found && itemData is VersionedItemData)
                    status = RemoveHistory(tcmItem, tcmDependentItem, out stackTraceMessage);

                if (status == LinkStatus.Found)
                {
                    result.Status = Status.Success;
                    result.Message = string.Format("Item \"{1}\" was removed from \"{0}\".", itemData.GetWebDav(), dependentItemData == null ? tcmDependentItem : dependentItemData.GetWebDav());
                }
            }
            else
            {
                //check if possible to remove linked schema
                if (itemType == ItemType.Folder && dependentItemType == ItemType.Schema)
                {
                    status = CheckRemoveFolderLinkedSchema(tcmItem);
                }

                //check if possible to remove metadata schema
                if (dependentItemType == ItemType.Schema)
                {
                    status = CheckRemoveMetadataSchema(tcmItem, tcmDependentItem);
                }

                //check if possible to remove parameters schema
                if (dependentItemType == ItemType.Schema && itemData is TemplateData)
                {
                    status = CheckRemoveParameterSchema(tcmItem, tcmDependentItem);
                }

                //check if possible to remove component template linked schema
                if (itemType == ItemType.ComponentTemplate && dependentItemType == ItemType.Schema)
                {
                    status = CheckRemoveCTLinkedSchema(tcmItem, tcmDependentItem);
                }

                //check if possible to remove CP
                if (itemType == ItemType.Page && (dependentItemType == ItemType.Component || dependentItemType == ItemType.ComponentTemplate))
                {
                    status = CheckRemoveComponentPresentation(tcmItem, tcmDependentItem);
                }
                //check if possible to remove TBB from page template
                else if (itemType == ItemType.PageTemplate && dependentItemType == ItemType.TemplateBuildingBlock)
                {
                    status = CheckRemoveTbbFromPageTemplate(tcmItem, tcmDependentItem);
                }
                //check if possible to remove TBB from component template
                else if (itemType == ItemType.ComponentTemplate && dependentItemType == ItemType.TemplateBuildingBlock)
                {
                    status = CheckRemoveTbbFromComponentTemplate(tcmItem, tcmDependentItem);
                }
                //check if possible to remove TBB from compound TBB
                else if (itemType == ItemType.TemplateBuildingBlock && dependentItemType == ItemType.TemplateBuildingBlock)
                {
                    status = CheckRemoveTbbFromCompoundTbb(tcmItem, tcmDependentItem);
                }
                //change schema keyword field to text field
                else if (itemType == ItemType.Schema && dependentItemType == ItemType.Category)
                {
                    status = CheckRemoveKeywordField(tcmItem, tcmDependentItem);
                }
                //check if possible to remove component or keyword link from component
                else if (itemType == ItemType.Component && (dependentItemType == ItemType.Component || dependentItemType == ItemType.Keyword))
                {
                    status = CheckRemoveLinkFromComponent(tcmItem, tcmDependentItem);

                    //component link is mandatory - schema field needs to be changed
                    if (status == LinkStatus.Mandatory)
                    {
                        CheckRemoveSchemaMandatoryLinkFields(itemData, tcmDependentItem, results);
                        status = LinkStatus.Found;
                    }
                }
                //check if possible to remove component or keyword link from metadata
                else if (dependentItemType == ItemType.Component || dependentItemType == ItemType.Keyword)
                {
                    status = CheckRemoveLinkFromMetadata(tcmItem, tcmDependentItem);

                    //component link is mandatory - schema field needs to be changed
                    if (status == LinkStatus.Mandatory)
                    {
                        CheckRemoveSchemaMandatoryLinkFields(itemData, tcmDependentItem, results);
                        status = LinkStatus.Found;
                    }
                }

                if (status == LinkStatus.Found)
                {
                    result.Status = Status.Info;
                    result.Message = string.Format("Remove item \"{1}\" from \"{0}\".", itemData.GetWebDav(), dependentItemData == null ? tcmDependentItem : dependentItemData.GetWebDav());
                }
            }

            if (status == LinkStatus.Error)
            {
                result.Status = Status.Error;
                result.StackTrace = stackTraceMessage;
                result.Message = string.Format("Not able to unlink \"{1}\" from \"{0}\".", itemData.GetWebDav(), dependentItemData == null ? tcmDependentItem : dependentItemData.GetWebDav());
            }

            if (status != LinkStatus.NotFound)
                results.Add(result);

            return status;
        }

        private static LinkStatus RemoveFolderLinkedSchema(string folderUri, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            FolderData innerFolderData = ReadItem(folderUri) as FolderData;
            if (innerFolderData == null || innerFolderData.LinkedSchema == null || string.IsNullOrEmpty(innerFolderData.LinkedSchema.IdRef) || innerFolderData.LinkedSchema.IdRef == "tcm:0-0-0")
                return LinkStatus.NotFound;

            try
            {
                //change schema id
                innerFolderData.LinkedSchema.IdRef = "tcm:0-0-0";

                //make non-mandatory to aviod conflicts with inner components
                innerFolderData.IsLinkedSchemaMandatory = false;

                Client.Save(innerFolderData, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveFolderLinkedSchema(string folderUri)
        {
            FolderData innerFolderData = ReadItem(folderUri) as FolderData;
            if (innerFolderData == null || innerFolderData.LinkedSchema == null || string.IsNullOrEmpty(innerFolderData.LinkedSchema.IdRef) || innerFolderData.LinkedSchema.IdRef == "tcm:0-0-0")
                return LinkStatus.NotFound;

            return LinkStatus.Found;
        }

        private static LinkStatus RemoveMetadataSchema(string itemUri, string schemaUri, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            RepositoryLocalObjectData itemData = ReadItem(itemUri) as RepositoryLocalObjectData;
            if (itemData == null || itemData.MetadataSchema == null || string.IsNullOrEmpty(itemData.MetadataSchema.IdRef) || itemData.MetadataSchema.IdRef != schemaUri)
                return LinkStatus.NotFound;

            if (itemData is VersionedItemData)
            {
                VersionedItemData versionedItemData = (VersionedItemData)itemData;

                if (versionedItemData.BluePrintInfo.IsShared == true)
                {
                    itemUri = GetBluePrintTopTcmId(itemUri);

                    versionedItemData = ReadItem(itemUri) as VersionedItemData;
                    if (versionedItemData == null)
                        return LinkStatus.NotFound;
                }

                try
                {
                    versionedItemData = Client.CheckOut(versionedItemData.Id, true, new ReadOptions());
                }
                catch (Exception ex)
                {

                }

                if (versionedItemData == null)
                    return LinkStatus.NotFound;

                //change schema id
                versionedItemData.MetadataSchema.IdRef = "tcm:0-0-0";

                try
                {
                    versionedItemData = (VersionedItemData)Client.Update(versionedItemData, new ReadOptions());
                    Client.CheckIn(versionedItemData.Id, new ReadOptions());
                    return LinkStatus.Found;
                }
                catch (Exception ex)
                {
                    stackTraceMessage = ex.Message;

                    if (versionedItemData == null)
                        return LinkStatus.Error;

                    Client.UndoCheckOut(versionedItemData.Id, true, new ReadOptions());
                    return LinkStatus.Error;
                }
            }

            try
            {
                //change schema id
                itemData.MetadataSchema.IdRef = "tcm:0-0-0";

                Client.Save(itemData, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveMetadataSchema(string itemUri, string schemaUri)
        {
            RepositoryLocalObjectData itemData = ReadItem(itemUri) as RepositoryLocalObjectData;
            if (itemData == null || itemData.MetadataSchema == null || string.IsNullOrEmpty(itemData.MetadataSchema.IdRef) || itemData.MetadataSchema.IdRef != schemaUri)
                return LinkStatus.NotFound;

            return LinkStatus.Found;
        }

        private static LinkStatus RemoveParameterSchema(string itemUri, string schemaUri, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            TemplateData templateData = ReadItem(itemUri) as TemplateData;
            if (templateData == null || templateData.ParameterSchema == null || string.IsNullOrEmpty(templateData.ParameterSchema.IdRef) || templateData.ParameterSchema.IdRef != schemaUri)
                return LinkStatus.NotFound;

            if (templateData.BluePrintInfo.IsShared == true)
            {
                itemUri = GetBluePrintTopTcmId(itemUri);

                templateData = ReadItem(itemUri) as TemplateData;
                if (templateData == null)
                    return LinkStatus.NotFound;
            }

            try
            {
                templateData = Client.CheckOut(templateData.Id, true, new ReadOptions()) as TemplateData;
            }
            catch (Exception ex)
            {

            }

            if (templateData == null)
                return LinkStatus.NotFound;

            //change schema id
            templateData.ParameterSchema.IdRef = "tcm:0-0-0";

            try
            {
                templateData = (TemplateData)Client.Update(templateData, new ReadOptions());
                Client.CheckIn(templateData.Id, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;

                if (templateData == null)
                    return LinkStatus.Error;

                Client.UndoCheckOut(templateData.Id, true, new ReadOptions());
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveParameterSchema(string itemUri, string schemaUri)
        {
            TemplateData templateData = ReadItem( itemUri) as TemplateData;
            if (templateData == null || templateData.ParameterSchema == null || string.IsNullOrEmpty(templateData.ParameterSchema.IdRef) || templateData.ParameterSchema.IdRef != schemaUri)
                return LinkStatus.NotFound;

            return LinkStatus.Found;
        }

        private static LinkStatus RemoveCTLinkedSchema(string tcmComponentTemplate, string tcmSchema, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            ComponentTemplateData componentTemplate = ReadItem(tcmComponentTemplate) as ComponentTemplateData;
            if (componentTemplate == null)
                return LinkStatus.NotFound;

            if (componentTemplate.RelatedSchemas.All(x => x.IdRef != tcmSchema))
                return LinkStatus.NotFound;

            if (componentTemplate.BluePrintInfo.IsShared == true)
            {
                tcmComponentTemplate = GetBluePrintTopTcmId(tcmComponentTemplate);

                componentTemplate = ReadItem(tcmComponentTemplate) as ComponentTemplateData;
                if (componentTemplate == null)
                    return LinkStatus.NotFound;
            }

            try
            {
                componentTemplate = Client.CheckOut(componentTemplate.Id, true, new ReadOptions()) as ComponentTemplateData;
            }
            catch (Exception ex)
            {

            }

            if (componentTemplate == null)
                return LinkStatus.NotFound;

            componentTemplate.RelatedSchemas = componentTemplate.RelatedSchemas.Where(x => x.IdRef != tcmSchema).ToArray();

            try
            {
                componentTemplate = (ComponentTemplateData)Client.Update(componentTemplate, new ReadOptions());
                Client.CheckIn(componentTemplate.Id, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;

                if (componentTemplate == null)
                    return LinkStatus.Error;

                Client.UndoCheckOut(componentTemplate.Id, true, new ReadOptions());
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveCTLinkedSchema(string tcmComponentTemplate, string tcmSchema)
        {
            ComponentTemplateData componentTemplate = ReadItem(tcmComponentTemplate) as ComponentTemplateData;
            if (componentTemplate == null)
                return LinkStatus.NotFound;

            if (componentTemplate.RelatedSchemas.All(x => x.IdRef != tcmSchema))
                return LinkStatus.NotFound;

            return LinkStatus.Found;
        }

        private static LinkStatus RemoveComponentPresentation(string tcmPage, string tcmDependentItem, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            PageData page = ReadItem(tcmPage) as PageData;
            if (page == null)
                return LinkStatus.NotFound;

            ComponentPresentationData[] newComponentPresentations = page.ComponentPresentations.Where(x => x.Component.IdRef.GetId() != tcmDependentItem.GetId() && x.ComponentTemplate.IdRef.GetId() != tcmDependentItem.GetId()).ToArray();

            if (page.ComponentPresentations.Length == newComponentPresentations.Length)
                return LinkStatus.NotFound;

            if (page.BluePrintInfo.IsShared == true)
            {
                tcmPage = GetBluePrintTopTcmId(tcmPage);

                page = ReadItem(tcmPage) as PageData;
                if (page == null)
                    return LinkStatus.NotFound;
            }

            try
            {
                page = Client.CheckOut(page.Id, true, new ReadOptions()) as PageData;
            }
            catch (Exception ex)
            {

            }

            if (page == null)
                return LinkStatus.NotFound;

            page.ComponentPresentations = newComponentPresentations;

            try
            {
                page = (PageData)Client.Update(page, new ReadOptions());
                Client.CheckIn(page.Id, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;

                if (page == null)
                    return LinkStatus.Error;

                Client.UndoCheckOut(page.Id, true, new ReadOptions());
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveComponentPresentation(string tcmPage, string tcmDependentItem)
        {
            PageData page = ReadItem(tcmPage) as PageData;
            if (page == null)
                return LinkStatus.NotFound;

            return page.ComponentPresentations.Any(x => x.Component.IdRef.GetId() == tcmDependentItem.GetId() || x.ComponentTemplate.IdRef.GetId() == tcmDependentItem.GetId()) ? LinkStatus.Found : LinkStatus.NotFound;
        }

        public static List<TbbInfo> GetTbbList(string templateContent)
        {
            List<TbbInfo> tbbList = new List<TbbInfo>();

            XNamespace ns = "http://www.tridion.com/ContentManager/5.3/CompoundTemplate";
            XNamespace linkNs = "http://www.w3.org/1999/xlink";

            XDocument xml = XDocument.Parse(templateContent);

            if (xml.Root == null)
                return tbbList;

            List<XElement> templateInvocations = xml.Root.Elements(ns + "TemplateInvocation").ToList();
            foreach (XElement invovation in templateInvocations)
            {
                TbbInfo tbbInfo = new TbbInfo();

                XElement template = invovation.Elements(ns + "Template").FirstOrDefault();
                if (template != null)
                {
                    tbbInfo.TcmId = template.Attribute(linkNs + "href").Value;
                    tbbInfo.Title = template.Attribute(linkNs + "title").Value;
                }

                XElement templateParameters = invovation.Elements(ns + "TemplateParameters").FirstOrDefault();
                if (templateParameters != null)
                {
                    tbbInfo.TemplateParameters = templateParameters;
                }

                tbbList.Add(tbbInfo);
            }

            return tbbList;
        }

        private static string GetTemplateContent(List<TbbInfo> tbbList)
        {
            XNamespace ns = "http://www.tridion.com/ContentManager/5.3/CompoundTemplate";

            XElement root = new XElement(ns + "CompoundTemplate");
            foreach (TbbInfo tbbInfo in tbbList)
            {
                XElement templateInvocation = new XElement(ns + "TemplateInvocation");
                
                XElement template = GetComponentLink(tbbInfo.TcmId, tbbInfo.Title, "Template");
                if(template != null)
                    templateInvocation.Add(template);

                if(tbbInfo.TemplateParameters != null)
                    templateInvocation.Add(tbbInfo.TemplateParameters);

                root.Add(templateInvocation);
            }

            return root.ToString().Replace(" xmlns=\"\"", "");
        }

        private static string RemoveTbbFromTemplate(string templateContent, string tcmTbb)
        {
            List<TbbInfo> tbbList = GetTbbList(templateContent).Where(x => x.TcmId.GetId() != tcmTbb.GetId()).ToList();
            return GetTemplateContent(tbbList);
        }

        private static LinkStatus RemoveTbbFromPageTemplate(string tcmPageTemplate, string tcmTbb, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            PageTemplateData pageTemplate = ReadItem(tcmPageTemplate) as PageTemplateData;
            if (pageTemplate == null)
                return LinkStatus.NotFound;

            List<TbbInfo> tbbList = GetTbbList(pageTemplate.Content);
            if (tbbList.Any(x => x.TcmId.GetId() == tcmTbb.GetId()))
            {
                if (tbbList.Count == 1)
                    return LinkStatus.Mandatory;
            }
            else
            {
                return LinkStatus.NotFound;
            }

            string newContent = RemoveTbbFromTemplate(pageTemplate.Content, tcmTbb);

            if (pageTemplate.BluePrintInfo.IsShared == true)
            {
                tcmPageTemplate = GetBluePrintTopTcmId(tcmPageTemplate);

                pageTemplate = ReadItem(tcmPageTemplate) as PageTemplateData;
                if (pageTemplate == null)
                    return LinkStatus.NotFound;
            }

            try
            {
                pageTemplate = Client.CheckOut(pageTemplate.Id, true, new ReadOptions()) as PageTemplateData;
            }
            catch (Exception ex)
            {

            }

            if (pageTemplate == null)
                return LinkStatus.NotFound;

            pageTemplate.Content = newContent;

            try
            {
                pageTemplate = (PageTemplateData)Client.Update(pageTemplate, new ReadOptions());
                Client.CheckIn(pageTemplate.Id, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;

                if (pageTemplate == null)
                    return LinkStatus.Error;

                Client.UndoCheckOut(pageTemplate.Id, true, new ReadOptions());
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveTbbFromPageTemplate(string tcmPageTemplate, string tcmTbb)
        {
            PageTemplateData pageTemplate = ReadItem(tcmPageTemplate) as PageTemplateData;
            if (pageTemplate == null)
                return LinkStatus.NotFound;

            List<TbbInfo> tbbList = GetTbbList(pageTemplate.Content);
            if (tbbList.Any(x => x.TcmId.GetId() == tcmTbb.GetId()))
            {
                return tbbList.Count == 1 ? LinkStatus.Mandatory : LinkStatus.Found;
            }
            return LinkStatus.NotFound;
        }

        private static LinkStatus RemoveTbbFromComponentTemplate(string tcmComponentTemplate, string tcmTbb, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            ComponentTemplateData componentTemplate = ReadItem(tcmComponentTemplate) as ComponentTemplateData;
            if (componentTemplate == null)
                return LinkStatus.NotFound;

            List<TbbInfo> tbbList = GetTbbList(componentTemplate.Content);
            if (tbbList.Any(x => x.TcmId.GetId() == tcmTbb.GetId()))
            {
                if (tbbList.Count == 1)
                    return LinkStatus.Mandatory;
            }
            else
            {
                return LinkStatus.NotFound;
            }

            string newContent = RemoveTbbFromTemplate(componentTemplate.Content, tcmTbb);

            if (componentTemplate.BluePrintInfo.IsShared == true)
            {
                tcmComponentTemplate = GetBluePrintTopTcmId(tcmComponentTemplate);

                componentTemplate = ReadItem(tcmComponentTemplate) as ComponentTemplateData;
                if (componentTemplate == null)
                    return LinkStatus.NotFound;
            }

            try
            {
                componentTemplate = Client.CheckOut(componentTemplate.Id, true, new ReadOptions()) as ComponentTemplateData;
            }
            catch (Exception ex)
            {

            }

            if (componentTemplate == null)
                return LinkStatus.NotFound;

            componentTemplate.Content = newContent;

            try
            {
                componentTemplate = (ComponentTemplateData)Client.Update(componentTemplate, new ReadOptions());
                Client.CheckIn(componentTemplate.Id, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;

                if (componentTemplate == null)
                    return LinkStatus.Error;

                Client.UndoCheckOut(componentTemplate.Id, true, new ReadOptions());
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveTbbFromComponentTemplate(string tcmComponentTemplate, string tcmTbb)
        {
            ComponentTemplateData componentTemplate = ReadItem(tcmComponentTemplate) as ComponentTemplateData;
            if (componentTemplate == null)
                return LinkStatus.NotFound;

            List<TbbInfo> tbbList = GetTbbList(componentTemplate.Content);
            if (tbbList.Any(x => x.TcmId.GetId() == tcmTbb.GetId()))
            {
                return tbbList.Count == 1 ? LinkStatus.Mandatory : LinkStatus.Found;
            }
            return LinkStatus.NotFound;
        }

        private static LinkStatus RemoveTbbFromCompoundTbb(string tcmCompoundTbb, string tcmTbb, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            TemplateBuildingBlockData compoundTbb = ReadItem(tcmCompoundTbb) as TemplateBuildingBlockData;
            if (compoundTbb == null || compoundTbb.TemplateType != "CompoundTemplate")
                return LinkStatus.NotFound;

            List<TbbInfo> tbbList = GetTbbList(compoundTbb.Content);
            if (tbbList.Any(x => x.TcmId.GetId() == tcmTbb.GetId()))
            {
                if (tbbList.Count == 1)
                    return LinkStatus.Mandatory;
            }
            else
            {
                return LinkStatus.NotFound;
            }

            string newContent = RemoveTbbFromTemplate(compoundTbb.Content, tcmTbb);

            if (compoundTbb.BluePrintInfo.IsShared == true)
            {
                tcmCompoundTbb = GetBluePrintTopTcmId(tcmCompoundTbb);

                compoundTbb = ReadItem(tcmCompoundTbb) as TemplateBuildingBlockData;
                if (compoundTbb == null)
                    return LinkStatus.NotFound;
            }

            try
            {
                compoundTbb = Client.CheckOut(compoundTbb.Id, true, new ReadOptions()) as TemplateBuildingBlockData;
            }
            catch (Exception ex)
            {

            }

            if (compoundTbb == null)
                return LinkStatus.NotFound;

            compoundTbb.Content = newContent;

            try
            {
                compoundTbb = (TemplateBuildingBlockData)Client.Update(compoundTbb, new ReadOptions());
                Client.CheckIn(compoundTbb.Id, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;

                if (compoundTbb == null)
                    return LinkStatus.Error;

                Client.UndoCheckOut(compoundTbb.Id, true, new ReadOptions());
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveTbbFromCompoundTbb(string tcmCompoundTbb, string tcmTbb)
        {
            TemplateBuildingBlockData compoundTbb = ReadItem(tcmCompoundTbb) as TemplateBuildingBlockData;
            if (compoundTbb == null || compoundTbb.TemplateType != "CompoundTemplate")
                return LinkStatus.NotFound;

            List<TbbInfo> tbbList = GetTbbList(compoundTbb.Content);
            if (tbbList.Any(x => x.TcmId.GetId() == tcmTbb.GetId()))
            {
                return tbbList.Count == 1 ? LinkStatus.Mandatory : LinkStatus.Found;
            }
            return LinkStatus.NotFound;
        }

        private static List<ComponentFieldData> RemoveLinkFromValues(List<ComponentFieldData> values, XNamespace ns, string linkUri)
        {
            List<ComponentFieldData> newValues = new List<ComponentFieldData>();
            XNamespace linkNs = "http://www.w3.org/1999/xlink";

            foreach (ComponentFieldData value in values)
            {
                if ((value.SchemaField.IsComponentLink() || value.SchemaField.IsMultimedia() || value.SchemaField.IsKeyword()) && value.IsMultiValue && value.Value is IList && ((IList)value.Value).Cast<XElement>().Any(x => x.Attribute(linkNs + "href").Value.GetId() == linkUri.GetId()))
                {
                    List<XElement> elements = ((IList)value.Value).Cast<XElement>().Where(x => x.Attribute(linkNs + "href").Value.GetId() != linkUri.GetId()).ToList();

                    if (value.IsMandatory && elements.Count == 0)
                        return values;

                    value.Value = elements;
                    newValues.Add(value);
                }
                else if ((value.SchemaField.IsComponentLink() || value.SchemaField.IsMultimedia() || value.SchemaField.IsKeyword()) && value.Value is XElement && ((XElement)value.Value).Attribute(linkNs + "href") != null && ((XElement)value.Value).Attribute(linkNs + "href").Value.GetId() == linkUri.GetId())
                {
                    if (value.IsMandatory)
                        return values;
                }
                else if (value.SchemaField.IsEmbedded())
                {
                    if (value.Value is XElement)
                    {
                        value.Value = RemoveLinkFromXml((XElement)value.Value, ns, (EmbeddedSchemaFieldDefinitionData)value.SchemaField, linkUri);
                        newValues.Add(value);
                    }
                    else if (value.Value is IList)
                    {
                        value.Value = ((IList)value.Value).Cast<XElement>().Select(x => RemoveLinkFromXml(x, ns, (EmbeddedSchemaFieldDefinitionData)value.SchemaField, linkUri)).ToList();
                        newValues.Add(value);
                    }
                }
                else
                {
                    newValues.Add(value);
                }
            }

            return newValues;
        }

        private static LinkStatus CheckRemoveLinkFromValues(XElement xml, XNamespace ns, List<ItemFieldDefinitionData> schemaFields, string linkUri)
        {
            XNamespace linkNs = "http://www.w3.org/1999/xlink";

            List<ComponentFieldData> values = GetValues(ns, schemaFields, xml);

            foreach (ComponentFieldData value in values)
            {
                if ((value.SchemaField.IsComponentLink() || value.SchemaField.IsMultimedia() || value.SchemaField.IsKeyword()) && value.IsMultiValue && value.Value is IList && ((IList)value.Value).Cast<XElement>().Any(x => x.Attribute(linkNs + "href").Value.GetId() == linkUri.GetId()))
                {
                    List<XElement> elements = ((IList)value.Value).Cast<XElement>().Where(x => x.Attribute(linkNs + "href").Value.GetId() != linkUri.GetId()).ToList();

                    if (value.IsMandatory && elements.Count == 0)
                        return LinkStatus.Mandatory;
                    
                    return LinkStatus.Found;
                }
                if ((value.SchemaField.IsComponentLink() || value.SchemaField.IsMultimedia() || value.SchemaField.IsKeyword()) && value.Value is XElement && ((XElement)value.Value).Attribute(linkNs + "href") != null && ((XElement)value.Value).Attribute(linkNs + "href").Value.GetId() == linkUri.GetId())
                {
                    if (value.IsMandatory)
                        return LinkStatus.Mandatory;

                    return LinkStatus.Found;
                }
                if (value.SchemaField.IsEmbedded())
                {
                    List<ItemFieldDefinitionData> embeddedSchemaFields = GetSchemaFields(((EmbeddedSchemaFieldDefinitionData)value.SchemaField).EmbeddedSchema.IdRef);
                    
                    if (value.Value is XElement)
                    {
                        LinkStatus status = CheckRemoveLinkFromValues((XElement)value.Value, ns, embeddedSchemaFields, linkUri);
                        if (status != LinkStatus.NotFound)
                            return status;
                    }
                    else if (value.Value is IList)
                    {
                        foreach (XElement childValue in ((IList)value.Value).Cast<XElement>())
                        {
                            LinkStatus status = CheckRemoveLinkFromValues(childValue, ns, embeddedSchemaFields, linkUri);
                            if (status != LinkStatus.NotFound)
                                return status;
                        }
                    }
                }
            }

            return LinkStatus.NotFound;
        }

        //gets list of pairs schemaUri|field that are mandatory and contains specified link
        private static Dictionary<string, ItemFieldDefinitionData> GetMandatoryLinkFields(XElement xml, XNamespace ns, string schemaUri, string linkUri)
        {
            Dictionary<string, ItemFieldDefinitionData> res = new Dictionary<string, ItemFieldDefinitionData>();

            if (xml == null || string.IsNullOrEmpty(schemaUri) || string.IsNullOrEmpty(linkUri) || schemaUri == "tcm:0-0-0" || linkUri == "tcm:0-0-0")
                return res;

            SchemaData schema = ReadItem(schemaUri) as SchemaData;
            if (schema == null)
                return res;

            List<ItemFieldDefinitionData> schemaFields = schema.Purpose == SchemaPurpose.Metadata ? GetSchemaMetadataFields(schemaUri) : GetSchemaFields(schemaUri);

            List<ComponentFieldData> values = GetValues(ns, schemaFields, xml);

            XNamespace linkNs = "http://www.w3.org/1999/xlink";

            foreach (ComponentFieldData value in values)
            {
                if ((value.SchemaField.IsComponentLink() || value.SchemaField.IsMultimedia() || value.SchemaField.IsKeyword()) && value.IsMultiValue && value.Value is IList && ((IList)value.Value).Cast<XElement>().Any(x => x.Attribute(linkNs + "href").Value.GetId() == linkUri.GetId()))
                {
                    List<XElement> elements = ((IList)value.Value).Cast<XElement>().Where(x => x.Attribute(linkNs + "href").Value.GetId() != linkUri.GetId()).ToList();

                    if (value.IsMandatory && elements.Count == 0)
                        res.Add(schemaUri, value.SchemaField);
                }
                if ((value.SchemaField.IsComponentLink() || value.SchemaField.IsMultimedia() || value.SchemaField.IsKeyword()) && value.Value is XElement && ((XElement)value.Value).Attribute(linkNs + "href") != null && ((XElement)value.Value).Attribute(linkNs + "href").Value.GetId() == linkUri.GetId())
                {
                    if (value.IsMandatory)
                        res.Add(schemaUri, value.SchemaField);
                }
                if (value.SchemaField.IsEmbedded())
                {
                    if (value.Value is XElement)
                    {
                        Dictionary<string, ItemFieldDefinitionData> resEmbedded = GetMandatoryLinkFields((XElement)value.Value, ns, ((EmbeddedSchemaFieldDefinitionData)value.SchemaField).EmbeddedSchema.IdRef, linkUri);
                        if (resEmbedded.Count > 0)
                        {
                            foreach (KeyValuePair<string, ItemFieldDefinitionData> pair in resEmbedded)
                            {
                                res.Add(pair.Key, pair.Value);
                            }
                        }
                    }
                    else if (value.Value is IList)
                    {
                        foreach (XElement childValue in ((IList)value.Value).Cast<XElement>())
                        {
                            Dictionary<string, ItemFieldDefinitionData> resEmbedded = GetMandatoryLinkFields(childValue, ns, ((EmbeddedSchemaFieldDefinitionData)value.SchemaField).EmbeddedSchema.IdRef, linkUri);
                            if (resEmbedded.Count > 0)
                            {
                                foreach (KeyValuePair<string, ItemFieldDefinitionData> pair in resEmbedded)
                                {
                                    res.Add(pair.Key, pair.Value);
                                }
                            }
                        }
                    }
                }
            }

            return res;
        }

        private static string RemoveLinkFromXml(string schemaUri, string xmlContent, string linkUri)
        {
            if (string.IsNullOrEmpty(xmlContent))
                return xmlContent;

            SchemaData schema = ReadItem(schemaUri) as SchemaData;
            if(schema == null)
                return xmlContent;

            List<ItemFieldDefinitionData> schemaFields = schema.Purpose == SchemaPurpose.Metadata ? GetSchemaMetadataFields(schemaUri) : GetSchemaFields(schemaUri);

            List<ComponentFieldData> values = GetValues(schema.NamespaceUri, schemaFields, xmlContent);

            List<ComponentFieldData> newValues = RemoveLinkFromValues(values, schema.NamespaceUri, linkUri);

            XElement xml = GetComponentXml(schema.NamespaceUri, schema.RootElementName, newValues);

            return xml == null ? string.Empty : xml.ToString().PrettyXml();
        }

        private static XElement RemoveLinkFromXml(XElement parent, XNamespace ns, EmbeddedSchemaFieldDefinitionData embeddedSchemaField, string linkUri)
        {
            List<ItemFieldDefinitionData> schemaFields = GetSchemaFields(embeddedSchemaField.EmbeddedSchema.IdRef);

            List<ComponentFieldData> values = GetValues(ns, schemaFields, parent);

            List<ComponentFieldData> newValues = RemoveLinkFromValues(values, ns, linkUri);

            return GetComponentXml(ns, parent.Name.LocalName, newValues);
        }

        private static LinkStatus RemoveLinkFromComponent(string tcmComponent, string tcmLink, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            ComponentData component = ReadItem(tcmComponent) as ComponentData;
            if (component == null)
                return LinkStatus.NotFound;

            SchemaData schema = ReadItem(component.Schema.IdRef) as SchemaData;
            if(schema == null)
                return LinkStatus.NotFound;

            List<ItemFieldDefinitionData> schemaFields = GetSchemaFields(component.Schema.IdRef);

            LinkStatus status = CheckRemoveLinkFromValues(XElement.Parse(component.Content), schema.NamespaceUri, schemaFields, tcmLink);
            if (status != LinkStatus.Found)
                return status;

            string newContent = RemoveLinkFromXml(component.Schema.IdRef, component.Content, tcmLink);
            string newMetadata = RemoveLinkFromXml(component.MetadataSchema.IdRef, component.Metadata, tcmLink);

            if (component.BluePrintInfo.IsShared == true)
            {
                tcmComponent = GetBluePrintTopTcmId(tcmComponent);

                component = ReadItem(tcmComponent) as ComponentData;
                if (component == null)
                    return LinkStatus.NotFound;
            }

            try
            {
                component = Client.CheckOut(component.Id, true, new ReadOptions()) as ComponentData;
            }
            catch (Exception ex)
            {

            }

            if (component == null)
                return LinkStatus.NotFound;

            component.Content = newContent;
            component.Metadata = newMetadata;

            try
            {
                component = (ComponentData)Client.Update(component, new ReadOptions());
                Client.CheckIn(component.Id, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;

                if (component == null)
                    return LinkStatus.Error;

                Client.UndoCheckOut(component.Id, true, new ReadOptions());
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveLinkFromComponent(string tcmComponent, string tcmLink)
        {
            ComponentData component = ReadItem(tcmComponent) as ComponentData;
            if (component == null)
                return LinkStatus.NotFound;

            SchemaData schema = ReadItem(component.Schema.IdRef) as SchemaData;
            if (schema == null)
                return LinkStatus.NotFound;

            List<ItemFieldDefinitionData> schemaFields = GetSchemaFields(component.Schema.IdRef);

            return CheckRemoveLinkFromValues(XElement.Parse(component.Content), schema.NamespaceUri, schemaFields, tcmLink);
        }

        private static LinkStatus RemoveLinkFromMetadata(string tcmItem, string tcmLink, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            RepositoryLocalObjectData item = ReadItem(tcmItem) as RepositoryLocalObjectData;
            if (item == null)
                return LinkStatus.NotFound;

            SchemaData metadataSchema = ReadItem(item.MetadataSchema.IdRef) as SchemaData;
            if (metadataSchema == null)
                return LinkStatus.NotFound;

            List<ItemFieldDefinitionData> metadataSchemaFields = GetSchemaFields(item.MetadataSchema.IdRef);

            LinkStatus status = CheckRemoveLinkFromValues(XElement.Parse(item.Metadata), metadataSchema.NamespaceUri, metadataSchemaFields, tcmLink);
            if (status != LinkStatus.Found)
                return status;

            string newMetadata = RemoveLinkFromXml(item.MetadataSchema.IdRef, item.Metadata, tcmLink);

            if (item.BluePrintInfo.IsShared == true)
            {
                tcmItem = GetBluePrintTopTcmId(tcmItem);

                item = ReadItem(tcmItem) as RepositoryLocalObjectData;
                if (item == null)
                    return LinkStatus.NotFound;
            }

            try
            {
                item = Client.CheckOut(item.Id, true, new ReadOptions());
            }
            catch (Exception ex)
            {

            }

            if (item == null)
                return LinkStatus.NotFound;

            item.Metadata = newMetadata;

            try
            {
                item = (RepositoryLocalObjectData)Client.Update(item, new ReadOptions());
                Client.CheckIn(item.Id, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;
                if (item == null)
                    return LinkStatus.Error;

                Client.UndoCheckOut(item.Id, true, new ReadOptions());
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveLinkFromMetadata(string tcmItem, string tcmLink)
        {
            RepositoryLocalObjectData item = ReadItem(tcmItem) as RepositoryLocalObjectData;
            if (item == null || item.MetadataSchema == null || item.MetadataSchema.IdRef == "tcm:0-0-0")
                return LinkStatus.NotFound;

            SchemaData metadataSchema = ReadItem(item.MetadataSchema.IdRef) as SchemaData;
            if (metadataSchema == null)
                return LinkStatus.NotFound;

            List<ItemFieldDefinitionData> metadataSchemaFields = GetSchemaFields(item.MetadataSchema.IdRef);

            return CheckRemoveLinkFromValues(XElement.Parse(item.Metadata), metadataSchema.NamespaceUri, metadataSchemaFields, tcmLink);
        }

        private static void RemoveSchemaMandatoryLinkFields(RepositoryLocalObjectData itemData, string tcmDependentItem, List<ResultInfo> results)
        {
            string schemaUri = string.Empty;
            XElement xml = null;

            if (itemData is ComponentData)
            {
                ComponentData component = (ComponentData)itemData;
                schemaUri = component.Schema.IdRef;
                xml = XElement.Parse(component.Content);
            }
            else if (!string.IsNullOrEmpty(itemData.Metadata) && !string.IsNullOrEmpty(schemaUri) && schemaUri != "tcm:0-0-0")
            {
                schemaUri = itemData.MetadataSchema.IdRef;
                xml = XElement.Parse(itemData.Metadata);
            }

            SchemaData schema = ReadItem(schemaUri) as SchemaData;
            if (schema == null)
                return;

            Dictionary<string, ItemFieldDefinitionData> mandatoryLinkFields = GetMandatoryLinkFields(xml, schema.NamespaceUri, schemaUri, tcmDependentItem);

            foreach (string innerSchemaUri in mandatoryLinkFields.Keys)
            {
                ItemFieldDefinitionData field = mandatoryLinkFields[innerSchemaUri];

                SchemaData innerSchemaData = ReadItem(innerSchemaUri) as SchemaData;
                if (innerSchemaData == null)
                    continue;

                SchemaFieldsData schemaFieldsData = Client.ReadSchemaFields(innerSchemaUri, false, null);
                if (schemaFieldsData.Fields != null && schemaFieldsData.Fields.Any(x => x.Name == field.Name))
                {
                    schemaFieldsData.Fields.First(x => x.Name == field.Name).MinOccurs = 0;
                }
                if (schemaFieldsData.MetadataFields != null && schemaFieldsData.MetadataFields.Any(x => x.Name == field.Name))
                {
                    schemaFieldsData.MetadataFields.First(x => x.Name == field.Name).MinOccurs = 0;
                }

                try
                {
                    innerSchemaData = Client.CheckOut(innerSchemaData.Id, true, new ReadOptions()) as SchemaData;
                }
                catch (Exception ex)
                {

                }

                if (innerSchemaData == null)
                    return;

                innerSchemaData.Xsd = Client.ConvertSchemaFieldsToXsd(schemaFieldsData).ToString();

                try
                {
                    Client.Save(innerSchemaData, new ReadOptions());
                    Client.CheckIn(innerSchemaUri, new ReadOptions());

                    results.Add(new ResultInfo
                    {
                        Status = Status.Success,
                        Item = innerSchemaData.ToItem(),
                        Message = string.Format("Make non-mandatory field \"{0}\" in \"{1}\".", field.Name, innerSchemaData.GetWebDav())
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new ResultInfo
                    {
                        Message = string.Format("Error removing folder linked schema for \"{0}\". Error message \"{1}\"", itemData.GetWebDav(), ex.Message),
                        Item = itemData.ToItem(),
                        Status = Status.Error,
                        StackTrace = ex.StackTrace
                    });
                }
            }
        }

        private static void CheckRemoveSchemaMandatoryLinkFields(RepositoryLocalObjectData itemData, string tcmDependentItem, List<ResultInfo> results)
        {
            string schemaUri = string.Empty;
            XElement xml = null;

            if (itemData is ComponentData)
            {
                ComponentData component = (ComponentData)itemData;
                schemaUri = component.Schema.IdRef;
                xml = XElement.Parse(component.Content);
            }
            else if (!string.IsNullOrEmpty(itemData.Metadata) && !string.IsNullOrEmpty(schemaUri) && schemaUri != "tcm:0-0-0")
            {
                schemaUri = itemData.MetadataSchema.IdRef;
                xml = XElement.Parse(itemData.Metadata);
            }

            SchemaData schema = ReadItem(schemaUri) as SchemaData;
            if (schema == null)
                return;

            Dictionary<string, ItemFieldDefinitionData> mandatoryLinkFields = GetMandatoryLinkFields(xml, schema.NamespaceUri, schemaUri, tcmDependentItem);

            foreach (string innerSchemaUri in mandatoryLinkFields.Keys)
            {
                ItemFieldDefinitionData field = mandatoryLinkFields[innerSchemaUri];

                SchemaData innerSchemaData = ReadItem(innerSchemaUri) as SchemaData;
                if (innerSchemaData == null)
                    continue;

                results.Add(new ResultInfo
                {
                    Status = Status.Info,
                    Item = innerSchemaData.ToItem(),
                    Message = string.Format("Make non-mandatory field \"{0}\" in \"{1}\".", field.Name, innerSchemaData.GetWebDav())
                });
            }
        }

        private static LinkStatus RemoveKeywordField(string schemaUri, string categoryUri, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            SchemaData schema = ReadItem(schemaUri) as SchemaData;
            if (schema == null)
                return LinkStatus.NotFound;

            if (schema.BluePrintInfo.IsShared == true)
            {
                schemaUri = GetBluePrintTopTcmId(schemaUri);

                schema = ReadItem(schemaUri) as SchemaData;
                if (schema == null)
                    return LinkStatus.NotFound;
            }

            SchemaFieldsData schemaFieldsData = Client.ReadSchemaFields(schemaUri, false, null);

            if (schema.Purpose != SchemaPurpose.Metadata && schemaFieldsData.Fields != null && schemaFieldsData.Fields.Any())
            {
                List<ItemFieldDefinitionData> schemaFields = schemaFieldsData.Fields.ToList();
                for (int index = 0; index < schemaFields.Count; index++)
                {
                    if (!(schemaFields[index] is KeywordFieldDefinitionData))
                        continue;

                    KeywordFieldDefinitionData keywordField = (KeywordFieldDefinitionData)schemaFields[index];
                    if (keywordField.Category.IdRef.GetId() != categoryUri.GetId())
                        continue;

                    schemaFieldsData.Fields[index] = new SingleLineTextFieldDefinitionData { Name = keywordField.Name, Description = keywordField.Description, DefaultValue = keywordField.DefaultValue == null ? null : keywordField.DefaultValue.Title, MinOccurs = keywordField.MinOccurs, MaxOccurs = keywordField.MaxOccurs };
                }
            }

            if (schemaFieldsData.MetadataFields != null && schemaFieldsData.MetadataFields.Any())
            {
                List<ItemFieldDefinitionData> metadataSchemaFields = schemaFieldsData.MetadataFields.ToList();
                for (int index = 0; index < metadataSchemaFields.Count; index++)
                {
                    if (!(metadataSchemaFields[index] is KeywordFieldDefinitionData))
                        continue;

                    KeywordFieldDefinitionData keywordField = (KeywordFieldDefinitionData)metadataSchemaFields[index];
                    if (keywordField.Category.IdRef.GetId() != categoryUri.GetId())
                        continue;

                    schemaFieldsData.MetadataFields[index] = new SingleLineTextFieldDefinitionData { Name = keywordField.Name, Description = keywordField.Description, DefaultValue = keywordField.DefaultValue == null ? null : keywordField.DefaultValue.Title, MinOccurs = keywordField.MinOccurs, MaxOccurs = keywordField.MaxOccurs };
                }
            }

            try
            {
                schema = Client.CheckOut(schema.Id, true, new ReadOptions()) as SchemaData;
            }
            catch (Exception ex)
            {

            }

            if (schema == null)
                return LinkStatus.NotFound;

            schema.Xsd = Client.ConvertSchemaFieldsToXsd(schemaFieldsData).ToString();

            try
            {
                schema = (SchemaData)Client.Update(schema, new ReadOptions());
                Client.CheckIn(schema.Id, new ReadOptions());
                return LinkStatus.Found;
            }
            catch (Exception ex)
            {
                stackTraceMessage = ex.Message;
                if (schema == null)
                    return LinkStatus.Error;

                Client.UndoCheckOut(schema.Id, true, new ReadOptions());
                return LinkStatus.Error;
            }
        }

        private static LinkStatus CheckRemoveKeywordField(string schemaUri, string categoryUri)
        {
            SchemaData schema = ReadItem(schemaUri) as SchemaData;
            if (schema == null)
                return LinkStatus.NotFound;

            SchemaFieldsData schemaFieldsData = Client.ReadSchemaFields(schemaUri, false, null);

            if (schema.Purpose != SchemaPurpose.Metadata && schemaFieldsData.Fields != null && schemaFieldsData.Fields.Any())
            {
                List<ItemFieldDefinitionData> schemaFields = schemaFieldsData.Fields.ToList();
                if (schemaFields.OfType<KeywordFieldDefinitionData>().Any(keywordField => keywordField.Category.IdRef.GetId() == categoryUri.GetId()))
                    return LinkStatus.Found;
            }

            if (schemaFieldsData.MetadataFields != null && schemaFieldsData.MetadataFields.Any())
            {
                List<ItemFieldDefinitionData> metadataSchemaFields = schemaFieldsData.MetadataFields.ToList();
                if (metadataSchemaFields.OfType<KeywordFieldDefinitionData>().Any(keywordField => keywordField.Category.IdRef.GetId() == categoryUri.GetId()))
                    return LinkStatus.Found;
            }

            return LinkStatus.NotFound;
        }

        private static LinkStatus RemoveHistory(string tcmItem, string parentTcmId, out string stackTraceMessage)
        {
            stackTraceMessage = "";

            List<HistoryItemInfo> history = GetItemHistory(tcmItem);

            if (history.Count <= 1)
                return LinkStatus.Mandatory;

            LinkStatus status = LinkStatus.NotFound;
            foreach (HistoryItemInfo historyItem in history)
            {
                if (historyItem.TcmId == history.Last().TcmId)
                    continue;

                List<string> historyItemUsedItems = GetUsedItems(historyItem.TcmId);
                if (historyItemUsedItems.Any(x => x.GetId() == parentTcmId.GetId()))
                {
                    try
                    {
                        Client.Delete(historyItem.TcmId);
                        status = LinkStatus.Found;
                    }
                    catch (Exception ex)
                    {
                        stackTraceMessage = ex.Message;
                        return LinkStatus.Error;
                    }
                }
            }

            return status;
        }

        public static void DeleteTridionObject(string tcmItem, bool delete, List<ResultInfo> results, string parentTcmId = "", bool currentVersion = true, int level = 0)
        {
            if (tcmItem.StartsWith("tcm:0-"))
                return;

            if (results.Any(x => x.Status == Status.Error))
                return;

            if (results.Count > 50)
            {
                results.Insert(0, new ResultInfo
                {
                    Message = "Delete stack exceeds 50 items. Please select other item",
                    Item = new ItemInfo { Title = "Delete stack exceeds 50 items" },
                    Status = Status.Error
                });

                return;
            }

            if (!ExistsItem(tcmItem))
                return;

            RepositoryLocalObjectData itemData = (RepositoryLocalObjectData)Client.Read(tcmItem, new ReadOptions());

            if (level > 3)
            {
                results.Add(new ResultInfo
                {
                    Message = String.Format("Recoursion level is bigger than 3. Try to select different item than \"{0}\"", itemData.GetWebDav().CutPath("/", 90, true)),
                    Item = itemData.ToItem(),
                    Status = Status.Error
                });

                return;
            }

            if (itemData.BluePrintInfo.IsShared == true)
            {
                tcmItem = GetBluePrintTopTcmId(tcmItem);

                itemData = ReadItem(tcmItem) as RepositoryLocalObjectData;
                if (itemData == null)
                    return;
            }

            bool isAnyLocalized = IsAnyLocalized(tcmItem);

            List<string> usingItems = GetUsingItems(tcmItem);
            List<string> usingCurrentItems = GetUsingCurrentItems(tcmItem);

            if (currentVersion)
            {
                foreach (string usingItem in usingItems)
                {
                    //using category includes category into using list
                    if (usingItem.GetId() == tcmItem.GetId())
                        continue;

                    LinkStatus status = RemoveDependency(usingItem, tcmItem, delete, results);
                    if (status == LinkStatus.Error)
                    {
                        return;
                    }

                    //not able to unlink objects - delete whole parent object
                    if (status != LinkStatus.Found)
                    {
                        ItemType usingItemType = GetItemType(usingItem);

                        if (usingItemType == ItemType.Folder || usingItemType == ItemType.StructureGroup || usingItemType == ItemType.Category)
                        {
                            DeleteFolderOrStructureGroup(usingItem, delete, results, level + 1);
                        }
                        else
                        {
                            DeleteTridionObject(usingItem, delete, results, tcmItem, usingCurrentItems.Any(x => x == usingItem), level + 1);
                        }
                    }
                }
            }

            if (delete)
            {
                //item is published - not possible to delete - STOP PROCESSING
                if (IsPublished(tcmItem))
                {
                    foreach (ItemInfo publishedItem in GetPublishedItems(itemData.ToItem()))
                    {
                        results.Add(new ResultInfo
                        {
                            Message = string.Format("Item \"{0}\" is published", publishedItem.Path),
                            Item = publishedItem,
                            Status = Status.Error
                        });
                    }

                    return;
                }

                //unlocalize before delete
                if (isAnyLocalized)
                {
                    try
                    {
                        UnLocalizeAll(tcmItem);

                        results.Add(new ResultInfo
                        {
                            Message = string.Format("Unlocalized item \"{0}\"", itemData.GetWebDav()),
                            Item = itemData.ToItem(),
                            Status = Status.Success
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new ResultInfo
                        {
                            Message = string.Format("Error unlocalizing item \"{0}\". Error message \"{1}\"", itemData.GetWebDav(), ex.Message),
                            Item = itemData.ToItem(),
                            Status = Status.Error,
                            StackTrace = ex.StackTrace
                        });
                    }
                }

                //undo checkout
                try
                {
                    Client.UndoCheckOut(tcmItem, true, new ReadOptions());
                }
                catch (Exception ex)
                {

                }

                if (!currentVersion && itemData is VersionedItemData)
                {
                    //remove used versions
                    string stackTraceMessage;
                    LinkStatus status = RemoveHistory(tcmItem, parentTcmId, out stackTraceMessage);
                    if (status == LinkStatus.Found)
                    {
                        results.Add(new ResultInfo
                        {
                            Message = string.Format("Removed history for item \"{0}\"", itemData.GetWebDav()),
                            Item = itemData.ToItem(),
                            Status = Status.Success
                        });
                    }
                    else
                    {
                        results.Add(new ResultInfo
                        {
                            Message = string.Format("Error removing history from item \"{0}\"", itemData.GetWebDav()),
                            Item = itemData.ToItem(),
                            Status = Status.Error,
                            StackTrace = stackTraceMessage
                        });
                    }
                }
                else
                {
                    //delete used item
                    try
                    {
                        Client.Delete(tcmItem);

                        results.Add(new ResultInfo
                        {
                            Message = string.Format("Deleteed item \"{0}\"", itemData.GetWebDav()),
                            Item = itemData.ToItem(),
                            Status = Status.Success
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new ResultInfo
                        {
                            Message = string.Format("Error deleting item \"{0}\". Error message \"{1}\"", itemData.GetWebDav(), ex.Message),
                            Item = itemData.ToItem(),
                            Status = Status.Error,
                            StackTrace = ex.StackTrace
                        });
                    }
                }
            }
            else
            {
                //item is published - not possible to delete - WARNING
                if (IsPublished(tcmItem))
                {
                    foreach (ItemInfo publishedItem in GetPublishedItems(itemData.ToItem()))
                    {
                        results.Add(new ResultInfo
                        {
                            Message = string.Format("Unpublish manually item \"{0}\"", publishedItem.Path),
                            Item = publishedItem,
                            Status = Status.Warning
                        });
                    }
                }

                if (isAnyLocalized)
                {
                    results.Add(new ResultInfo
                    {
                        Message = string.Format("Unlocalize item \"{0}\"", itemData.GetWebDav()),
                        Item = itemData.ToItem(),
                        Status = Status.Info
                    });
                }

                if (!currentVersion)
                {
                    results.Add(new ResultInfo
                    {
                        Message = string.Format("Remove old versions of item \"{0}\"", itemData.GetWebDav()),
                        Item = itemData.ToItem(),
                        Status = Status.Info
                    });
                }
                else
                {
                    results.Add(new ResultInfo
                    {
                        Message = string.Format("Delete item \"{0}\"", itemData.GetWebDav()),
                        Item = itemData.ToItem(),
                        Status = Status.Delete
                    });
                }
            }
        }

        public static void DeleteFolderOrStructureGroup(string tcmFolder, bool delete, List<ResultInfo> results, int level = 0)
        {
            if (results.Any(x => x.Status == Status.Error))
                return;

            if (results.Count > 50)
            {
                results.Insert(0, new ResultInfo
                {
                    Message = "Delete stack exceeds 50 items. Please select other item",
                    Item = new ItemInfo { Title = "Delete stack exceeds 50 items" },
                    Status = Status.Error
                });

                return;
            }

            if (level > 3)
            {
                RepositoryLocalObjectData itemData = (RepositoryLocalObjectData)ReadItem(tcmFolder);

                results.Add(new ResultInfo
                {
                    Message = string.Format("Recoursion level is bigger than 3. Try delete item  manually \"{0}\"", itemData.GetWebDav()),
                    Item = itemData.ToItem(),
                    Status = Status.Error
                });

                return;
            }

            List<BluePrintNodeData> bluePrintItems = GetBluePrintItems(tcmFolder);
            bluePrintItems.Reverse();

            foreach (BluePrintNodeData bluePrintItem in bluePrintItems)
            {
                List<ItemInfo> childItems = GetItemsByParentContainer(bluePrintItem.Item.Id).Where(x => x.IsLocal).ToList();

                //delete inner items
                foreach (ItemInfo childItem in childItems)
                {
                    if (childItem.ItemType == ItemType.Folder || childItem.ItemType == ItemType.StructureGroup)
                    {
                        DeleteFolderOrStructureGroup(childItem.TcmId, delete, results, level + 1);
                    }
                    else
                    {
                        if (ExistsItem(childItem.TcmId))
                            DeleteTridionObject(childItem.TcmId, delete, results, tcmFolder, true, level);
                    }

                    if (results.Any(x => x.Status == Status.Error))
                        return;
                }
            }

            DeleteTridionObject(tcmFolder, delete, results, string.Empty, true, level);
        }

        public static void DeletePublication(string tcmPublication, bool delete, List<ResultInfo> results, int level = 0)
        {
            if (results.Any(x => x.Status == Status.Error))
                return;

            if (results.Count > 50)
            {
                results.Insert(0, new ResultInfo
                {
                    Message = "Delete stack exceeds 50 items. Please select other item",
                    Item = new ItemInfo { Title = "Delete stack exceeds 50 items" },
                    Status = Status.Error
                });

                return;
            }

            PublicationData publication = (PublicationData)ReadItem(tcmPublication);

            if (level > 3)
            {
                results.Add(new ResultInfo
                {
                    Message = string.Format("Recoursion level is bigger than 3. Try delete publication  manually \"{0}\"", publication.Title),
                    Item = publication.ToItem(),
                    Status = Status.Error
                });

                return;
            }

            //delete dependent publications
            List<string> usingItems = GetUsingItems(tcmPublication);
            foreach (string usingItem in usingItems)
            {
                ItemType itemType = GetItemType(usingItem);
                if (itemType == ItemType.Publication)
                {
                    DeletePublication(usingItem, delete, results, level + 1);
                }
                else
                {
                    DeleteTridionObject(usingItem, delete, results, string.Empty, true, level + 1);
                }
            }

            //delete / inform published items
            List<ItemInfo> pulishedItems = GetItemsByPublication(tcmPublication, true).Where(x => x.IsPublished).ToList();
            foreach (ItemInfo publishedItem in pulishedItems)
            {
                DeleteTridionObject(publishedItem.TcmId, delete, results, string.Empty, true, level);
            }

            try
            {
                if (delete)
                {
                    //delete publication as an object
                    Client.Delete(tcmPublication);

                    results.Add(new ResultInfo
                    {
                        Message = string.Format("Deleted publication \"{0}\"", publication.Title),
                        Item = publication.ToItem(),
                        Status = Status.Success
                    });
                }
                else
                {
                    results.Add(new ResultInfo
                    {
                        Message = string.Format("Delete publication \"{0}\"", publication.Title),
                        Item = publication.ToItem(),
                        Status = Status.Delete
                    });
                }
            }
            catch (Exception ex)
            {
                results.Add(new ResultInfo
                {
                    Message = string.Format("Error deleting publication \"{0}\". Error message \"{1}\"", publication.Title, ex.Message),
                    Item = publication.ToItem(),
                    Status = Status.Error,
                    StackTrace = ex.StackTrace
                });
            }
        }

        #endregion

        #region Tridion publishing

        private static bool IsPublished(string tcmItem)
        {
            return Client.GetListPublishInfo(tcmItem).Any();
        }

        private static List<ItemInfo> GetPublishedItems(ItemInfo item)
        {
            return Client.GetListPublishInfo(item.TcmId).Select(p => new ItemInfo { TcmId = GetBluePrintItemTcmId(item.TcmId, p.Repository.IdRef), Title = item.Title, ItemType = item.ItemType, IsPublished = true, FromPub = p.Repository.Title, Path = string.IsNullOrEmpty(item.Path) ? string.Empty : item.Path.Replace(item.Path.Trim('\\').Split('\\')[0], p.Repository.Title) }).ToList();
        }

        #endregion

        #region Tridion Blueprint

        public static string GetBluePrintTopTcmId(string id)
        {
            if (id.StartsWith("tcm:0-"))
                return id;

            var list = Client.GetSystemWideList(new BluePrintFilterData { ForItem = new LinkToRepositoryLocalObjectData { IdRef = id } });
            if (list == null || list.Length == 0)
                return id;

            var list2 = list.Cast<BluePrintNodeData>().Where(x => x.Item != null).ToList();

            return list2.First().Item.Id;
        }

        public static List<BluePrintNodeData> GetBluePrintItems(string id)
        {
            if (id.StartsWith("tcm:0-"))
                return null;

            var list = Client.GetSystemWideList(new BluePrintFilterData { ForItem = new LinkToRepositoryLocalObjectData { IdRef = id } });
            if (list == null || list.Length == 0)
                return null;

            return list.Cast<BluePrintNodeData>().Where(x => x.Item != null).ToList();
        }

        public static bool IsAnyLocalized(string id)
        {
            var list = Client.GetSystemWideList(new BluePrintFilterData { ForItem = new LinkToRepositoryLocalObjectData { IdRef = id } });
            if (list == null || list.Length == 0)
                return false;

            var list2 = list.Cast<BluePrintNodeData>().Where(x => x.Item != null && x.Id == x.Item.BluePrintInfo.OwningRepository.IdRef);

            return list2.Count() > 1;
        }

        public static void Localize(ItemInfo item)
        {
            if (item.IsShared)
                Client.Localize(item.TcmId, new ReadOptions());
        }

        public static void UnLocalize(ItemInfo item)
        {
            if (item.IsLocalized)
                Client.UnLocalize(item.TcmId, new ReadOptions());
        }

        public static void UnLocalizeAll(string id)
        {
            var list = Client.GetSystemWideList(new BluePrintFilterData { ForItem = new LinkToRepositoryLocalObjectData { IdRef = id } });
            if (list == null || list.Length == 0)
                return;

            var list2 = list.Cast<BluePrintNodeData>().Where(x => x.Item != null && x.Id == x.Item.BluePrintInfo.OwningRepository.IdRef).ToList();

            string topTcmId = list2.First().Item.Id;

            foreach (BluePrintNodeData item in list2)
            {
                UnLocalize(item.Item.ToItem(topTcmId));
            }
        }

        private static void UnlocalizeTridionObject(ItemInfo item, bool delete, List<ResultInfo> results)
        {
            RepositoryLocalObjectData itemData = (RepositoryLocalObjectData)Client.Read(item.TcmId, new ReadOptions());

            if (delete)
            {
                try
                {
                    UnLocalize(item);

                    results.Add(new ResultInfo
                    {
                        Message = String.Format("Unlocalized item \"{0}\"", itemData.GetWebDav().CutPath("/", 90, true)),
                        Item = itemData.ToItem(),
                        Status = Status.Success
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new ResultInfo
                    {
                        Message = String.Format("Error unlocalizing item \"{0}\"", itemData.GetWebDav().CutPath("/", 80, true)),
                        Status = Status.Error,
                        StackTrace = ex.StackTrace
                    });
                }
            }
            else
            {
                results.Add(new ResultInfo
                {
                    Message = String.Format("Unlocalize item \"{0}\"", itemData.GetWebDav().CutPath("/", 90, true)),
                    Item = item,
                    Status = Status.Info
                });
            }
        }

        #endregion

        #region Permissions

        public static void ClonePermissions(string tcmSource, string tcmTarget)
        {
            RepositoryData sourceItemData = Client.Read(tcmSource, new ReadOptions()) as RepositoryData;
            if (sourceItemData == null)
                return;

            List<AccessControlEntryData> accessControlEntries = sourceItemData.AccessControlList.AccessControlEntries.ToList();

            RepositoryData targetItemData = Client.Read(tcmTarget, new ReadOptions()) as RepositoryData;
            if (targetItemData == null)
                return;

            targetItemData.AccessControlList.AccessControlEntries = accessControlEntries.ToArray();

            Client.Save(targetItemData, null);
        }

        public static List<GroupData> GetUserGroups()
        {
            return Client.GetSystemWideList(new GroupsFilterData()).Cast<GroupData>().ToList();
        }

        public static List<ItemInfo> GetGroupWhereUsed(string groupTcmId)
        {
            List<ItemInfo> res = new List<ItemInfo>();

            foreach (ItemInfo publication in GetPublications())
            {
                PublicationData publicationData = Client.Read(publication.TcmId, new ReadOptions()) as PublicationData;
                if (publicationData == null)
                    continue;

                foreach (AccessControlEntryData entry in publicationData.AccessControlList.AccessControlEntries)
                {
                    if (entry.AllowedRights == Rights.None)
                        continue;

                    if (entry.Trustee.IdRef != groupTcmId)
                        continue;

                    res.Add(publication);
                }

                //folders
                foreach (ItemInfo folder in GetFoldersByPublication(publication.TcmId))
                {
                    res.AddRange(GetGroupWhereUsed(groupTcmId, folder));
                }

                //structure groups
                foreach (ItemInfo sg in GetStructureGroupsByPublication(publication.TcmId))
                {
                    res.AddRange(GetGroupWhereUsed(groupTcmId, sg));
                }

                //categories
                foreach (ItemInfo cat in GetCategoriesByPublication(publication.TcmId))
                {
                    res.AddRange(GetGroupWhereUsed(groupTcmId, cat));
                }
            }

            return res;
        }

        private static List<ItemInfo> GetGroupWhereUsed(string groupTcmId, ItemInfo container)
        {
            List<ItemInfo> res = new List<ItemInfo>();

            OrganizationalItemData containerItemData = Client.Read(container.TcmId, new ReadOptions()) as OrganizationalItemData;
            if (containerItemData == null)
                return res;

            if (containerItemData.BluePrintInfo.IsShared != true)
            {
                foreach (AccessControlEntryData entry in containerItemData.AccessControlList.AccessControlEntries)
                {
                    if (entry.AllowedPermissions == Permissions.None)
                        continue;

                    if (entry.Trustee.IdRef != groupTcmId)
                        continue;

                    container.WebDav = containerItemData.GetWebDav();
                    res.Add(container);
                }
            }

            //inner items
            foreach (ItemInfo item in GetItemsByParentContainer(container.TcmId))
            {
                res.AddRange(GetGroupWhereUsed(groupTcmId, item));
            }

            return res;
        }

        #endregion

        #region Collection helpers

        public static List<ItemInfo> Intersect(List<ItemInfo>[] arrayOfSets, bool includeEmptySets)
        {
            List<ItemInfo>[] arr = includeEmptySets ? arrayOfSets : arrayOfSets.Where(x => x != null && x.Count > 0).ToArray();

            if (arr.Length == 0 || arr.Length == 1 && arr[0].Count == 0) return new List<ItemInfo>();
            if (arr.Length == 1 && arr[0].Count > 0) return arr[0];

            List<ItemInfo> res = arr[0];
            for (int i = 1; i < arr.Length; i++)
            {
                res = res.Intersect(arr[i], new ItemInfoComparer()).ToList();
            }

            return res;
        }

        private class ItemInfoComparer : IEqualityComparer<ItemInfo>
        {
            public bool Equals(ItemInfo x, ItemInfo y)
            {
                return x.TcmId == y.TcmId;
            }

            public int GetHashCode(ItemInfo obj)
            {
                return obj.TcmId.GetHashCode();
            }
        }

        private class ResultInfoComparer : IEqualityComparer<ResultInfo>
        {
            public bool Equals(ResultInfo x, ResultInfo y)
            {
                return x.TcmId == y.TcmId && x.Status == y.Status;
            }

            public int GetHashCode(ResultInfo obj)
            {
                return obj.TcmId.GetHashCode();
            }
        }

        public static List<ItemInfo> ToList(this XElement xml, ItemType itemType)
        {
            List<ItemInfo> res = new List<ItemInfo>();
            if (xml != null && xml.HasElements)
            {
                foreach (XElement element in xml.Elements())
                {
                    ItemInfo item = new ItemInfo();
                    item.TcmId = element.Attribute("ID").Value;
                    item.ItemType = itemType;
                    item.Title = element.Attributes().Any(x => x.Name == "Title") ? element.Attribute("Title").Value : item.TcmId;
                    item.Path = element.Attributes().Any(x => x.Name == "Path") ? element.Attribute("Path").Value : string.Empty;

                    if (item.ItemType == ItemType.Schema)
                    {
                        if (element.Attributes().Any(x => x.Name == "Icon"))
                        {
                            string icon = element.Attribute("Icon").Value;
                            if (icon.EndsWith("S7"))
                            {
                                item.SchemaType = SchemaType.Bundle;
                            }
                            else if (icon.EndsWith("S6"))
                            {
                                item.SchemaType = SchemaType.Parameters;
                            }
                            else if (icon.EndsWith("S3"))
                            {
                                item.SchemaType = SchemaType.Metadata;
                            }
                            else if (icon.EndsWith("S2"))
                            {
                                item.SchemaType = SchemaType.Embedded;
                            }
                            else if (icon.EndsWith("S1"))
                            {
                                item.SchemaType = SchemaType.Multimedia;
                            }
                            else if (icon.EndsWith("S0"))
                            {
                                item.SchemaType = SchemaType.Component;
                            }
                            else
                            {
                                item.SchemaType = SchemaType.None;
                            }
                        }
                        else
                        {
                            item.SchemaType = SchemaType.None;
                        }
                    }
                    else
                    {
                        item.SchemaType = SchemaType.None;
                    }

                    if (item.ItemType == ItemType.Component)
                    {
                        item.MimeType = element.Attributes().Any(x => x.Name == "MIMEType") ? element.Attribute("MIMEType").Value : null;
                    }

                    item.FromPub = element.Attributes().Any(x => x.Name == "FromPub") ? element.Attribute("FromPub").Value : null;
                    item.IsPublished = element.Attributes().Any(x => x.Name == "Icon") && element.Attribute("Icon").Value.EndsWith("P1");

                    res.Add(item);
                }
            }
            return res;
        }

        public static List<ItemInfo> ToList(this XElement xml)
        {
            List<ItemInfo> res = new List<ItemInfo>();
            if (xml != null && xml.HasElements)
            {
                foreach (XElement element in xml.Elements())
                {
                    ItemInfo item = new ItemInfo();
                    item.TcmId = element.Attribute("ID").Value;
                    item.ItemType = element.Attributes().Any(x => x.Name == "Type") ? (ItemType)Int32.Parse(element.Attribute("Type").Value) : GetItemType(item.TcmId);
                    item.Title = element.Attributes().Any(x => x.Name == "Title") ? element.Attribute("Title").Value : item.TcmId;
                    item.Path = element.Attributes().Any(x => x.Name == "Path") ? element.Attribute("Path").Value : string.Empty;

                    if (item.ItemType == ItemType.Schema)
                    {
                        if (element.Attributes().Any(x => x.Name == "Icon"))
                        {
                            string icon = element.Attribute("Icon").Value;
                            if (icon.EndsWith("S7"))
                            {
                                item.SchemaType = SchemaType.Bundle;
                            }
                            else if (icon.EndsWith("S6"))
                            {
                                item.SchemaType = SchemaType.Parameters;
                            }
                            else if (icon.EndsWith("S3"))
                            {
                                item.SchemaType = SchemaType.Metadata;
                            }
                            else if (icon.EndsWith("S2"))
                            {
                                item.SchemaType = SchemaType.Embedded;
                            }
                            else if (icon.EndsWith("S1"))
                            {
                                item.SchemaType = SchemaType.Multimedia;
                            }
                            else if (icon.EndsWith("S0"))
                            {
                                item.SchemaType = SchemaType.Component;
                            }
                            else
                            {
                                item.SchemaType = SchemaType.None;
                            }
                        }
                        else
                        {
                            item.SchemaType = SchemaType.None;
                        }
                    }
                    else
                    {
                        item.SchemaType = SchemaType.None;
                    }

                    if (item.ItemType == ItemType.Component)
                    {
                        item.MimeType = element.Attributes().Any(x => x.Name == "MIMEType") ? element.Attribute("MIMEType").Value : null;    
                    }

                    item.FromPub = element.Attributes().Any(x => x.Name == "FromPub") ? element.Attribute("FromPub").Value : null;
                    item.IsPublished = element.Attributes().Any(x => x.Name == "Icon") && element.Attribute("Icon").Value.EndsWith("P1");
                    
                    res.Add(item);
                }
            }
            return res;
        }

        public static ItemInfo ToItem(this RepositoryLocalObjectData dataItem, string topTcmId = null)
        {
            ItemInfo item = new ItemInfo();
            item.TcmId = dataItem.Id;
            item.ItemType = GetItemType(dataItem.Id);
            item.Title = dataItem.Title;

            string webDav = dataItem.GetWebDav();
            item.Path = string.IsNullOrEmpty(webDav) ? string.Empty : Path.GetDirectoryName(webDav.Replace('/', '\\'));

            if (item.ItemType == ItemType.Schema)
            {
                SchemaData schemaDataItem = (SchemaData)dataItem;
                if (schemaDataItem.Purpose == SchemaPurpose.Bundle)
                {
                    item.SchemaType = SchemaType.Bundle;
                }
                else if (schemaDataItem.Purpose == SchemaPurpose.TemplateParameters)
                {
                    item.SchemaType = SchemaType.Parameters;
                }
                else if (schemaDataItem.Purpose == SchemaPurpose.Metadata)
                {
                    item.SchemaType = SchemaType.Metadata;
                }
                else if (schemaDataItem.Purpose == SchemaPurpose.Embedded)
                {
                    item.SchemaType = SchemaType.Embedded;
                }
                else if (schemaDataItem.Purpose == SchemaPurpose.Multimedia)
                {
                    item.SchemaType = SchemaType.Multimedia;
                }
                else if (schemaDataItem.Purpose == SchemaPurpose.Component)
                {
                    item.SchemaType = SchemaType.Component;
                }
                else
                {
                    item.SchemaType = SchemaType.None;
                }
            }

            if (GetPublicationTcmId(dataItem.Id) == dataItem.BluePrintInfo.OwningRepository.IdRef && dataItem.Id == topTcmId)
                item.FromPub = string.Empty;
            else if (GetPublicationTcmId(dataItem.Id) == dataItem.BluePrintInfo.OwningRepository.IdRef)
                item.FromPub = "(Local copy)";
            else
                item.FromPub = dataItem.BluePrintInfo.OwningRepository.Title;

            if (dataItem.IsPublishedInContext != null)
                item.IsPublished = dataItem.IsPublishedInContext.Value;

            return item;
        }

        public static ItemInfo ToItem(this PublicationData publicationData)
        {
            ItemInfo item = new ItemInfo();
            item.TcmId = publicationData.Id;
            item.ItemType = ItemType.Publication;
            item.Title = publicationData.Title;

            return item;
        }

        public static ItemType GetItemType(string tcmItem)
        {
            if (string.IsNullOrEmpty(tcmItem))
                return ItemType.None;

            string[] arr = tcmItem.Replace("tcm:", string.Empty).Split('-');
            if (arr.Length == 2) return ItemType.Component;

            return (ItemType)Int32.Parse(arr[2]);
        }

        public static List<ItemInfo> MakeExpandable(this List<ItemInfo> list)
        {
            foreach (ItemInfo item in list)
            {
                if (item.ChildItems == null && (item.ItemType == ItemType.Publication || item.ItemType == ItemType.Folder || item.ItemType == ItemType.StructureGroup || item.ItemType == ItemType.Category))
                    item.ChildItems = new List<ItemInfo> { new ItemInfo { Title = "Loading..." } };
            }
            return list;
        }

        public static ItemType[] GetItemTypes(this TridionSelectorMode tridionSelectorMode)
        {
            return Enum.GetValues(typeof(TridionSelectorMode)).Cast<TridionSelectorMode>().Where(flag => tridionSelectorMode.HasFlag(flag)).Select(flag => (ItemType)(int)flag).ToArray();
        }

        public static List<ItemInfo> Expand(this List<ItemInfo> list, TridionSelectorMode tridionSelectorMode, List<string> tcmItemPath, string selectedTcmId)
        {
            if (tcmItemPath == null || String.IsNullOrEmpty(selectedTcmId))
                return list;

            foreach (ItemInfo item in list)
            {
                if (tcmItemPath.Any(x => x == item.TcmId))
                {
                    item.IsExpanded = true;
                    item.IsSelected = item.TcmId == selectedTcmId;

                    if (item.IsSelected)
                        continue;

                    if (String.IsNullOrEmpty(item.TcmId))
                        continue;

                    item.ChildItems = null;
                    if (item.ItemType == ItemType.Publication)
                    {
                        if (tridionSelectorMode.HasFlag(TridionSelectorMode.Any))
                        {
                            item.ChildItems = GetItemsByPublication(item.TcmId);
                        }
                        else if (tridionSelectorMode.HasFlag(TridionSelectorMode.Folder) && tridionSelectorMode.HasFlag(TridionSelectorMode.StructureGroup))
                        {
                            item.ChildItems = GetContainersByPublication(item.TcmId);
                        }
                        else if (tridionSelectorMode.HasFlag(TridionSelectorMode.Folder))
                        {
                            item.ChildItems = GetFoldersByPublication(item.TcmId);
                        }
                        else if (tridionSelectorMode.HasFlag(TridionSelectorMode.StructureGroup))
                        {
                            item.ChildItems = GetStructureGroupsByPublication(item.TcmId);
                        }
                    }
                    if (item.ItemType == ItemType.Folder)
                    {
                        if (tridionSelectorMode.HasFlag(TridionSelectorMode.Any))
                        {
                            if (item.Title == "Categories and Keywords" && item.TcmId.StartsWith("catman-"))
                            {
                                item.ChildItems = GetCategoriesByPublication(item.TcmId.Replace("catman-", ""));
                            }
                            else if (item.Title == "Process Definitions" && item.TcmId.StartsWith("proc-"))
                            {
                                item.ChildItems = GetProcessDefinitionsByPublication(item.TcmId.Replace("proc-", ""));
                            }
                            else
                            {
                                item.ChildItems = GetItemsByParentContainer(item.TcmId);
                            }
                        }
                        else
                        {
                            item.ChildItems = GetItemsByParentContainer(item.TcmId, tridionSelectorMode.GetItemTypes());
                        }
                    }
                    if (item.ItemType == ItemType.StructureGroup)
                    {
                        if (tridionSelectorMode.HasFlag(TridionSelectorMode.StructureGroup) && tridionSelectorMode.HasFlag(TridionSelectorMode.Page) || tridionSelectorMode.HasFlag(TridionSelectorMode.Any))
                        {
                            item.ChildItems = GetItemsByParentContainer(item.TcmId);
                        }
                        else if (tridionSelectorMode.HasFlag(TridionSelectorMode.StructureGroup))
                        {
                            item.ChildItems = GetStructureGroupsByParentStructureGroup(item.TcmId);
                        }
                    }
                    if (item.ItemType == ItemType.Category)
                    {
                        item.ChildItems = GetKeywordsByCategory(item.TcmId);
                    }

                    if (item.ChildItems != null && item.ChildItems.Count > 0)
                    {
                        item.ChildItems.SetParent(item);
                    }

                    if (item.ChildItems != null && item.ChildItems.Count > 0)
                    {
                        item.ChildItems.Expand(tridionSelectorMode, tcmItemPath, selectedTcmId);
                    }
                }
                else
                {
                    if (item.ItemType == ItemType.Publication || item.ItemType == ItemType.Folder || item.ItemType == ItemType.StructureGroup || item.ItemType == ItemType.Category)
                        item.ChildItems = new List<ItemInfo> { new ItemInfo { Title = "Loading..." } };
                }
            }
            return list;
        }

        public static void OnItemExpanded(ItemInfo item, TridionSelectorMode tridionSelectorMode)
        {
            if (item.ChildItems != null && item.ChildItems.All(x => x.Title != "Loading..."))
                return;

            if (String.IsNullOrEmpty(item.TcmId))
                return;

            if (item.ItemType == ItemType.Publication)
            {
                if (tridionSelectorMode.HasFlag(TridionSelectorMode.Any))
                {
                    item.ChildItems = GetItemsByPublication(item.TcmId).MakeExpandable().SetParent(item);
                }
                else if (tridionSelectorMode.HasFlag(TridionSelectorMode.Folder) && tridionSelectorMode.HasFlag(TridionSelectorMode.StructureGroup))
                {
                    item.ChildItems = GetContainersByPublication(item.TcmId).MakeExpandable().SetParent(item);
                }
                else if (tridionSelectorMode.HasFlag(TridionSelectorMode.Folder))
                {
                    item.ChildItems = GetFoldersByPublication(item.TcmId).MakeExpandable().SetParent(item);
                }
                else if (tridionSelectorMode.HasFlag(TridionSelectorMode.StructureGroup))
                {
                    item.ChildItems = GetStructureGroupsByPublication(item.TcmId).MakeExpandable().SetParent(item);
                }
            }
            if (item.ItemType == ItemType.Folder)
            {
                if (tridionSelectorMode.HasFlag(TridionSelectorMode.Any))
                {
                    if (item.Title == "Categories and Keywords" && item.TcmId.StartsWith("catman-"))
                    {
                        item.ChildItems = GetCategoriesByPublication(item.TcmId.Replace("catman-", "")).MakeExpandable().SetParent(item);
                    }
                    else if (item.Title == "Process Definitions" && item.TcmId.StartsWith("proc-"))
                    {
                        item.ChildItems = GetProcessDefinitionsByPublication(item.TcmId.Replace("proc-", "")).MakeExpandable().SetParent(item);
                    }
                    else
                    {
                        item.ChildItems = GetItemsByParentContainer(item.TcmId).MakeExpandable().SetParent(item);
                    }
                }
                else
                {
                    item.ChildItems = GetItemsByParentContainer(item.TcmId, tridionSelectorMode.GetItemTypes()).MakeExpandable().SetParent(item);
                }
            }
            if (item.ItemType == ItemType.StructureGroup)
            {
                if (tridionSelectorMode.HasFlag(TridionSelectorMode.StructureGroup) && tridionSelectorMode.HasFlag(TridionSelectorMode.Page) || tridionSelectorMode.HasFlag(TridionSelectorMode.Any))
                {
                    item.ChildItems = GetItemsByParentContainer(item.TcmId).MakeExpandable().SetParent(item);
                }
                else if (tridionSelectorMode.HasFlag(TridionSelectorMode.StructureGroup))
                {
                    item.ChildItems = GetStructureGroupsByParentStructureGroup(item.TcmId).MakeExpandable().SetParent(item);
                }
            }
            if (item.ItemType == ItemType.Category)
            {
                item.ChildItems = GetKeywordsByCategory(item.TcmId).MakeExpandable().SetParent(item);
            }
        }

        public static void AddPathItem(List<ItemInfo> list, ItemInfo item)
        {
            if (item == null)
                return;

            list.Add(item);

            if (item.Parent != null)
                AddPathItem(list, item.Parent);
        }

        public static List<ItemInfo> SetParent(this List<ItemInfo> list, ItemInfo parent)
        {
            foreach (ItemInfo item in list)
            {
                item.Parent = parent;
            }
            return list;
        }

        public static string GetMimeTypeId(string filePath)
        {
            List<MultimediaTypeData> allMimeTypes = Client.GetSystemWideList(new MultimediaTypesFilterData()).Cast<MultimediaTypeData>().ToList();
            foreach (MultimediaTypeData mt in allMimeTypes)
            {
                foreach (string ext in mt.FileExtensions)
                {
                    if (Path.GetExtension(filePath).ToLower().Replace(".", string.Empty) == ext.ToLower().Replace(".", string.Empty))
                        return mt.Id;
                }
            }
            return string.Empty;
        }

        public static List<ItemInfo> FindCheckedOutItems()
        {
            return Client.GetSystemWideListXml(new RepositoryLocalObjectsFilterData()).ToList();
        }

        public static bool IsCheckedOut(string id)
        {
            return FindCheckedOutItems().Any(x => x.TcmId == id);
        }

        public static FieldType GetFieldType(this ItemFieldDefinitionData field)
        {
            if (field is SingleLineTextFieldDefinitionData)
            {
                return FieldType.SingleLineText;
            }
            if (field is MultiLineTextFieldDefinitionData)
            {
                return FieldType.MultiLineText;
            }
            if (field is XhtmlFieldDefinitionData)
            {
                return FieldType.Xhtml;
            }
            if (field is DateFieldDefinitionData)
            {
                return FieldType.Date;
            }
            if (field is NumberFieldDefinitionData)
            {
                return FieldType.Number;
            }
            if (field is KeywordFieldDefinitionData)
            {
                return FieldType.Keyword;
            }
            if (field is MultimediaLinkFieldDefinitionData)
            {
                return FieldType.Multimedia;
            }
            if (field is ExternalLinkFieldDefinitionData)
            {
                return FieldType.ExternalLink;
            }
            if (field is ComponentLinkFieldDefinitionData)
            {
                return FieldType.ComponentLink;
            }
            if (field is EmbeddedSchemaFieldDefinitionData)
            {
                return FieldType.EmbeddedSchema;
            }
            
            return FieldType.None;
        }

        public static string GetFieldTypeName(this ItemFieldDefinitionData field)
        {
            return field.GetFieldType() == FieldType.None ? string.Empty : field.GetFieldType().ToString();
        }

        public static bool IsText(this ItemFieldDefinitionData field)
        {
            return field is SingleLineTextFieldDefinitionData && !field.IsTextSelect() || field is MultiLineTextFieldDefinitionData;
        }

        public static bool IsRichText(this ItemFieldDefinitionData field)
        {
            return field is XhtmlFieldDefinitionData;
        }

        public static bool IsDate(this ItemFieldDefinitionData field)
        {
            return field is DateFieldDefinitionData;
        }

        public static bool IsNumber(this ItemFieldDefinitionData field)
        {
            return field is NumberFieldDefinitionData;
        }

        public static bool IsKeyword(this ItemFieldDefinitionData field)
        {
            return field is KeywordFieldDefinitionData;
        }

        public static bool IsMultimedia(this ItemFieldDefinitionData field)
        {
            return field is MultimediaLinkFieldDefinitionData;
        }

        public static bool IsTextSelect(this ItemFieldDefinitionData field)
        {
            if (field is SingleLineTextFieldDefinitionData)
            {
                SingleLineTextFieldDefinitionData textField = (SingleLineTextFieldDefinitionData)field;
                return textField.List != null && textField.List.Entries != null && textField.List.Entries.Length > 0;
            }
            return false;
        }

        public static bool IsEmbedded(this ItemFieldDefinitionData field)
        {
            return field is EmbeddedSchemaFieldDefinitionData;
        }

        public static bool IsComponentLink(this ItemFieldDefinitionData field)
        {
            return field is ComponentLinkFieldDefinitionData;
        }

        public static bool IsMultimediaComponentLink(this ItemFieldDefinitionData field)
        {
            ComponentLinkFieldDefinitionData clField = field as ComponentLinkFieldDefinitionData;
            if (clField == null)
                return false;
            return clField.AllowMultimediaLinks;
        }

        public static bool IsMultiValue(this ItemFieldDefinitionData field)
        {
            return field.MaxOccurs == -1 || field.MaxOccurs > 1;
        }
        
        public static bool IsMandatory(this ItemFieldDefinitionData field)
        {
            return field.MinOccurs == 1;
        }

        public static bool IsCastAllowed(this ItemFieldDefinitionData from, ItemFieldDefinitionData to)
        {
            if (from.GetFieldType() == to.GetFieldType())
                return true;

            if (from.GetFieldType() == FieldType.EmbeddedSchema && to.GetFieldType() == FieldType.ComponentLink)
                return true;

            if (from.GetFieldType() == FieldType.ComponentLink && to.GetFieldType() == FieldType.EmbeddedSchema)
                return true;

            if (from.GetFieldType() == FieldType.Number)
            {
                if (to.GetFieldType() == FieldType.SingleLineText)
                    return true;
                if (to.GetFieldType() == FieldType.MultiLineText)
                    return true;
                if (to.GetFieldType() == FieldType.Xhtml)
                    return true;
            }

            if (from.GetFieldType() == FieldType.Date)
            {
                if (to.GetFieldType() == FieldType.SingleLineText)
                    return true;
                if (to.GetFieldType() == FieldType.MultiLineText)
                    return true;
                if (to.GetFieldType() == FieldType.Xhtml)
                    return true;
            }

            if (from.GetFieldType() == FieldType.SingleLineText)
            {
                if (to.GetFieldType() == FieldType.MultiLineText)
                    return true;
                if (to.GetFieldType() == FieldType.Xhtml)
                    return true;
            }

            if (from.GetFieldType() == FieldType.MultiLineText)
            {
                if (to.GetFieldType() == FieldType.SingleLineText)
                    return true;
                if (to.GetFieldType() == FieldType.Xhtml)
                    return true;
            }

            if (from.GetFieldType() == FieldType.Keyword)
            {
                if (to.GetFieldType() == FieldType.SingleLineText)
                    return true;
                if (to.GetFieldType() == FieldType.MultiLineText)
                    return true;
                if (to.GetFieldType() == FieldType.Xhtml)
                    return true;
            }

            return false;
        }

        public static bool IsPrimitive(this ItemFieldDefinitionData field)
        {
            FieldType fieldType = field.GetFieldType();

            return fieldType == FieldType.SingleLineText ||
                fieldType == FieldType.MultiLineText ||
                fieldType == FieldType.Xhtml || 
                fieldType == FieldType.Date ||
                fieldType == FieldType.Number ||
                fieldType == FieldType.Keyword ||
                fieldType == FieldType.ExternalLink;
        }

        #endregion

        #region Text helpers

        public static string GetId(this string tcmId)
        {
            if (string.IsNullOrEmpty(tcmId) || !tcmId.StartsWith("tcm:") || !tcmId.Contains("-"))
                return string.Empty;

            return tcmId.Split('-')[1];
        }

        public static string GetPublicationTcmId(string id)
        {
            ItemType itemType = GetItemType(id);
            if (itemType == ItemType.Publication)
                return id;
            
            return "tcm:0-" + id.Replace("tcm:", string.Empty).Split('-')[0] + "-1";
        }

        public static string GetBluePrintItemTcmId(string id, string publicationId)
        {
            if (string.IsNullOrEmpty(id) || !id.StartsWith("tcm:") || !id.Contains("-") || string.IsNullOrEmpty(publicationId) || !publicationId.StartsWith("tcm:") || !publicationId.Contains("-"))
                return string.Empty;

            return "tcm:" + publicationId.GetId() + "-" + id.GetId() + (id.Split('-').Length > 2 ? "-" + id.Split('-')[2] : string.Empty);
        }

        public static string CutPath(this string path, string separator, int maxLength)
        {
            if (path == null || path.Length <= maxLength)
                return path;

            var list = path.Split(new[] { separator[0] });
            int itemMaxLength = maxLength / list.Length;

            return String.Join(separator, list.Select(item => item.Cut(itemMaxLength)).ToList());
        }

        public static string CutPath(this string path, string separator, int maxLength, bool fullLastItem)
        {
            if (path == null || path.Length <= maxLength)
                return path;

            if (!fullLastItem)
                return path.CutPath(separator, maxLength);

            string lastItem = path.Substring(path.LastIndexOf(separator, StringComparison.Ordinal));

            if (lastItem.Length > maxLength)
                return path.CutPath(separator, maxLength);

            return path.Substring(0, path.LastIndexOf(separator, StringComparison.Ordinal)).CutPath(separator, maxLength - lastItem.Length) + lastItem;
        }

        public static string Cut(this string str, int maxLength)
        {
            if (maxLength < 5)
                maxLength = 5;

            if (str.Length > maxLength)
            {
                return str.Substring(0, maxLength - 2) + "..";
            }

            return str;
        }

        public static string PrettyXml(this string xml)
        {
            try
            {
                return XElement.Parse(xml).ToString().Replace(" xmlns=\"\"", "");
            }
            catch (Exception)
            {
                return xml;
            }
        }

        public static string PlainXml(this string xml)
        {
            try
            {
                return Regex.Replace(xml, "\\s+", " ").Replace("> <", "><");
            }
            catch (Exception)
            {
                return xml;
            }
        }

        public static string GetInnerXml(this XElement node)
        {
            var reader = node.CreateReader();
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }

        public static string GetFieldFullName(this FieldInfo field, bool includePath)
        {
            if (field == null || field.Field == null || String.IsNullOrEmpty(field.Field.Name))
                return "< ignore >";

            if (field.Field.Name == "< this component link >")
                return "< this component link >";

            if (field.Field.Name == "< target component link >")
                return "< target component link >";

            if (field.Field.Name == "< new >")
                return "< new >";

            string span = "";
            for (int i = 0; i < field.Level; i++)
            {
                span += "  ";
            }

            string path = includePath && field.GetFieldNamePath() != field.Field.Name ? String.Format(" | ({0})", field.GetFieldNamePath()) : "";

            if (field.Field.IsEmbedded())
                return String.Format("{0}{1} | {2}{3}{4}", span, field.Field.Name, ((EmbeddedSchemaFieldDefinitionData)field.Field).EmbeddedSchema.Title, (field.IsMeta ? " | [meta]" : ""), path);

            if (field.Field.IsComponentLink())
            {
                ComponentLinkFieldDefinitionData componentLinkField = ((ComponentLinkFieldDefinitionData)field.Field);
                if (componentLinkField.AllowedTargetSchemas.Any())
                    return String.Format("{0}{1} | {2}{3}{4}", span, componentLinkField.Name, componentLinkField.AllowedTargetSchemas[0].Title, (field.IsMeta ? " | [meta]" : ""), path);
            }

            return String.Format("{0}{1} | {2}{3}{4}", span, field.Field.Name, field.Field.GetFieldTypeName(), (field.IsMeta ? " | [meta]" : ""), path);
        }

        public static string GetFieldFullName(this FieldInfo field)
        {
            return field.GetFieldFullName(true);
        }

        public static string GetFieldNamePath(this FieldInfo field, bool breakComponentLinkPath = false)
        {
            if (field == null)
                return string.Empty;

            if (breakComponentLinkPath && field.Field.IsComponentLink())
                return field.IsMeta ? "Metadata" : field.RootElementName;

            if (field.Parent == null)
                return field.Field.Name;

            return String.Format("{0}/{1}", field.Parent.GetFieldNamePath(breakComponentLinkPath), field.Field.Name);
        }

        public static string GetDomainName(this string url)
        {
            if (!url.Contains(Uri.SchemeDelimiter))
            {
                url = string.Concat(Uri.UriSchemeHttp, Uri.SchemeDelimiter, url);
            }
            Uri uri = new Uri(url);
            return uri.Host;
        }

        public static string GetCurrentVersionTcmId(this string tcmId)
        {
            if (tcmId.Contains("-v"))
                return tcmId.Substring(0, tcmId.IndexOf("-v", StringComparison.Ordinal));
            return tcmId;
        }

        private  static string GetTransformedName(string title, string tcmId, List<ComponentFieldData> componentValues, List<ComponentFieldData> metadataValues, string formatString, List<ReplacementInfo> replacements)
        {
            if (replacements == null || replacements.Count == 0)
                return title;

            List<object> replacementResults = new List<object>();
            foreach (ReplacementInfo replacement in replacements)
            {
                if (replacement.Fragment == "[Title]")
                {
                    replacementResults.Add(String.IsNullOrEmpty(replacement.Regex) ? title : Regex.Match(title, replacement.Regex).Value);
                }
                else if (replacement.Fragment == "[TcmId]")
                {
                    replacementResults.Add(String.IsNullOrEmpty(replacement.Regex) ? tcmId : Regex.Match(tcmId, replacement.Regex).Value);
                }
                else if (replacement.Fragment == "[ID]")
                {
                    replacementResults.Add(String.IsNullOrEmpty(replacement.Regex) ? tcmId.GetId() : Regex.Match(tcmId.GetId(), replacement.Regex).Value);
                }
                else if (replacement.Field != null && componentValues != null)
                {
                    ComponentFieldData field = componentValues.FirstOrDefault(x => x.SchemaField.Name == replacement.Field.Field.Name);
                    if (field != null)
                    {
                        replacementResults.Add(String.IsNullOrEmpty(replacement.Regex) ? field.Value : Regex.Match(field.Value.ToString(), replacement.Regex).Value);
                    }
                }
                else if (replacement.Field != null && metadataValues != null)
                {
                    ComponentFieldData field = metadataValues.FirstOrDefault(x => x.SchemaField.Name == replacement.Field.Field.Name);
                    if (field != null)
                    {
                        replacementResults.Add(String.IsNullOrEmpty(replacement.Regex) ? field.Value : Regex.Match(field.Value.ToString(), replacement.Regex).Value);
                    }
                }
            }

            return String.Format(formatString, replacementResults.ToArray());
        }

        public static string GetId(params object[] keys)
        {
            return String.Join("_", keys).Replace("tcm:", "").Replace("http:", "").Replace("https:", "").Replace("/", "").Replace("<", "").Replace(">", "").Replace("[", "").Replace("]", "").Replace("-", "").Replace(" ", "");
        }

        public static string GetItemCmsUrl(string host, string tcmId, string title = "")
        {
            if (title == "Categories and Keywords" && tcmId.StartsWith("catman-"))
                return string.Format("http://{0}/SDL/#app=wcm&entry=cme&url=%23locationId%3Dcatman-tcm%3A{1}", host, tcmId.Replace("catman-tcm:", ""));

            if (title == "Process Definitions" && tcmId.StartsWith("proc-"))
                return string.Format("http://{0}", host);

            ItemType itemType = GetItemType(tcmId);
            if (itemType == ItemType.Folder || itemType == ItemType.StructureGroup)
            {
                return string.Format("http://{0}/SDL/#app=wcm&entry=cme&url=%23locationId%3Dtcm%3A{1}", host, tcmId.Replace("tcm:", ""));
            }

            return string.Format("http://{0}/WebUI/item.aspx?tcm={1}#id={2}", host, (int)itemType, tcmId);
        }

        public static string GetTimeId()
        {
            DateTime now = DateTime.Now;
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
        }

        public static bool IsHtml(this string html)
        {
            Regex tagRegex = new Regex(@"<[^>]+>");
            return tagRegex.IsMatch(html);
        }

        #endregion

        #region Isolated storage

        public static string GetFromIsolatedStorage(string key)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, typeof(System.Security.Policy.Url), typeof(System.Security.Policy.Url)))
            {
                if (!isf.FileExists(key + ".txt"))
                    return string.Empty;

                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(key + ".txt", FileMode.Open, isf))
                {
                    using (StreamReader sr = new StreamReader(isfs, Encoding.UTF8))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        public static void SaveToIsolatedStorage(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;
            
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, typeof(System.Security.Policy.Url), typeof(System.Security.Policy.Url)))
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(key + ".txt", FileMode.Create, isf))
                {
                    byte[] data = Encoding.UTF8.GetBytes(value);
                    isfs.Write(data, 0, data.Length);
                }
            }
        }

        private static readonly XmlSerializer HistoryMappingSerializer = new XmlSerializer(typeof(HistoryMappingInfo));

        public static HistoryMappingInfo GetHistoryMapping(string key)
        {
            try
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, typeof(System.Security.Policy.Url), typeof(System.Security.Policy.Url)))
                {
                    if (!isf.FileExists(key + ".txt"))
                        return null;

                    using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(key + ".txt", FileMode.Open, isf))
                    {
                        return HistoryMappingSerializer.Deserialize(isfs) as HistoryMappingInfo;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public static void SaveHistoryMapping(string key, HistoryMappingInfo historyMapping)
        {
            if (historyMapping == null)
                return;

            XmlWriterSettings settings = new XmlWriterSettings { Indent = true, ConformanceLevel = ConformanceLevel.Auto, OmitXmlDeclaration = false };

            try
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, typeof(System.Security.Policy.Url), typeof(System.Security.Policy.Url)))
                {
                    using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(key + ".txt", FileMode.Create, isf))
                    {
                        using (var writer = XmlWriter.Create(isfs, settings))
                        {
                            HistoryMappingSerializer.Serialize(writer, historyMapping);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}