# AdventureScript

AdventureScript is a scripting language and interpreter designed for implementing text
adventure games.

## Organization of this Project

This repo contains the following subdirectories:

- **AdventureScript** is a shared library that implements the interpreter and game
  engine.

- **TextAdventure** is a terminal-based application implemented using the
  AdventureScript library.

- **OxbowCastle** is a WinUI3 application implemented using the AdventureScript library.

- **Games** contains games implemented in AdventureScript. This includes a Demo game,
  which is intended as a code sample for writing your own games.

- **AdventureTest** implements a set of tests for the game engine and for games.

- **Docs** contains a walkthrough of the [Demo](Docs/Demo.md) game as well as reference
  documentation for the [AdventureScript Language](Docs/AdventureScript-Language.md).

## Contributions

Contributions are welcome. In particular, it is hoped that this project inspires people to
create their own games. Please see <CONTRIBUTING.md> for guidance on how to contribute to
this project.

## Building the Project

To build the project, open `AdventureScript.sln` in Visual Studio and choose "Build Solution"
from the "Build" menu (or press CTRL+SHIFT+B).

## Playing Games

You can play text adventure games in one of two ways:

- Using the **TextAdventure** console application from the terminal.
- Using the graphical **Oxbow Castle** application.

The graphical application has some additional capabilities, such as the ability to display
images.

### Running TextAdventure

The easiest way to run the TextAdventure game is using the Adventure-Helpers PowerShell
module:

1. Start a PowerShell prompt navigate to the repo root.
2. Type `Import-Module .\Adventure-Helpers.psm1`.
3. Type `Get-Games` to see a list of available games.
4. Type `Invoke-Game <Name>` to play a game.
5. Type `q` to quit.

For more information about the Adventure-Helpers module, see <CONTRIBUTING.md>.

### Running the Oxbow Castle Application

**Oxbow Castle** is a graphical application built on the WinUI3 framework. It is named
after the flagship "Oxbow Castle" game but actually includes all the games in this project.
You can also browse to a directory containing your own game. Oxbow Castle provides a more
user-friendly experience than TextAdventure and also has additional capabilities such as
the ability to display images and save game progress.

You can run Oxbow Castle from Visual Studio as follows:

1. Open `AdventureScript.sln` in Visual Studio.
2. Build the solution if you haven't already.
3. Right-click the **OxbowCastle** project and choose "Set as Startup Project".
4. Click the "Start Without Debugging" button on the toolbar (or press CTRL+F5).

Alternatively, you can install the Oxbow Castle app from the Store app in Windows.
