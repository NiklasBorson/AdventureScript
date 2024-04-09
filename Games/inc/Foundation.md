# AdventureScript Foundation Library

The AdventureScript Foundation library is a helper library written in AdventureScript that implements basic functionality common to many text adventure games. This includes:

- Rooms, doors, containers, and keys
- Navigation commands
- Item labels and descriptions
- Customizable actions like "use"
- Portable items, "take" and "drop"
- Darkness and light sources
- Weapons, armor, and combat

## Player and Location

The `Location` property specifies an item's location, such as a room or container.

The `player` item is a special item representing the player. Its `Location` property
is the current room.

The `player` item can also be the location of items the player has picked up using the
"take" command.

```text
## Location of an item, such as a room or a container.
property Location : Item;

## The player item represents the player.
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

## Noun and Adjectives Properties

The `Noun` and `Adjectives` properties are used to refer to an item both in game
output (such as listing the items in a room) and in input (when a user command references
an item). The `Noun` must be a single word. The `Adjectives` property can be zero or more
words separated by spaces.

```text
## Name that may be used to refer to an item.
property Noun : String;

## Space-separated adjectives that may be used to refer to an item along with its Noun.
property Adjectives : String;
```

## SetLabelProperties Function

The `SetLabelProperties` function sets the adjectives and noun for an item. These
words are used by the player to refer to the item and also comprise the item's label.

```text
## Sets the adjectives and noun for an item, which together comprise its label.
## $item: Item to set properties of.
## $adjectives: New value of the Adjectives property.
## $noun: New value of the Noun property.
function SetLabelProperties($item:Item, $adjectives:String, $noun:String)
{
    $item.Adjectives = $adjectives;
    $item.Noun = $noun;
}
```

## Label Function

The `Label` function gets a short label that can be used to refer to an item.

```text
## Gets an item's label, which is a string that can be used to refer to the item.
## $item: Item to get the label of.
## $return: Returns the item label.
function Label($item:Item) => $item.Adjectives != null ?
    $"{$item.Adjectives} {$item.Noun}" :
    $item.Noun;

## Gets an item's label with Markdown italic formatting.
## $item: Item to get the label of.
## $return: Returns the item label.
function ItalicLabel($item:Item) => $"_{Label($item)}_";
```

## Common Delegate Types

Delegates are references to functions. The delegate types defined in this section are
used to implement _action properties_ (see below).

`ItemDelegate` is a delegate type with a single parameter of type `Item`.

`Item2Delegate` is a delegate type with two parameters of type `Item`.

`ItemPredicate` is a delegate type that takes an `Item` parameter and returns a `Bool`.

```text
## Reference to a function that takes an Item parameter.
delegate ItemDelegate($item:Item);

## Reference to a function that takes two Item parameters.
delegate Item2Delegate($item1:Item, $item2:Item);

## Reference to a function that takes an Item parameter and returns a Bool.
delegate ItemPredicate($item:Item) : Bool;
```

## Action Helper Functions

The functions in this section are used to invoke action delegates.

```text
## Invokes an ItemAction delegate or displays an error message.
## $func: Delegate to invoke, which is typically a property of $item.
## $item: Item passed as a parameter to the delegate.
## $verb: Verb used in an error message if the delegate is null.
function InvokeItemAction($func:ItemDelegate, $item:Item, $verb:String)
{
    if ($func != null)
    {
        $func($item);
    }
    else
    {
        Message($"You cannot {$verb} the {Label($item)}.");
    }
}

## Invokes an ItemAction delegate or invokes a fallback action.
## $func: Delegate to invoke, which is typically a property of $item.
## $item: Item passed as a parameter to the delegate.
## $fallback: Alternative delegate to invoke if $func is null.
function InvokeItemActionWithFallback($func:ItemDelegate, $item:Item, $fallback:ItemDelegate)
{
    if ($func != null)
    {
        $func($item);
    }
    else
    {
        $fallback($item);
    }
}
```

## Action Properties

An _action property_ is a delegate property that may be used to implement customized,
item-specific behavior for a function or command. The following table lists action
properties along with the associated functions and commands that may invoke the
action.

| Action Property           | Function          | Related Commands              |
|---------------------------|-------------------|-------------------------------|
| `TakeAction`              | `Take`            | "take (item)"                 |
| `DropAction`              | `Drop`            | "drop (item)"                 |
| `UseAction`               | `Use`             | "use (item)"                  |
| `UseOnAction`             | `UseOn`           | "use (item) on (item)"        |
| `OpenAction`              | `Open`            | "open (item)"                 |
| `CloseAction`             | `Close`           | "close (item)"                |
| `DescribeAction`          | `Describe`        | "look", "look at (item)"      |
| `DescribeHealthAction`    | N/A               | N/A                           |
| `TurnOnAction`            | `TurnOn`          | "turn on (item)"              |
| `TurnOffAction`           | `TurnOff`         | "turn off (item)"             |
| `IgniteAction`            | `Ignite`          | N/A                           |
| `PutOutAction`            | `PutOut`          | "put out (item)"              |
| `PutInAction`             | `PutIn`           | "put (item) in (container)"   |
| `PutOnAction`             | `PutOn`           | "put (item) on (table)"       |

```text
## Delegate invoked by "take" command.
property TakeAction : ItemDelegate;

## Delegate invoked by "drop" command.
property DropAction : ItemDelegate;

## Delegate invoked by "use" command.
property UseAction : ItemDelegate;

## Delegate invoked by "use...on" command. The property
## applies to the item being used.
property UseOnAction : Item2Delegate;

## Delegate invoked by "open" command.
property OpenAction : ItemDelegate;

## Delgate invoked by "close" command.
property CloseAction : ItemDelegate;

## Delegate invoked by the Describe function.
property DescribeAction : ItemDelegate;

## Delegate invoked to describe the health of an item.
property DescribeHealthAction : ItemDelegate;

