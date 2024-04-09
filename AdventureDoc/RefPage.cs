using AdventureScript;
using System.Xml;

namespace AdventureDoc
{
    abstract class RefPage : IComparable<RefPage>
    {
        string m_fileName;
        string m_description;
        string m_name;

        public RefPage(Definition def, string name)
        {
            m_fileName = Path.GetFileName(def.SourcePos.FileName);
            m_description = def.Description;
            m_name = name;
        }

        public string FileName => m_fileName;
        public string Description => m_description;
        public string Name => m_name;

        public int CompareTo(RefPage? other)
        {
            return string.Compare(this.Name, other?.Name, StringComparison.OrdinalIgnoreCase);
        }
        public abstract string Heading { get; }

        public abstract void Write(HtmlWriter writer);
    }

    sealed class FunctionPage : RefPage
    {
        FunctionDef m_funcDef;
        FunctionDescription m_func;

        public FunctionPage(Definition def, FunctionDef funcDef) : base(def, funcDef.Name)
        {
            m_funcDef = funcDef;
            m_func = new FunctionDescription(def, funcDef.Name, funcDef.ParamList, funcDef.ReturnType);
        }

        public override string Heading => $"{Name} Function";

        public override void Write(HtmlWriter writer)
        {
            // TODO
        }

    }

    sealed class DelegatePage : RefPage
    {
        DelegateTypeDef m_typeDef;
        FunctionDescription m_func;

        public DelegatePage(Definition def, DelegateTypeDef typeDef) : base(def, typeDef.Name)
        {
            m_typeDef = typeDef;
            m_func = new FunctionDescription(def, typeDef.Name, typeDef.ParamList, typeDef.ReturnType);
        }
        public override string Heading => $"{Name} Delegate";

        public override void Write(HtmlWriter writer)
        {
            // TODO
        }
    }

    internal class EnumPage : RefPage
    {
        EnumTypeDef m_typeDef;
        KeyValuePair<string, string>[] m_members;

        public EnumPage(Definition def, EnumTypeDef typeDef) : base(def, typeDef.Name)
        {
            m_typeDef = typeDef;

            var members = def.Members;
            var valueNames = typeDef.ValueNames;

            if (members.Count == valueNames.Count)
            {
                m_members = new KeyValuePair<string, string>[valueNames.Count];

                for (int i = 0; i < m_members.Length; i++)
                {
                    if (members[i].Key == valueNames[i])
                    {
                        m_members[i] = members[i];
                    }
                    else
                    {
                        m_members[i] = new KeyValuePair<string, string>(valueNames[i], string.Empty);
                        def.WriteWarning($"{members[i].Key} does not match {typeDef.Name} member {i}.");
                    }
                }
            }
            else
            {
                m_members = new KeyValuePair<string, string>[0];

                if (members.Count != 0)
                {
                    def.WriteWarning($"Doc comments do mot match member count for {typeDef.Name}.");
                }
            }
        }
        public override string Heading => $"{Name} Enum";

        public override void Write(HtmlWriter writer)
        {
            // TODO
        }
    }

    internal class ItemPage : RefPage
    {
        public ItemPage(Definition def, string name) : base(def, name)
        {
        }
        public override string Heading => $"{Name} Item";

        public override void Write(HtmlWriter writer)
        {
            // TODO
        }
    }

    internal class PropertyPage : RefPage
    {
        TypeDef m_typeDef;

        public PropertyPage(Definition def, string name, TypeDef typeDef) : base(def, name)
        {
            m_typeDef = typeDef;
        }
        public override string Heading => $"{Name} Property";

        public override void Write(HtmlWriter writer)
        {
            // TODO
        }
    }

    internal class VariablePage : RefPage
    {
        TypeDef m_typeDef;

        public VariablePage(Definition def, string name, TypeDef typeDef) : base(def, name)
        {
            m_typeDef = typeDef;
        }

        public override string Heading => $"{Name.Substring(1)} Variable";

        public override void Write(HtmlWriter writer)
        {
            // TODO
        }
    }

    internal class ConstantPage : RefPage
    {
        TypeDef m_typeDef;

        public ConstantPage(Definition def, string name, TypeDef typeDef) : base(def, name)
        {
            m_typeDef = typeDef;
        }

        public override string Heading => $"{Name.Substring(1)} Constant";

        public override void Write(HtmlWriter writer)
        {
            // TODO
        }
    }
}
