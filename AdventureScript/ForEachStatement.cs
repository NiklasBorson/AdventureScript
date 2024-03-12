namespace AdventureLib
{
    class ForEachStatement : Statement
    {
        VariableExpr m_loopVar;
        TypeDef m_type;
        Statement m_body;

        public ForEachStatement(VariableExpr loopVar, TypeDef type, Statement body)
        {
            m_loopVar = loopVar;
            m_type = type;
            m_body = body;
        }

        public override void Invoke(GameState game, int[] frame)
        {
            if (m_type == Types.Item)
            {
                // Loop over all the items except the null item.
                Loop(game, frame, 1, game.Items.Count);
            }
            else
            {
                // Loop over all the enum values.
                Loop(game, frame, 0, m_type.ValueNames.Count);
            }
        }

        void Loop(GameState game, int[] frame, int iMin, int iLim)
        {
            for (int id = iMin; id < iLim; id++)
            {
                m_loopVar.SetValue(game, frame, id);
                m_body.Invoke(game, frame);
            }
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            writer.Write("foreach (");
            writer.Write(m_loopVar.Name);
            if (m_type != Types.Item)
            {
                writer.Write(":");
                writer.Write(m_type.Name);
            }
            writer.Write(")");
            writer.BeginBlock();
            m_body.WriteStatement(game, writer);
            writer.EndBlock();
        }
    }
}