## Delegate invoked by the "turn on" command.
property TurnOnAction : ItemDelegate;

## Delegate invoked by the "turn off" command.
property TurnOffAction : ItemDelegate;

## Delegate invoked by the UseLighterOn function.
property IgniteAction : ItemDelegate;

## Delegate invoked by the "put out" command.
property PutOutAction : ItemDelegate;

## Delegate invoked by the "put...in" command. The property
## applies to the container.
property PutInAction : Item2Delegate;

## Delegate invoked by the "put..on" command. The property
## applies to the table.
property PutOnAction : Item2Delegate;   # applies to table
```

## LeaveAction and EnterAction Properties

The `LeaveAction` and `EnterAction` properties specify deligates that are invoked
when the user moves from one room to another. The `LeaveAction` is invoked on the
room being left, and the `EnterAction` is invoked on the room being entered. If
either function returns true then navigation is cancelled.

```text
## Delegate type for the LeaveAction and EnterAction properties.
## $from: Room being left.
## $to: Room being entered.
## $return: Return true to cancel navigation, or false to allow it.
delegate NavigationAction($from:Item, $to:Item) : Bool;

## Delegate invoked when leaving a room.
property LeaveAction : NavigationAction;

## Delegate invoked when entering a room.
property EnterAction : NavigationAction;
```

## DoorState Property

The `DoorState` property represents the state of an object that can be opened or closed.
This could be a door or an openable container. The default value is `None`, meaning the
item is not a door. Other possible values are `Open`, `Closed`, and `Locked`.

```text
## Represents the state of a door or container. The default None value
## applies to non-door items.
enum DoorState(None,Open,Closed,Locked);

## Represents the state of a door or container. The default None value
## applies to non-door items.
property DoorState : DoorState;
```

## IsClosedOrLocked

Returns true if a `DoorState` value is either closed or locked.

```text
## Returns true if the specified door state is either closed or locked.
## $fromValue: DoorState value.
## $return: Returns true for Closed or Locked.
map IsClosedOrLocked DoorState -> Bool {
    None -> false,
    Open -> false,
    Closed -> true,
    Locked -> true
}
```

## Key Property

The `Key` property specfies the key to a door or container. More than one door or
container can specify the same item as a key.

The door or container item's `DoorState` property must be something other than
`DoorState.None`.

The value must be a key item created using the `NewKey` function or initialized using
the `InitializeKey` function.

```text
## Specifies the item that can be used to unlock this item.
property Key : Item;
```

## InitializePortableItem and NewPortableItem Functions

The `IsPortable` function tests whether an item is portable.

The `InitializePortableItem` sets the properties of a portable item.

The `NewPortableItem` function creates and initializes a portable item.

```text
## TakeAction implementation for portable items.
## $item: Item the action applies to.
function TakePortableItem($item:Item)
{
    if ($item.Location != player)
    {
        $item.Location = player;
        Message($"The {Label($item)} is now in your inventory.");
    }
    else
    {
        Message($"The {Label($item)} is already in your inventory.");
    }
}

## DropAction implementation for portable items.
## $item: Item the action applies to.
function DropPortableItem($item:Item)
{
    if ($item.Location == player)
    {
        $item.Location = player.Location;
        Message($"You've dropped the {Label($item)}.");
    }
    else
    {
        Message($"The {Label($item)} is not in your inventory.");
    }
}

## Sets the properties of a portable item.
## $item: Item to set the properties of.
## $adjectives: Space-separated adjectives used to refer to the item.
## $noun: Noun used to refer to the item.
## $loc: Initial location of the item, such as a room or container.
function InitializePortableItem($item:Item, $adjectives:String, $noun:String, $loc:Item)
{
    SetLabelProperties($item, $adjectives, $noun);
    $item.TakeAction = TakePortableItem;
    $item.DropAction = DropPortableItem;
    $item.Location = $loc;
}

## Creates and initializes a portable item.
## $adjectives: Space-separated adjectives used to refer to the item.
## $noun: Noun used to refer to the item.
## $loc: Initial location of the item, such as a room or container.
## $return: Returns the newly-created item.
function NewPortableItem($adjectives:String, $noun:String, $loc:Item) : Item
{
    $return = NewItem($"_{$noun}{$loc}");
    InitializePortableItem($return, $adjectives, $noun, $loc);
}

