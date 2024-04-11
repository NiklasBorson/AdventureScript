using AdventureScript;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;

namespace AdventureDoc
{
    record class Module(string SourceFilePath, string SourceFileName, string ModuleName, string ModuleTitle);

    internal class ApiSet : IApiSink
    {
        GameState m_game;
        string m_indexTitle;
        string m_headingText;
        string m_headingUrl;

        List<Module> m_modules = new List<Module>();

        PageType m_enumPages = new PageType("Enum", "Enums", new List<RefPage>());
        PageType m_delegatePages = new PageType("Delegate", "Delegates", new List<RefPage>());
        PageType m_variablePages = new PageType("Variable", "Variables", new List<RefPage>());
        PageType m_constPages = new PageType("Constant", "Constants", new List<RefPage>());
        PageType m_propertyPages = new PageType("Property", "Properties", new List<RefPage>());
        PageType m_functionPages = new PageType("Function", "Functions", new List<RefPage>());
        PageType[] m_pageTypes;

        Dictionary<string, RefPage> m_apiMap = new Dictionary<string, RefPage>();

        public ApiSet(GameState game, string indexTitle, string headingText, string headingUrl)
        {
            m_game = game;
            m_indexTitle = indexTitle;
            m_headingText = headingText;
            m_headingUrl = headingUrl;

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
            m_modules.Add(new Module("", "", "Intrinsic", "Intrinsics"));

            // Get the APIs.
            game.GetApis(this);

            // Sort the pages and initialize the dictionary.
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
        }

        public string IndexTitle => m_indexTitle;
        public string HeadingText => m_headingText;
        public string HeadingUrl => m_headingUrl;

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
            var newModule = new Module(filePath, fileName, fileName, $"{fileName} Module");
            m_modules.Add(newModule);
            return newModule;
        }

        Doc NewDoc(PageType pageType, SourcePos sourcePos, string[] docComments)
        {
            return new Doc(GetModule(sourcePos), pageType, sourcePos, docComments);
        }

        void IApiSink.AddEnum(EnumTypeDef def)
        {
            var doc = NewDoc(m_enumPages, def.SourcePos, def.DocComments);
            new EnumPage(doc, def);
        }

        void IApiSink.AddDelegate(DelegateTypeDef def)
        {
            var doc = NewDoc(m_delegatePages, def.SourcePos, def.DocComments);
            new DelegatePage(doc, def);
        }

        void IApiSink.AddFunction(FunctionDef def)
        {
            var doc = NewDoc(m_functionPages, def.SourcePos, def.DocComments);
            new FunctionPage(doc, def, m_game);
        }

        void IApiSink.AddProperty(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef)
        {
            var doc = NewDoc(m_propertyPages, sourcePos, docComments);
            new PropertyPage(doc, name, typeDef);
        }

        void IApiSink.AddVariable(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef)
        {
            var doc = NewDoc(m_variablePages, sourcePos, docComments);
            new VariablePage(doc, name, typeDef);
        }

        void IApiSink.AddConstant(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef)
        {
            var doc = NewDoc(m_constPages, sourcePos, docComments);
            new ConstantPage(doc, name, typeDef);
        }

        public void Write(string outputDir)
        {
            // Write the top index.
            using (var writer = new HtmlWriter(outputDir, "index.html", IndexTitle, this))
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
                        string title = HtmlWriter.GetIndexTitle(module.ModuleName, pageType);
                        string fileName = HtmlWriter.GetIndexFileName(module.ModuleName, pageType);

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
