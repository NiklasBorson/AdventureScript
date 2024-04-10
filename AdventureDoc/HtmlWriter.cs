using AdventureScript;
using System.Text;
using System.Xml;

namespace AdventureDoc
{
    internal class HtmlWriter : IDisposable
    {
        string m_title;
        XmlWriter m_writer;
        ApiSet m_apiSet;
        RefPage? m_currentPage = null;

        public HtmlWriter(string outputDir, string fileName, string title, ApiSet apiSet)
        {
            string filePath = Path.Combine(outputDir, fileName);

            m_title = title;
            m_writer = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true });
            m_apiSet = apiSet;
        }

        public void Dispose()
        {
            m_writer.Dispose();
        }

        public void WriteTopIndex()
        {
            BeginDocument();
            WriteHeading("h1", m_title);

            BeginToc();

            foreach (var pageType in m_apiSet.PageTypes)
            {
                if (pageType.Pages.Count != 0)
                {
                    WriteTocItem(
                        GetGlobalIndexTitle(pageType),
                        GetGlobalIndexFileName(pageType)
                        );
                }
            }

            EndToc();

            foreach (var module in m_apiSet.Modules)
            {
                WriteHeading("h2", $"{module.ModuleTitle} Module");

                BeginToc();

                foreach (var pageType in m_apiSet.PageTypes)
                {
                    if (GetIndexPages(module, pageType).Count != 0)
                    {
                        WriteTocItem(
                            GetIndexTitle(module.ModuleTitle, pageType),
                            GetIndexFileName(module.ModuleTitle, pageType)
                            );
                    }
                }

                EndToc();
            }

            EndDocument();
        }

        public static string GetGlobalIndexTitle(PageType pageType)
        {
            return GetIndexTitle("All", pageType);
        }

        public static string GetGlobalIndexFileName(PageType pageType)
        {
            return GetIndexFileName("All", pageType);
        }

        public static string GetIndexTitle(string moduleTitle, PageType pageType)
        {
            return $"{moduleTitle} {pageType.PluralName}";
        }

        public static string GetIndexFileName(string moduleTitle, PageType pageType)
        {
            return $"{moduleTitle}-{pageType.PluralName}.html";
        }

        public static List<RefPage> GetIndexPages(Module module, PageType pageType)
        {
            var pages = new List<RefPage>();

            foreach (var page in pageType.Pages)
            {
                if (page.Module == module)
                {
                    pages.Add(page);
                }
            }

            return pages;
        }

        public void WriteIndex(List<RefPage> pages)
        {
            BeginDocument();
            WriteHeading("h1", m_title);

            BeginToc();
            foreach (var page in pages)
            {
                WriteTocItem(page.Name, page.OutputFileName);
            }
            EndToc();

            WriteHeading("h2", "See Also");
            BeginToc();
            WriteTocItem(
                "Index",
                "index.html"
                );
            EndToc();

            EndDocument();
        }

        public void WritePage(RefPage page)
        {
            m_currentPage = page;
            BeginDocument();

            WriteHeading("h1", m_title);
            WriteParagraph(page.Description);

            BeginElement("pre");
            WriteString(page.GetSyntax(), /*linkTypesOnly*/ true);
            EndElement(); // </pre>

            page.WriteMembers(this);

            WriteHeading("h4", "See Also");
            BeginToc();
            WriteTocItem(
                GetIndexTitle(page.Module.ModuleTitle, page.PageType),
                GetIndexFileName(page.Module.ModuleTitle, page.PageType)
                );
            WriteTocItem(
                GetGlobalIndexTitle(page.PageType),
                GetGlobalIndexFileName(page.PageType)
                );
            WriteTocItem(
                "Index",
                "index.html"
                );
            EndToc();

            EndDocument();
            m_currentPage = null;
        }

        void BeginDocument()
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
        }

        void EndDocument()
        {
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

        public void BeginToc()
        {
            BeginElement("div");
            WriteAttribute("class", "toc");
        }

        public void EndToc()
        {
            EndElement();
        }

        public void WriteTocItem(string linkText, string url)
        {
            BeginElement("p");
            WriteLink(linkText, url);
            EndElement();
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
                else if (ch == '\"')
                {
                    string value;
                    i += StringHelpers.ParseStringLiteral(text.AsSpan(i), out value) - 1;
                }
                else if (ch == '$')
                {
                    if (i + 1 < text.Length)
                    {
                        if (IsNameStartChar(text[i + 1]))
                        {
                            return i;
                        }
                        else if (text[i + 1] == '\"')
                        {
                            string value;
                            i += StringHelpers.ParseStringLiteral(text.AsSpan(i + 1), out value);
                        }
                    }
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

                RefPage? page = m_apiSet.TryGetPage(word);
                if (page == null)
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
                WriteAttribute("href", $"{page.OutputFileName}");
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

        public void WriteHeading(string tag, string text)
        {
            BeginElement(tag);
            WriteRawString(text);
            EndElement(); // </h1>
        }

        public void WriteLink(string text, string url)
        {
            BeginElement("a");
            WriteAttribute("href", url);
            WriteRawString(text);
            EndElement();
        }
    }
}
