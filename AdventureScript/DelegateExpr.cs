namespace AdventureScript
{
    internal class DelegateExpr : Expr
    {
        Expr m_delegateExpr;
        DelegateTypeDef m_delegateType;
        IList<Expr> m_argList;

        public DelegateExpr(Parser parser, Expr delegateExpr, DelegateTypeDef delegateType, IList<Expr> argList)
        {
            FunctionExpr.CheckArguments(parser, delegateType.Name, delegateType.ParamList, argList);

            m_delegateExpr = delegateExpr;
            m_delegateType = delegateType;
            m_argList = argList;
        }

        public override TypeDef Type => m_delegateType.ReturnType;

        public override int Evaluate(GameState game, int[] frame)
        {
            int funcIndex = m_delegateExpr.Evaluate(game, frame);
            if (funcIndex == 0)
            {
                // Special case for "null" delegate.
                return 0;
            }

            var def = game.Functions[funcIndex];

            // Allocate a new frame of the specified size.
            var newFrame = new int[def.FrameSize];

            // Evaluate arguments and store the values starting
            // at frame index 1.
            int argCount = m_argList.Count;
            for (int i = 0; i < argCount; i++)
            {
                int arg = m_argList[i].Evaluate(game, frame);
                newFrame[i + 1] = arg;
            }

            return def.Invoke(game, newFrame);
        }

        public override bool HasSideEffects => true;
        public override bool IsConstant => false;
        public override Precedence Precedence => Precedence.Member;

        public override void WriteExpr(GameState game, CodeWriter writer)
        {
            WriteSubExpr(game, m_delegateExpr, writer);
            FunctionExpr.WriteArgs(m_argList, game, writer);
        }
    }
}
