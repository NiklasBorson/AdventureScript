namespace AdventureLib
{
    sealed class FunctionExpr : Expr
    {
        FunctionDef m_def;
        IList<Expr> m_argList;

        public FunctionExpr(Parser parser, FunctionDef def, IList<Expr> argList)
        {
            CheckArguments(parser, def.Name, def.ParamList, argList);

            m_def = def;
            m_argList = argList;
        }

        public static void CheckArguments(
            Parser parser,
            string functionName,
            IList<ParamDef> paramList,
            IList<Expr> argList
            )
        {
            int argCount = argList.Count;
            if (argCount != paramList.Count)
            {
                parser.Fail($"Incorrect number of arguments to {functionName}.");
            }

            for (int i = 0; i < argCount; i++)
            {
                var argType = argList[i].Type;
                if (argType != Types.Null && argType != paramList[i].Type)
                {
                    parser.Fail($"Type mismatch for argument {i + 1} calling {functionName}.");
                }
            }
        }

        public override TypeDef Type => m_def.ReturnType;

        public override int Evaluate(GameState game, int[] frame)
        {
            // Allocate a new frame of the specified size.
            var newFrame = new int[m_def.FrameSize];

            // Evaluate arguments and store the values starting
            // at frame index 1.
            int argCount = m_argList.Count;
            for (int i = 0; i < argCount; i++)
            {
                int arg = m_argList[i].Evaluate(game, frame);
                newFrame[i + 1] = arg;
            }

            return m_def.Invoke(game, newFrame);
        }

        public override bool HasSideEffects => true;
        public override bool IsConstant => false;
        public override Precedence Precedence => Precedence.Atomic;

        public override void WriteExpr(GameState game, CodeWriter writer)
        {
            writer.Write(m_def.Name);
            WriteArgs(m_argList, game, writer);
        }

        public static void WriteArgs(IList<Expr> argList, GameState game, CodeWriter writer)
        {
            writer.Write("(");

            if (argList.Count > 0)
            {
                argList[0].WriteExpr(game, writer);

                for (int i = 1; i < argList.Count; i++)
                {
                    writer.Write(", ");
                    argList[i].WriteExpr(game, writer);
                }
            }

            writer.Write(")");
        }
    }
}
