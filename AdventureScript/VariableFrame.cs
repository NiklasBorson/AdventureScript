using System.Diagnostics;
using System.Xml.Linq;

namespace AdventureScript
{
    class VariableFrame
    {
        TypeDef m_returnType = Types.Void;
        GlobalVarMap m_globals;
        Dictionary<string, VariableExpr> m_varMap = new Dictionary<string, VariableExpr>();
        Stack<VariableExpr> m_varStack = new Stack<VariableExpr>();
        Stack<int> m_blockStack = new Stack<int>(); // stack of variable counts
        int m_maxVarCount = 0;

        public VariableFrame(Parser parser)
        {
            m_globals = parser.Game.GlobalVars;
        }

        public void InitializeFunction(Parser parser, IList<ParamDef> paramDefs, TypeDef returnType)
        {
            // This should be the first thing we do.
            Debug.Assert(m_maxVarCount == 0);

            // Always reserve space for the "$return" variable
            // at index zero in the frame.
            var returnVar = PushVariable("$return", returnType);

            // Add the "$return" variable to the variable map only
            // if the return type is not void.
            m_returnType = returnType;
            if (returnType != Types.Void)
            {
                AddVarToMap(parser, returnVar);
            }

            foreach (var def in paramDefs)
            {
                AddVariable(parser, def.Name, def.Type);
            }
        }

        public TypeDef ReturnType => m_returnType;

        public int FrameSize => m_maxVarCount;

        public VariableExpr AddVariable(Parser parser, string name, TypeDef type)
        {
            var expr = PushVariable(name, type);
            AddVarToMap(parser, expr);
            return expr;
        }

        public VariableExpr AddHiddenVariable(TypeDef type)
        {
            return PushVariable(string.Empty, type);
        }

        public VariableExprBase? TryGetVar(string varName)
        {
            VariableExpr? expr;
            return m_varMap.TryGetValue(varName, out expr) ? 
                expr : 
                m_globals.TryGet(varName);
        }

        protected VariableExpr PushVariable(string name, TypeDef type)
        {
            var expr = new VariableExpr(name, type, m_varStack.Count);
            m_varStack.Push(expr);

            int newCount = m_varStack.Count;
            if (newCount > m_maxVarCount)
            {
                m_maxVarCount = newCount;
            }

            return expr;
        }

        protected void AddVarToMap(Parser parser, VariableExpr expr)
        {
            if (m_globals.ContainsKey(expr.Name) ||
                !m_varMap.TryAdd(expr.Name, expr))
            {
                parser.Fail($"Variable {expr.Name} is already defined.");
            }
        }

        public void PushScope()
        {
            // Push the variable count for the outer block.
            m_blockStack.Push(m_varStack.Count);
        }

        public void PopScope()
        {
            // Get the variable count for the outer block.
            int count = m_blockStack.Pop();

            // Remove variables for the current block.
            while (m_varStack.Count > count)
            {
                var expr = m_varStack.Pop();
                if (expr != null)
                {
                    m_varMap.Remove(expr.Name);
                }
            }
        }
    }
}
