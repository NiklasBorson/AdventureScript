# AdventureScript Language

The AdventureScript language is a scripting language designed for implementing
text adventure games.

This document describes the syntax and core concepts of the AdventureScript
language.

Many building blocks of text adventure games are not part of the language itself
but instead are implemented by the AdventureScript Foundation library. This
library is itself implemented in AdventureScript and implements functionality
such as rooms, doors, movement, and so on. For a guide to creating text
adventure games using the AdventureScript Foundation Library, see
[Foundation Library Guide](Foundation-Library-Guide.md).

For a reference to the AdventureScript language grammer, see
[AdventureScript Grammar](AdventureScript-Grammar.md).

## Core Concepts

Core concepts of the language include:

- Items, which are objects with properties.
- Properties, which are named values associated with items.
- Variables, which can be global or local.
- Types, built-in and user-defined.
- Functions, built-in and user-defined.
- Commands, which map user input to code.
- Game blocks, which execute once per game.
- Turn blocks, which execute once per turn.

## AdventureScript Source Files

AdventureScript source files can either be text files or markdown files. Use of
markdown enables AdventureScript files such as the AdventureScript Foundation Library
to be self-documenting.

The AdventureScript parser recognizes a file as a markdown file if it has the `.md`
file extension. This means the following rules apply:

