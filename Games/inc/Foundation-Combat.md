# AdventureScript Foundation Library - Combat Module

This module implements functionality related to weapons and damage. It depends
on the `Foundation.md` module, which must be included first.

## CurrentWeapon Property

This property specifies the current weapon (if any) of the player or non-player
character.

```text
property CurrentWeapon : Item;
```

## AttackDamage Property

The `AttackDamage` property specifies the nominal amount of damage inflicted
by a weapon or monster. This may be adjusted by the damage resistence of the
thing being attacked.

```text
property AttackDamage : Int;
```

## DamageResistance Property

The `DamageResistance` property specifies how much the target of an attack
resists damage due to armor or natural durability. Valid values range from
0 to 100, where 100 is complete invincibility.

Damage resistence yields both an absolute reduction in damage (so weak attacks
might have no effect) and a percentage reduction (so damage from strong attacks
is reduced proportionally).

```text
property DamageResistance : Int;
```

## ComputeDamage Function

The `ComputeDamage` function computes the actual damage (health reduction) based
on the `AttackDamange` of the attacker or weapon and the `DamageResistance` of
the target.

```text
function ComputeDamage($attackDamage:Int, $DamageResistance:Int) : Int
{
    # Compute absolute reduction.
    var $damage = ($attackDamage - $DamageResistance);

    # Compute percentage reduction.
    $damage = $damage - ($damage * $DamageResistance / 100);

    $return = $damage > 0 ? $damage : 0;
}
```

## ArmorKind Property

The `ArmorKind` property specifies where a piece of armor is worn (i.e., head,
torso, or legs). Only one piece of armor of each kind may be worn. The default
value is None.

```text
# Enum and propery definition
enum ArmorKind(None, Head, Torso, Leg);
property ArmorKind : ArmorKind;
```

## Armor Helper Functions

This section contains internal helper methods related to armor.

```text
# Current armor of each kind for the player.
var $headArmor : Item;
var $torsoArmor : Item;
var $legArmor : Item;

# Get the current armor of the specified kind.
function GetArmor($kind:ArmorKind) : Item
{
    switch ($kind)
    {
        case ArmorKind.Head { $return = $headArmor; }
        case ArmorKind.Torso { $return = $torsoArmor; }
        case ArmorKind.Leg { $return = $legArmor; }
    }
}

# Set the current armor of the specified kind.
function SetArmor($kind:ArmorKind, $item:Item)
{
    switch ($kind)
    {
        case ArmorKind.Head { $headArmor = $item; }
        case ArmorKind.Torso { $torsoArmor = $item; }
        case ArmorKind.Leg { $legArmor = $item; }
    }
}

# Compute the player's damage resistence as a weighted average of
# the damage resistence for each piece of armor.
function SetPlayerDamageResistance()
{
    player.DamageResistance = (
        $headArmor.DamageResistance * 30 + 
        $torsoArmor.DamageResistance * 50 +
        $legArmor.DamageResistance * 20
        ) / 100;
}

function TakeArmor($item:Item)
{
    var $kind = $item.ArmorKind;
    if ($kind  != ArmorKind.None)
    {
        var $current = GetArmor($kind);
        if ($current == $item)
        {
            Message($"You're already wearing the {Label($item)}.");
        }
        else
        {
            if ($current != null)
            {
                Message($"You drop the {Label($current)} and put on the {Label($item)}.");
                $current.Location = player.Location;
            }
            else
            {
                Message($"You put on the {Label($item)}.");
            }

            $item.Location = player;
            SetArmor($kind, $item);
            SetPlayerDamageResistance();
        }
    }
    else
    {
        # Not armor...
        Message($"You can't take the {Label($item)}.");
    }
}
function DropArmor($item:Item)
{
    if ($item.Location == player)
    {
        Message($"You remove the {Label($item)}.");
        $item.Location = player.Location;
        SetArmor($item.ArmorKind, null);
        SetPlayerDamageResistance();
    }
    else
    {
        Message($"You're not wearing the {Label($item)}.");
    }
}
```

## InitializeArmor and NewArmor Functions

The `InitializeArmor` function sets the properties of an armor item. The
`NewArmor` function creates and initializes a new armor item.

```text
function InitializeArmor(
    $item:Item,             # item to initialize
    $adjectives:String,     # E.g., "leather"
    $noun:String,           # E.g., "vest" or "cap"
    $kind:ArmorKind,        # Head, Torso, or Leg
    $DamageResistance:Int,  # 1-100
    $loc:Item               # initial location
    )
{
    SetLabelProperties($item, $adjectives, $noun);
    $item.ArmorKind = $kind;
    $item.Location = $loc;
    $item.TakeAction = TakeArmor;
    $item.UseAction = TakeArmor;
    $item.DropAction = DropArmor;
}

function NewArmor(
    $adjectives:String,     # E.g., "leather"
    $noun:String,           # E.g., "vest" or "cap"
    $kind:ArmorKind,        # Head, Torso, or Leg
    $DamageResistance:Int,  # 1-100
    $loc:Item               # initial location
    ) : Item
{
    $return = NewItem($"_{$loc}_{$noun}");
    InitializeArmor($return, $adjectives, $noun, $kind, $DamageResistance, $loc);
}
```

## Damage Helper Functions

This section contains internal helper methods for inflicting damage on and/or
destroying items.

