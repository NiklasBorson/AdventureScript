# Introduction

Thank you for considering contributing to AdventureScript! We hope to have a growing
library of free text adventure games thanks to the efforts of people like you.

Thanks also for reading these guidelines. Following the guidelines of any open-source
project shows that you respect the time of the developers managing the project. It
also helps smooth the process of getting your pull requests approved and completed.

## Types of Contributions

AdventureScript is an open-source project and we welcome your contributions. Some
possible types of contributions are:

- Adding or revising a game.
- Adding or revising documentation.
- Improving tests or scripts.
- Contributing to the foundation library.
- Contributing to the AdventureScript language/interpreter/game engine.
- Contributing to the TextAdventure console app.
- Contributing to the OxbowCastle GUI app.
- Others?

## Ground Rules

Please be courteous, kind, and respectful. We want this to be a fun and welcoming place
for newcomers and people of all backgrounds and abilities!

Be sure to run tests before creating a pull request if your change involves any code
changes -- including changes to AdventureScript code. When adding a game, you should
add a regression test for the game.

## Running Tests

1. Start a PowerShell prompt and navigate to the repo root.
2. Type `Import-Module .\Adventure-Helpers.psm1`
3. Type `Invoke-Tests`

## Game Regression Tests

Regression tests ensure that future changes to the game engine or shared libraries
don't inadvertently break existing games.

Adding a regression test for a game is easy:

1. Start a PowerShell prompt and navigate to the repo root.
2. Type `Import-Module .\Adventure-Helpers.psm1`
3. Type `Build-GameTrace <YourGame>`
4. Play through the game

The above commands generate two files:

- `EngineTest\TestFiles\<YourGame>-input.txt`
- `EngineTest\Baseline\<YourGame>-trace.txt`

The input file contains all the commands you entered while playing the game. The trace
file contains both the commands and the resulting game output. The regression test
"plays" the game by re-entering your commands and captures a new trace. It then
compares the new trace with the baseline trace.

Note that a test failure just means something changed, which may or may not be a bug.
If a change is intentional, you can either recapture a trace or just updated the
baseline file by typing `Update-Baselines`.

## Creating a Git Pull Request

The basic workflow for contributing to the project is as follows. This assumes
you already have git installed and have cloned the AdventureScript repo.

1. Create a topic branch named `user/<YourName>/<BranchName>`

   - Including your Github login name helps ensure your branch name is unique.
   - The _BranchName_ can be anything that makes sense to you.
   - Make sure your main branch is up-to-date before branching from it.
   - `git checkout main`
   - `git pull`
   - `git checkout -b user/JohnSmith/MyNewBranch`

2. Make changes in your local working tree.

   - Edit or add whatever files are necessary, e.g., adding a game
   - Run tests or add tests as appopriate

3. Commit changes to your topic branch.

   - `git add --all`
   - `git commit --message "Add a new game."`

4. Repeat steps 2-3 as many times as you want.

5. Push your changes to the github.

   - `git push`
   - The first time, you need to set the upstream branch. You can just copy the
     command from the `git push` error message.

6. Navigate to the repo on github.com and create a pull request for your branch.
