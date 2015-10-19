using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;
using SchemaType = TridionDesktopTools.Core.SchemaType;

namespace TridionDesktopTools.ComponentImporter.Custom
{
    [SourceTable(Title = "Articles")]
    [TargetSchema(Title = "Article", Type = SchemaType.Component)]
    public class DxaArticleImport : ICustomImporter
    {
        public string GetContent(string sourceTable, DataRow sourceDataRow, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string targetFolderUri, List<ResultInfo> results)
        {
            XNamespace ns = targetSchema.NamespaceUri;
            XElement resElement = new XElement(ns + targetSchema.RootElementName);

            //article header field
            XElement heading = new XElement(ns + "headline", sourceDataRow["Summary"]);
            resElement.Add(heading);

            //article image field - create multimedia component
            string imagePath = sourceDataRow["ImageUrl"] == null ? null : "C:\\web" + sourceDataRow["ImageUrl"].ToString().Replace("/", "\\");

            XElement metadata = new XElement(ns + "Metadata");
            metadata.Add(new XElement(ns + "altText", sourceDataRow["Summary"]));
            
            //create multimedia component
            ResultInfo imageResult = Functions.SaveMultimediaComponentFromBinary(imagePath, null, metadata.ToString(), targetFolderUri, "Image");
            if (imageResult.Status == Status.Success || imageResult.Status == Status.None)
            {
                //create component link
                ComponentData multimediaComponent = Functions.GetComponent(imageResult.TcmId);
                XElement imageLink = Functions.GetComponentLink(multimediaComponent.Id, multimediaComponent.Title, "image");
                resElement.Add(imageLink);
            }
            //add operation status to dialog
            results.Add(imageResult);

            //article body field - create paragraphs collection
            string body = sourceDataRow["Body"].ToString();
            MatchCollection matches = Regex.Matches(body, @"<p>\s*(.+?)\s*</p>");
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string value = match.Value;
                    value = Regex.Replace(value, "<p[^>]*>", string.Empty);
                    value = Regex.Replace(value, "</p>", string.Empty);

                    XElement articleBody = new XElement(ns + "articleBody");
                    XElement content = new XElement(ns + "content", value);
                    articleBody.Add(content);
                    resElement.Add(articleBody);
                }
            }
            else
            {
                //single paragraph
                XElement articleBody = new XElement(ns + "articleBody");
                XElement content = new XElement(ns + "content", body);
                articleBody.Add(content);
                resElement.Add(articleBody);
            }

            string resText = resElement.ToString();
            resText = resText.Replace(" xmlns=\"\"", string.Empty);

            return resText;
        }
    }
}