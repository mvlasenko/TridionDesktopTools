using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;

namespace TridionDesktopTools.ComponentTransformer.Custom
{
    [SourceSchema(Title = "Navigation", Type = SchemaType.Component)]
    [TargetSchema(Title = "Navigation", Type = SchemaType.Component)]
    [SourceObject(Type = ObjectType.Component)]
    public class Test1 : ICustomTransformer
    {
        public string GetFixedContent(SchemaData sourceSchema, List<ItemFieldDefinitionData> sourceComponentFields, List<ItemFieldDefinitionData> sourceMetadataFields, string sourceComponentXml, string sourceMetadataXml, string sourceUri, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string targetFolderUri, List<ResultInfo> results)
        {
            if (sourceComponentXml.Contains("<NavigationType>"))
                return string.Empty;
            
            string pubId = Functions.GetPublicationTcmId(sourceUri);

            string category = "tcm:3-3202-512";
            category = Functions.GetBluePrintItemTcmId(category, pubId);

            ComponentData component = Functions.GetComponent(sourceUri);

            List<ItemInfo> keywords = Functions.GetKeywordsByCategory(category);

            XDocument sourceDoc = XDocument.Parse(sourceComponentXml);
            XElement root = sourceDoc.Root;
            if(root == null)
                return string.Empty;

            var xTitle = root.Elements("Title").FirstOrDefault();
            string title = xTitle != null ? xTitle.Value : component.Title;

            bool ShowInTopNavigation = sourceComponentXml.Contains("<ShowInTopNavigation>Yes</ShowInTopNavigation>");
            bool ShowInFooterNavigation = sourceComponentXml.Contains("<ShowInFooterNavigation>Yes</ShowInFooterNavigation>");
            bool ShowInBreadcrumbs = sourceComponentXml.Contains("<ShowInBreadcrumbs>Yes</ShowInBreadcrumbs>");
            bool ShowInLeftSideNavigation = sourceComponentXml.Contains("<ShowInLeftSideNavigation>Yes</ShowInLeftSideNavigation>");
            bool ShowInSiteMapNavigation = sourceComponentXml.Contains("<ShowInSiteMapNavigation>Yes</ShowInSiteMapNavigation>");
            bool ShowInMobileNavigation = sourceComponentXml.Contains("<ShowInMobileNavigation>Yes</ShowInMobileNavigation>");

            XNamespace ns = sourceSchema.NamespaceUri;
            XElement resElement = new XElement(ns + sourceSchema.RootElementName);

            if (ShowInTopNavigation)
            {
                XElement NavigationType = new XElement(ns + "NavigationType");
                NavigationType.Add(new XElement(ns + "Title", title));
                ItemInfo keyword = keywords.First(x => x.Title == "Top");
                NavigationType.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "NavigationType"));
                resElement.Add(NavigationType);
            }

            if (ShowInFooterNavigation)
            {
                XElement NavigationType = new XElement(ns + "NavigationType");
                NavigationType.Add(new XElement(ns + "Title", title));
                ItemInfo keyword = keywords.First(x => x.Title == "Footer");
                NavigationType.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "NavigationType"));
                resElement.Add(NavigationType);
            }

            if (ShowInBreadcrumbs)
            {
                XElement NavigationType = new XElement(ns + "NavigationType");
                NavigationType.Add(new XElement(ns + "Title", title));
                ItemInfo keyword = keywords.First(x => x.Title == "Breadcrumbs");
                NavigationType.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "NavigationType"));
                resElement.Add(NavigationType);
            }

            if (ShowInLeftSideNavigation)
            {
                XElement NavigationType = new XElement(ns + "NavigationType");
                NavigationType.Add(new XElement(ns + "Title", title));
                ItemInfo keyword = keywords.First(x => x.Title == "LeftSide");
                NavigationType.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "NavigationType"));
                resElement.Add(NavigationType);
            }

            if (ShowInSiteMapNavigation)
            {
                XElement NavigationType = new XElement(ns + "NavigationType");
                NavigationType.Add(new XElement(ns + "Title", title));
                ItemInfo keyword = keywords.First(x => x.Title == "SiteMap");
                NavigationType.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "NavigationType"));
                resElement.Add(NavigationType);
            }

            if (ShowInMobileNavigation)
            {
                XElement NavigationType = new XElement(ns + "NavigationType");
                NavigationType.Add(new XElement(ns + "Title", title));
                ItemInfo keyword = keywords.First(x => x.Title == "Mobile");
                NavigationType.Add(Functions.GetKeywordLink(keyword.TcmId, keyword.Title, "NavigationType"));
                resElement.Add(NavigationType);
            }

            string resText = resElement.ToString();
            resText = resText.Replace(" xmlns=\"\"", string.Empty);

            return resText;
        }
    }
}
