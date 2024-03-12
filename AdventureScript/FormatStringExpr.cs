namespace AdventureLib
{
    class FormatStringExpr : Expr
    {
        string m_formatString;
        IList<Expr> m_exprList;
        TypeDef[] m_exprTypes;
        bool m_hasSideEffects = false;
        bool m_isConst = true;

        public FormatStringExpr(Parser parser, string formatString, IList<Expr> exprList)
        {
            m_formatString = formatString;
            m_exprList = exprList;
            m_exprTypes = new TypeDef[exprList.Count];

            for (int i = 0; i < m_exprTypes.Length; i++)
            {
                var expr = m_exprList[i];
                var type = expr.Type;
                if (type == Types.Void)
                {
                    parser.Fail("Expression in format string has no return value.");
                }
                m_exprTypes[i] = type;

                if (expr.HasSideEffects)
                {
                    m_hasSideEffects = true;
                }
                if (!expr.IsConstant)
                {
                    m_isConst = false;
                }
            }
        }

        public override TypeDef Type => Types.String;

        public override bool HasSideEffects => m_hasSideEffects;


        public override bool IsConstant => m_isConst;

        public override Precedence Precedence => Precedence.Atomic;

        public override int Evaluate(GameState game, int[] frame)
        {
            var args = new string[m_exprTypes.Length];
            for (int i = 0; i < args.Length; i++)
            {
                int value = m_exprList[i].Evaluate(game, frame);
                args[i] = m_exprTypes[i].ValueToString(game, value);
            }

            string stringValue = string.Format(m_formatString, args);
            return game.Strings[stringValue];
        }

        public override void WriteExpr(GameState game, CodeWriter writer)
        {
            var args = new string[m_exprTypes.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var stringWriter = new StringWriter();
                var codeWriter = new CodeWriter(stringWriter);
                codeWriter.Write("{");
                m_exprList[i].WriteExpr(game, codeWriter);
                codeWriter.Write("}");
                args[i] = stringWriter.ToString();
            }

            string stringValue = string.Format(m_formatString, args);
            writer.Write("$\"");
            writer.Write(stringValue);
            writer.Write("\"");
        }
    }
}