## Determines whether an item is portable.
## $return: Returns true if the item's TakeAction is not null.
function IsPortable($item:Item) => $item.TakeAction != null;
```

## IsHidden Property

The `IsHidden` property can be used to implement secret doors and similar items that
must be unhidden before the player can see or interact with them.

```text
## Specifies whether this item is hidden.
property IsHidden : Bool;
```

## IsAccessible Function

The `IsAccessible` function tests whether an item is in the current room, in an open
container in the current room, on a table in the current room, or in the inventory.

```text
## Determines whether an item is accessible to the player.
function IsAccessible($item:Item) : Bool
{
    if (!$item.IsHidden && $item.Noun != null)
    {
        var $loc = $item.Location;
        if ($loc == player.Location || $loc == player)
        {
            # Item is in the current room or player inventory.
            return true;
        }
        elseif ($loc.Location == player.Location && !IsClosedOrLocked($loc.DoorState))
        {
            # Item is in an open container or on a table in the current room.
            return true;
        }
    }
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
## Represents the state of a light source. Some states apply only to light sources
## that behave like electric lights. Other states apply only to light sources that
## behave like candles.
## None: The item is not a light source.
## Off: The light is off.
## On: The light is on.
## Unlit: The candle is unlit.
## Lit: The candle is lit.
enum LightState(None, Off, On, Unlit, Lit);

## Represents the state ofa  light source.
property LightState : LightState;
```

## Current Time

The `$currentTime` global variable represents the current time in minutes from midnight.
It is automatically incremented at the start of each turn. An `IsDark` predicate might
use the current time to determine if there is daylight.

```text
var $currentTime = 11*60; # default initial time is 11am

const $dawnTime = 8 * 60;   # 8:00 AM
const $duskTime = 20 * 60;  # 8:00 PM
const $minutesPerDay = 24 * 60;

function IsNight() => $currentTime < $dawnTime || $currentTime >= $duskTime;

function IncrementTime()
{
    $currentTime = ($currentTime + 1) % $minutesPerDay;
}

turn
{
    IncrementTime();
}
```

## IsDark Property

The `IsDark` property is an `ItemPredicate` delegate that applies to a room. If this
method returns `true` then a light source is required for the user to see in the room.

Possible values of the `IsDark` property include the following functions:

- `IsDarkAtNight` returns true if it is night.
- `IsDarkAlways` returns true unconditionally.

```text
property IsDark : ItemPredicate;

function IsDarkAtNight($item:Item) => IsNight();

function IsDarkAlways($item:Item) => true;
```

## IsDark Helpers

Helper functions in this section are used to determine if it is currently dark.

```text
map IsActiveLightState LightState -> Bool {
    None -> false,
    Off -> false,
    On -> true,
    Unlit -> false,
    Lit -> true
}

# Called during turn initialization.
var $currentLightSource : Item = null;
var $isNowDark = false;

function InitializeLighting()
{
    $currentLightSource = null;

    if (player.Location.IsDark(player.Location))
    {
        foreach (var $item) where LightState != LightState.None
        {
            if (IsActiveLightState($item.LightState) && IsAccessible($item))
            {
                $currentLightSource = $item;
                break;
            }
        }
        $isNowDark = $currentLightSource == null;
    }
    else
    {
        $isNowDark = false;
    }
}
```

## LabelWithState Function

The `LabelWithState` function gets a short label that may include a state qualifier.
Some possible examples:

- _broken_ sword
- _open_ chest
- lantern, _which is lit_

```text
map LightStateAdj LightState -> String {
    None -> "",
    Off -> "off",
    On -> "on",
    Unlit -> "unlit",
    Lit -> "lit"
}

map DoorStateAdj DoorState -> String
{
    None -> "",
    Open -> "open",
    Closed -> "closed",
    Locked -> "closed"
}

function AddStateQualifier($item:Item, $phrase:String) =>
    $item.DoorState != DoorState.None ? $"{DoorStateAdj($item.DoorState)} {$phrase}" :
    $item.LightState != LightState.None ? $"{$phrase}, which is {LightStateAdj($item.LightState)}" :
    $phrase;

function LabelWithState($item:Item) => AddStateQualifier($item, Label($item));

function ItalicLabelWithState($item:Item) => AddStateQualifier($item, ItalicLabel($item));
```

## Inventory Function and Command

The `Inventory` function lists items in the player's inventory. The associated commands
are `inventory` and `i`.

```text
function Inventory()
{
    var $haveItems = false;
    foreach (var $item) where Location == player
    {
        if (!$haveItems)
        {
            Message("You have the following items:");
            $haveItems = true;
        }
        Message($"- A {ItalicLabelWithState($item)}.");
    }
    if (!$haveItems)
    {
        Message("There is nothing in your inventory.");
    }
}

```

## Description Property

The `Description` is a string describing an item. The default behavior of the
`Describe` function is to output this string if specified. The description of a room
typically begins with "You are in...".

```text
property Description : String;
```

## Image Property

The optional `Image` property is the relative path of an image file to display along
with the room or other item's description.

```text
property Image : String;
```

## DrawAction Property

The option `DrawAction` property draws an item and returns an integer identifier.

```text
delegate DrawDelegate($item:Item) : Int;

property DrawAction: DrawDelegate;
```

## Describe Helpers

This section contains internal helpers used to implement the Describe function.

```text
# Default behavior of Describe if DescribeAction is not set
function DescribeCommon($item:Item)
{
    if ($item.Description != null)
    {
        Message($item.Description);

        if ($item.DoorState != null && $item.Noun != null)
        {
            Message($"The {Label($item)} is {DoorStateAdj($item.DoorState)}.");
        }
    }
    elseif ($item.Noun != null)
    {
        Message($"You see a {ItalicLabelWithState($item)}.");
    }

    if ($item.Image != null)
    {
        Message($"![{$item}]({$item.Image})");
    }

    if ($item.DrawAction != null)
    {
        var $id = $item.DrawAction($item);
        Message($"![{$item}](#{$id})");
    }

    $item.DescribeHealthAction($item);
}
```

## Action Functions and Commands

The functions in this section invoke the associated action properties or display an
error message if the action property is null.

| Function          | Action Property           | Command                   |
|-------------------|---------------------------|---------------------------|
| `Take`            | `TakeAction`              | "take (item)"             |
| `Drop`            | `DropAction`              | "drop (item)"             |
| `Use`             | `UseAction`               | "use (item)"              |
| `UseOn`           | `UseOnAction`             | "use (item) on (item)"    |
| `Open`            | `OpenAction`              | "open (item)"             |
| `Close`           | `CloseAction`             | "close (item)"            |
| `TurnOn`          | `TurnOnAction`            | "turn on (item)"          |
| `TurnOff`         | `TurnOffAction`           | "turn off (item)"         |
| `PutOut`          | `PutOutAction`            | "put out (item)"          |
| `Describe`        | `DescribeAction`          | "look at (item)"          |

```text
function Take($item:Item) => InvokeItemAction($item.TakeAction, $item, "take");
function Drop($item:Item) => InvokeItemAction($item.DropAction, $item, "drop");
function Use($item:Item) => InvokeItemAction($item.UseAction, $item, "use");
function Open($item:Item) => InvokeItemAction($item.OpenAction, $item, "open");
function Close($item:Item) => InvokeItemAction($item.CloseAction, $item, "close");
function TurnOn($item:Item) => InvokeItemAction($item.TurnOnAction, $item, "turn on");
function TurnOff($item:Item) => InvokeItemAction($item.TurnOffAction, $item, "turn off");
function PutOut($item:Item) => InvokeItemAction($item.PutOutAction, $item, "put out");
function Describe($item:Item) => InvokeItemActionWithFallback($item.DescribeAction, $item, DescribeCommon);

function UseOn($item:Item, $target:Item)
{
    # The UseOnAction property applies to the item being used
    if ($item.UseOnAction != null)
    {
        $item.UseOnAction($item, $target);
    }
    else
    {
        Message($"You can't use the {Label($item)}.");
    }
}

function PutIn($item:Item, $target:Item)
{
    # The PutInAction property applies to the target (container)
    if ($target.PutInAction != null)
    {
        $target.PutInAction($item, $target);
    }
    else
    {
        Message($"You can't put the {Label($item)} in the {Label($target)}.");
    }
}

function PutOn($item:Item, $target:Item)
{
    # The PutOnAction property applies to the target (container)
    if ($target.PutOnAction != null)
    {
        $target.PutOnAction($item, $target);
    }
    else
    {
        Message($"You can't put the {Label($item)} on the {Label($target)}.");
    }
}
```

## Direction Type

The `Direction` type represents the possible directions a player can go. A direction
may also be specified as a parameter when linking objects such as rooms and doors.

```text
enum Direction(North,South,East,West,Up,Down);
```

## Opposite Function

The `Opposite` function returns the direction opposite to the specified direction.

```text
map Opposite Direction -> Direction
{
    North -> South,
    South -> North,
    East -> West,
    West -> East,
    Up -> Down,
    Down -> Up
}
```

## DirectionPhrase Function

The `DirectionPhrase` returns a phrase representing the specified direction, e.g.,
"to the North" or "leading upward".

```text
map DirectionPhrase Direction -> String
{
    North -> "to the North",
    South -> "to the South",
    East -> "to the East",
    West -> "to the West",
    Up -> "leading upward",
    Down -> "leading downward"
}
```

## Link Properties

Link properties are used to connect rooms together via doors. There is a link property
for each direction. A room links to a door item, which in turn links to the destination
room.

```text
property LinkN,LinkS,LinkE,LinkW,LinkU,LinkD : Item;
```

## GetLink and SetLink Functions

The `GetLink` and `SetLink` functions get and set the link property associated with
the specified direction.

```text
function GetLink($item:Item, $dir:Direction) : Item
{
    switch ($dir)
    {
        case Direction.North { return $item.LinkN; }
        case Direction.South { return $item.LinkS; }
        case Direction.East  { return $item.LinkE; }
        case Direction.West  { return $item.LinkW; }
        case Direction.Up    { return $item.LinkU; }
        case Direction.Down  { return $item.LinkD; }
    }
}

function SetLink($from:Item, $to:Item, $dir:Direction) 
{
    switch ($dir)
    {
        case Direction.North { $from.LinkN = $to; }
        case Direction.South { $from.LinkS = $to; }
        case Direction.East  { $from.LinkE = $to; }
        case Direction.West  { $from.LinkW = $to; }
        case Direction.Up    { $from.LinkU = $to; }
        case Direction.Down  { $from.LinkD = $to; }
    }
}
```

## OpenDoor and CloseDoor Functions

The `OpenDoor` and `CloseDoor` functions provide default implementations of the open
and close actions. The `IsOpenable` function tests whether an item supports the open
action.

```text
function OpenDoor($item:Item)
{
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
}
function CloseDoor($item:Item)
{
    switch ($item.DoorState)
    {
        case DoorState.None {
            Message("You can't open that.");
        }
        case DoorState.Open {
            $item.DoorState = DoorState.Open;
            Message($"The {Label($item)} is now closed.");
        }
        case DoorState.Closed {
            Message($"The {Label($item)} is already closed.");
        }
        case DoorState.Locked {
            Message($"The {Label($item)} is already closed.");
        }
    }
}
function IsOpenable($item:Item) => $item.OpenAction != null;
```

## Door Functions

This section defines functions for linking rooms together with _door items_. Door items
may be actual doors, which can be opened or closed, or mere openings.

| Function              | Description                                                   |
|-----------------------|---------------------------------------------------------------|
| `InitializeDoor`      | Sets properties on a door item.                               |
| `LinkRoomsOneWay`     | Creates a one-way link from one room to another via a door.   |
| `LinkRooms`           | Links two rooms via a specified door item.                    |
| `NewDoorItem`         | Links two rooms with a new door item.                         |
| `NewClosedDoor`       | Links two rooms with a new "door" in the Closed state.        |
| `NewLockedDoor`       | Links two rooms with a new "door" in the Locked state.        |
| `NewOpening`          | Links two rooms with a new "opening".                         |
| `NewLink`             | Links two rooms with a new unnamed object.                    |

```text
function LinkRoomsOneWay($from:Item, $to:Item, $via:Item, $dir:Direction)
{
    SetLink($from, $via, $dir);
    SetLink($via, $to, $dir);
}
function LinkRooms($from:Item, $to:Item, $via:Item, $dir:Direction)
{
    LinkRoomsOneWay($from, $to, $via, $dir);
    LinkRoomsOneWay($to, $from, $via, Opposite($dir));
}
function InitializeDoor($door:Item, $from:Item, $to:Item, $dir:Direction, $adjectives:String, $noun:String, $state:DoorState)
{
    SetLabelProperties($door, $adjectives, $noun);
    $door.DoorState = $state;

    if ($state != DoorState.None)
    {
        $door.OpenAction = OpenDoor;
        $door.CloseAction = CloseDoor;
    }
    
    LinkRooms($from, $to, $door, $dir);
}
function NewDoorItem($from:Item, $to:Item, $dir:Direction, $adjectives:String, $noun:String, $state:DoorState) : Item
{
    $return = NewItem($"_{$noun}_{$from}{$to}");
    InitializeDoor($return, $from, $to, $dir, $adjectives, $noun, $state);
}
function NewClosedDoor($from:Item, $to:Item, $dir:Direction) : Item
{
    return NewDoorItem($from, $to, $dir, "", "door", DoorState.Closed);
}
function NewLockedDoor($from:Item, $to:Item, $dir:Direction, $key:Item) : Item
{
    $return = NewDoorItem($from, $to, $dir, "", "door", DoorState.Locked);
    $return.Key = $key;
}
function NewOpening($from:Item, $to:Item, $dir:Direction) : Item
{
    return NewDoorItem($from, $to, $dir, "", "opening", DoorState.None);
}
function NewLink($from:Item, $to:Item, $dir:Direction) : Item
{
    return NewDoorItem($from, $to, $dir, "", "", DoorState.None);
}
```

## Key Helper Functions

This section contains internal functions related to keys.

```text
# UseOn action for keys
function UseKeyOn($key:Item, $target:Item)
{
    if ($target.Key == $key && $target.DoorState != DoorState.None)
    {
        if ($target.DoorState == DoorState.Locked)
        {
            $target.DoorState = DoorState.Closed;
            Message($"The {Label($target)} is now unlocked.");
        }
        else
        {
            $target.DoorState = DoorState.Locked;
            Message($"The {Label($target)} is now locked.");
        }
    }
    else
    {
        Message($"The {Label($key)} doesn't work on that.");
    }
}
```

## InitializeKey and UseKey Functions

The `InitializeKey` function initialize the properties of a key item.

The `NewKey` function creates and initializes a new key item.

The `IsKey` function tests whether a specified item is a key.

```text
function InitializeKey($key:Item, $adjectives:String, $noun:String, $loc:Item)
{
    InitializePortableItem($key, $adjectives, $noun, $loc);
    $key.UseOnAction = UseKeyOn;
}
function NewKey($adjectives:String, $noun:String, $loc:Item) : Item
{
    var $key = NewItem($"_{$noun}_{$loc}");
    InitializeKey($key, $adjectives, $noun, $loc);
    return $key;
}
function IsKey($item:Item) => $item.UseOnAction == UseKeyOn;
```

## Container Functions

The functions in this section apply to _container items_, which are items that contain
other items. Some containers can be opened or closed. You can put items in a container
and take items that are in an open container.

The `InitializeContainer` function initialize the properties of a container item such
as a box or chest.

The `NewContainer` function creates and initializes a new container item.

The `IsContainer` function tests whether the specified item is a container.

The `IsOpenContainer` function tests whether the specified item is a container that is
not closed or locked.

```text
function ListContainerContents($container:Item)
{
    var $haveItems = false;
    foreach (var $item) where Location == $container
    {
        if (!$haveItems)
        {
            Message($"Inside the {$container.Noun} are the following:");
            $haveItems = true;
        }
        Message($" - A {ItalicLabelWithState($item)}.");
    }
    if (!$haveItems)
    {
        Message($"The {$container.Noun} is empty.");
    }
}

function OpenContainer($item:Item)
{
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
            ListContainerContents($item);
        }
        case DoorState.Locked {
            Message($"The {Label($item)} is locked.");
        }
    }
}

