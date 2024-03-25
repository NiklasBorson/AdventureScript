namespace AdventureScript
{
    class UnaryExpr : Expr
    {
        public delegate TypeDef DeriveType(TypeDef argType);
        public delegate int Compute(GameState game, TypeDef argType, int value);

        UnaryOp m_op;
        TypeDef m_argType;
        Expr m_arg;
        TypeDef m_resultType;

        public static Expr Create(Parser parser, UnaryOp op, Expr arg)
        {
            var argType = arg.Type;
            if (argType == Types.Void)
            {
                parser.Fail($"The argument of '{op.SymbolText}' does not return a value.");
            }
            var resultType = op.DeriveType(argType);
            if (resultType == Types.Void)
            {
                parser.Fail($"Incompatible type used with '{op.SymbolText}' operator.");
            }

            if (arg.IsConstant)
            {
                int value = arg.EvaluateConst(parser.Game);
                value = op.Compute(parser.Game, argType, value);
                return new LiteralExpr(resultType, value);
            }
            else
            {
                return new UnaryExpr(op, argType, arg, resultType);
            }
        }

        UnaryExpr(UnaryOp op, TypeDef argType, Expr arg, TypeDef resultType)
        {
            m_op = op;
            m_argType = argType;
            m_arg = arg;
            m_resultType = resultType;
        }

        public override int Evaluate(GameState game, int[] frame)
        {
            int value = m_arg.Evaluate(game, frame);
            return m_op.Compute(game, m_argType, value);
        }

        public override TypeDef Type => m_resultType;

        public override bool HasSideEffects => m_arg.HasSideEffects;

        public override bool IsConstant => m_arg.IsConstant;
        public override Precedence Precedence => Precedence.UnaryNegative;

        public override void WriteExpr(GameState game, CodeWriter writer)
        {
            writer.Write(m_op.SymbolText);
            WriteSubExpr(game, m_arg, writer);
        }
    }
}
