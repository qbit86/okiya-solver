using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        var assembly = Assembly.GetExecutingAssembly();
        string[] resourceNames = assembly.GetManifestResourceNames();
        string resourceName = resourceNames.Single(it => it.EndsWith("style.css", StringComparison.Ordinal));
        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        using StreamReader streamReader = new(stream, Encoding.UTF8);

        XElement preConnectFontLink = new("link", new XAttribute("href", "https://fonts.googleapis.com"));
        XElement fontLink = new("link",
            new XAttribute("href", "https://fonts.googleapis.com/css2?family=JetBrains+Mono"),
            new XAttribute("rel", "stylesheet"));
        XElement style = new("style",
            new XAttribute("type", "text/css"),
            Environment.NewLine,
            streamReader.ReadToEnd()
        );

        XElement html = new("html", new XElement("head", preConnectFontLink, fontLink, style, title), body);
        document.Add(html);
        return document;
    }
}