function DescribeContainer($container:Item)
{
    # Output basic description, including the door state.
    DescribeCommon($container);

    if (!IsClosedOrLocked($container.DoorState))
    {
        ListContainerContents($container);
    }
}

# PutInAction implementation for containers
function PutInContainer($item:Item, $container:Item)
{
    if (!IsPortable($item))
    {
        Message($"You can't move the {Label($item)}.");
    }
    elseif (IsClosedOrLocked($container.DoorState))
    {
        Message($"The {Label($container)} is closed.");
    }
    elseif ($item.Location == $container)
    {
        Message($"The {Label($item)} is already in the {Label($container)}.");
    }
    else
    {
        $item.Location = $container;
        Message($"The {Label($item)} is now in the {Label($container)}.");
    }
}

function InitializeContainer(
    $container:Item,
    $adjectives:String,
    $noun:String,
    $state:DoorState,
    $loc:Item
    )
{
    SetLabelProperties($container, $adjectives, $noun);
    $container.DoorState = $state;
    $container.PutInAction = PutInContainer;
    $container.OpenAction = OpenContainer;
    $container.CloseAction = CloseDoor;
    $container.DescribeAction = DescribeContainer;
    $container.Location = $loc;
}

function NewContainer($adjectives:String, $noun:String, $state:DoorState, $loc:Item) : Item
{
    var $container = NewItem($"_{$noun}_{$loc}");
    InitializeContainer($container, $adjectives, $noun, $state, $loc);
    return $container;    
}

