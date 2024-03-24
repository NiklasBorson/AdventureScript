namespace AdventureScript
{
    sealed class ForEachItemWhereStatement : LoopStatement
    {
        VariableExpr m_loopVar;
        int m_frameIndex;

        PropertyDef m_propDef;
        BinaryOp m_op;
        Expr m_rightArg;
        int m_rightArgFrameIndex;

        public ForEachItemWhereStatement(
            VariableExpr loopVar,   // named loop variable
            PropertyDef propDef,    // property named in where clause
            BinaryOp op,            // comparison operator in where clause
            Expr rightArg,          // right-hand expression in where clause
            int rightArgFrameIndex  // index of hidden variable for right-hand value
            )
        {
            m_loopVar = loopVar;
            m_frameIndex = loopVar.FrameIndex;
            m_propDef = propDef;
            m_op = op;
            m_rightArg = rightArg;
            m_rightArgFrameIndex = rightArgFrameIndex;
        }

        int InvokeInternal(GameState game, int[] frame, int itemId)
        {
            var op = m_op.Compute;
            int itemCount = game.Items.Count;

            for (; itemId < itemCount; itemId++)
            {
                // Is the where clause true?
                int leftValue = m_propDef[itemId];
                int rightValue = frame[m_rightArgFrameIndex];
                if (op(leftValue, rightValue) != 0)
                {
                    // Update the loop variable and jump to the loop body.
                    frame[m_frameIndex] = itemId;
                    return NextStatementIndex;
                }
            }

            // We're falling out of the loop.
            return this.EndStatement.NextStatementIndex;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            // First iteration evalutes the right-hand expression and stores
            // it in an unnamed variable.
            frame[m_rightArgFrameIndex] = m_rightArg.Evaluate(game, frame);

            // First iteration starts with item ID 1.
            return InvokeInternal(game, frame, /*itemId*/ 1);
        }

        public override int InvokeNext(GameState game, int[] frame)
        {
            // Subsequent iterations increment the item id.
            return InvokeInternal(game, frame, frame[m_frameIndex] + 1);
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write($"foreach (var {m_loopVar.Name})");
            writer.Write($" where {m_propDef.Name} {m_op.SymbolText} ");
            Expr.WriteSubExpr(game, m_rightArg, m_op.Precedence, writer);
            writer.BeginBlock();
        }
    }
}
