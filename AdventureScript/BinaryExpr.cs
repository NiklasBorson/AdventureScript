namespace AdventureScript
{
    class BinaryExpr : Expr
    {
        public delegate TypeDef DeriveType(TypeDef type1, TypeDef type2);
        public delegate int Compute(int arg1, int arg2);

        BinaryOp m_op;
        TypeDef m_type1;
        TypeDef m_type2;
        Expr m_arg1;
        Expr m_arg2;
        TypeDef m_resultType;

        public static Expr Create(Parser parser, BinaryOp op, Expr arg1, Expr arg2)
        {
            var type1 = arg1.Type;
            var type2 = arg2.Type;

            if (type1 == Types.Void)
            {
                parser.Fail($"The left argument of '{op.SymbolText}' does not return a value.");
            }
            if (type2 == Types.Void)
            {
                parser.Fail($"The right argument of '{op.SymbolText}' does not return a value.");
            }
            var resultType = op.DeriveType(type1, type2);
            if (resultType == Types.Void)
            {
                parser.Fail($"Invalid argument types for '{op.SymbolText}' operator.");
            }

            if (arg1.IsConstant && arg2.IsConstant)
            {
                int val1 = arg1.EvaluateConst(parser.Game);
                int val2 = arg2.EvaluateConst(parser.Game);
                int value = op.Compute(val1, val2);
                return new LiteralExpr(resultType, value);
            }
            else
            {
                return new BinaryExpr(op, type1, type2, arg1, arg2, resultType);
            }
        }

        BinaryExpr(BinaryOp op, TypeDef type1, TypeDef type2, Expr arg1, Expr arg2, TypeDef resultType)
        {
            m_op = op;
            m_type1 = type1;
            m_type2 = type2;
            m_arg1 = arg1;
            m_arg2 = arg2;
            m_resultType = resultType;
        }

        public override int Evaluate(GameState game, int[] frame)
        {
            int arg1 = m_arg1.Evaluate(game, frame);
            int arg2 = m_arg2.Evaluate(game, frame);
            return m_op.Compute(arg1, arg2);
        }

        public override TypeDef Type => m_resultType;

        public override bool HasSideEffects => m_arg1.HasSideEffects || m_arg2.HasSideEffects;

        public override bool IsConstant => false;

        public override Precedence Precedence => m_op.Precedence;

        public override void WriteExpr(GameState game, CodeWriter writer)
        {
            WriteSubExpr(game, m_arg1, writer);
            writer.Write($" {m_op.SymbolText} ");
            WriteSubExpr(game, m_arg2, writer);
        }
    }
}
