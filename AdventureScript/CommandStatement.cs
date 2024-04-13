namespace AdventureScript
{
    internal class CommandStatement : Statement
    {
        CommandDef m_def;

        public CommandStatement(CommandDef def)
        {
            m_def = def;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            game.TurnCommands.Add(m_def);
            return NextStatementIndex;
        }

        public override void WriteStatement(GameState game, CodeWriter writer)
        {
            CommandMap.SaveCommand(game, m_def, writer);
        }
    }
}
