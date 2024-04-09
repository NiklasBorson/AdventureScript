using AdventureScript;
using System.Xml;

namespace AdventureDoc
{
    internal class ParserSink : IParserSink
    {
        Definition m_def = new Definition();

        List<RefPage> m_enums = new List<RefPage>();
        List<RefPage> m_delegates = new List<RefPage>();
        List<RefPage> m_variables = new List<RefPage>();
        List<RefPage> m_constants = new List<RefPage>();
        List<RefPage> m_properties = new List<RefPage>();
        List<RefPage> m_items = new List<RefPage>();
        List<RefPage> m_functions = new List<RefPage>();

        public void BeginDefinition(Lexer lexer)
        {
            m_def = new Definition(lexer);
        }

        public void AddEnum(EnumTypeDef def)
        {
            m_enums.Add(new EnumPage(m_def, def));
        }

        public void AddDelegate(DelegateTypeDef def)
        {
            m_delegates.Add(new DelegatePage(m_def, def));
        }

        public void AddFunction(FunctionDef def)
        {
            m_functions.Add(new FunctionPage(m_def, def));
        }

        public void AddItem(string name)
        {
            m_items.Add(new ItemPage(m_def, name));
        }

        public void AddProperty(string name, TypeDef typeDef)
        {
            m_properties.Add(new PropertyPage(m_def, name, typeDef));
        }

        public void AddVariable(string name, TypeDef typeDef, bool isConst)
        {
            if (isConst)
            {
                m_variables.Add(new VariablePage(m_def, name, typeDef));
            }
            else
            {
                m_constants.Add(new ConstantPage(m_def, name, typeDef));
            }
        }

        public void Write(string fileName)
        {
            Section[] sections = new Section[]
            {
                new Section("Enums", m_enums),
                new Section("Delegates", m_delegates),
                new Section("Variables", m_variables),
                new Section("Constants", m_constants),
                new Section("Properties", m_properties),
                new Section("Items", m_items),
                new Section("Functions", m_functions)
            };

            foreach (var section in sections)
            {
                section.Pages.Sort();
            }

            using (var writer = new HtmlWriter(fileName))
            {
                writer.Write(sections);
            }
        }
    }
}
