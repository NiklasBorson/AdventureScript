namespace AdventureScript
{
    sealed class PropertyExpr : Expr
    {
        Expr m_itemExpr;
        PropertyDef m_propDef;

        public PropertyExpr(Parser parser, Expr itemExpr, PropertyDef propDef)
        {
            m_itemExpr = itemExpr;
            m_propDef = propDef;

            if (m_itemExpr.Type != Types.Item)
            {
                parser.Fail("Expression to left of '.' must have type Item.");
            }
        }

        public override TypeDef Type => m_propDef.Type;

        public override bool HasSideEffects => false;

        public override int Evaluate(GameState game, int[] frame)
        {
            int itemId = m_itemExpr.Evaluate(game, frame);
            return m_propDef[itemId];
        }

        public override bool CanSetValue => true;

        public override void SetValue(GameState game, int[] frame, int value)
        {
            int itemId = m_itemExpr.Evaluate(game, frame);
            m_propDef[itemId] = value;
        }

        public override bool IsConstant => false;

        public override Precedence Precedence => Precedence.Member;

        public override void WriteExpr(GameState game, CodeWriter writer)
        {
            WriteSubExpr(game, m_itemExpr, writer);
            writer.Write($".{m_propDef.Name}");
        }
    }
}