function IsContainer($item:Item) => $item.PutInAction != null;

function IsOpenContainer($item:Item) =>
    IsContainer($item) &&
    !IsClosedOrLocked($item.DoorState);
```

## Table Functions

The functions in this section apply to tables and similar items like desks and shelves.
These are similar to containers except that another item may be "on" a table, as opposed
to "in" a container. Also, table items cannot be closed.

The `InitializeTable` function initialize the properties of a table item.

The `NewTable` function creates and initializes a new table item.

The `IsTable` function tests whether the specified item is a table.

```text
function DescribeTable($table:Item)
{
    # Output basic description.
    DescribeCommon($table);

    var $haveItems = false;
    foreach (var $item) where Location == $table
    {
        if (!$haveItems)
        {
            Message($"On the {$table.Noun} are the following:");
            $haveItems = true;
        }
        Message($" - A {ItalicLabelWithState($item)}.");
    }
}

# PutOnAction implementation for table items
function PutOnTable($item:Item, $table:Item)
{
    if (!IsPortable($item))
    {
        Message($"You can't move the {Label($item)}.");
    }
    elseif ($item.Location == $table)
    {
        Message($"The {Label($item)} is already on the {Label($table)}.");
    }
    else
    {
        $item.Location = $table;
        Message($"The {Label($item)} is now on the {Label($table)}.");
    }
}

