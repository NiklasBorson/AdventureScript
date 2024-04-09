using AdventureScript;
using System.ComponentModel.Design.Serialization;

namespace AdventureDoc
{
    internal class FunctionInfo
    {
        string m_description;
        KeyValuePair<string, string>[]? m_params;
        string m_returnValue = string.Empty;

        public FunctionInfo(Definition def, string name, IList<ParamDef> paramList, TypeDef returnType)
        {
            m_description = def.Description;

            var members = def.Members;
            int memberCount = members.Count;

            // Process the $return member if present.
            if (memberCount > 0 && members[memberCount - 1].Key == "$return")
            {
                if (returnType == Types.Void)
                {
                    def.WriteWarning($"$return: {name} does not return a value.");
                }
                else
                {
                    m_returnValue = members[memberCount - 1].Value;
                }
                memberCount--;
            }

            if (memberCount != 0 && memberCount == paramList.Count)
            {
                m_params = new KeyValuePair<string, string>[paramList.Count];
                for (int i = 0; i < memberCount; i++)
                {
                    if (members[i].Key == paramList[i].Name)
                    {
                        m_params[i] = members[i];
                    }
                    else
                    {
                        m_params[i] = new KeyValuePair<string, string>(paramList[i].Name, string.Empty);
                        def.WriteWarning($"{members[i].Key} does not match {name} parameter {i}.");
                    }
                }
            }
            else
            {
                if (memberCount != 0)
                {
                    def.WriteWarning($"Doc comments do not match parameter count for {name}.");
                }
            }
        }

        public static string GetSyntax(string keyword, string name, IList<ParamDef> paramList, TypeDef returnType)
        {
            var writer = new StringWriter();

            writer.Write($"{keyword} {name}(");

            if (paramList.Count == 0)
            {
                writer.Write(')');
            }
            else
            {
                for (int i = 0; i < paramList.Count; i++)
                {
                    if (i != 0)
                    {
                        writer.Write(',');
                    }
                    writer.Write($"\n    {paramList[i].Name} : {paramList[i].Type.Name}");
                }
                writer.Write("\n    )");
            }

            if (returnType != Types.Void)
            {
                writer.Write($" : {returnType.Name}");
            }

            writer.Write(';');

            return writer.ToString();
        }

        public void Write(HtmlWriter writer)
        {
            if (m_params != null)
            {
                writer.BeginElement("h4");
                writer.WriteString("Parameters");
                writer.EndElement();

                writer.WriteTermDefList(m_params);
            }

            if (m_returnValue.Length != 0)
            {
                writer.BeginElement("h4");
                writer.WriteString("Return value");
                writer.EndElement();

                writer.WriteParagraph(m_returnValue);
            }
        }
    }
}
