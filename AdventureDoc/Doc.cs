using AdventureScript;
using System.Text;

namespace AdventureDoc
{
    internal class Doc
    {
        Module m_module;
        PageType m_pageType;
        SourcePos m_sourcePos;
        string m_description = string.Empty;
        List<KeyValuePair<string, string>> m_members = new List<KeyValuePair<string, string>>();

        public Doc(Module module, PageType pageType, SourcePos sourcePos, string[] docComments)
        {
            m_module = module;
            m_pageType = pageType;
            m_sourcePos = sourcePos;

            var description = new StringBuilder();

            foreach (var line in docComments)
            {
                int i = line.IndexOf(": ");
                if (i > 0)
                {
                    m_members.Add(new KeyValuePair<string, string>(
                        line.Substring(3, i - 3),
                        line.Substring(i + 2)
                        ));
                }
                else if (m_members.Count == 0)
                {
                    if (description.Length != 0)
                    {
                        // Include whitespace.
                        description.Append(line, 2, line.Length - 2);
                    }
                    else
                    {
                        // Do not include whitespace.
                        description.Append(line, 3, line.Length - 3);
                    }
                }
                else 
                { 
                    WriteWarning("Non-member after member.");
                }
            }

            m_description = description.ToString();
        }

        public void WriteWarning(string message)
        {
            Console.Error.WriteLine($"Warning: {m_sourcePos}: {message}");
        }

        public Module Module => m_module;
        public PageType PageType => m_pageType;
        public SourcePos SourcePos => m_sourcePos;
        public string Description => m_description;
        public IList<KeyValuePair<string, string>> Members => m_members;
    }
}