function InitializeTable(
    $table:Item,
    $adjectives:String,
    $noun:String,
    $loc:Item
    )
{
    SetLabelProperties($table, $adjectives, $noun);
    $table.PutOnAction = PutOnTable;
    $table.DescribeAction = DescribeTable;
    $table.Location = $loc;
}

function NewTable($adjectives:String, $noun:String, $loc:Item) : Item
{
    $return = NewItem($"_{$noun}_{$loc}");
    InitializeTable($return, $adjectives, $noun, $loc);
}

function IsTable($item:Item) => $item.PutOnAction != null;
```

## Look Function

The `Look` function describes the current room and its contents.

```text
function Look()
{
    var $room = player.Location;

    if ($isNowDark)
    {
        Message("You are in the dark.");

        # Check for an open door letting some light in.
        foreach (var $dir : Direction)
        {
            var $door = GetLink($room, $dir);
            if ($door != null && !IsClosedOrLocked($door.DoorState))
            {
                var $linkedRoom = GetLink($door, $dir);
                if (!$linkedRoom.IsDark($linkedRoom))
                {
                    Message($"A faint light filters in from the {$door.Noun} {DirectionPhrase($dir)}.");
                }
            }
        }
    }
    else
    {
        if ($currentLightSource != null)
        {
            if (player.Location.IsDark == IsDarkAtNight)
            {
                Message($"It is night, but the {Label($currentLightSource)} illuminates the darkness.");
            }
            else
            {
                Message($"The {Label($currentLightSource)} illuminates the darkness.");
            }
        }

        Describe($room);

        foreach (var $dir:Direction)
        {
            var $door = GetLink(player.Location, $dir);
            if ($door != null && $door.Noun != null && !$door.IsHidden)
            {
                Message($"There is a {LabelWithState($door)} {DirectionPhrase($dir)}.");
            }
        }

        foreach (var $item) where Location == $room
        {
            if (!$item.IsHidden && $item.Noun != null)
            {
                Message($"There is a {ItalicLabelWithState($item)} here.");
            }
        }
    }
}
```

## Lighting Helpers

The section contains internal helpers used to implement lighting.

```text
function OnLightActivated($lightSource:Item)
{
    if ($isNowDark)
    {
        $isNowDark = false;
        $currentLightSource = $lightSource;
        Look();
    }
}

# TurnOnAction for a light source that behaves like an electric light
function TurnOnLight($item:Item)
{
    if ($item.LightState != LightState.On)
    {
        $item.LightState = LightState.On;
        Message($"The {Label($item)} is now on.");
        OnLightActivated($item);
    }
    else
    {
        Message($"The {Label($item)} is already on.");
    }
}
# TurnOfAction for a light source that behaves like an electric light
function TurnOffLight($item:Item)
{
    if ($item.LightState == LightState.On)
    {
        $item.LightState = LightState.Off;
        Message($"The {Label($item)} is now off.");
    }
    else
    {
        Message($"The {Label($item)} is already off.");
    }
}
# TurnOnAction for a candle-like light source
function TurnOnCandle($item:Item)
{
    Message($"You can't turn on the {Label($item)}. You need to light it with something.");
}
# IgniteAction for a candle-like light source
function IgniteCandle($item:Item)
{
    if ($item.LightState != LightState.Lit)
    {
        $item.LightState = LightState.Lit;
        Message($"The {Label($item)} is now lit.");
        OnLightActivated($item);
    }
    else
    {
        Message($"The {Label($item)} is already lit.");
    }
}
# PutOutAction for a candle-like light source
function PutOutCandle($item:Item)
{
    if ($item.LightState == LightState.Lit)
    {
        $item.LightState = LightState.Unlit;
        Message($"The {Label($item)} is now out.");
    }
    else
    {
        Message($"The {Label($item)} is already out.");
    }
}
# UseAction for a lighter item
function UseLighterOn($item:Item, $target:Item)
{
    if ($target.IgniteAction != null)
    {
        $target.IgniteAction($target);
    }
    else
    {
        Message($"You cannot light the {Label($target)}.");
    }
}
```

## InitializeLight and NewLight Functions

The functions in this section create or initialize light sources that can be turned on
or off light electric lights.

The `InitializeLight` function sets the properties of a light item.

The `NewLight` function creates and initializes a new light item.

The `IsLight` function tests whether an item is a light.

```text
function InitializeLight($item:Item, $adjectives:String, $noun:String, $loc:Item)
{
    InitializePortableItem($item, $adjectives, $noun, $loc);
    $item.LightState = LightState.Off;
    $item.TurnOnAction = TurnOnLight;
    $item.TurnOffAction = TurnOffLight;
    $item.PutOutAction = TurnOffLight;
}
function NewLight($adjectives:String, $noun:String, $loc:Item) : Item
{
    var $item = NewItem($"_{$noun}_{$loc}");
    InitializeLight($item, $adjectives, $noun, $loc);
    return $item;
}
function IsLight($item:Item) => $item.TurnOnAction == TurnOnLight;
```

## InitializeCandle and NewCandle Functions

The functions in this section create or initialize light sources that behave like
candles. This includes oil lamps, torches, or any other light source that must be
"lit" rather than merely turned on.

The `InitializeCandle` function sets the properties of a candle item.

The `NewCandle` function creates and initializes a new candle item.

The `IsCandle` function tests whether an item is a candle.

```text
function InitializeCandle($item:Item, $adjectives:String, $noun:String, $loc:Item)
{
    InitializePortableItem($item, $adjectives, $noun, $loc);
    $item.LightState = LightState.Unlit;
    $item.TurnOnAction = TurnOnCandle;  # displays error message
    $item.IgniteAction = IgniteCandle;  # sets to LightState.Lit
    $item.TurnOffAction = PutOutCandle; # sets to LightState.Unlit
    $item.PutOutAction = PutOutCandle;  # sets to LightState.Unlit
}
function NewCandle($adjectives:String, $noun:String, $loc:Item) : Item
{
    var $item = NewItem($"_{$noun}_{$loc}");
    InitializeCandle($item, $adjectives, $noun, $loc);
    return $item;
}
function IsCandle($item:Item) => $item.TurnOnAction == TurnOnCandle;
```

## InitializeLighter and NewLighter Functions

The functions in this section create or initialize _lighter items_, which can be used
to invoke the ignite action on other items.

The `InitializeLighter` function sets the properties of a lighter item.

The `NewLighter` function creates and initializes a new lighter item.

The `IsLighter` function tests whether an item is a lighter.

```text
function InitializeLighter($item:Item, $adjectives:String, $noun:String, $loc:Item)
{
    InitializePortableItem($item, $adjectives, $noun, $loc);
    $item.UseOnAction = UseLighterOn;
}
function NewLighter($adjectives:String, $noun:String, $loc:Item) : Item
{
    var $item = NewItem($"_{$noun}_{$loc}");
    InitializeLighter($item, $adjectives, $noun, $loc);
    return $item;
}
function IsLighter($item:Item) => $item.UseOnAction == UseLighterOn;

