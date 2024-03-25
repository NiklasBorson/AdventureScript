namespace AdventureScript
{
    sealed class LiteralExpr : Expr
    {
        TypeDef m_type;
        int m_value;

        public LiteralExpr(TypeDef type, int value)
        {
            m_type = type;
            m_value = value;
        }

        public override TypeDef Type => m_type;

        public override bool HasSideEffects => false;

        public override int Evaluate(GameState game, int[] frame)
        {
            return m_value;
        }

        public override bool IsConstant => true;
        public override Precedence Precedence => Precedence.Atomic;

        public override void WriteExpr(GameState game, CodeWriter writer)
        {
            m_type.WriteValue(game, m_value, writer.TextWriter);
        }
    }
}
