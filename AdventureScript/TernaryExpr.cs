namespace AdventureScript
{
    class TernaryExpr : Expr
    {
        Expr m_condition;
        Expr m_first;
        Expr m_second;
        TypeDef m_type;

        public TernaryExpr(Parser parser, Expr condition, Expr first, Expr second)
        {
            m_condition = condition;
            m_first = first;
            m_second = second;

            m_type = first.Type;
            if (second.Type != m_type)
            {
                parser.Fail("The second and third arguments to '?' have different types.");
            }
        }

        public override int Evaluate(GameState game, int[] frame)
        {
            return m_condition.Evaluate(game, frame) != 0 ?
                m_first.Evaluate(game, frame) :
                m_second.Evaluate(game, frame);
        }

        public override TypeDef Type => m_type;

        public override bool HasSideEffects =>
            m_condition.HasSideEffects ||
            m_first.HasSideEffects ||
            m_second.HasSideEffects;

        public override bool IsConstant =>
            m_condition.IsConstant &&
            m_first.IsConstant &&
            m_second.IsConstant;

        public override Precedence Precedence => Precedence.Ternary;

        public override void WriteExpr(GameState game, CodeWriter writer)
        {
            WriteSubExpr(game, m_condition, writer);
            writer.Write(" ? ");
            WriteSubExpr(game, m_first, writer);
            writer.Write(" : ");
            WriteSubExpr(game, m_second, writer);
        }
    }
}
