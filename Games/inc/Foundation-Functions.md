# AdventureScriptLib Functions

This module defines functions in AdventureScriptLib.

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

## Label Function

The `Label` function gets a short label that can be used to refer to an item.

```text
function Label($item:Item) => $"{$item.Adjectives} {$item.Noun}";
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
    $item.Health < 0 ? $"broken {$phrase}" :
    $item.DoorState != DoorState.None ? $"{DoorStateAdj($item.DoorState)} {$phrase}" :
    $item.LightState != LightState.None ? $"{$phrase}, which is {LightStateAdj($item.LightState)}" :
    $phrase;

function LabelWithState($item:Item) => AddStateQualifier($item, Label($item));
```

## SetLabelProperties Function

The `SetLabelProperties` function sets the adjectives and noun for an item. These
words are used by the player to refer to the item and also comprise the item's label.

```text
function SetLabelProperties($item:Item, $adjectives:String, $noun:String)
{
    $item.Adjectives = $adjectives;
    $item.Noun = $noun;
}
```

## Action Helper Functions

The functions in this section are used to invoke action delegates.

```text
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

function InvokeItem2Action($func:Item2Delegate, $item1:Item, $item2:Item, $verb:String)
{
    if ($func != null)
    {
        $func($item1, $item2);
    }
    else
    {
        Message($"You cannot {$verb} the {Label($item1)}.");
    }
}
```

## SetHealth Function

The `SetHealth` function sets an item's `Health` and `MaxHealth` properties.

```text
function SetHealth($item:Item, $health:Int, $maxHealth:Int)
{
    $item.Health = $health;
    $item.MaxHealth = $maxHealth;
}
```

## DescribeHealthCommon Function

The `DescribeHealthCommon` function implements the default behavior of the
`DescribeHealth` function if the `DescribeHealthAction` property has not been
set.

```text
function DescribeHealthCommon($item:Item)
{
    if ($item.MaxHealth != 0)
    {
        var $health = $item.Health;
        var $percentage = $health * 100 / $item.MaxHealth;
        if ($health < 0)
        {
            # The broken state is already reflected in the label.
        }
        elseif ($percentage < 20)
        {
            Message($"The {Label($item)} is critically damanged.");
        }
        elseif ($percentage < 40)
        {
            Message($"The {Label($item)} is severely damanged.");
        }
        elseif ($percentage < 60)
        {
            Message($"The {Label($item)} is significantly damanged.");
        }
        elseif ($percentage < 80)
        {
            Message($"The {Label($item)} is slightly damaged.");
        }
    }
}
```

## DestroyCommon Function

The `DestroyCommon` function implements the default behavior of the `Destroy`
function if the `DestroyAction` property has not been set.

```text
function DestroyCommon($item:Item)
{
    $item.Description = $"The {Label($item)} is destroyed!";
    Message($item.Description);

    # Anything contained by the destroyed item is now outside of it.
    foreach (var $inner)
    {
        if ($inner.Location == $item)
        {
            $inner.Location = $item.Location;
        }
    }

    # The destroyed item can't do anything, so clear all its action properties.
    $item.TakeAction = null;
    $item.DropAction = null;
    $item.UseAction = null;
    $item.OpenAction = null;
    $item.CloseAction = null;
    $item.DescribeAction = null;
    $item.DescribeHealthAction = null;
    $item.DestroyAction = null;
    $item.TurnOnAction = null;
    $item.TurnOffAction = null;
    $item.IgniteAction = null;
    $item.PutOutAction = null;

    # Set various "state" properties to "None".
    $item.DoorState = DoorState.None;
    $item.LightState = LightState.None;
}
```

## DescribePlayerHealth and DestroyPlayer Functions

The `DescribePlayerHealth` function provides an immplementation of the
`DescribeHealthAction` property for the player item.

The `DestroyPlayer` function provides an implementation of the `DestroyAction`
property for the player item.

```text
function DescribePlayerHealth($item:Item)
{
    if ($item.MaxHealth != 0)
    {
        var $health = $item.Health;
        var $percentage = $health * 100 / $item.MaxHealth;
        if ($health < 0)
        {
            Message("You are dead.");
        }
        elseif ($percentage < 20)
        {
            Message("You are critically injured.");
        }
        elseif ($percentage < 40)
        {
            Message("You are severaly injured.");
        }
        elseif ($percentage < 60)
        {
            Message("You are significantly injured.");
        }
        elseif ($percentage < 80)
        {
            Message("You are slightly injured.");
        }
    }
}

function DestroyPlayer($item:Item)
{
    Message("You have sustained fatal injuries. The game is over. Better luck next time!");
    EndGame(false);
}

game
{
    SetHealth(player, 99, 99);
    player.DescribeHealthAction = DescribePlayerHealth;
    player.DestroyAction = DestroyPlayer;
}

turn
{
    # The player heals one health unit per turn.
    if (player.Health < player.MaxHealth)
    {
        player.Health = player.Health + 1;
    }
}
```

