namespace AdventureLib
{
    class IntrinsicVars
    {
        StringMap m_stringMap;
        GlobalVariableExpr m_isNounFirst;
        GlobalVariableExpr m_whichItemFormatString;

        public IntrinsicVars(GlobalVarMap varMap, StringMap stringMap)
        {
            m_stringMap = stringMap;

            m_isNounFirst = AddBoolVar(
                varMap, 
                "$IsNounFirst"
                );

            m_whichItemFormatString = AddStringVar(
                varMap,
                "$WhichItemFormatString",
                "Which {0}?"
                );
        }

        public bool IsNounFirst => m_isNounFirst.Value != 0;
        public string WhichItemFormatString => GetStringValue(m_whichItemFormatString);

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
