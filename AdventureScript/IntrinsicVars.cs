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
                new string[]
                {
                    "## Specifies whether nouns precede adjectives in the game's language. ",
                    "## False if adjectives precede nouns, as in English (e.g., cold water).",
                    "## True if nouns precede adjectives, as in Spanish (e.g., agua fria)."
                },
                varMap, 
                "$IsNounFirst"
                );

            m_invalidCommandString = AddStringVar(
                new string[]
                {
                    "## Error message if the user input does not match a command.",
                    "## E.g., \"I don't understand that.\""
                },
                varMap,
                "$InvalidCommandString",
                "I don't understand that."
                );

            m_invalidArgFormatString = AddStringVar(
                new string[]
                {
                    "## Error message format if a command has an invalid argument.",
                    "## E.g., \"I don't understand {0}.\""
                },
                varMap,
                "$InvalidArgFormatString",
                "I don't understand {0}."
                );

            m_noItemFormatString = AddStringVar(
                new string[]
                {
                    "## Error message format if a command argument doesn't match an item.",
                    "## E.g., \"I couldn't find {0}.\""
                },
                varMap,
                "$NoItemFormatString",
                "I couldn't find {0}."
                );

            m_ambiguousItemFormatString = AddStringVar(
                new string[]
                {
                    "## Error message format if a command argument could match multiple items.",
                    "## E.g., \"I don't know which {0} you mean. It could be:\""
                },
                varMap,
                "$AmbiguousItemFormatString",
                "I don't know which {0} you mean. It could be:"
                );

            m_ignoreWords = AddStringVar(
                new string[]
                {
                    "## Space-separated list of words to remove from user input.",
                    "## E.g., \"a an the\"."
                },
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

        GlobalVariableExpr AddVar(string[] docComments, GlobalVarMap varMap, string varName, TypeDef type)
        {
            var varExpr = varMap.TryAdd(
                new SourcePos(),
                docComments,
                varName,
                type, /*isConst*/ false
                );
            if (varExpr == null)
            {
                throw new InvalidOperationException();
            }
            return varExpr;
        }

        GlobalVariableExpr AddBoolVar(string[] docComments, GlobalVarMap varMap, string varName)
        {
            var varExpr = AddVar(docComments, varMap, varName, Types.Bool);
            return varExpr;
        }

        GlobalVariableExpr AddStringVar(string[] docComments, GlobalVarMap varMap, string varName, string value)
        {
            var varExpr = AddVar(docComments, varMap, varName, Types.String);
            varExpr.Value = m_stringMap[value];
            return varExpr;
        }
        string GetStringValue(GlobalVariableExpr varExpr)
        {
            return m_stringMap[varExpr.Value];
        }
    }
}