function LightWith($target:Item, $lighter:Item)
{
    if (IsLighter($lighter))
    {
        UseLighterOn($lighter, $target);
    }
    else
    {
        Message($"The {Label($lighter)} has no effect.");
    }
}
```

## Navigation Functions

The `Go` function navigates in the specified direction.

```text
var $lastRoom : Item;

function Go($dir:Direction)
{
    var $source = player.Location;
    var $door = GetLink($source, $dir);
    if ($door == null || $door.IsHidden)
    {
        Message($"You cannot go {$dir}.");
    }
    elseif (IsClosedOrLocked($door.DoorState) && $isNowDark)
    {
        Message($"You cannot go {$dir}.");
    }
    elseif ($door.DoorState == DoorState.Locked)
    {
        Message($"The {Label($door)} is locked.");
    }
    else
    {
        if ($door.DoorState == DoorState.Closed)
        {
            Message($"You open the {Label($door)}.");
            $door.DoorState = DoorState.Open;
            Tick();
        }

        Message($"You go {$dir}.");

        var $dest = GetLink($door, $dir);
        if (!$source.LeaveAction($source, $dest) &&
            !$dest.EnterAction($source, $dest))
        {
            $lastRoom = player.Location;
            player.Location = $dest;
            InitializeLighting();
            Look();
        }
    }
}
```

## Turn Initialization

This section contains internal functions that are called during turn initialization.

```text
function AddItemWords($item:Item)
{
    AddAdjectives($item.Adjectives, $item);
    AddNoun($item.Noun, $item);
}

function InitializeWordMap()
{
    if ($isNowDark)
    {
        # The room is dark, so only add labels for items in the inventory.
        foreach (var $item) where Location == player
        {
            AddItemWords($item);
        }
    }
    else
    {
        # Add labels for all accessible items.
        foreach (var $item)
        {
            if (IsAccessible($item))
            {
                AddItemWords($item);
            }
        }

        # Add labels for doors linked from the current room.
        foreach (var $dir:Direction)
        {
            var $door = GetLink(player.Location, $dir);
            if ($door.DoorState != DoorState.None)
            {
                AddAdjectives($"{$dir}", $door);
                AddItemWords($door);
            }
        }
    }
}
turn
{
    # Make sure player's location was set during game initialization.
    if (player.Location == null)
    {
        Message("Error: player.Location has not been set.");
    }
    else
    {
        InitializeLighting();
        InitializeWordMap();
    }
}
```

## Drawing Functions

The following functions can be set as the `DrawAction` property of a room:

- `DrawSquareRoom`
- `DrawRoom_200x100`
- `DrawRoom_100x200`
- `DrawRoom_300x150`
- `DrawRoundRoom`

The other functions in this section are helper functions that are used to
implement the above functions or could be used to implement game-specific
drawing functions.

```text
const $colorWhite = 0xffffffff; # argb
const $colorBlack = 0xff000000; # argb

