using AdventureScript;
using System.Text;

namespace AdventureDoc
{
    internal class Definition
    {
        SourcePos m_sourcePos;
        string m_description = string.Empty;
        List<KeyValuePair<string, string>> m_members = new List<KeyValuePair<string, string>>();

        public Definition()
        {
        }

        public Definition(Lexer lexer)
        {
            m_sourcePos = lexer.SourcePos;

            var description = new StringBuilder();

            foreach (var line in lexer.DocComments)
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

        public SourcePos SourcePos => m_sourcePos;
        public string Description => m_description;
        public IList<KeyValuePair<string, string>> Members => m_members;
    }
}
