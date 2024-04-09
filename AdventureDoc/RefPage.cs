using AdventureScript;

namespace AdventureDoc
{
    abstract class RefPage : IComparable<RefPage>
    {
        string? m_fileName;
        string m_description;
        string m_name;

        public RefPage(Doc doc, string name)
        {
            m_fileName = Path.GetFileName(doc.SourcePos.FileName);
            m_description = doc.Description;
            m_name = name;
        }

        public string? FileName => m_fileName;
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
        FunctionDef m_def;
        FunctionInfo m_funcInfo;

        public FunctionPage(Doc doc, FunctionDef def) : base(doc, def.Name)
        {
            m_def = def;
            m_funcInfo = new FunctionInfo(doc, def.Name, def.ParamList, def.ReturnType);
        }

        public override string GetSyntax()
        {
            return FunctionInfo.GetSyntax("function", Name, m_def.ParamList, m_def.ReturnType);
        }

        public override void WriteMembers(HtmlWriter writer)
        {
            m_funcInfo.Write(writer);
        }
    }

    sealed class DelegatePage : RefPage
    {
        DelegateTypeDef m_def;
        FunctionInfo m_funcInfo;

        public DelegatePage(Doc doc, DelegateTypeDef def) : base(doc, def.Name)
        {
            m_def = def;
            m_funcInfo = new FunctionInfo(doc, def.Name, def.ParamList, def.ReturnType);
        }

        public override string GetSyntax()
        {
            return FunctionInfo.GetSyntax("delegate", Name, m_def.ParamList, m_def.ReturnType);
        }

        public override void WriteMembers(HtmlWriter writer)
        {
            m_funcInfo.Write(writer);
        }
    }

    internal class EnumPage : RefPage
    {
        EnumTypeDef m_def;
        KeyValuePair<string, string>[]? m_members;

        public EnumPage(Doc doc, EnumTypeDef def) : base(doc, def.Name)
        {
            m_def = def;

            var members = doc.Members;
            var valueNames = def.ValueNames;

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
                        doc.WriteWarning($"{members[i].Key} does not match {def.Name} member {i}.");
                    }
                }
            }
            else
            {
                if (members.Count != 0)
                {
                    doc.WriteWarning($"Doc comments do mot match member count for {def.Name}.");
                }
            }
        }

        public override string GetSyntax()
        {
            var b = new StringWriter();
            m_def.SaveDefinition(b);
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

    internal class PropertyPage : RefPage
    {
        TypeDef m_typeDef;

        public PropertyPage(Doc doc, string name, TypeDef typeDef) : base(doc, name)
        {
            m_typeDef = typeDef;
        }

        public override string GetSyntax() => $"property {Name} : {m_typeDef.Name};";
    }

    internal class VariablePage : RefPage
    {
        TypeDef m_typeDef;

        public VariablePage(Doc def, string name, TypeDef typeDef) : base(def, name)
        {
            m_typeDef = typeDef;
        }

        public override string GetSyntax() => $"var {Name} : {m_typeDef.Name};";
    }

    internal class ConstantPage : RefPage
    {
        TypeDef m_typeDef;

        public ConstantPage(Doc doc, string name, TypeDef typeDef) : base(doc, name)
        {
            m_typeDef = typeDef;
        }

        public override string GetSyntax() => $"const {Name} : {m_typeDef.Name};";
    }
}
