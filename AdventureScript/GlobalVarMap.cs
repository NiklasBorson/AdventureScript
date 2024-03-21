using System.Collections;

namespace AdventureLib
{
    class GlobalVarMap : IEnumerable<GlobalVariableExpr>
    {
        List<GlobalVariableExpr> m_vars = new List<GlobalVariableExpr>();
        Dictionary<string, GlobalVariableExpr> m_map = new Dictionary<string, GlobalVariableExpr>();
        IntrinsicVars m_intrinsics;
        int m_intrinsicCount;

        public GlobalVarMap(StringMap stringMap)
        {
            m_intrinsics = new IntrinsicVars(this, stringMap);
            m_intrinsicCount = m_vars.Count;
        }

        public IntrinsicVars Intrinsics => m_intrinsics;

        public GlobalVariableExpr? TryAdd(string varName, TypeDef type)
        {
            var expr = new GlobalVariableExpr(varName, type);
            if (m_map.TryAdd(varName, expr))
            {
                m_vars.Add(expr);
                return expr;
            }
            else
            {
                return null;
            }
        }

        public GlobalVariableExpr? TryGet(string varName)
        {
            GlobalVariableExpr? expr;
            return m_map.TryGetValue(varName, out expr) ? expr : null;
        }

        public bool ContainsKey(string varName) => m_map.ContainsKey(varName);

        public void SaveDefinitions(GameState game, CodeWriter writer)
        {
            for (int i = m_intrinsicCount; i < m_vars.Count; i++)
            {
                var expr = m_vars[i];
                writer.Write("var ");
                writer.Write(expr.Name);
                if (expr.Value == 0)
                {
                    writer.Write($" : {expr.Type.Name}");
                }
                else
                {
                    writer.Write(" = ");
                    expr.Type.WriteValue(game, expr.Value, writer.TextWriter);
                }
                writer.Write(";");
                writer.EndLine();
            }
        }

        public IEnumerator<GlobalVariableExpr> GetEnumerator()
        {
            return m_vars.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_vars.GetEnumerator();
        }
    }
}
