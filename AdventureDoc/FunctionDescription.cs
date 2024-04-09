using AdventureScript;
using System.ComponentModel.Design.Serialization;

namespace AdventureDoc
{
    internal class FunctionDescription
    {
        string m_name;
        string m_description;
        KeyValuePair<string, string>[] m_params;
        string m_returnValue = string.Empty;

        public FunctionDescription(Definition def, string name, IList<ParamDef> paramList, TypeDef returnType)
        {
            m_name = name;
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

            if (memberCount == paramList.Count)
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
                m_params = new KeyValuePair<string, string>[0];

                if (memberCount != 0)
                {
                    def.WriteWarning($"Doc comments do not match parameter count for {name}.");
                }
            }
        }
    }
}
