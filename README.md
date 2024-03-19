# AdventureScript

AdventureScript is a scripting language and interpreter designed for implementing text
adventure games. This repo contains the following subdirectories:

- **AdventureScript** is a shared library that implements the interpreter and game
  engine.

- **EngineTest** implements a set of tests for the game engine.

- **TextAdventure** is a terminal-based application implemented using the
  AdventureScript library.

- **Games** contains games implemented in AdventureScript.

- **Games/inc** contains the [AdventureScript Foundation Library](Games/inc/Foundation.md).

## AdventureScript Language

The AdventureScript language is a scripting language designed for implementing text
adventure games. The core language is fairly general-purpose and includes basic elements
such as items, properties, variables, and functions.

The core language does not implement game mechanics such as rooms, doors, movement,
weapons, or any game commands. These are implemented by the
[AdventureScript Foundation Library](Games/inc/Foundation.md), which is itself written
in AdventureScript. This split between the core language and a standard library means
you can implement different game mechanics by extending the standard library or using
your own library.

## AdventureScript Foundation Library

