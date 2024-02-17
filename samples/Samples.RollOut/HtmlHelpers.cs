using System.Xml;
using System.Xml.Linq;

namespace Okiya;

internal static class HtmlHelpers
{
    internal static XDocument CreateHtmlDocument(out XElement title, out XElement body)
    {
        XDocument document = new();
        using (XmlWriter writer = document.CreateWriter())
            writer.WriteDocType("html", null, null, null);
        title = new("title");
        body = new("body");

        XElement html = new("html", new XElement("head", title), body);
        document.Add(html);
        return document;
    }
}
