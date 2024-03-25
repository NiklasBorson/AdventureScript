namespace AdventureScript
{
    internal record class UnaryOp(
        SymbolId SymbolId,
        string SymbolText,
        UnaryExpr.DeriveType DeriveType,
        UnaryExpr.Compute Compute
        );

    static class UnaryOperators
    {
        static readonly UnaryOp m_not = new UnaryOp(
            SymbolId.Not,
            "!",
            Not_DeriveType,
            Not_Compute
            );

        static readonly UnaryOp m_neg = new UnaryOp(
            SymbolId.Minus, 
            "-", 
            Negative_DeriveType, 
            Negative_Compute
            );

        public static UnaryOp? GetOp(SymbolId symbol)
        {
            switch (symbol)
            {
                case SymbolId.Not: return m_not;
                case SymbolId.Minus: return m_neg;
                default: return null;
            }
        }

        static TypeDef Not_DeriveType(TypeDef argType)
        {
            return argType == Types.Bool ? Types.Bool : Types.Void;
        }

        static int Not_Compute(GameState game, TypeDef argType, int value)
        {
            return value == 0 ? 1 : 0;
        }

        static TypeDef Negative_DeriveType(TypeDef argType)
        {
            return argType == Types.Int ? Types.Int : Types.Void;
        }

        static int Negative_Compute(GameState game, TypeDef argType, int value)
        {
            return -value;
        }
    }
}
