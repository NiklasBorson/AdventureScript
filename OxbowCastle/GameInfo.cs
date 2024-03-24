namespace OxbowCastle
{
    abstract class GameInfo
    {
        public GameInfo(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract string Action { get; }

        public abstract void Invoke(MainWindow window);
    }

    sealed class SavedGameInfo : GameInfo
    {
        public SavedGameInfo(string name) : base(name)
        {
        }

        public override string Action => "Resume saved game";

        public override void Invoke(MainWindow window)
        {
            window.LoadSavedGame(this);
        }
    }

    sealed class NewGameInfo : GameInfo
    {
        public NewGameInfo(string name) : base(name)
        {
        }

        public override string Action => "Play new game";

        public override void Invoke(MainWindow window)
        {
            window.LaunchNewGame(this);
        }
    }
}
