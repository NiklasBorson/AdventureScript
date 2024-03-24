namespace AdventureScript
{
    sealed class ForEachItemStatement : LoopStatement
    {
        VariableExpr m_loopVar;
        int m_frameIndex;

        public ForEachItemStatement(VariableExpr loopVar)
        {
            m_loopVar = loopVar;
            m_frameIndex = loopVar.FrameIndex;
        }

        int InvokeInternal(GameState game, int[] frame, int itemId)
        {
            if (itemId < game.Items.Count)
            {
                // Update the loop variable and jump to the loop body.
                frame[m_frameIndex] = itemId;
                return NextStatementIndex;
            }
            else
            {
                // We're falling out of the loop.
                return this.EndStatement.NextStatementIndex;
            }
        }

        public override int Invoke(GameState game, int[] frame)
        {
            // First iteration starts with item ID 1.
            return InvokeInternal(game, frame, 1);
        }

        public override int InvokeNext(GameState game, int[] frame)
        {
            // Next iteration.
            return InvokeInternal(game, frame, frame[m_frameIndex] + 1);
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write($"foreach (var {m_loopVar.Name})");
            writer.BeginBlock();
        }
    }
}
