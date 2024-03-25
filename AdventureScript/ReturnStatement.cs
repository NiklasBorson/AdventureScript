namespace AdventureScript
{
    sealed class ReturnStatement : Statement
    {
        public override int Invoke(GameState game, int[] frame)
        {
            // Jump to the end of the function.
            return int.MaxValue;
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("return;");
            writer.EndLine();
        }
    }

    sealed class ReturnValueStatement : Statement
    {
        Expr m_expr;
        public ReturnValueStatement(Expr expr)
        {
            m_expr = expr;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            // Store the return value.
            frame[0] = m_expr.Evaluate(game, frame);

            // Jump to the end of the function.
            return int.MaxValue;
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("return ");
            m_expr.WriteExpr(game, writer);
            writer.Write(";");
            writer.EndLine();
        }
    }
}
