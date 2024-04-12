using Microsoft.UI.Xaml;

namespace OxbowCastle
{
    abstract class GameReference
    {
        public GameReference(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract string Action { get; }

        public virtual Visibility DeleteButtonVisibility => Visibility.Collapsed;

        public abstract void Invoke(StartPage page);
    }

    sealed class SavedGameReference : GameReference
    {
        public SavedGameReference(string name) : base(name)
        {
        }

        public override string Action => "Resume saved game";

        public override Visibility DeleteButtonVisibility => Visibility.Visible;

        public override void Invoke(StartPage page)
        {
            page.LoadSavedGame(this);
        }
    }

    sealed class NewGameReference : GameReference
    {
        public NewGameReference(string name) : base(name)
        {
        }

        public override string Action => "Play new game";

        public override void Invoke(StartPage page)
        {
            page.LaunchNewGame(this);
        }
    }

    sealed class BrowseGameReference : GameReference
    {
        public BrowseGameReference() : base("Browse")
        {
        }

        public override string Action => "Select game from disk";

        public override void Invoke(StartPage page)
        {
            page.BrowseForGame();
        }
    }
}
