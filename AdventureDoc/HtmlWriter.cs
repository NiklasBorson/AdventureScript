using System.Text;
using System.Xml;

namespace AdventureDoc
{
    record struct Section(string Heading, List<RefPage> Pages);

    internal class HtmlWriter : IDisposable
    {
        string m_title;
        XmlWriter m_writer;
        Section[] m_sections;
        Dictionary<string, RefPage> m_apiMap = new Dictionary<string, RefPage>();
        RefPage? m_currentPage = null;

        public HtmlWriter(string fileName, Section[] sections)
        {
            // Sort the pages in each section, and initialize the API map.
            foreach (var section in sections)
            {
                section.Pages.Sort();

                foreach (var page in section.Pages)
                {
                    if (!m_apiMap.TryAdd(page.Name, page))
                    {
                        var other = m_apiMap[page.Name];
                        while (other.Next != null)
                        {
                            other = other.Next;
                        }
                        other.Next = page;
                    }
                }
            }

            m_title = Path.GetFileNameWithoutExtension(fileName).Replace('-', ' ');
            m_writer = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true });
            m_sections = sections;
        }

        public void Dispose()
        {
            m_writer.Dispose();
        }

        public void Write()
        {
            BeginElement("html");
            BeginElement("head");
            BeginElement("title");
            WriteRawString(m_title);
            EndElement(); // </title>
            BeginElement("link");
            WriteAttribute("rel", "stylesheet");
            WriteAttribute("href", "styles.css");
            EndElement(); // </link>
            EndElement(); // </head>

            BeginElement("body");

            WriteHeading("h1", m_title);

            BeginElement("ul");
            foreach (var section in m_sections)
            {
                BeginElement("li");
                WriteLink(section.Heading);
                EndElement(); // </li>
            }
            EndElement(); // </ul>

            foreach (var section in m_sections)
            {
                WriteHeading("h2", section.Heading);

                BeginElement("ul");
                foreach (var page in section.Pages)
                {
                    BeginElement("li");
                    WriteLink(page.Name);
                    EndElement(); // </li>
                }
                EndElement(); // </ul>

                foreach (var page in section.Pages)
                {
                    m_currentPage = page;

                    WriteHeading("h3", page.Name);

                    WriteParagraph(page.Description);

                    BeginElement("pre");
                    if (page.FileName != null)
                    {
                        WriteRawString($"include \"{page.FileName}\";\n\n");
                    }
                    WriteString(page.GetSyntax(), /*linkTypesOnly*/ true);
                    EndElement(); // </pre>

                    page.WriteMembers(this);
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

        static bool IsNameStartChar(char ch)
        {
            return ch == '_' || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
        }

        static bool IsNameChar(char ch)
        {
            return IsNameStartChar(ch) || (ch >= '0' && ch <= '9');
        }

        static bool IsTypeToken(string text, int pos)
        {
            for (int i = pos - 1; i >= 0; i--)
            {
                char ch = text[i];
                if (ch == ':')
                    return true;
                else if (ch != ' ')
                    return false;
            }
            return false;
        }

        static int FindNameStart(string text, int startPos)
        {
            for (int i = startPos; i < text.Length; i++)
            {
                char ch = text[i];
                if (ch == '#')
                {
                    break;
                }
                else if (ch == '$')
                {
                    if (i + 1 < text.Length && IsNameStartChar(text[i + 1]))
                        return i;
                }
                else if (IsNameStartChar(ch))
                {
                    return i;
                }
            }
            return text.Length;
        }

        static int FindNameEnd(string text, int startPos)
        {
            for (int i = startPos + 1; i < text.Length; i++)
            {
                if (!IsNameChar(text[i]))
                    return i;
            }
            return text.Length;
        }

        public void WriteRawString(string text)
        {
            m_writer.WriteString(text);
        }

        public void WriteString(string text, bool linkTypesOnly)
        {
            int pos = 0;
            int seekPos = 0;

            while (seekPos < text.Length)
            {
                seekPos = FindNameStart(text, seekPos);
                if (seekPos >= text.Length)
                    break;

                int wordPos = seekPos;
                seekPos = FindNameEnd(text, seekPos);

                bool isType = IsTypeToken(text, wordPos);
                if (linkTypesOnly && !isType)
                    continue;

                var word = text.Substring(wordPos, seekPos - wordPos);

                RefPage? page;
                if (!m_apiMap.TryGetValue(word, out page))
                    continue;

                while (page != null && (page == m_currentPage || isType != page.IsType))
                {
                    page = page.Next;
                }

                if (page == null)
                    continue;

                // Write text before the link.
                if (pos < wordPos)
                {
                    m_writer.WriteString(text.Substring(pos, wordPos - pos));
                }

                // Write the link.
                BeginElement("a");
                WriteAttribute("href", $"#{GetAnchor(page.Name)}");
                m_writer.WriteString(word);
                EndElement();

                // Advance past the linked word.
                pos = seekPos;
            }

            if (pos < text.Length)
            {
                m_writer.WriteString(text.Substring(pos));
            }
        }

        public void WriteParagraph(string text)
        {
            if (text.Length != 0)
            {
                BeginElement("p");
                WriteString(text, /*linkTypesOnly*/ false);
                EndElement();
            }
        }

        public void WriteTermDefList(IList<KeyValuePair<string, string>> list)
        {
            foreach (var item in list)
            {
                BeginElement("p");
                WriteAttribute("class", "term");
                WriteRawString(item.Key);
                EndElement();

                BeginElement("p");
                WriteAttribute("class", "def");
                WriteString(item.Value, /*linkTypesOnly*/ false);
                EndElement();
            }
        }

        public static string GetAnchor(string headingText) => 
            headingText.Replace(' ', '_').Replace('$', '_');

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
