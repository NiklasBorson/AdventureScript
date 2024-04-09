namespace AdventureScript
{
    public interface IParserSink
    {
        void BeginDefinition(Lexer lexer);
        void AddFunction(FunctionDef def);
        void AddEnum(EnumTypeDef def);
        void AddDelegate(DelegateTypeDef def);
        void AddProperty(string name, TypeDef typeDef);
        void AddItem(string name);
        void AddVariable(string name, TypeDef typeDef, bool isConst);
    }
}
