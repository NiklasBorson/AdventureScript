namespace AdventureLib
{
    record class WhereClause(
        PropertyDef Property,
        BinaryOp Op,
        Expr RightArg
        );

    class ForEachStatement : Statement
    {
        VariableExpr m_loopVar;
        TypeDef m_type;
        WhereClause? m_whereClause;
        Statement m_body;

        public ForEachStatement(VariableExpr loopVar, TypeDef type, WhereClause? whereClause, Statement body)
        {
            m_loopVar = loopVar;
            m_type = type;
            m_whereClause = whereClause;
            m_body = body;
        }

        public override void Invoke(GameState game, int[] frame)
        {
            if (m_whereClause != null)
            {
                // Get the property definition and operator delegate.
                var propDef = m_whereClause.Property;
                var op = m_whereClause.Op.Compute;

                // Evalute the right expression once outside the loop.
                int rightArg = m_whereClause.RightArg.Evaluate(game, frame);

                // Iterate over all the item IDs except the null item id.
                int itemCount = game.Items.Count;
                for (int itemId = 1; itemId < itemCount; itemId++)
                {
                    // Evaluate the Boolean operator.
                    int leftArg = propDef[itemId];
                    if (op(leftArg, rightArg) != 0)
                    {
                        m_loopVar.SetValue(game, frame, itemId);
                        m_body.Invoke(game, frame);
                    }
                }
            }
            else if (m_type == Types.Item)
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
            for (int i = iMin; i < iLim; i++)
            {
                m_loopVar.SetValue(game, frame, i);
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

            if (m_whereClause != null)
            {
                string propName = m_whereClause.Property.Name;
                var op = m_whereClause.Op;

                writer.Write($" where {propName} {op.SymbolText} ");

                Expr.WriteSubExpr(
                    game,
                    m_whereClause.RightArg,
                    op.Precedence,
                    writer
                    );
            }

            writer.BeginBlock();
            m_body.WriteStatement(game, writer);
            writer.EndBlock();
        }
    }
}
