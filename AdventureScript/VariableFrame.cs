using System.Xml.Linq;

namespace AdventureLib
{
    class VariableFrame
    {
        GlobalVarMap m_globals;
        Dictionary<string, VariableExpr> m_varMap = new Dictionary<string, VariableExpr>();
        Stack<VariableExpr> m_varStack = new Stack<VariableExpr>();
        Stack<int> m_blockStack = new Stack<int>();
        int m_maxVarCount = 0;

        public VariableFrame(Parser parser)
        {
            m_globals = parser.Game.GlobalVars;
        }

        public int FrameSize => m_maxVarCount;

        public VariableExpr AddVar(Parser parser, string name, TypeDef type)
        {
            var expr = PushVar(name, type);
            AddVarToMap(parser, expr);
            return expr;
        }

        public VariableExprBase? TryGetVar(string varName)
        {
            VariableExpr? expr;
            return m_varMap.TryGetValue(varName, out expr) ? 
                expr : 
                m_globals.TryGet(varName);
        }

        protected VariableExpr PushVar(string name, TypeDef type)
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

        public void BeginBlock()
        {
            // Push the variable count for the outer block.
            m_blockStack.Push(m_varStack.Count);
        }

        public void EndBlock()
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

    class FunctionVariableFrame : VariableFrame
    {
        public FunctionVariableFrame(Parser parser, IList<ParamDef> paramDefs) : base(parser)
        {
            // Always reserve space for the "$return" variable, but we'll add it to the variable
            // make later only if the function has a return type.
            this.ReturnVariable = PushVar("$return", Types.Void);

            foreach (var def in paramDefs)
            {
                AddVar(parser, def.Name, def.Type);
            }
        }

        public VariableExpr ReturnVariable { get; }

        public void SetReturnType(Parser parser, TypeDef type)
        {
            if (type != Types.Void)
            {
                this.ReturnVariable.SetType(type);
                AddVarToMap(parser, this.ReturnVariable);
            }
        }
    };
}
