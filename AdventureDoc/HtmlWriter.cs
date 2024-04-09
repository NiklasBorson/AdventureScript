using System.Xml;

namespace AdventureDoc
{
    record struct Section(string Heading, List<RefPage> Pages);

    internal class HtmlWriter : IDisposable
    {
        string m_title;
        XmlWriter m_writer;

        public HtmlWriter(string fileName)
        {
            m_title = Path.GetFileNameWithoutExtension(fileName).Replace('-', ' ');
            m_writer = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true });
        }

        public void Dispose()
        {
            m_writer.Dispose();
        }

        public void Write(Section[] sections)
        {
            BeginElement("html");
            BeginElement("head");
            BeginElement("title");
            WriteString(m_title);
            EndElement(); // </title>
            BeginElement("link");
            WriteAttribute("rel", "stylesheet");
            WriteAttribute("href", "styles.css");
            EndElement(); // </link>
            EndElement(); // </head>

            BeginElement("body");

            WriteHeading("h1", m_title);

            BeginElement("ul");
            foreach (var section in sections)
            {
                BeginElement("li");
                WriteLink(section.Heading);
                EndElement(); // </li>
            }
            EndElement(); // </ul>

            foreach (var section in sections)
            {
                WriteHeading("h2", section.Heading);

                BeginElement("ul");
                foreach (var page in section.Pages)
                {
                    BeginElement("li");
                    WriteLink(page.Heading);
                    EndElement(); // </li>
                }
                EndElement(); // </ul>

                foreach (var page in section.Pages)
                {
                    WriteHeading("h3", page.Heading);

                    WriteParagraph(page.Description);

                    page.Write(this);
                }
            }

            EndElement(); // </body>
            EndElement(); // </html>
        }

        public void BeginElement(string name)
        {
            m_writer.WriteStartElement(name, "http://www.w3.org/1999/xhtml");
        }

        public void EndElement()
        {
            m_writer.WriteEndElement();
        }

        public void WriteAttribute(string name, string value)
        {
            m_writer.WriteAttributeString(name, value);
        }

        public void WriteString(string text)
        {
            m_writer.WriteString(text);
        }

        public void WriteParagraph(string text)
        {
            if (text.Length != 0)
            {
                BeginElement("p");
                WriteString(text);
                EndElement();
            }
        }

        public static string GetAnchor(string headingText) => headingText.Replace(' ', '_');

        public void WriteHeading(string tag, string text)
        {
            BeginElement(tag);
            BeginElement("a");
            m_writer.WriteAttributeString("name", GetAnchor(text));
            m_writer.WriteEndElement(); // </a>
            m_writer.WriteString(text);
            m_writer.WriteEndElement(); // </h1>
        }

        public void WriteLink(string text, string url)
        {
            BeginElement("a");
            m_writer.WriteAttributeString("href", url);
            m_writer.WriteString(text);
            m_writer.WriteEndElement();
        }

        public void WriteLink(string text)
        {
            WriteLink(text, "#" + GetAnchor(text));
        }
    }
}
