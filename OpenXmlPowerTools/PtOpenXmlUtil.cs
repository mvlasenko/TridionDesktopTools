/***************************************************************************

Copyright (c) Microsoft Corporation 2011.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license
can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;

namespace OpenXmlPowerTools
{
    public class WmlDocument
    {
        public byte[] RawDocument { get; set; }

        public WordprocessingDocument GetWordprocessingDocument()
        {
            MemoryStream mem = new MemoryStream();
            mem.Write(RawDocument, 0, RawDocument.Length);
            WordprocessingDocument doc = WordprocessingDocument.Open(mem, true);
            return doc;
        }

        public WmlDocument(WmlDocument original)
        {
            RawDocument = new byte[original.RawDocument.Length];
            Array.Copy(original.RawDocument, RawDocument, original.RawDocument.Length);
        }

        public WmlDocument(string fileName)
        {
            RawDocument = File.ReadAllBytes(fileName);
        }

        public WmlDocument(byte[] byteArray)
        {
            RawDocument = new byte[byteArray.Length];
            Array.Copy(byteArray, RawDocument, byteArray.Length);
        }

        public void Save(string fileName)
        {
            File.WriteAllBytes(fileName, RawDocument);
        }
    }

    public static class W
    {
        public static XNamespace w =
            "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        public static XName _default = w + "default";
        public static XName _w = w + "w";
        public static XName abstractNum = w + "abstractNum";
        public static XName abstractNumId = w + "abstractNumId";
        public static XName altChunk = w + "altChunk";
        public static XName annotationRef = w + "annotationRef";
        public static XName attachedTemplate = w + "attachedTemplate";
        public static XName b = w + "b";
        public static XName basedOn = w + "basedOn";
        public static XName bidiVisual = w + "bidiVisual";
        public static XName body = w + "body";
        public static XName bookmarkEnd = w + "bookmarkEnd";
        public static XName bookmarkStart = w + "bookmarkStart";
        public static XName br = w + "br";
        public static XName cellDel = w + "cellDel";
        public static XName cellIns = w + "cellIns";
        public static XName cellMerge = w + "cellMerge";
        public static XName comment = w + "comment";
        public static XName commentRangeEnd = w + "commentRangeEnd";
        public static XName commentRangeStart = w + "commentRangeStart";
        public static XName commentReference = w + "commentReference";
        public static XName comments = w + "comments";
        public static XName control = w + "control";
        public static XName continuationSeparator = w + "continuationSeparator";
        public static XName cr = w + "cr";
        public static XName customXmlDelRangeEnd = w + "customXmlDelRangeEnd";
        public static XName customXmlDelRangeStart = w + "customXmlDelRangeStart";
        public static XName customXmlInsRangeEnd = w + "customXmlInsRangeEnd";
        public static XName customXmlInsRangeStart = w + "customXmlInsRangeStart";
        public static XName customXmlMoveFromRangeEnd = w + "customXmlMoveFromRangeEnd";
        public static XName customXmlMoveFromRangeStart = w + "customXmlMoveFromRangeStart";
        public static XName customXmlMoveToRangeEnd = w + "customXmlMoveToRangeEnd";
        public static XName customXmlMoveToRangeStart = w + "customXmlMoveToRangeStart";
        public static XName dayLong = w + "dayLong";
        public static XName dayShort = w + "dayShort";
        public static XName dataBinding = w + "dataBinding";
        public static XName dataSource = w + "dataSource";
        public static XName del = w + "del";
        public static XName delInstrText = w + "delInstrText";
        public static XName delText = w + "delText";
        public static XName docDefaults = w + "docDefaults";
        public static XName document = w + "document";
        public static XName drawing = w + "drawing";
        public static XName element = w + "element";
        public static XName embedBold = w + "embedBold";
        public static XName embedBoldItalic = w + "embedBoldItalic";
        public static XName embedItalic = w + "embedItalic";
        public static XName embedRegular = w + "embedRegular";
        public static XName endnote = w + "endnote";
        public static XName endnotes = w + "endnotes";
        public static XName endnoteReference = w + "endnoteReference";
        public static XName fldChar = w + "fldChar";
        public static XName fldCharType = w + "fldCharType";
        public static XName fldData = w + "fldData";
        public static XName fldSimple = w + "fldSimple";
        public static XName footerReference = w + "footerReference";
        public static XName footnote = w + "footnote";
        public static XName footnotes = w + "footnotes";
        public static XName footnoteReference = w + "footnoteReference";
        public static XName lvlPicBulletId = w + "lvlPicBulletId";
        public static XName numbering = w + "numbering";
        public static XName numPicBullet = w + "numPicBullet";
        public static XName numPicBulletId = w + "numPicBulletId";
        public static XName nsid = w + "nsid";
        public static XName font = w + "font";
        public static XName ftr = w + "ftr";
        public static XName gridSpan = w + "gridSpan";
        public static XName hanging = w + "hanging";
        public static XName headerReference = w + "headerReference";
        public static XName hdr = w + "hdr";
        public static XName hyperlink = w + "hyperlink";
        public static XName id = w + "id";
        public static XName ilvl = w + "ilvl";
        public static XName ind = w + "ind";
        public static XName ins = w + "ins";
        public static XName instrText = w + "instrText";
        public static XName isLgl = w + "isLgl";
        public static XName jc = w + "jc";
        public static XName lang = w + "lang";
        public static XName lastRenderedPageBreak = w + "lastRenderedPageBreak";
        public static XName left = w + "left";
        public static XName lvl = w + "lvl";
        public static XName lvlJc = w + "lvlJc";
        public static XName lvlOverride = w + "lvlOverride";
        public static XName lvlRestart = w + "lvlRestart";
        public static XName lvlText = w + "lvlText";
        public static XName monthLong = w + "monthLong";
        public static XName monthShort = w + "monthShort";
        public static XName moveFrom = w + "moveFrom";
        public static XName moveFromRangeEnd = w + "moveFromRangeEnd";
        public static XName moveFromRangeStart = w + "moveFromRangeStart";
        public static XName moveTo = w + "moveTo";
        public static XName moveToRangeEnd = w + "moveToRangeEnd";
        public static XName moveToRangeStart = w + "moveToRangeStart";
        public static XName name = w + "name";
        public static XName noBreakHyphen = w + "noBreakHyphen";
        public static XName noProof = w + "noProof";
        public static XName num = w + "num";
        public static XName numFmt = w + "numFmt";
        public static XName numId = w + "numId";
        public static XName numPr = w + "numPr";
        public static XName mailMerge = w + "mailMerge";
        public static XName frameset = w + "frameset";
        public static XName numStyleLink = w + "numStyleLink";
        public static XName numberingChange = w + "numberingChange";
        public static XName outlineLvl = w + "outlineLvl";
        public static XName p = w + "p";
        public static XName pPr = w + "pPr";
        public static XName pPrChange = w + "pPrChange";
        public static XName pPrDefault = w + "pPrDefault";
        public static XName pStyle = w + "pStyle";
        public static XName pTab = w + "pTab";
        public static XName permEnd = w + "permEnd";
        public static XName permStart = w + "permStart";
        public static XName pgNum = w + "pgNum";
        public static XName pict = w + "pict";
        public static XName proofErr = w + "proofErr";
        public static XName r = w + "r";
        public static XName rFonts = w + "rFonts";
        public static XName rPr = w + "rPr";
        public static XName rPrChange = w + "rPrChange";
        public static XName rPrDefault = w + "rPrDefault";
        public static XName rStyle = w + "rStyle";
        public static XName rsid = w + "rsid";
        public static XName rsidDel = w + "rsidDel";
        public static XName rsidP = w + "rsidP";
        public static XName rsidR = w + "rsidR";
        public static XName rsidRDefault = w + "rsidRDefault";
        public static XName rsidRPr = w + "rsidRPr";
        public static XName rsidSect = w + "rsidSect";
        public static XName rsidTr = w + "rsidTr";
        public static XName sdt = w + "sdt";
        public static XName sdtContent = w + "sdtContent";
        public static XName sectPr = w + "sectPr";
        public static XName sectPrChange = w + "sectPrChange";
        public static XName separator = w + "separator";
        public static XName shd = w + "shd";
        public static XName smartTag = w + "smartTag";
        public static XName softHyphen = w + "softHyphen";
        public static XName spacing = w + "spacing";
        public static XName start = w + "start";
        public static XName startOverride = w + "startOverride";
        public static XName storeItemID = w + "storeItemID";
        public static XName style = w + "style";
        public static XName styleId = w + "styleId";
        public static XName suff = w + "suff";
        public static XName sym = w + "sym";
        public static XName t = w + "t";
        public static XName tab = w + "tab";
        public static XName tbl = w + "tbl";
        public static XName tblBorders = w + "tblBorders";
        public static XName tblCellMar = w + "tblCellMar";
        public static XName tblCellSpacing = w + "tblCellSpacing";
        public static XName tblGridChange = w + "tblGridChange";
        public static XName tblInd = w + "tblInd";
        public static XName tblLayout = w + "tblLayout";
        public static XName tblLook = w + "tblLook";
        public static XName tblOverlap = w + "tblOverlap";
        public static XName tblPr = w + "tblPr";
        public static XName tblPrChange = w + "tblPrChange";
        public static XName tblPrExChange = w + "tblPrExChange";
        public static XName tblStyle = w + "tblStyle";
        public static XName tblStyleColBandSize = w + "tblStyleColBandSize";
        public static XName tblStylePr = w + "tblStylePr";
        public static XName tblStyleRowBandSize = w + "tblStyleRowBandSize";
        public static XName tblW = w + "tblW";
        public static XName tblpPr = w + "tblpPr";
        public static XName tc = w + "tc";
        public static XName tcPr = w + "tcPr";
        public static XName tcPrChange = w + "tcPrChange";
        public static XName tcW = w + "tcW";
        public static XName textbox = w + "textbox";
        public static XName tr = w + "tr";
        public static XName trPr = w + "trPr";
        public static XName trPrChange = w + "trPrChange";
        public static XName txbxContent = w + "txbxContent";
        public static XName type = w + "type";
        public static XName uri = w + "uri";
        public static XName vMerge = w + "vMerge";
        public static XName val = w + "val";
        public static XName webHidden = w + "webHidden";
        public static XName yearLong = w + "yearLong";
        public static XName yearShort = w + "yearShort";
        public static XName headerSource = w + "headerSource";
        public static XName longDesc = w + "longDesc";
        public static XName printerSettings = w + "printerSettings";
        public static XName recipientData = w + "recipientData";
        public static XName saveThroughXslt = w + "saveThroughXslt";
        public static XName sourceFileName = w + "sourceFileName";
        public static XName src = w + "src";
        public static XName subDoc = w + "subDoc";
        public static XName footnotePr = w + "footnotePr";
        public static XName endnotePr = w + "endnotePr";
        public static XName numIdMacAtCleanup = w + "numIdMacAtCleanup";
        
        public static XName[] BlockLevelContentContainers =
        {
            W.body,
            W.tc,
            W.txbxContent,
            W.hdr,
            W.ftr,
            W.endnote,
            W.footnote
        };

        public static XName[] SubRunLevelContent =
        {
            W.br,
            W.cr,
            W.dayLong,
            W.dayShort,
            W.drawing,
            W.drawing,
            W.monthLong,
            W.monthShort,
            W.noBreakHyphen,
            W.pTab,
            W.pgNum,
            W.pict,
            W.softHyphen,
            W.sym,
            W.t,
            W.tab,
            W.yearLong,
            W.yearShort,
            MC.AlternateContent,
        };
    }

    public static class W10
    {
        public static XNamespace w10 =
            "urn:schemas-microsoft-com:office:word";
    }

    public static class W14
    {
        public static XNamespace w14 =
            "http://schemas.microsoft.com/office/word/2010/wordml";
    }

    public static class WP14
    {
        public static XNamespace wp14 =
            "http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing";
    }

    public static class WPS
    {
        public static XNamespace wps =
            "http://schemas.microsoft.com/office/word/2010/wordprocessingShape";
    }

    public static class WPC
    {
        public static XNamespace wpc =
            "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas";
    }

    public static class WPG
    {
        public static XNamespace wpg =
            "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup";
    }

    public static class WPI
    {
        public static XNamespace wpi =
            "http://schemas.microsoft.com/office/word/2010/wordprocessingInk";
    }

    public static class DS
    {
        public static XNamespace ds =
            "http://schemas.openxmlformats.org/officeDocument/2006/customXml";
        public static XName itemID = ds + "itemID";
    }

    public static class WP
    {
        public static XNamespace wp =
            "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing";
        public static XName inline = wp + "inline";
        public static XName anchor = wp + "anchor";
        public static XName extent = wp + "extent";
        public static XName docPr = wp + "docPr";
    }

    public static class WNE
    {
        public static XNamespace wne = "http://schemas.microsoft.com/office/word/2006/wordml";

        public static XName toolbarData = wne + "toolbarData";
    }

    public static class WPNoNamespace
    {
        public static XName cx = "cx";
        public static XName cy = "cy";
        public static XName name = "name";
        public static XName descr = "descr";
    }

    public static class C
    {
        public static XNamespace c = "http://schemas.openxmlformats.org/drawingml/2006/chart";

        public static XName chart = c + "chart";
        public static XName externalData = c + "externalData";
        public static XName userShapes = c + "userShapes";
    }

    public static class Pic
    {
        public static XNamespace picNs =
            "http://schemas.openxmlformats.org/drawingml/2006/picture";
        public static XName pic = picNs + "pic";
        public static XName blipFill = picNs + "blipFill";
    }

    public static class M
    {
        public static XNamespace m =
            "http://schemas.openxmlformats.org/officeDocument/2006/math";
        public static XName ctrlPr = m + "ctrlPr";
        public static XName f = m + "f";
        public static XName fPr = m + "fPr";
        public static XName oMath = m + "oMath";
        public static XName r = m + "r";
        public static XName rPr = m + "rPr";
        public static XName sty = m + "sty";
        public static XName t = m + "t";
    }

    public static class DGM
    {
        public static XNamespace dgm = "http://schemas.openxmlformats.org/drawingml/2006/diagram";

        public static XName relIds = dgm + "relIds";
    }

    public static class O
    {
        public static XNamespace o = "urn:schemas-microsoft-com:office:office";

        public static XName OLEObject = o + "OLEObject";
        public static XName gfxdata = o + "gfxdata";
    }

    public static class R
    {
        public static XNamespace r =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        public static XName id = r + "id";
        public static XName embed = r + "embed";
        public static XName cs = r + "cs";
        public static XName dm = r + "dm";
        public static XName href = r + "href";
        public static XName link = r + "link";
        public static XName lo = r + "lo";
        public static XName pict = r + "pict";
        public static XName qs = r + "qs";
    }

    public static class A
    {
        public static XNamespace a =
            "http://schemas.openxmlformats.org/drawingml/2006/main";
        public static XName theme = a + "theme";
        public static XName themeElements = a + "themeElements";
        public static XName clrScheme = a + "clrScheme";
        public static XName dk1 = a + "dk1";
        public static XName sysClr = a + "sysClr";
        public static XName fontScheme = a + "fontScheme";
        public static XName majorFont = a + "majorFont";
        public static XName latin = a + "latin";
        public static XName ea = a + "ea";
        public static XName cs = a + "cs";
        public static XName font = a + "font";
        public static XName minorFont = a + "minorFont";
        public static XName graphic = a + "graphic";
        public static XName graphicData = a + "graphicData";
        public static XName srcRect = a + "srcRect";
        public static XName stretch = a + "stretch";
        public static XName fillRect = a + "fillRect";
        public static XName blip = a + "blip";
        public static XName hlinkClick = a + "hlinkClick";
        public static XName relIds = a + "relIds";
    }

    public static class DrawingmlNoNamespace
    {
        public static XName val = "val";
        public static XName lastClr = "lastClr";
        public static XName lt1 = "lt1";
        public static XName typeface = "typeface";
        public static XName script = "script";
        public static XName id = "id";
    }

    public static class MC
    {
        public static XNamespace mc =
            "http://schemas.openxmlformats.org/markup-compatibility/2006";
        public static XName AlternateContent = mc + "AlternateContent";
        public static XName Choice = mc + "Choice";
        public static XName Fallback = mc + "Fallback";
        public static XName Ignorable = mc + "Ignorable";
    }

    public static class VML
    {
        public static XNamespace vml = "urn:schemas-microsoft-com:vml";
        public static XName shape = vml + "shape";
        public static XName imagedata = vml + "imagedata";
        public static XName fill = vml + "fill";
        public static XName stroke = vml + "stroke";
    }

    public static class PtOpenXmlExtensions
    {
        public static XDocument GetXDocument(this OpenXmlPart part)
        {
            
            XDocument partXDocument = part.Annotation<XDocument>();
            if (partXDocument != null)
                return partXDocument;
            using (Stream partStream = part.GetStream())
            using (XmlReader partXmlReader = XmlReader.Create(partStream))
                partXDocument = XDocument.Load(partXmlReader);
            part.AddAnnotation(partXDocument);
            return partXDocument;
        }

        public static void PutXDocument(this OpenXmlPart part)
        {
            XDocument partXDocument = part.GetXDocument();
            if (partXDocument != null)
            {
                using (Stream partStream = part.GetStream(FileMode.Create, FileAccess.Write))
                using (XmlWriter partXmlWriter = XmlWriter.Create(partStream))
                    partXDocument.Save(partXmlWriter);
            }
        }

        public static void PutXDocument(this OpenXmlPart part, XDocument document)
        {
            using (Stream partStream = part.GetStream(FileMode.Create, FileAccess.Write))
            using (XmlWriter partXmlWriter = XmlWriter.Create(partStream))
                document.Save(partXmlWriter);
            part.RemoveAnnotations<XDocument>();
            part.AddAnnotation(document);
        }

        public static IEnumerable<XElement> LogicalChildrenContent(this XElement element)
        {
            if (element.Name == W.document)
                return element.Descendants(W.body).Take(1);
            if (element.Name == W.body ||
                element.Name == W.tc ||
                element.Name == W.txbxContent)
                return element
                    .DescendantsTrimmed(e =>
                        e.Name == W.tbl ||
                        e.Name == W.p)
                    .Where(e =>
                        e.Name == W.p ||
                        e.Name == W.tbl);
            if (element.Name == W.tbl)
                return element
                    .DescendantsTrimmed(W.tr)
                    .Where(e => e.Name == W.tr);
            if (element.Name == W.tr)
                return element
                    .DescendantsTrimmed(W.tc)
                    .Where(e => e.Name == W.tc);
            if (element.Name == W.p)
                return element
                    .DescendantsTrimmed(e => e.Name == W.r ||
                        e.Name == W.pict ||
                        e.Name == W.drawing)
                    .Where(e => e.Name == W.r ||
                        e.Name == W.pict ||
                        e.Name == W.drawing);
            if (element.Name == W.r)
                return element
                    .DescendantsTrimmed(e => W.SubRunLevelContent.Contains(e.Name))
                    .Where(e => W.SubRunLevelContent.Contains(e.Name));
            if (element.Name == MC.AlternateContent)
                return element
                    .DescendantsTrimmed(e =>
                        e.Name == W.pict ||
                        e.Name == W.drawing ||
                        e.Name == MC.Fallback)
                    .Where(e =>
                        e.Name == W.pict ||
                        e.Name == W.drawing);
            if (element.Name == W.pict || element.Name == W.drawing)
                return element
                    .DescendantsTrimmed(W.txbxContent)
                    .Where(e => e.Name == W.txbxContent);
            return XElement.EmptySequence;
        }

        public static IEnumerable<XElement> LogicalChildrenContent(
            this IEnumerable<XElement> source)
        {
            foreach (XElement e1 in source)
                foreach (XElement e2 in e1.LogicalChildrenContent())
                    yield return e2;
        }

        public static IEnumerable<XElement> LogicalChildrenContent(
            this XElement element, XName name)
        {
            return element.LogicalChildrenContent().Where(e => e.Name == name);
        }

        public static IEnumerable<XElement> LogicalChildrenContent(
            this IEnumerable<XElement> source, XName name)
        {
            foreach (XElement e1 in source)
                foreach (XElement e2 in e1.LogicalChildrenContent(name))
                    yield return e2;
        }

        // Used to track changes to parts
        private class ChangedSemaphore { }
        private static EventHandler<XObjectChangeEventArgs> ElementChanged = new EventHandler<XObjectChangeEventArgs>(ElementChangedHandler);

        /// <summary>
        /// Gets the XDocument for a part	
        /// </summary>
        public static XDocument GetXDocumentWithTracking(this OpenXmlPart part)
        {
            XDocument xdoc = part.Annotation<XDocument>();
            if (xdoc != null)
                return xdoc;
            try
            {
                using (StreamReader sr = new StreamReader(part.GetStream()))
                using (XmlReader xr = XmlReader.Create(sr))
                {
                    xdoc = XDocument.Load(xr);
                    xdoc.Changed += ElementChanged;
                    xdoc.Changing += ElementChanged;
                }
            }
            catch (XmlException)
            {
                XDeclaration xdec = new XDeclaration("1.0", "UTF-8", "yes");
                xdoc = new XDocument(xdec);
                xdoc.AddAnnotation(new ChangedSemaphore());
            }
            part.AddAnnotation(xdoc);
            return xdoc;
        }

        private static void ElementChangedHandler(object sender, XObjectChangeEventArgs e)
        {
            XDocument xDocument = ((XObject)sender).Document;
            if (xDocument != null)
            {
                xDocument.Changing -= ElementChanged;
                xDocument.Changed -= ElementChanged;
                xDocument.AddAnnotation(new ChangedSemaphore());
            }
        }

        /// <summary>
        /// Writes out all XDocuments	
        /// </summary>
        public static void FlushTrackedXDocuments(this OpenXmlPackage doc)
        {
            HashSet<OpenXmlPart> visited = new HashSet<OpenXmlPart>();
            foreach (IdPartPair item in doc.Parts)
                FlushPart(item.OpenXmlPart, visited);
        }

        private static void FlushPart(OpenXmlPart part, HashSet<OpenXmlPart> visited)
        {
            visited.Add(part);
            XDocument xdoc = part.Annotation<XDocument>();
            if (xdoc != null && xdoc.Annotation<ChangedSemaphore>() != null)
            {
                using (XmlWriter xw = XmlWriter.Create(part.GetStream(FileMode.Create, FileAccess.Write)))
                {
                    xdoc.Save(xw);
                }
                xdoc.RemoveAnnotations<ChangedSemaphore>();
                xdoc.Changing += ElementChanged;
                xdoc.Changed += ElementChanged;
            }
            foreach (IdPartPair item in part.Parts)
                if (!visited.Contains(item.OpenXmlPart))
                    FlushPart(item.OpenXmlPart, visited);
        }
    }

    public class FieldInfo
    {
        public string FieldType;
        public string[] Switches;
        public string[] Arguments;
    }

    public static class FieldParser
    {
        private enum State
        {
            InToken,
            InWhiteSpace,
            InQuotedToken,
            OnOpeningQuote,
            OnClosingQuote,
            OnBackslash,
        }

        private static string[] GetTokens(string field)
        {
            State state = State.InWhiteSpace;
            int tokenStart = 0;
            char quoteStart = char.MinValue;
            List<string> tokens = new List<string>();
            for (int c = 0; c < field.Length; c++)
            {
                if (Char.IsWhiteSpace(field[c]))
                {
                    if (state == State.InToken)
                    {
                        tokens.Add(field.Substring(tokenStart, c - tokenStart));
                        state = State.InWhiteSpace;
                        continue;
                    }
                    if (state == State.OnOpeningQuote)
                    {
                        tokenStart = c;
                        state = State.InQuotedToken;
                    }
                    if (state == State.OnClosingQuote)
                        state = State.InWhiteSpace;
                    continue;
                }
                if (field[c] == '\\')
                {
                    if (state == State.InQuotedToken)
                    {
                        state = State.OnBackslash;
                        continue;
                    }
                }
                if (state == State.OnBackslash)
                {
                    state = State.InQuotedToken;
                    continue;
                }
                if (field[c] == '"' || field[c] == '\'' || field[c] == 0x201d)
                {
                    if (state == State.InWhiteSpace)
                    {
                        quoteStart = field[c];
                        state = State.OnOpeningQuote;
                        continue;
                    }
                    if (state == State.InQuotedToken)
                    {
                        if (field[c] == quoteStart)
                        {
                            tokens.Add(field.Substring(tokenStart, c - tokenStart));
                            state = State.OnClosingQuote;
                            continue;
                        }
                        continue;
                    }
                    if (state == State.OnOpeningQuote)
                    {
                        if (field[c] == quoteStart)
                        {
                            state = State.OnClosingQuote;
                            continue;
                        }
                        else
                        {
                            tokenStart = c;
                            state = State.InQuotedToken;
                            continue;
                        }
                    }
                    continue;
                }
                if (state == State.InWhiteSpace)
                {
                    tokenStart = c;
                    state = State.InToken;
                    continue;
                }
                if (state == State.OnOpeningQuote)
                {
                    tokenStart = c;
                    state = State.InQuotedToken;
                    continue;
                }
                if (state == State.OnClosingQuote)
                {
                    tokenStart = c;
                    state = State.InToken;
                    continue;
                }
            }
            if (state == State.InToken)
                tokens.Add(field.Substring(tokenStart, field.Length - tokenStart));
            return tokens.ToArray();
        }

        public static FieldInfo ParseField(string field)
        {
            FieldInfo emptyField = new FieldInfo
            {
                FieldType = "",
                Arguments = new string[] {},
                Switches = new string[] {},
            };

            if (field.Length == 0)
                return emptyField;
            string fieldType = field.TrimStart().Split(' ').FirstOrDefault();
            if (fieldType == null || fieldType.ToUpper() != "HYPERLINK")
                return emptyField;
            string[] tokens = GetTokens(field);
            if (tokens.Length == 0)
                return emptyField;
            FieldInfo fieldInfo = new FieldInfo()
            {
                FieldType = tokens[0],
                Switches = tokens.Where(t => t[0] == '\\').ToArray(),
                Arguments = tokens.Skip(1).Where(t => t[0] != '\\').ToArray(),
            };
            return fieldInfo;
        }
    }

    public class XEntity : XText
    {
        public override void WriteTo(XmlWriter writer)
        {
            writer.WriteEntityRef(this.Value);
        }
        public XEntity(string value) : base(value) { }
    }
}
