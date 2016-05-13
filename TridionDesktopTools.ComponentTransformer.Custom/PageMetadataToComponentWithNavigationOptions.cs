using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;

namespace TridionDesktopTools.ComponentTransformer.Custom
{
    [SourceSchema(Title = "Page Metadata", Type = SchemaType.Metadata)]
    [TargetSchema(Title = "Page Metadata Component", Type = SchemaType.Component)]
    [SourceObject(Type = ObjectType.PageOrStructureGroup)]
    public class PageMetadataToComponentWithNavigationOptions : ICustomTransformer
    {
        public string GetFixedContent(SchemaData sourceSchema, List<ItemFieldDefinitionData> sourceComponentFields, List<ItemFieldDefinitionData> sourceMetadataFields, string sourceComponentXml, string sourceMetadataXml, string sourceUri, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string targetFolderUri, List<ResultInfo> results)
        {
            XDocument sourceDoc = XDocument.Parse(sourceMetadataXml);
            XElement root = sourceDoc.Root;
            if (root == null)
                return string.Empty;

            XNamespace sourceNs = sourceSchema.NamespaceUri;

            XNamespace ns = targetSchema.NamespaceUri;
            XElement resElement = new XElement(ns + targetSchema.RootElementName);

            // just copy from source to target
            XElement xTitle = root.Elements(sourceNs + "title").FirstOrDefault();
            if (xTitle != null)
            {
                resElement.Add(xTitle);
            }

            // just copy from source to target
            XElement xDescription = root.Elements(sourceNs + "description").FirstOrDefault();
            if (xDescription != null)
            {
                resElement.Add(xDescription);
            }

            // just copy from source to target
            XElement xKeywords = root.Elements(sourceNs + "keywords").FirstOrDefault();
            if (xKeywords != null)
            {
                resElement.Add(xKeywords);
            }

            // just copy from source to target
            XElement xOpenGraph = root.Elements(sourceNs + "openGraph").FirstOrDefault();
            if (xOpenGraph != null)
            {
                resElement.Add(xOpenGraph);
            }

            // find which checkboxes are checked
            bool showInTopNavigation = sourceDoc.Root.GetByXPath("Metadata/navigationOptions/ShowInTopNavigation", sourceNs) != null;
            bool showInFooterNavigation = sourceDoc.Root.GetByXPath("Metadata/navigationOptions/ShowInFooterNavigation", sourceNs) != null;
            bool showInBreadcrumbs = sourceDoc.Root.GetByXPath("Metadata/navigationOptions/ShowInBreadcrumbs", sourceNs) != null;
            bool showInLeftSideNavigation = sourceDoc.Root.GetByXPath("Metadata/navigationOptions/ShowInLeftSideNavigation", sourceNs) != null;
            bool showInSiteMapNavigation = sourceDoc.Root.GetByXPath("Metadata/navigationOptions/ShowInSiteMapNavigation", sourceNs) != null;
            bool showInMobileNavigation = sourceDoc.Root.GetByXPath("Metadata/navigationOptions/ShowInMobileNavigation", sourceNs) != null;

            if (showInTopNavigation || showInFooterNavigation || showInBreadcrumbs || showInLeftSideNavigation || showInSiteMapNavigation || showInMobileNavigation)
            {
                //find keywors that are used in target component
                string publicationId = Functions.GetPublicationTcmId(targetFolderUri);
                string categoryId = Functions.GetCategoriesByPublication(publicationId).First(x => x.Title == "Navigation").TcmId;
                List<ItemInfo> keywords = Functions.GetKeywordsByCategory(categoryId);

                // create field with selected Top keyword
                if (showInTopNavigation)
                {
                    ItemInfo keyword = keywords.First(x => x.Title == "Top");

                    XElement navigationOption = new XElement(ns + "navigationOption");
                    navigationOption.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "Navigation"));
                    resElement.Add(navigationOption);
                }

                // create field with selected Footer keyword
                if (showInFooterNavigation)
                {
                    ItemInfo keyword = keywords.First(x => x.Title == "Footer");

                    XElement navigationOption = new XElement(ns + "navigationOption");
                    navigationOption.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "Navigation"));
                    resElement.Add(navigationOption);
                }

                // create field with selected Breadcrumbs keyword
                if (showInBreadcrumbs)
                {
                    ItemInfo keyword = keywords.First(x => x.Title == "Breadcrumbs");

                    XElement navigationOption = new XElement(ns + "navigationOption");
                    navigationOption.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "Navigation"));
                    resElement.Add(navigationOption);
                }

                // create field with selected LeftSide keyword
                if (showInLeftSideNavigation)
                {
                    ItemInfo keyword = keywords.First(x => x.Title == "LeftSide");

                    XElement navigationOption = new XElement(ns + "navigationOption");
                    navigationOption.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "Navigation"));
                    resElement.Add(navigationOption);
                }

                // create field with selected SiteMap keyword
                if (showInSiteMapNavigation)
                {
                    ItemInfo keyword = keywords.First(x => x.Title == "SiteMap");

                    XElement navigationOption = new XElement(ns + "navigationOption");
                    navigationOption.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "Navigation"));
                    resElement.Add(navigationOption);
                }

                // create field with selected Mobile keyword
                if (showInMobileNavigation)
                {
                    ItemInfo keyword = keywords.First(x => x.Title == "Mobile");

                    XElement navigationOption = new XElement(ns + "navigationOption");
                    navigationOption.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "Navigation"));
                    resElement.Add(navigationOption);
                }
            }
          
            string resText = resElement.ToString();
            resText = resText.Replace(" xmlns=\"\"", string.Empty);
            resText = resText.Replace(String.Format(" xmlns=\"{0}\"", sourceNs), string.Empty);

            return resText;
        }
    }
}
