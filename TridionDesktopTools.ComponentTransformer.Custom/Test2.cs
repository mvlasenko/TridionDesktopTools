using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;
using TridionDesktopTools.Core;

namespace TridionDesktopTools.ComponentTransformer.Custom
{
    [SourceSchema(Type = SchemaType.Component)]
    [TargetSchema(Type = SchemaType.Component)]
    [SourceObject(Type = ObjectType.ComponentOrFolder)]
    public class Test2 : ICustomTransformer
    {
        public string GetFixedContent(SchemaData sourceSchema, List<ItemFieldDefinitionData> sourceComponentFields, List<ItemFieldDefinitionData> sourceMetadataFields, string sourceComponentXml, string sourceMetadataXml, string sourceUri, SchemaData targetSchema, List<ItemFieldDefinitionData> targetComponentFields, List<ItemFieldDefinitionData> targetMetadataFields, string targetFolderUri, List<ResultInfo> results)
        {
            ComponentData component = Functions.GetComponent(sourceUri);

            Dictionary<string, ComponentData> multimediaComponents = GetDublicatedMultimediaComponents(component, sourceSchema.NamespaceUri, sourceComponentFields);

            List<string> filenames = multimediaComponents.Values.Select(x => x.BinaryContent.Filename).ToList();

            if (filenames.Count != filenames.Distinct().Count())
            {
                var thumb = multimediaComponents.Keys.FirstOrDefault(x => x == "Thumbnail");
                if (thumb != null)
                {
                    return DuplicateMulimediaComponentFileName(multimediaComponents.First(x => x.Key == "Thumbnail").Value, component);
                }
            }

            return string.Empty;
        }

        private static Dictionary<string, ComponentData> GetDublicatedMultimediaComponents(ComponentData component, XNamespace ns, List<ItemFieldDefinitionData> fields)
        {
            Dictionary<string, ComponentData> res = new Dictionary<string, ComponentData>();

            XNamespace imgNs = "http://www.w3.org/1999/xlink";

            foreach (ItemFieldDefinitionData field in fields)
            {
                XElement imageValue = Functions.GetComponentSingleValue(component, field, ns) as XElement;
                if (imageValue == null)
                    continue;

                if (field is MultimediaLinkFieldDefinitionData)
                {
                    string tcmImage = imageValue.Attribute(imgNs + "href").Value;
                    ComponentData imageComponent = Functions.GetComponent(tcmImage);
                    res.Add(field.Name, imageComponent);
                }
            }

            return res;
        }

        private static string DuplicateMulimediaComponentFileName(ComponentData multComponent, ComponentData component)
        {
            string name = multComponent.BinaryContent.Filename;
            string newName = Path.GetFileNameWithoutExtension(name) + "_thumb" + Path.GetExtension(name);

            //todo: change bynary file name

            string tcm = Functions.GetItemTcmId(multComponent.LocationInfo.OrganizationalItem.IdRef, newName);

            if (string.IsNullOrEmpty(tcm))
                return string.Empty;

            string xml = component.Content.PrettyXml();
            string format = "<Thumbnail xlink:href=\"{0}\" xlink:title=\"{1}\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" />";

            xml = xml.Replace(string.Format(format, multComponent.Id, name), string.Format(format, tcm, newName));

            return xml;
        }
    }
}
