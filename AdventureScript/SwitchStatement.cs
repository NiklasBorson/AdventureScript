namespace AdventureLib
{
    sealed class SwitchStatement : Statement
    {
        Expr m_expr;
        TypeDef m_type;
        IDictionary<int, Statement> m_cases;
        Statement? m_defaultCase;

        public SwitchStatement(Expr expr, TypeDef type, IDictionary<int, Statement> cases, Statement? defaultCase)
        {
            m_expr = expr;
            m_type = type;
            m_cases = cases;
            m_defaultCase = defaultCase;
        }

        public override void Invoke(GameState game, int[] frame)
        {
            int value = m_expr.Evaluate(game, frame);

            Statement? branch;
            if (m_cases.TryGetValue(value, out branch))
            {
                branch.Invoke(game, frame);
            }
            else if (m_defaultCase != null)
            {
                m_defaultCase.Invoke(game, frame);
            }
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("switch (");
            m_expr.WriteExpr(game, writer);
            writer.Write(")");
            writer.BeginBlock();

            foreach (var elem in m_cases)
            {
                writer.Write("case ");
                m_type.WriteValue(game, elem.Key, writer.TextWriter);
                writer.BeginBlock();
                elem.Value.WriteStatement(game, writer);
                writer.EndBlock();
            }

            writer.EndBlock();
        }
    }
}
