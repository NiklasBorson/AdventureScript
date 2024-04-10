using AdventureScript;
using static System.Collections.Specialized.BitVector32;

namespace AdventureDoc
{
    record class Module(string SourceFilePath, string SourceFileName, string ModuleTitle);

    internal class ApiSet : IApiSink
    {
        List<Module> m_modules = new List<Module>();

        PageType m_enumPages = new PageType("Enum", "Enums", new List<RefPage>());
        PageType m_delegatePages = new PageType("Delegate", "Delegates", new List<RefPage>());
        PageType m_variablePages = new PageType("Variable", "Variables", new List<RefPage>());
        PageType m_constPages = new PageType("Constant", "Constants", new List<RefPage>());
        PageType m_propertyPages = new PageType("Property", "Properties", new List<RefPage>());
        PageType m_functionPages = new PageType("Function", "Functions", new List<RefPage>());
        PageType[] m_pageTypes;

        Dictionary<string, RefPage> m_apiMap = new Dictionary<string, RefPage>();

        public ApiSet()
        {
            m_pageTypes = new PageType[]
            {
                m_enumPages,
                m_delegatePages,
                m_variablePages,
                m_constPages,
                m_propertyPages,
                m_functionPages
            };

            // Add the special intrinsic module first.
            m_modules.Add(new Module("", "", "Intrinsic"));
        }

        public PageType[] PageTypes => m_pageTypes;

        public IList<Module> Modules => m_modules;

        public RefPage? TryGetPage(string name)
        {
            RefPage? page = null;
            return m_apiMap.TryGetValue(name, out page) ? page : null;
        }

        Module GetModule(SourcePos sourcePos)
        {
            string filePath = sourcePos.FileName;
            foreach (var module in m_modules)
            {
                if (module.SourceFilePath == filePath)
                    return module;
            }

            string fileName = Path.GetFileName(filePath);
            var newModule = new Module(filePath, fileName, fileName);
            m_modules.Add(newModule);
            return newModule;
        }

        Doc NewDoc(PageType pageType, SourcePos sourcePos, string[] docComments)
        {
            return new Doc(GetModule(sourcePos), pageType, sourcePos, docComments);
        }

        public void AddEnum(EnumTypeDef def)
        {
            var doc = NewDoc(m_enumPages, def.SourcePos, def.DocComments);
            new EnumPage(doc, def);
        }

        public void AddDelegate(DelegateTypeDef def)
        {
            var doc = NewDoc(m_delegatePages, def.SourcePos, def.DocComments);
            new DelegatePage(doc, def);
        }

        public void AddFunction(FunctionDef def)
        {
            var doc = NewDoc(m_functionPages, def.SourcePos, def.DocComments);
            new FunctionPage(doc, def);
        }

        public void AddProperty(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef)
        {
            var doc = NewDoc(m_propertyPages, sourcePos, docComments);
            new PropertyPage(doc, name, typeDef);
        }

        public void AddVariable(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef)
        {
            var doc = NewDoc(m_variablePages, sourcePos, docComments);
            new VariablePage(doc, name, typeDef);
        }

        public void AddConstant(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef)
        {
            var doc = NewDoc(m_constPages, sourcePos, docComments);
            new ConstantPage(doc, name, typeDef);
        }

        public void Write(string outputDir)
        {
            foreach (var pageType in m_pageTypes)
            {
                pageType.Pages.Sort();

                foreach (var page in pageType.Pages)
                {
                    if (!m_apiMap.TryAdd(page.Name, page))
                    {
                        var other = m_apiMap[page.Name];
                        while (other.Next != null)
                        {
                            other = other.Next;
                        }
                        other.Next = page;
                    }
                }
            }

            // Write the top index.
            using (var writer = new HtmlWriter(outputDir, "index.html", "AdventureScript API", this))
            {
                writer.WriteTopIndex();
            }

            // Write a global index for each page type.
            foreach (var pageType in m_pageTypes)
            {
                if (pageType.Pages.Count != 0)
                {
                    string title = HtmlWriter.GetGlobalIndexTitle(pageType);
                    string fileName = HtmlWriter.GetGlobalIndexFileName(pageType);

                    using (var writer = new HtmlWriter(outputDir, fileName, title, this))
                    {
                        writer.WriteIndex(pageType.Pages);
                    }
                }
            }

            // Write per-module indices for each page type.
            foreach (var module in m_modules)
            {
                foreach (var pageType in m_pageTypes)
                {
                    var pages = HtmlWriter.GetIndexPages(module, pageType);

                    if (pages.Count != 0)
                    {
                        string title = HtmlWriter.GetIndexTitle(module.ModuleTitle, pageType);
                        string fileName = HtmlWriter.GetIndexFileName(module.ModuleTitle, pageType);

                        using (var writer = new HtmlWriter(outputDir, fileName, title, this))
                        {
                            writer.WriteIndex(pages);
                        }
                    }
                }
            }

            // Write the individual reference pages.
            foreach (var pageType in m_pageTypes)
            {
                foreach (var page in pageType.Pages)
                {
                    using (var writer = new HtmlWriter(outputDir, page.OutputFileName, page.Title, this))
                    {
                        writer.WritePage(page);
                    }
                }
            }
        }
    }
}
