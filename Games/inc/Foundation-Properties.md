# AdventureScriptLib Properties and Types

This module defines properties and types for AdventureScriptLib.

## Location

The `Location` property specifies an item's location, such as a room or container.

The `player` item is a special item representing the player. Its `Location` property
is the current room.

The `player` item can also be the location of items the player has picked up using the
"take" command.

```text
property Location : Item;
item player;

turn
{
    # Make sure player's location was set during game initialization.
    if (player.Location == null)
    {
        Message("Error: player.Location has not been set.");
    }
}
```

## Noun, Adj1, and Adj2 Properties

Noun and adjective properties enable a user to refer to an item. They may also be used
as a label or description for an item.

```text
property Noun,Adj1,Adj2 : String;
```

## Health and Damage Properties

The `Health` and `MaxHealth` properties are used to keep track of the amount of
injury or damage to a character or object. Damage reduces the `Health` property
by specified amount. The _relative health_ of a character or object is the ratio
of `Health` to `MaxHealth`. If both properties are zero, the item is not subject
to damage.

The `WeaponDamage` property specifies the amount of damage inflicted by a
weapon item.

```text
property Health : Int;
property MaxHealth : Int;
property WeaponDamage : Int;
```

## Common Delegate Types

Delegates are references to functions. The delegate types defined in this section are
used to implement _action properties_ (see below).

`ItemDelegate` is a delegate type with a single parameter of type `Item`.

`Item2Delegate` is a delegate type with two parameters of type `Item`.

`ItemPredicate` is a delegate type that takes an `Item` parameter and returns a `Bool`.

```text
delegate ItemDelegate($item:Item);
delegate Item2Delegate($item1:Item, $item2:Item);
delegate ItemPredicate($item:Item) : Bool;
```

## Action Properties

An _action property_ is a delegate property that may be used to implement customized,
item-specific behavior for a function or command. The following table lists action
properties along with the associated functions and commands that may invoke the
action.

| Action Property           | Function          | Related Commands          |
|---------------------------|-------------------|---------------------------|
| `TakeAction`              | `Take`            | "take (item)"             |
| `DropAction`              | `Drop`            | "drop (item)"             |
| `UseAction`               | `Use`             | "use (item)"              |
| `UseOnAction`             | `UseOn`           | "use (item) on (item)"    |
| `OpenAction`              | `Open`            | "open (item)"             |
| `CloseAction`             | `Close`           | "close (item)"            |
| `DescribeAction`          | `Describe`        | "look", "look at (item)"  |
| `DescribeHealthAction`    | `DescribeHealth`  | N/A                       |
| `DestroyAction`           | `Destroy`         |                           |
| `TurnOnAction`            | `TurnOn`          | "turn on (item)"          |
| `TurnOffAction`           | `TurnOff`         | "turn off (item)"         |
| `IgniteAction`            | `Ignite`          | N/A                       |
| `PutOutAction`            | `PutOut`          | "put out (item)"          |

```text
property TakeAction : ItemDelegate;
property DropAction : ItemDelegate;
property UseAction : ItemDelegate;
property UseOnAction : Item2Delegate;
property OpenAction : ItemDelegate;
property CloseAction : ItemDelegate;
property DescribeAction : ItemDelegate;
property DescribeHealthAction : ItemDelegate;
property DestroyAction : ItemDelegate;
property TurnOnAction : ItemDelegate;
property TurnOffAction : ItemDelegate;
property IgniteAction : ItemDelegate;
property PutOutAction : ItemDelegate;
```

## LeaveAction and EnterAction Properties

The `LeaveAction` and `EnterAction` properties specify deligates that are invoked
when the user moves from one room to another. The `LeaveAction` is invoked on the
room being left, and the `EnterAction` is invoked on the room being entered. If
either function returns true then navigation is cancelled.

```text
delegate NavigationAction($from:Item, $to:Item) : Bool;
property LeaveAction : NavigationAction;
property EnterAction : NavigationAction;
```

## Description Property

The `Description` is a string describing an item. The default behavior of the
`Describe` function is to output this string if specified. The description of a room
typically begins with "You are in...".

```text
property Description : String;
```

## Direction

The `Direction` type represents the possible directions a player can go. A direction
may also be specified as a parameter when linking objects such as rooms and doors.

```text
enum Direction(North,South,East,West,Up,Down);
```

## Link Properties

Link properties are used to connect rooms together via doors. There is a link property
for each direction. A room links to a door item, which in turn links to the destination
room.

```text
property LinkN,LinkS,LinkE,LinkW,LinkU,LinkD : Item;
```

## DoorState Property

The `DoorState` property represents the state of an object that can be opened or closed.
This could be a door or an openable container. The default value is `None`, meaning the
item is not a door. Other possible values are `Open`, `Closed`, and `Locked`.

```text
enum DoorState(None,Open,Closed,Locked);
property DoorState : DoorState;
```

## Key Property

The `Key` property specfies the key to a door or container. More than one door or
container can specify the same item as a key.

The door or container item's `DoorState` property must be something other than
`DoorState.None`.

The value must be a key item created using the `NewKey` function or initialized using
the `InitializeKey` function.

```text
property Key : Item;
```

## IsHidden Property

The `IsHidden` property can be used to implement secret doors and similar items that
must be unhidden before the player can see or interact with them.

```text
property IsHidden : Bool;
```

## IsDark Property

The `IsDark` property is an `ItemPredicate` delegate that applies to a room. If this
method returns `true` then a light source is required for the user to see in the room.

The following global variables are initialized at the beginning of each turn:

- `$isNowDark` is true if the current room's `IsDark` predicate returns is true
  _and_ there is no light source providing illumination.
- `$currentLightSource` refers to the light source illuminating an otherwise dark
  room, if any.

```text
property IsDark : ItemPredicate;
var $currentLightSource : Item = null;
var $isNowDark = false;
```

## Current Time

The `$currentTime` global variable represents the current time in minutes from midnight.
It is automatically incremented at the start of each turn. An `IsDark` predicate might
use the current time to determine if there is daylight.

```text
var $currentTime = 11*60; # default initial time is 11am

turn
{
    $currentTime = $currentTime < (23*60) + 59 ? $currentTime + 1 : 0;
}
```

## LightState Type and Property

The `LightState` property is used to implement light source objects. Light source objects
come in two categories: "lights" that can be on or off, and "candles" that can be lit or
unlit. Possible values are:

| Value     | Meaning                                   |
|-----------|-------------------------------------------|
| `None`    | The item is not a light source.           |
| `Off`     | The light is off.                         |
| `On`      | The light is on.                          |
| `Unlit`   | The candle is unlit.                      |
| `Lit`     | The candle is lit.                        |

The following action properties apply to "light" and "candle" light sources:

| Action            | Light Behavior                    | Candle Behavior               |
|-------------------|-----------------------------------|-------------------------------|
| `TurnOnAction`    | Sets the light state to On.       | Outputs an error message.     |
| `IgniteAction`    | null                              | Sets the light state to On.   |
| `TurnOffAction`   | Sets the light state to Off.      | Sets the light state to Off.  |
| `PutOutAction`    | Sets the light state to Off.      | Sets the light state to Off.  |

The "turn off" and "put out" actions are interchangeable for light sources but may
mean different things for other kinds of items.

```text
enum LightState(None, Off, On, Unlit, Lit);
property LightState : LightState;
```