```text
# Default implementation of the Destroy function if DestroyAction is not set.
function DestroyCommon($item:Item)
{
    $item.Description = $"The {Label($item)} is destroyed!";
    Message($item.Description);

    # Anything contained by the destroyed item is now outside of it.
    foreach (var $inner) where Location == $item
    {
        $inner.Location = $item.Location;
    }

    # Set the item's location to null, so in effect it doesn't exist.
    $item.Location = null;
}

# Destroy action for the player item (ends the game).
function DestroyPlayer($item:Item)
{
    Message("You have sustained fatal injuries. The game is over. Better luck next time!");
    EndGame(false);
}

game
{
    player.DestroyAction = DestroyPlayer;
}

# The player heals one health unit per turn.
turn
{
    if (player.Health < player.MaxHealth)
    {
        player.Health = player.Health + 1;
    }
}

# The Destroy function invokes the DestroyAction or calls DestroyCommon.
function Destroy($item:Item) => InvokeItemActionWithFallback($item.DestroyAction, $item, DestroyCommon);

# Function invoked when the player attacks something.
function AttackItemWith($target:Item, $weapon:Item)
{
    if ($target.MaxHealth == 0)
    {
        # You can't attack an item that does not have health.
        Message($"You can't attack the {Label($target)}.");
    }
    elseif ($weapon != null && $weapon.AttackDamage == 0)
    {
        Message($"The {Label($weapon)} is not a weapon.");
    }
    else
    {
        var $weaponName = $weapon != null ? $"the {Label($weapon)}" : "your bare hands";
        var $weaponDamage = $weapon != null ? $weapon.AttackDamage : player.AttackDamage;
        var $damage = ComputeDamage($weaponDamage, $target.DamageResistance);
        if ($damage <= 0)
        {
            Message($"You attack the {Label($target)} with {$weaponName} but do no damage.");
        }
        else
        {
            Message($"You attack the {Label($target)} with {$weaponName}.");

            $target.Health = $target.Health - $damage;
            if ($target.Health < 0)
            {
                Destroy($target);
            }
            else
            {
                DescribeHealth($target);
            }
        }
    }
}

# Attack commands.
command "attack {$target:Item} with {$weapon:Item}"
{
    AttackItemWith($target, $weapon);
}
command "attack {$target:Item}"
{
    AttackItemWith($target, player.CurrentWeapon);
}

```

## AttackPlayer Function

The `AttackPlayer` function is invoked when a non-player character (e.g.,
monster or enemy) attacks the player.

```text
function AttackPlayer($foe:Item)
{
    var $weapon = $foe.CurrentWeapon;
    var $withPhrase = $weapon != null ? $" with {Label($weapon)}" : "";
    var $weaponDamage = $weapon != null ? $weapon.AttackDamage : $foe.AttackDamage;
    var $damage = ComputeDamage($weaponDamage, player.DamageResistance);

    if ($damage <= 0)
    {
        Message($"The {Label($foe)} attacks you{$withPhrase} but does no damage.");
    }
    else
    {
        Message($"The {Label($foe)} attacks you{$withPhrase}.");

        player.Health = player.Health - $damage;
        if (player.Health < 0)
        {
            Destroy(player);
        }
        else
        {
            DescribeHealth(player);
        }
    }
}
```

## Weapon Helper Functions

This section contains internal helper functions associated with weapon items.

```text
function UseWeapon($item:Item)
{
    if (player.CurrentWeapon == $item)
    {
        Message($"The {Label($item)} is already your current weapon.");
    }
    else
    {
        # Add the weapon to the player's inventory if it isn't already.
        if ($item.Location != player)
        {
            $item.Location = player;
            Message($"The {Label($item)} is now in your inventory.");
        }

        # Make it the current weapon.
        player.CurrentWeapon = $item;
        Message($"The {Label($item)} is now your current weapon.");
    }
}

function TakeWeapon($item:Item)
{
    TakePortableItem($item);

    if (player.CurrentWeapon == null)
    {
        # Make it the current weapon as a convenience.
        player.CurrentWeapon = $item;
        Message($"The {Label($item)} is now your current weapon.");
    }
    else
    {
        Message($"The {Label(player.CurrentWeapon)} is still your current weapon.");
        Message($"You can 'use {Label($item)}' to change that.");
    }
}

function DropWeapon($item:Item)
{
    if ($item.Location == player)
    {
        $item.Location = player.Location;
        Message($"You've dropped the {Label($item)}.");

        if (player.CurrentWeapon == $item)
        {
            player.CurrentWeapon = null;
            Message("You no longer have a current weapon.");
        }
    }
    else
    {
        Message($"The {Label($item)} is not in your inventory.");
    }
}
```

## InitializeWeapon and NewWeapon Functions

The `InitializeWeapon` function sets the properties of a weapon item. The
`NewWeapon` function creates and initializes a new weapon item.

```text
function InitializeWeapon($item:Item, $adjectives:String, $noun:String, $damage:Int, $loc:Item)
{
    SetLabelProperties($item, $adjectives, $noun);
    $item.UseAction = UseWeapon;
    $item.TakeAction = TakeWeapon;
    $item.DropAction = DropWeapon;
    $item.AttackDamage = $damage;
    $item.Location = $loc;
}

function NewWeapon($adjectives:String, $noun:String, $damage:Int, $loc:Item) : Item
{
    $return = NewItem($"_{$noun}_{$loc}");
    InitializeWeapon($return, $adjectives, $noun, $damage, $loc);
}
```

## Describe Helper Functions

This code in this section overrides the player's DescribeAction to include armor and
weapons information.

```text
function DescribePlayerWithArms($item:Item)
{
    DescribePlayer($item);

    if ($headArmor != null)
    {
        Message($"On your head is a {Label($headArmor)}.");
    }
    if ($torsoArmor != null)
    {
        Message($"You're wearing a {Label($torsoArmor)}.");
    }
    if ($legArmor != null)
    {
        Message($"On your legs are {Label($legArmor)}.");
    }
    if (player.CurrentWeapon != null)
    {
        Message($"You wield a {Label(player.CurrentWeapon)}.");
    }
}
game
{
    player.DescribeAction = DescribePlayerWithArms;
}

```