## DescribeHealth Function

The `DescribeHealth` function invokes the describe health action for the specified
item, falling back to the `DescribeHealthCommon` function if the `DescribeHealthAction`
property has not been set.

```text
function DescribeHealth($item:Item) => InvokeItemActionWithFallback($item.DescribeHealthAction, $item, DescribeHealthCommon);
```

## Destroy Function

The `Destroy` function invokes the destroy health action for the specified item,
falling back to the `DestroyCommon` function if the `DestroyAction` property has not
been set.

```text
function Destroy($item:Item) => InvokeItemActionWithFallback($item.DestroyAction, $item, DestroyCommon);
```

## DescribeCommon Function

The `DescribeCommon` function implements the default behavior of the `Describe`
function if the `DescribeAction` property has not been set. Item-specific describe
actions can also call `DescribeCommon` to output basic information.

```text
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
        Message($"You see a {LabelWithState($item)}.");
    }

    DescribeHealth($item);
}
```

## SetPortable Function

The `SetPortable` function makes an item portable by setting its `TakeAction` and
`DropAction` properties.

The `IsPortable` function tests whether an item is portable.

```text
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
function SetPortable($item:Item)
{
    $item.TakeAction = TakePortableItem;
    $item.DropAction = DropPortableItem;
}
function IsPortable($item:Item) => $item.TakeAction == TakePortableItem;
```

## Inventory Function

The `Inventory` function lists items in the player's inventory. The associated commands
are `inventory` and `i`.

```text
function Inventory()
{
    var $haveItems = false;
    foreach (var $item)
    {
        if ($item.Location == player)
        {
            Message(LabelWithState($item));
            $haveItems = true;
        }
    }
    if (!$haveItems)
    {
        Message("There is nothing in your inventory.");
    }
}

command "inventory" { Inventory(); }
command "i" { Inventory(); }
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
function UseOn($item:Item, $target:Item) => InvokeItem2Action($item.UseOnAction, $item, $target, "use");
function Open($item:Item) => InvokeItemAction($item.OpenAction, $item, "open");
function Close($item:Item) => InvokeItemAction($item.CloseAction, $item, "close");
function TurnOn($item:Item) => InvokeItemAction($item.TurnOnAction, $item, "turn on");
function TurnOff($item:Item) => InvokeItemAction($item.TurnOffAction, $item, "turn off");
function PutOut($item:Item) => InvokeItemAction($item.PutOutAction, $item, "put out");
function Describe($item:Item) => InvokeItemActionWithFallback($item.DescribeAction, $item, DescribeCommon);

# Action commands
command "take {$item:Item}" { Take($item); }
command "drop {$item:Item}" { Take($item); }
command "use {$item:Item} on {$target:Item}" { UseOn($item, $target); }
command "use {$item:Item}" { Use($item); }
command "open {$item:Item}" { Open($item); }
command "close {$item:Item}" { Close($item); }
command "turn on {$item:Item}" { TurnOn($item); }
command "turn {$item:Item} on" { TurnOn($item); }
command "turn off {$item:Item}" { TurnOff($item); }
command "turn {$item:Item} off" { TurnOff($item); }
command "put out {$item:Item}" { PutOut($item); }
command "put {$item:Item} out" { PutOut($item); }
command "look at {$item:Item}" { Describe($item); }
command "look {$item:Item}" { Describe($item); }
```

## IsClosedOrLocked

Returns true if a `DoorState` value is either closed or locked.

```text
map IsClosedOrLocked DoorState -> Bool {
    None -> false,
    Open -> false,
    Closed -> true,
    Locked -> true
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

## Container Functions

The `InitializeContainer` function initialize the properties of a container item such
as a box or chest.

The `NewContainer` function creates and initializes a new container item.

The `IsContainer` function tests whether the specified item is a container.

The `IsOpenContainer` function tests whether the specified item is a container that is
not closed or locked.

The `PutInContainer` function puts an item in a container.

The "put (item) in (item)" command invokes the `PutInContainer` function.

```text
function IsContainerEmpty($container:Item) : Bool
{
    $return = true;
    foreach (var $item)
    {
        if ($item.Location == $container)
        {
            $return = false;
        }
    }
}

