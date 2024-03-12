namespace AdventureLib
{
    sealed class ExpressionStatement : Statement
    {
        Expr m_expr;
        public ExpressionStatement(Expr expr)
        {
            m_expr = expr;
        }

        public override void Invoke(GameState game, int[] frame)
        {
            m_expr.Evaluate(game, frame);
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            m_expr.WriteExpr(game, writer);
            writer.Write(";");
        }
    }
}
