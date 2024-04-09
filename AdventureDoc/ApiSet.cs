using AdventureScript;

namespace AdventureDoc
{
    internal class ApiSet : IApiSink
    {
        List<RefPage> m_enums = new List<RefPage>();
        List<RefPage> m_delegates = new List<RefPage>();
        List<RefPage> m_variables = new List<RefPage>();
        List<RefPage> m_constants = new List<RefPage>();
        List<RefPage> m_properties = new List<RefPage>();
        List<RefPage> m_functions = new List<RefPage>();

        public void AddEnum(EnumTypeDef def)
        {
            var doc = new Doc(def.SourcePos, def.DocComments);

            m_enums.Add(new EnumPage(doc, def));
        }

        public void AddDelegate(DelegateTypeDef def)
        {
            var doc = new Doc(def.SourcePos, def.DocComments);

            m_delegates.Add(new DelegatePage(doc, def));
        }

        public void AddFunction(FunctionDef def)
        {
            var doc = new Doc(def.SourcePos, def.DocComments);

            m_functions.Add(new FunctionPage(doc, def));
        }

        public void AddProperty(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef)
        {
            var doc = new Doc(sourcePos, docComments);
            m_properties.Add(new PropertyPage(doc, name, typeDef));
        }

        public void AddVariable(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef)
        {
            var doc = new Doc(sourcePos, docComments);
            m_variables.Add(new VariablePage(doc, name, typeDef));
        }

        public void AddConstant(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef)
        {
            var doc = new Doc(sourcePos, docComments);
            m_constants.Add(new ConstantPage(doc, name, typeDef));
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