- Code blocks (delimited by ```) are treated as AdventureScript code.
- Markdown links to other `.md` files are treated as include statements.
` All other markdown content is ignored.

## Top-Level Elements

The following subsections describe declarations and definitinos that appear at global
scope in an AdventureScript file -- that is, out side of any functions or statement
blocks.

### Include Declaration

Causes the contents of another AdventureScript source file to be included in the
current file. For example:

```text
include "inc/Foundation.md";
```

### Enum Definition

The `enum` keyword defines an enumerated type. A variable or property of this type can
be one of the specified, named values.

```text
# Example enum type
enum Direction(North,South,East,West,Up,Down);
```

The first named value is the default value for the type, which is the initial value if
a variable or property of this type has not been assigned to.

### Delegate Definition

The `delegate` keyword introduces a delegate type. A variable or property of this type
can refer to a function with the specified parameters and return value. You can call a
delegate in the same way you would call a function.

```text
# Example delegate type
delegate ItemPredicate($item:Item) : Bool;
```

The above defines a delegate type named ItemPredicate. A variable or property of this
type can refer to a function that takes a single parameter of type Item and returns a
Boolean value.

Delegates are extensively used in the foundation library to implement "actions", which
are behaviors that depend on a particular item. For example, the "use" command invokes
the delegate assigned to the UseAction property of the specified item.

The default value of a delegate variable or property is null. Calling a null delegate
is allowed but has no effect. If the delegate has a return type, calling a null delegate
returns the default value for that type (e.g., zero or null).

### Property Definition

The `property` keyword defines a named property of a specified type, which can be
associated with any item.

```text
# Example property
property Health : Int;
```

The above defines a property named Health of type Int. Once the above property is defined,
you can get or set the Health of any item. Note that any item can have any property.
Properties are not specific to "classes" of items, as there is no class system in
AdventureScript.

There is also an abbreviated syntax for defining multiple properties of the same type:

```text
# Multiple properties
property Health, MaxHealth : Int;
```

### Item Definition

The `item` keyword creates an item with the specified name.

```text
# Example item
item front_hall;
```

You can subsequently refer to the item by name, e.g., to get or set its properties.

You can enclose the item name in quotes if includes spaces or is otherwise not a valid
name token. However, this makes it less convenient to refer to the item elsewhere in the
script. Instead of simply using the item name directly, you need to pass the item name
to the GetItem intrinsic function.

```text
item "front hall";
```

### Global Variable Definition

The `var` keyword at global scope creates a global variable.

```text
# Global variable example
var $playerScore : Int;
```

Variable names must begin with a dollar sign. The above examlpe creates a global variable
named `$playerScore` of type `Int`. Its initial value is the default value for type Int,
which is zero.

You can specify the initial value of a global variable at the same time as defining it.
In this case, the variable's type can be omitted if it can be deduced from the type of
the initializer expression.

```text
# Global variable with initializer
var $playerScore = 0;
```

The initializer must be a constant expression. It cannot include function calls or
refer to other variables.

### Function Definition

The `function` keyword defines a function, which may be called elsewhere in the script.

```text
# Function example
function SetHealth($item:Item, $health:Int, $maxHealth:Int)
{
    $item.Health = $health;
    $item.MaxHealth = $maxHealth;
}
```

The above example creates a function named `SetHealth`, which takes a parameter of type
Item and two parameters of type Int.

A function may return a value. In this case, the return type is specified after the
parameter list as follows:

```text
# Function returning a value
function HealthPercentage($item:Item) : Int
{
    # Note: divide-by-zero in AdventureScript returns zero.
    $return = $item.Health * 100 / $item.MaxHealth;
}
```

If a function has a return type, a local variable of that type is automatically defined
named `$return`. The return value of the function is whatever value is assigned to that
variable. If no value is assigned to the `$return` variable, the default value for that
type is returned.

### Lambda Function Definition

The `function` keyword can be used with the `=>` operator to define a lambda function,
which is a function consisting of a single expression.

```text
# Lambda function example
function HealthPercentage($item:Item) => $item.Health * 100 / $item.MaxHealth;
```

The return type of a lambda function can be omitted if it can be deduced from the
type of the expression.

### Map Definition

The `map` keyword creates a named mapping of enum values to other values. Once defined, a
mapping can be called like a function.

```text
# Example enum type with four possible values
enum DoorState(None, Open, Closed, Locked);

# Example mapping of DoorState values to Boolean values
map IsClosedOrLocked DoorState -> Bool {
    None -> false,
    Open -> false,
    Closed -> true,
    Locked -> true
}
```

The above mapping can be called like a function that takes a parameter of type DoorState
and returns a Bool. For example, `IsClosedOrLocked(DoorState.Open)` would return false.

The value on the right-hand side of each mapping can either be an enum value name
(if the target type is an enum type) or a constant expression.

### Command Definition

The `command` keyword specifies a command string followed by a statement block. The command
string is matched against user input and may include placeholders, which behave like
function parameters. The statement block is executed if user input matches the command
string.

```text
command "go {$dir:Direction}"
{
    Go($dir);
}
```

Placeholders in the command string are enclosed in curly braces. A placholder works
like a function in that it specifies a name and a type. The placeholder name can be
referenced like a variable in the statement block which follows.

When matching the command string against user input, the game engine each placeholder
is matched against the corresponding part of the user input string. The game engine
attempts to convert the input substring to specified value.

In the above example, `Direction` is assumed to be a previously-defined enum type.
If the user typed "go north", the `$dir` parameter would be set to `Direction.North`.

### Game Blocks

The `game` keyword specifes a statement block that is executed once to initialize the
game. There can be multipile game blocks, in which case they are executed in order.

```text
# Example game block
game
{
    # Set the player's initial location to the starting room.
    player.Location = start_room;
}
```

### Turn Blocks

The `turn` keyword specifies a statement block that is executed before each turn.

```text
# Example turn block
turn
{
    # The player heals one health unit per turn.
    if (player.Health < player.MaxHealth)
    {
        player.Health = player.Health + 1;
    }
}
```

## Statements

A statement block comprises a sequence of statements enclosed in curly braces. Examples
of statement blocks include the body of a function, the body of a "game" or "turn" block,
the body of a command definition, the body of a foreach loop, and so on.

The following subsections describe the different kinds of statements that can be used in
a statement block.

### Local Variable Statement

The `var` keyword in a statement block adds a local variable.

```text
# Examples of local variables
var $room1 : Item;
var $room2 = NewItem("front_hall");
var $roomCount = 2;
```

A variable must have a name beginning with a dollar sign.

A variable may have an explicit type declaration, as in the `$room1` variable above.
The type must be declared if it cannot be inferred from the initializer.

A variable may have an initializer, as in the `$room2` and `$roomCount` variables above.
The initializer can be any expression that returns a value. It need not be a constant
expression.

### Expression Statement

An expression followed by semicolon is a valid statement, provided the expression has
side-effects.

```text
# Function call expression
Message("Hello World!");

# Error: this expression has no effect.
2 + 2;
```

### Assignment Statement

The result of one expression can be assigned to another expression. The left-hand expression
must be something that can be assigned to, like a variable or property.

```text
# Variable initialization (not assignment).
var $score = 0;

# Assigning to a variable.
$score = $score + 1;

# Assigning to a property.
player.Health = player.Health - 1;
```

### If Statement

The `if` statement executs a code block if a specified condition is true. The condition is an
expression enclosed in parentheses after the `if` keyword and must return a Boolean value
(i.e., true or false).

The `if` block may be followed by zero or more `elseif` blocks. If the "if" condition is false
then the first "elseif" condition is evaluated. If it's condition is false then the second
"elseif" condition is evaluated, and so on.

The `if` block and any `elseif` blocks may optionally be followed by an `else` block. The
else block is executed if none of the preceding conditions is true.

```text
# Example if statement
if ($item.Description != null)
{
    Message($item.Description);
}
elseif ($item.Noun != null)
{
    Message($"You see a {$item.Noun}");
}
else
{
    Message("You see something unidentifiable.");
}
```

### Switch Statement

The `switch` statement evalutes an expression and then executes the statement block for the
`case` matching the value of that expression. An optional `default` block is executed if
none of the cases match the value of the expression. Each `case` must be a constant
expression.

```text
# Example switch statement
switch ($item.DoorState)
{
    case DoorState.None {
        Message("You can't open that.");
    }
    case DoorState.Open {
        Message($"The {Label($item)} is already open.");
    }
    case DoorState.Closed {
        $item.DoorState = DoorState.Open;
        Message($"The {Label($item)} is now open.");
    }
    case DoorState.Locked {
        Message($"The {Label($item)} is locked.");
    }
}
```

### While Statement

The `while` statement executes a statement block as long as an expression is true. The
following example counts down from 10 to 1.

```text
# Example while statement
var $count = 10;
while ($count > 0)
{
    Message($"{$count}");
    $count = $count - 1;
}
```

### Foreach Statement

The `foreach` statement executes a statement block once for each item or once for each possible
value of an enum type.

```text
# Example of foreach with an enum type.
# Output is "North", "South", "East", "West", "Up", "Down".
foreach (var $dir : Direction)
{
    Message($"{$dir}");
}

# Example of foreach item.
# Note that Item is the default type and can be omitted.
foreach (var $item)
{
    if ($item.Location == player.Location)
    {
        Message($"You see a {Label($item)}.");
    }
}

# Example of foreach item with where clause.
# Effect is the same as the previous example.
foreach (var $item) where Location == player.Location
{
    Message($"You see a {Label($item)}.");
}
```

## Expressions

Expressions are the building blocks of statements.

### Literal Values

String literal
    A sequence of characters enclosed in quotation marks represents a
    literal value of type String.

Integer literal
    A sequence of decimal digits represents a literal value of type Int.

Boolean literal
    The keywords `true` and `false` represent literal values of type Bool.

Null literal
    The `null` keyword has no specific type but can be converted to the
    default "null" value of any type.

### Format Strings

A format string is string literal with a '$' prefix, and may contain embedded
expressions enclosed in curly braces. For example:

```text
$"You see a {$item.Noun}."
```

At run time, the expression inside the curly braces is evaluted, and the
resulting value is inserted in place of the embedded expression.

### Variable References

A variable name is an expression of the same type as the variable.

### Item Names

The name of an item is an expression of type Item.

### Property Expressions

Any expression of type Item can be followed by the `.` operator and a
property name to refer to the specified property of that item.

```text
# Example property expressions
player.Location             # where 'player' is an item
$item.Location              # where $item is a variable of type Item
$item.Location.Location     # where the Location property has type Item
GetItem("player").Location  # where the GetItem function returns an Item
```

### Function Call Expressions

A function name followed by a list of arguments in parentheses is an
expression. The type of the expression is the return type of the function.
Each comma-separated argument is itself an expression.

The argument types must match the parameter types of the function. The
function can be an intrisic function (built in to the game engine), or
a function defined using the `function` or `map` keyword.

```text
# Example function call expressions
Message("Hello World")
IsClosedOrLocked($item.DoorState)
```

### Delegate Expressions

Delegate expressions resemble function call expressions except that an
expression returning a delegate takes the place of the function name.

```text
# Example delegate expression
$item.UseAction($item)    # where UseAction is a delegate property
```

### Unary Operators

A Boolean expression may be preceded by the `!` operator to return
the Boolean complement (logical "not") of the expression.

An integer expression may be preceded by the `-` operator to return
the additive inverse (negative) of the expression.

### Grouping

Any expression enclosed in parentheses is itself an expression.

### Binary Expressions

A binary expression comprises a left argument, a binary operator, and a
right argument. The left and right arguments are themselves expressions.

For example, in the binary expression `$item.Noun != null`, the left
argument is the expression `$item.Noun`, the right argument is the
literal value `null`, and the binary expression returns true if the
left argument is not equal to null and false otherwise.

The arguments to a binary expression may themselves be binary expressions.
In this case, the order (or grouping) of subexpressions depends on
operator precedence. For example, multiplication and division have
higher precedence than addition and subtraction. Therefore, `1 + 2 * 3`
is evaluated as `1 + (2 * 3)`, not `(1 + 2) * 3`.

Following are the binary operators in decreasing order of precedence.

| Operators             | Meaning                           |
|=======================|===================================|
| `*` and `/`           | Multiplication and division       |
| `+` and `-`           | Addition and subtraction          |
| `== != > < >= <=`     | Comparison                        |
| `&&` and `||`         | Logical AND and OR                |

### Ternary Expressions

A ternary expression has the form _condition_ ? _expr1_ : _expr2_. If the
Boolean expression _condition_ evalutes to true then _expr1_ is evaluated.
If _condition_ is values than _expr2_ is evaluated.
