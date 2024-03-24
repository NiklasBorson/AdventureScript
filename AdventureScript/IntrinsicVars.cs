namespace AdventureScript
{
    class IntrinsicVars
    {
        StringMap m_stringMap;
        GlobalVariableExpr m_isNounFirst;
        GlobalVariableExpr m_invalidCommandString;
        GlobalVariableExpr m_invalidArgFormatString;
        GlobalVariableExpr m_noItemFormatString;
        GlobalVariableExpr m_ambiguousItemFormatString;
        GlobalVariableExpr m_ignoreWords;

        string m_ignoreWordsValue = string.Empty;
        string[] m_ignoreWordsArray = new string[0];

        public IntrinsicVars(GlobalVarMap varMap, StringMap stringMap)
        {
            m_stringMap = stringMap;

            m_isNounFirst = AddBoolVar(
                varMap, 
                "$IsNounFirst"
                );

            m_invalidCommandString = AddStringVar(
                varMap,
                "$InvalidCommandString",
                "I don't understand that."
                );

            m_invalidArgFormatString = AddStringVar(
                varMap,
                "$InvalidArgFormatString",
                "I don't understand {0}."
                );

            m_noItemFormatString = AddStringVar(
                varMap,
                "$NoItemFormatString",
                "I couldn't find {0}."
                );

            m_ambiguousItemFormatString = AddStringVar(
                varMap,
                "$AmbiguousItemFormatString",
                "I don't know which {0} you mean. It could be:"
                );

            m_ignoreWords = AddStringVar(
                varMap,
                "$IgnoreWords",
                "a an the"
                );
        }

        public bool IsNounFirst => m_isNounFirst.Value != 0;
        public string InvalidCommandString => GetStringValue(m_invalidCommandString);
        public string InvalidArgFormatString => GetStringValue(m_invalidArgFormatString);
        public string NoItemFormatString => GetStringValue(m_noItemFormatString);
        public string AmbiguousItemFormatString => GetStringValue(m_ambiguousItemFormatString);

        public string[] IgnoreWords
        {
            get
            {
                string value = GetStringValue(m_ignoreWords);
                if (!object.ReferenceEquals(value, m_ignoreWordsValue))
                {
                    m_ignoreWordsValue = value;
                    m_ignoreWordsArray = value.Split();
                }
                return m_ignoreWordsArray;
            }
        }

        GlobalVariableExpr AddVar(GlobalVarMap varMap, string varName, TypeDef type)
        {
            var varExpr = varMap.TryAdd(varName, type);
            if (varExpr == null)
            {
                throw new InvalidOperationException();
            }
            return varExpr;
        }

        GlobalVariableExpr AddBoolVar(GlobalVarMap varMap, string varName)
        {
            var varExpr = AddVar(varMap, varName, Types.Bool);
            return varExpr;
        }

        GlobalVariableExpr AddStringVar(GlobalVarMap varMap, string varName, string value)
        {
            var varExpr = AddVar(varMap, varName, Types.String);
            varExpr.Value = m_stringMap[value];
            return varExpr;
        }
        string GetStringValue(GlobalVariableExpr varExpr)
        {
            return m_stringMap[varExpr.Value];
        }
    }
}