function ListContents($container:Item)
{
    if (IsContainerEmpty($container))
    {
        Message($"The {$container.Noun} is empty.");
    }
    else
    {
        Message($"Inside the {$container.Noun} are the following:");
        foreach (var $item)
        {
            if ($item.Location == $container)
            {
                Message($" - A {LabelWithState($item)}.");
            }
        }
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
            ListContents($item);
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
        ListContents($container);
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
    $container.OpenAction = OpenContainer;
    $container.CloseAction = CloseDoor;
    $container.DescribeAction = DescribeContainer;
    $container.Location = $loc;
}

function NewContainer($adjectives:String, $noun:String, $state:DoorState, $loc:Item) : Item
{
    var $container = NewItem($"_{$noun}_{$loc}");
    InitializeContainer($container, $adjectives, $noun, $state, $loc);
    $return = $container;    
}

function IsContainer($item:Item) => $item.DescribeAction == DescribeContainer;

function IsOpenContainer($item:Item) =>
    IsContainer($item) &&
    !IsClosedOrLocked($item.DoorState);

function PutInContainer($item:Item, $container:Item)
{
    if (!IsPortable($item))
    {
        Message($"You can't move the {Label($item)}.");
    }
    elseif (!IsContainer($container))
    {
        Message($"The {Label($container)} is not a container.");
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

command "put {$item:Item} in {$container:Item}" { PutInContainer($item, $container); }
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

## GetLink and SetLink Functions

The `GetLink` and `SetLink` functions get and set the link property associated with
the specified direction.

```text
function GetLink($item:Item, $dir:Direction) : Item
{
    switch ($dir)
    {
        case Direction.North { $return = $item.LinkN; }
        case Direction.South { $return = $item.LinkS; }
        case Direction.East  { $return = $item.LinkE; }
        case Direction.West  { $return = $item.LinkW; }
        case Direction.Up    { $return = $item.LinkU; }
        case Direction.Down  { $return = $item.LinkD; }
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

## Door Functions

This section defines functions for linking rooms together with _door items_. Door items
may be actual doors, which can be opened or closed, or mere openings.

| Function              | Description                                                   |
|-----------------------|---------------------------------------------------------------|
| `InitializeDoor`      | Sets properties on a door item.                               |
| `LinkRoomsOneWay`     | Creates a one-way link from one room to another via a door.   |
| `LinkRooms`           | Links two rooms via a specified door item.                    |
| `NewDoorItem`         | Links two rooms with a new door item.                         |
| `NewClosedDoor`       | Links two rooms with a new door in the Closed state.          |
| `NewOpening`          | Links two rooms with a new opening.                           |

```text
function InitializeDoor($door:Item, $adjectives:String, $noun:String, $state:DoorState)
{
    SetLabelProperties($door, $adjectives, $noun);
    $door.DoorState = $state;

    if ($state != DoorState.None)
    {
        $door.OpenAction = OpenDoor;
        $door.CloseAction = CloseDoor;
    }
}
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
function NewDoorItem($from:Item, $to:Item, $dir:Direction, $adjectives:String, $noun:String, $state:DoorState) : Item
{
    var $door = NewItem($"_{$noun}_{$from}{$to}");
    InitializeDoor($door, $adjectives, $noun, $state);
    LinkRooms($from, $to, $door, $dir);
    $return = $door;
}
function NewClosedDoor($from:Item, $to:Item, $dir:Direction) : Item
{
    $return = NewDoorItem($from, $to, $dir, "", "door", DoorState.Closed);
}
function NewOpening($from:Item, $to:Item, $dir:Direction) : Item
{
    $return = NewDoorItem($from, $to, $dir, "", "opening", DoorState.None);
}
```

## Key Functions

The `InitializeKey` function initialize the properties of a key item.

The `NewKey` function creates and initializes a new key item.

The `IsKey` function tests whether a specified item is a key.

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
function InitializeKey($key:Item, $adjectives:String, $noun:String, $loc:Item)
{
    SetLabelProperties($key, $adjectives, $noun);
    SetPortable($key);
    $key.UseOnAction = UseKeyOn;
    $key.Location = $loc;
}
function NewKey($adjectives:String, $noun:String, $loc:Item) : Item
{
    var $key = NewItem($"_{$noun}_{$loc}");
    InitializeKey($key, $adjectives, $noun, $loc);
    $return = $key;
}
function IsKey($item:Item) => $item.UseOnAction == UseKeyOn;
```

## Weapon Functions

The `InitializeWeapon` function sets the properties of a weapon item. The
`NewWeapon` function creates and initializes a new weapon item.

```text
function InflictDamage($target:Item, $damage:Int)
{
    if ($target.Health < 0)
    {
        Message($"The {Label($target)} is already destroyed.");
    }
    elseif ($target.MaxHealth != 0 && $damage != 0)
    {
        $target.Health = $target.Health - $damage;
        if ($target.Health >= 0)
        {
            Message($"The {Label($target)} takes some damage.");
            DescribeHealth($target);
        }
        else
        {
            Destroy($target);
        }
    }
    else
    {
        Message("That has no effect.");
    }
}

function UseWeaponOn($item:Item, $target:Item)
{
    InflictDamage($target, $item.WeaponDamage);
}

function InitializeWeapon($item:Item, $adjectives:String, $noun:String, $damage:Int, $loc:Item)
{
    SetLabelProperties($item, $adjectives, $noun);
    SetPortable($item);
    $item.UseOnAction = UseWeaponOn;
    $item.WeaponDamage = $damage;
    $item.Location = $loc;
}

function NewWeapon($adjectives:String, $noun:String, $damage:Int, $loc:Item) : Item
{
    var $item = NewItem($"_{$noun}_{$loc}");
    InitializeWeapon($item, $adjectives, $noun, $damage, $loc);
    $return = $item;
}
```

## Light Functions

The functions in this section create or initialize light sources that can be turned on
or off light electric lights.

The `InitializeLight` function sets the properties of a light item.

The `NewLight` function creates and initializes a new light item.

The `IsLight` function tests whether an item is a light.

```text
function TurnOnLight($item:Item)
{
    if ($item.LightState != LightState.On)
    {
        $item.LightState = LightState.On;
        Message($"The {Label($item)} is now on.");
    }
    else
    {
        Message($"The {Label($item)} is already on.");
    }
}
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
function InitializeLight($item:Item, $adjectives:String, $noun:String, $loc:Item)
{
    SetLabelProperties($item, $adjectives, $noun);
    SetPortable($item);
    $item.LightState = LightState.Off;
    $item.TurnOnAction = TurnOnLight;
    $item.TurnOffAction = TurnOffLight;
    $item.PutOutAction = TurnOffLight;
    $item.Location = $loc;
}
function NewLight($adjectives:String, $noun:String, $loc:Item) : Item
{
    var $item = NewItem($"_{$noun}_{$loc}");
    InitializeLight($item, $adjectives, $noun, $loc);
    $return = $item;
}
function IsLight($item:Item) => $item.TurnOnAction == TurnOnLight;
```

## Candle Functions

The functions in this section create or initialize light sources that behave like
candles. This includes oil lamps, torches, or any other light source that must be
"lit" rather than merely turned on.

The `InitializeCandle` function sets the properties of a candle item.

The `NewCandle` function creates and initializes a new candle item.

The `IsCandle` function tests whether an item is a candle.

```text
function TurnOnCandle($item:Item)
{
    Message($"You can't turn on the {Label($item)}. You need to light it with something.");
}
function IgniteCandle($item:Item)
{
    if ($item.LightState != LightState.Lit)
    {
        $item.LightState = LightState.Lit;
        Message($"The {Label($item)} is now lit.");
    }
    else
    {
        Message($"The {Label($item)} is already lit.");
    }
}
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
function InitializeCandle($item:Item, $adjectives:String, $noun:String, $loc:Item)
{
    SetLabelProperties($item, $adjectives, $noun);
    SetPortable($item);
    $item.LightState = LightState.Unlit;
    $item.TurnOnAction = TurnOnCandle;  # displays error message
    $item.IgniteAction = IgniteCandle;  # sets to LightState.Lit
    $item.TurnOffAction = PutOutCandle; # sets to LightState.Unlit
    $item.PutOutAction = PutOutCandle;  # sets to LightState.Unlit
    $item.Location = $loc;
}
function NewCandle($adjectives:String, $noun:String, $loc:Item) : Item
{
    var $item = NewItem($"_{$noun}_{$loc}");
    InitializeCandle($item, $adjectives, $noun, $loc);
    $return = $item;
}
function IsCandle($item:Item) => $item.TurnOnAction == TurnOnCandle;
```

## IsDark Predicates

The functions in this section can be specified as the `IsDark` property of a room

- `IsDarkAtNight` returns true if it is night.
- `IsDarkAlways` returns true unconditionally.

```text
function IsDarkAtNight($item:Item) => $currentTime < 8*60 || $currentTime >= 20*60;
function IsDarkAlways($item:Item) => true;
```

## Lighter Functions

The functions in this section create or initialize _lighter items_, which can be used
to invoke the ignite action on other items.

The `InitializeLighter` function sets the properties of a lighter item.

The `NewLighter` function creates and initializes a new lighter item.

The `IsLighter` function tests whether an item is a lighter.

```text
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
function InitializeLighter($item:Item, $adjectives:String, $noun:String, $loc:Item)
{
    SetLabelProperties($item, $adjectives, $noun);
    SetPortable($item);
    $item.UseOnAction = UseLighterOn;
    $item.Location = $loc;
}
function NewLighter($adjectives:String, $noun:String, $loc:Item) : Item
{
    var $item = NewItem($"_{$noun}_{$loc}");
    InitializeLighter($item, $adjectives, $noun, $loc);
    $return = $item;
}
function IsLighter($item:Item) => $item.UseOnAction == UseLighterOn;

command "light {$target:Item} with {$lighter:Item}"
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

## IsAccessible Function

The `IsAccessible` function tests whether an item is in the current room, in an
open container in the current room, or in the player's inventory.

```text
function IsCurrentRoomOrInventory($loc:Item) =>
    $loc == player.Location ||
    $loc == player;

function IsAccessible($item:Item) : Bool
{
    if (!$item.IsHidden && $item.Noun != null)
    {
        var $loc = $item.Location;
        if (IsCurrentRoomOrInventory($loc))
        {
            # Item is in the current room or player inventory.
            $return = true;
        }
        elseif (IsOpenContainer($loc) && $loc.Location == player.Location)
        {
            # Item is in an open container in the current room.
            $return = true;
        }
    }
}
```

## InitializeLighting Function

The `InitializeLighting` function is called during turn initialization to initialize
the `$isNowDark` and `$currentLightSource` global variables.

```text
map IsActiveLightState LightState -> Bool {
    None -> false,
    Off -> false,
    On -> true,
    Unlit -> false,
    Lit -> true
}

function InitializeLighting()
{
    $currentLightSource = null;

    if (player.Location.IsDark(player.Location))
    {
        foreach (var $item)
        {
            if (IsActiveLightState($item.LightState) && IsAccessible($item))
            {
                $currentLightSource = $item;
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

## InitializeWordMap Function

The `InitializeWordMap` function is called at the beginning of each turn to add nouns
and adjectives for accessible items.

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
        foreach (var $item)
        {
            if ($item.Location == player)
            {
                AddItemWords($item);
            }
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
            Message($"The {Label($currentLightSource)} illuminates the dark room.");
        }

        Describe($room);

        foreach (var $dir:Direction)
        {
            var $door = GetLink(player.Location, $dir);
            if ($door != null)
            {
                Message($"There is a {LabelWithState($door)} {DirectionPhrase($dir)}.");
            }
        }

        foreach (var $item) where Location == $room
        {
            if (!$item.IsHidden && $item.Noun != null)
            {
                Message($"There is a {LabelWithState($item)} here.");
            }
        }
    }
}

command "look" { Look(); }
```

## Navigation Functions

The `Go` function navigates in the specified direction.

```text
function Go($dir:Direction)
{
    var $source = player.Location;
    var $door = GetLink($source, $dir);
    if ($door == null)
    {
        Message($"You cannot go {$dir}.");
    }
    elseif (IsClosedOrLocked($door.DoorState))
    {
        Message($"The {Label($door)} is closed.");
    }
    else
    {
        var $dest = GetLink($door, $dir);
        if (!$source.LeaveAction($source, $dest) &&
            !$dest.EnterAction($source, $dest))
        {
            player.Location = $dest;
            InitializeLighting();
            Look();
        }
    }
}

command "go {$dir:Direction}" { Go($dir); }
command "n" { Go(Direction.North); }
command "s" { Go(Direction.South); }
command "e" { Go(Direction.East); }
command "w" { Go(Direction.West); }
command "up" { Go(Direction.Up); }
command "down" { Go(Direction.Down); }
```

## Turn Initializeation

```text
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
