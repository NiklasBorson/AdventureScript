using AdventureScript;
using System.Text;
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

        public abstract string GetSyntax();

        public virtual void WriteMembers(HtmlWriter writer)
        {
        }
    }

    sealed class FunctionPage : RefPage
    {
        FunctionDef m_funcDef;
        FunctionInfo m_funcInfo;

        public FunctionPage(Definition def, FunctionDef funcDef) : base(def, funcDef.Name)
        {
            m_funcDef = funcDef;
            m_funcInfo = new FunctionInfo(def, funcDef.Name, funcDef.ParamList, funcDef.ReturnType);
        }

        public override string GetSyntax()
        {
            return FunctionInfo.GetSyntax("function", Name, m_funcDef.ParamList, m_funcDef.ReturnType);
        }

        public override void WriteMembers(HtmlWriter writer)
        {
            m_funcInfo.Write(writer);
        }
    }

    sealed class DelegatePage : RefPage
    {
        DelegateTypeDef m_typeDef;
        FunctionInfo m_funcInfo;

        public DelegatePage(Definition def, DelegateTypeDef typeDef) : base(def, typeDef.Name)
        {
            m_typeDef = typeDef;
            m_funcInfo = new FunctionInfo(def, typeDef.Name, typeDef.ParamList, typeDef.ReturnType);
        }

        public override string GetSyntax()
        {
            return FunctionInfo.GetSyntax("delegate", Name, m_typeDef.ParamList, m_typeDef.ReturnType);
        }

        public override void WriteMembers(HtmlWriter writer)
        {
            m_funcInfo.Write(writer);
        }
    }

    internal class EnumPage : RefPage
    {
        EnumTypeDef m_typeDef;
        KeyValuePair<string, string>[]? m_members;

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
                if (members.Count != 0)
                {
                    def.WriteWarning($"Doc comments do mot match member count for {typeDef.Name}.");
                }
            }
        }

        public override string GetSyntax()
        {
            var b = new StringWriter();
            m_typeDef.SaveDefinition(b);
            return b.ToString();
        }

        public override void WriteMembers(HtmlWriter writer)
        {
            if (m_members != null)
            {
                writer.BeginElement("h4");
                writer.WriteString("Values");
                writer.EndElement();

                writer.WriteTermDefList(m_members);
            }
        }
    }

    internal class ItemPage : RefPage
    {
        public ItemPage(Definition def, string name) : base(def, name)
        {
        }

        public override string GetSyntax() => $"item {Name};";
    }

    internal class PropertyPage : RefPage
    {
        TypeDef m_typeDef;

        public PropertyPage(Definition def, string name, TypeDef typeDef) : base(def, name)
        {
            m_typeDef = typeDef;
        }

        public override string GetSyntax() => $"property {Name} : {m_typeDef.Name};";
    }

    internal class VariablePage : RefPage
    {
        TypeDef m_typeDef;

        public VariablePage(Definition def, string name, TypeDef typeDef) : base(def, name)
        {
            m_typeDef = typeDef;
        }

        public override string GetSyntax() => $"var {Name} : {m_typeDef.Name};";
    }

    internal class ConstantPage : RefPage
    {
        TypeDef m_typeDef;

        public ConstantPage(Definition def, string name, TypeDef typeDef) : base(def, name)
        {
            m_typeDef = typeDef;
        }

        public override string GetSyntax() => $"const {Name} : {m_typeDef.Name};";
    }
}