function DrawDoors(
    $room:Item,
    $left:Int,
    $top:Int,
    $width:Int,
    $height:Int,
    $backColor:Int
    )
{
    var $openW = false;
    var $openE = false;

    if ($room.LinkW != null && !$room.LinkW.IsHidden)
    {
        if ($room.LinkW.DoorState != DoorState.None)
        {
            # West door
            DrawRectangle($left, $top + $height / 2 - 25, 17, 50, $colorWhite, $colorBlack, 2);
        }
        else
        {
            # West opening
            DrawRectangle($left, $top + 15, 17, $height - 30, $backColor, 0, 0);
            $openW = true;
        }
    }
    if ($room.LinkE != null && !$room.LinkE.IsHidden)
    {
        if ($room.LinkE.DoorState != DoorState.None)
        {
            # East door
            DrawRectangle($left + $width - 17, $top + $height / 2 - 25, 17, 50, $colorWhite, $colorBlack, 2);
        }
        else
        {
            # East opening
            DrawRectangle($left + $width - 17, $top + 15, 17, $height - 30, $backColor, 0, 0);
            $openE = true;
        }
    }
    if ($room.LinkN != null && !$room.LinkN.IsHidden)
    {
        if ($room.LinkN.DoorState != DoorState.None)
        {
            # North door
            DrawRectangle($left + $width / 2 - 25, $top, 50, 17, $colorWhite, $colorBlack, 2);
        }
        else
        {
            # North opening
            var $x1 = $openW ? 0 : 15;
            var $x2 = $openE ? $width : $width - 15;
            DrawRectangle($left + $x1, $top, $x2 - $x1, 17, $backColor, 0, 0);
        }
    }
    if ($room.LinkS != null && !$room.LinkS.IsHidden)
    {
        if ($room.LinkS.DoorState != DoorState.None)
        {
            # South door
            DrawRectangle($left + $width / 2 - 25, $top + $height - 17, 50, 17, $colorWhite, $colorBlack, 2);
        }
        else
        {
            # South opening
            var $x1 = $openW ? 0 : 15;
            var $x2 = $openE ? $width : $width - 15;
            DrawRectangle($left + $x1, $top + $height - 17, $x2 - $x1, 17, $backColor, 0, 0);
        }
    }

    # Draw stairs up if present
    if ($room.LinkU != null && !$room.LinkU.IsHidden)
    {
        var $x = $left + $width / 2 - 40;
        var $y = $top + $height / 2 - 35;
        DrawRectangle($x, $y, 35, 70, 0, 0xFF000000, 2);
        DrawRectangle($x, $y + 10, 35, 2, 0xFF000000, 0, 0);
        DrawRectangle($x, $y + 20, 35, 2, 0xFF000000, 0, 0);
        DrawRectangle($x, $y + 30, 35, 2, 0xFF000000, 0, 0);
        DrawRectangle($x, $y + 40, 35, 2, 0xFF000000, 0, 0);
        DrawRectangle($x, $y + 50, 35, 2, 0xFF000000, 0, 0);
        DrawRectangle($x, $y + 60, 35, 2, 0xFF000000, 0, 0);
    }

    # Draw stairs down if present
    if ($room.LinkD != null && !$room.LinkD.IsHidden)
    {
        var $x = $left + $width / 2 + 5;
        var $y = $top + $height / 2 - 35;
        DrawRectangle($x, $y, 35, 70, 0xFF000000, 0, 0);
        DrawRectangle($x + 4, $y + 2, 27, 8, 0xFFFFFFFF, 0, 0);
        DrawRectangle($x + 5, $y + 13, 25, 8, 0xFFFFFFFF, 0, 0);
        DrawRectangle($x + 6, $y + 24, 23, 7, 0xFFFFFFFF, 0, 0);
        DrawRectangle($x + 7, $y + 34, 21, 7, 0xFFFFFFFF, 0, 0);
        DrawRectangle($x + 8, $y + 44, 19, 6, 0xFFFFFFFF, 0, 0);
        DrawRectangle($x + 9, $y + 53, 17, 6, 0xFFFFFFFF, 0, 0);
        DrawRectangle($x + 10, $y + 62, 15, 5, 0xFFFFFFFF, 0, 0);
    }
}

function DrawRectangularRoom(
    $room:Item,
    $width:Int,
    $height:Int,
    $wallColor:Int,
    $backColor:Int
    ) : Int
{
    BeginDrawing($width, $height);

    DrawRectangle(0, 0, $width, $height, $backColor, $wallColor, 15);
    DrawDoors($room, 0, 0, $width, $height, $backColor);

    return EndDrawing();
}

function DrawEllipticalRoom($room:Item, $width:Int, $height:Int) : Int
{
    BeginDrawing($width, $height);

    DrawEllipse(0, 0, $width, $height, $colorWhite, $colorBlack, 15);
    DrawDoors($room, 0, 0, $width, $height, $colorWhite);

    return EndDrawing();
}

function DrawSquareRoom($room:Item) : Int
{
    return DrawRectangularRoom($room, 200, 200, $colorBlack, $colorWhite);
}

function DrawRoom_200x100($room:Item) : Int
{
    return DrawRectangularRoom($room, 200, 100, $colorBlack, $colorWhite);
}

function DrawRoom_100x200($room:Item) : Int
{
    return DrawRectangularRoom($room, 100, 200, $colorBlack, $colorWhite);
}

function DrawRoom_300x150($room:Item) : Int
{
    return DrawRectangularRoom($room, 300, 150, $colorBlack, $colorWhite);
}

function DrawRoundRoom($room:Item) : Int
{
    return DrawEllipticalRoom($room, 200, 200);
}
```

## Commands

This library implements the following commands.

```text
command "go {$dir:Direction}" { Go($dir); }
command "n" { Go(Direction.North); }
command "s" { Go(Direction.South); }
command "e" { Go(Direction.East); }
command "w" { Go(Direction.West); }
command "up" { Go(Direction.Up); }
command "down" { Go(Direction.Down); }
command "inventory" { Inventory(); }
command "i" { Inventory(); }
command "take {$item1:Item} and {$item2:Item} and {$item3:Item}" { Take($item1); Take($item2); Take($item3); }
command "take {$item1:Item} and {$item2:Item}" { Take($item1); Take($item2); }
command "take {$item:Item}" { Take($item); }
command "drop {$item:Item}" { Drop($item); }
command "put {$item:Item} in {$container:Item}" { PutIn($item, $container); }
command "put {$item:Item} on {$table:Item}" { PutOn($item, $table); }
command "use {$item:Item} on {$target:Item}" { UseOn($item, $target); }
command "use {$item:Item}" { Use($item); }
command "open {$item:Item}" { Open($item); }
command "close {$item:Item}" { Close($item); }
command "turn on {$item:Item}" { TurnOn($item); }
command "turn off {$item:Item}" { TurnOff($item); }
command "light {$target:Item} with {$lighter:Item}" { LightWith($target, $lighter); }
command "put out {$item:Item}" { PutOut($item); }
command "look at me" { Describe(player); }
command "look at self" { Describe(player); }
command "look at {$item:Item}" { Describe($item); }
command "look" { Look(); }
```
