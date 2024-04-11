using AdventureScript;
using System.Text;

namespace AdventureDoc
{
    record struct PageType(string Name, string PluralName, List<RefPage> Pages);

    abstract class RefPage : IComparable<RefPage>
    {
        Doc m_doc;
        string m_name;
        string m_title;
        string m_outputFileName;
        bool m_isType;

        protected RefPage(Doc doc, string name, bool isType)
        {
            m_doc = doc;
            m_name = name;
            m_isType = isType;

            var b = new StringBuilder();
            if (name.StartsWith('$'))
            {
                b.Append(name, 1, name.Length - 1);
            }
            else
            {
                b.Append(name);
            }
            b.Append('-');
            b.Append(doc.PageType.Name);
            b.Append(".html");
            m_outputFileName = b.ToString();
            m_title = $"{name} {doc.PageType.Name}";

            doc.PageType.Pages.Add(this);
        }

        public Doc Doc => m_doc;
        public string Name => m_name;
        public string OutputFileName => m_outputFileName;
        public string Title => m_title;
        public bool IsType => m_isType;

        public RefPage? Next { get; set; }

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
        GameState m_game;
        FunctionInfo m_funcInfo;

        public FunctionPage(Doc doc, FunctionDef def, GameState game) : base(doc, def.Name, /*isType*/ false)
        {
            m_def = def;
            m_game = game;
            m_funcInfo = new FunctionInfo(doc, def.Name, def.ParamList, def.ReturnType);
        }

        public override string GetSyntax()
        {
            return FunctionInfo.GetSyntax("function", Name, m_def.ParamList, m_def.ReturnType);
        }

        public override void WriteMembers(HtmlWriter writer)
        {
            m_funcInfo.Write(writer);

            if (this.Doc.Module.SourceFileName != "")
            {
                writer.WriteHeading("h3", "Source");

                var b = new StringWriter();
                m_def.SaveDefinition(m_game, new CodeWriter(b));

                writer.BeginElement("pre");
                writer.WriteString(b.ToString(), /*linkTypesOnly*/ false);
                writer.EndElement();
            }
        }
    }

    sealed class DelegatePage : RefPage
    {
        DelegateTypeDef m_def;
        FunctionInfo m_funcInfo;

        public DelegatePage(Doc doc, DelegateTypeDef def) : base(doc, def.Name, /*isType*/ true)
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

        public EnumPage(Doc doc, EnumTypeDef def) : base(doc, def.Name, /*isType*/ true)
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
                writer.BeginElement("h3");
                writer.WriteRawString("Values");
                writer.EndElement();

                writer.WriteTermDefList(m_members);
            }
        }
    }

    internal class PropertyPage : RefPage
    {
        TypeDef m_typeDef;

        public PropertyPage(Doc doc, string name, TypeDef typeDef) : base(doc, name, /*isType*/ false)
        {
            m_typeDef = typeDef;
        }

        public override string GetSyntax() => $"property {Name} : {m_typeDef.Name};";
    }

    internal class VariablePage : RefPage
    {
        TypeDef m_typeDef;

        public VariablePage(Doc def, string name, TypeDef typeDef) : base(def, name, /*isType*/ false)
        {
            m_typeDef = typeDef;
        }

        public override string GetSyntax() => $"var {Name} : {m_typeDef.Name};";
    }

    internal class ConstantPage : RefPage
    {
        TypeDef m_typeDef;

        public ConstantPage(Doc doc, string name, TypeDef typeDef) : base(doc, name, /*isType*/ false)
        {
            m_typeDef = typeDef;
        }

        public override string GetSyntax() => $"const {Name} : {m_typeDef.Name};";
    }
}
