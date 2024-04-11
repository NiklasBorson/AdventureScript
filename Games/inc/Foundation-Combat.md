# AdventureScript Foundation Library - Combat Module

This optional module implements a basic combat system, including armor, weapons,
monsters, and the "attack" command. It depends on `Foundation.md`, which must be
included first.

## UpdateAction Property

The `UpdateAction` delegate is invoked each turn. This is used to implement
behavior for "active" items like monsters and non-player characters.

```text
## Delegate invoked each turn for each item. Can be used to implement
## behavior for monsters, non-player characters, etc.
## @UpdateItems,UpdatePlayer,UpdateSurprisedMonster,UpdateHostileMonster,UpdateFriendlyMonster
property UpdateAction : ItemDelegate;

## Called each turn to invoke the UpdateAction for each item.
## @UpdateAction
function UpdateItems()
{
    foreach (var $item)
    {
        $item.UpdateAction($item);
    }
}

turn
{
    UpdateItems();
}
```

## OnAttackedAction Property

The `OnAttackedAction` is invoked on the target of an attack, giving the target
a chance to respond, e.g., by changing its behavior.

```text
## Delegate invoked on an item when the item is attacked. For example, a monster
## or non-player character might change its behavior (e.g., become more hostile)
## when this action is invoked. Changing behavior can be implemented by setting
## the UpdateAction to a different function.
## @OnMonsterAttacked,OnPlayerAttacked
property OnAttackedAction : ItemDelegate;
```

## Health and MaxHealth Properties

The `Health` and `MaxHealth` properties are used to keep track of the amount of
injury or damage to a character or object. Damage reduces the `Health` property
by specified amount. The _relative health_ of a character or object is the ratio
of `Health` to `MaxHealth`. If both properties are zero, the item is not subject
to damage.

```text
## Health of a player, non-player character, or other item.
## @SetItemHealth,MaxHealth
property Health : Int;

## Maximum health of an item. The relative health is the ratio of Health to MaxHealth.
## @SetItemHealth,Health
property MaxHealth : Int;
```

## AttackDamage Property

The `AttackDamage` property specifies the nominal amount of damage inflicted
by a weapon or monster. This may be adjusted by the damage resistence of the
thing being attacked.

```text
## Specifies the nominal amount of damage inflicted by a weapon or monster.
## This may be adjusted by the damage resistence of the thing being attacked.
## @DamageResistance,ComputeDamage
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
## Specifies how much the target of an attack resists damage. The ComputeDamage
## function uses AttackDamage of the attacker and the DamageResistance of the
## target to compute the actual damage.
## @AttackDamage,ComputeDamage
property DamageResistance : Int;
```

## ComputeDamage Function

The `ComputeDamage` function computes the actual damage (health reduction) based
on the `AttackDamange` of the attacker or weapon and the `DamageResistance` of
the target.

```text
## Computes the actual damage inflicted by an attack.
## $attackDamage: AttackDamage property of the attacker or weapon.
## $damageResistance: DamageResistance property of the target.
## $return: Returns the actual damage inflicted.
## @AttackDamage,DamageResistance
function ComputeDamage($attackDamage:Int, $damageResistance:Int) : Int
{
    # Compute absolute reduction.
    var $damage = ($attackDamage - $damageResistance);

    # Compute percentage reduction.
    $damage = $damage - ($damage * $damageResistance / 100);

    $return = $damage > 0 ? $damage : 0;
}
```

## Health Helpers

This section contains internal helper functions used to implement the
DescribeHealthAction delegate.

```text
## DescribeHealthAction implementation suitable for inaninate objects.
## $item: Item to describe the health of.
## @DescribeHealthAction,DescribeCreatureHealth,DescribePlayerHealth
function DescribeItemHealth($item:Item)
{
    if ($item.MaxHealth > 0)
    {
        var $health = $item.Health;
        var $percentage = $health * 100 / $item.MaxHealth;
        var $label = Label($item);
        if ($health < 0)
        {
            Message($"The {$label} is destroyed!");
        }
        elseif ($percentage < 20)
        {
            Message($"The {$label} is critically damanged.");
        }
        elseif ($percentage < 40)
        {
            Message($"The {$label} is severely damanged.");
        }
        elseif ($percentage < 70)
        {
            Message($"The {$label} is significantly damanged.");
        }
        elseif ($percentage < 100)
        {
            Message($"The {$label} is slightly damaged.");
        }
        else
        {
            Message($"The {$label} is undamaged.");
        }
    }
}

## DescribeHealthAction implementation suitable for monsters, NPCs, etc.
## $item: Item to describe the health of.
## @DescribeHealthAction,DescribeItemHealth,DescribePlayerHealth
function DescribeCreatureHealth($item:Item)
{
    if ($item.MaxHealth > 0)
    {
        var $health = $item.Health;
        var $percentage = $health * 100 / $item.MaxHealth;
        var $label = Label($item);
        if ($health < 0)
        {
            Message($"The {$label} is dead!");
        }
        elseif ($percentage < 20)
        {
            Message($"The {$label} is critically injured.");
        }
        elseif ($percentage < 40)
        {
            Message($"The {$label} is severely injured.");
        }
        elseif ($percentage < 70)
        {
            Message($"The {$label} is significantly injured.");
        }
        elseif ($percentage < 100)
        {
            Message($"The {$label} is slightly injured.");
        }
        else
        {
            Message($"The {$label} is uninjured.");
        }
    }
}

## DescribeHealthAction implementation for the player.
## $item: Item to describe the health of.
## @DescribeHealthAction,DescribeItemHealth,DescribeCreatureHealth
function DescribePlayerHealth($item:Item)
{
    if ($item.MaxHealth != 0)
    {
        var $health = $item.Health;
        var $percentage = $health * 100 / $item.MaxHealth;
        if ($health < 0)
        {
            Message("You are dead!");
        }
        elseif ($percentage < 20)
        {
            Message("You are critically injured.");
        }
        elseif ($percentage < 40)
        {
            Message("You are severely injured.");
        }
        elseif ($percentage < 70)
        {
            Message("You are significantly injured.");
        }
        elseif ($percentage < 100)
        {
            Message("You are slightly injured.");
        }
        else
        {
            Message("You are uninjured.");
        }
    }
}

## The UpdateAction for the player is to heal one health unit.
## $item: Item the action is invoked on.
## @UpdateAction,Health,MaxHealth
function UpdatePlayer($item:Item)
{
    if ($item.Health < $item.MaxHealth)
    {
        $item.Health = $item.Health + 1;
    }
}

game
{
    # Initialize the player's health and related properties.
    player.MaxHealth = 250;
    player.Health = 250;
    player.AttackDamage = 5;  # damage using bare hands
    player.DescribeHealthAction = DescribePlayerHealth;
    player.DescribeAction = DescribePlayerHealth;
    player.UpdateAction = UpdatePlayer;
}
```

## SetItemHealth Function

The `SetItemHealth` function sets an inanimate item's `Health`, `MaxHealth`, and
`DescribeHealthAction` properties.

```text
## Initializes health-related properties of an inanimate item.
## $item: Item to initialize.
## $health: Initial value of the Health and MaxHealth properties.
## @InitializeMonster,NewMonster
function SetItemHealth($item:Item, $health:Int)
{
    $item.Health = $health;
    $item.MaxHealth = $health;
    $item.DescribeHealthAction = DescribeItemHealth;
}
```

## CurrentWeapon Property

This property specifies the current weapon (if any) of the player or non-player
character.

```text
## Specifies the current weapon of the player, which is the default weapon
## used by the "attack" command.
## @InitializeWeapon,NewWeapon
property CurrentWeapon : Item;
```

## ArmorKind Property

The `ArmorKind` property specifies where a piece of armor is worn (i.e., head,
torso, or legs). Only one piece of armor of each kind may be worn. The default
value is None.

```text
## Identifies a type of armor. The player can wear one piece of armor of each type.
## None: Default value for non-armor items.
## Head: Head armor, such as a helmet.
## Torso: Body armor, such as a mail shirt.
## Leg: Leg armor.
## @ArmorKind,GetArmor,SetArmor,InitializeArmor,NewArmor
enum ArmorKind(None, Head, Torso, Leg);

## Property specifying what type of armor an item is.
## @ArmorKind,GetArmor,SetArmor,InitializeArmor,NewArmor
property ArmorKind : ArmorKind;
```

## Armor Helper Functions

This section contains internal helper methods related to armor.

```text
## Current head armor for the player.
## @ArmorKind
var $headArmor : Item;

## Current torso armor for the player.
## @ArmorKind
var $torsoArmor : Item;

## Current leg armor for the player.
## @ArmorKind
var $legArmor : Item;

## Get the current armor of the specified kind.
## $kind: Kind of armor to get.
## $return: Returns the player's current armor of the specified kind.
function GetArmor($kind:ArmorKind) : Item
{
    switch ($kind)
    {
        case ArmorKind.Head { $return = $headArmor; }
        case ArmorKind.Torso { $return = $torsoArmor; }
        case ArmorKind.Leg { $return = $legArmor; }
    }
}

## Set the current armor of the specified kind.
## $kind: Kind of armor to set.
## $item: Item to set, which can be null.
function SetArmor($kind:ArmorKind, $item:Item)
{
    switch ($kind)
    {
        case ArmorKind.Head { $headArmor = $item; }
        case ArmorKind.Torso { $torsoArmor = $item; }
        case ArmorKind.Leg { $legArmor = $item; }
    }
}

## Sets the player's DamageResistance to a weighted average of
## the damage resistence for each piece of armor.
## @ArmorKind,InitializeArmor,NewArmor
function SetPlayerDamageResistance()
{
    player.DamageResistance = (
        $headArmor.DamageResistance * 30 + 
        $torsoArmor.DamageResistance * 50 +
        $legArmor.DamageResistance * 20
        ) / 100;
}

## TakeAction implementation for armor items.
## $item: Item the action is invoked on.
## @TakeAction,InitializeArmor,NewArmor
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

## DropAction implementation for armor items.
## $item: Item the action is invoked on.
## @DropAction,InitializeArmor,NewArmor
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
## Initializes the properties of an armor item.
## $item: Item to initialize.
## $adjectives: Space-separated adjectives used to refer to the item.
## $noun: Noun used to refer to the item.
## $kind: Value of the ArmorKind property.
## $damageResistance: DamanageResistance conferred by the armor.
## $loc: Initial location of the item, such as a room or container.
## @ArmorKind,NewArmor
function InitializeArmor(
    $item:Item,             # item to initialize
    $adjectives:String,     # E.g., "leather"
    $noun:String,           # E.g., "vest" or "cap"
    $kind:ArmorKind,        # Head, Torso, or Leg
    $damageResistance:Int,  # 1-100
    $loc:Item               # initial location
    )
{
    SetLabelProperties($item, $adjectives, $noun);
    $item.ArmorKind = $kind;
    $item.DamageResistance = $damageResistance;
    $item.Location = $loc;
    $item.TakeAction = TakeArmor;
    $item.UseAction = TakeArmor;
    $item.DropAction = DropArmor;
}

## Creates and initializes an armor item.
## $adjectives: Space-separated adjectives used to refer to the item.
## $noun: Noun used to refer to the item.
## $kind: Value of the ArmorKind property.
## $damageResistance: DamanageResistance conferred by the armor.
## $loc: Initial location of the item, such as a room or container.
## $return: Returns the newly-created item.
## @ArmorKind,InitializeArmor
function NewArmor(
    $adjectives:String,     # E.g., "leather"
    $noun:String,           # E.g., "vest" or "cap"
    $kind:ArmorKind,        # Head, Torso, or Leg
    $damageResistance:Int,  # 1-100
    $loc:Item               # initial location
    ) : Item
{
    $return = NewItem($"_{$loc}_{$noun}");
    InitializeArmor($return, $adjectives, $noun, $kind, $damageResistance, $loc);
}
```

## Damage Helper Functions

This section contains internal helper methods for inflicting damage on and/or
destroying items.

```text
## Function invoked when an item is destroyed.
## $item: Item to destroy.
function DestroyItem($item:Item)
{
    # Anything contained by the destroyed item is now outside of it.
    foreach (var $inner) where Location == $item
    {
        $inner.Location = $item.Location;
    }

    # Set the item's location to null, so in effect it doesn't exist.
    $item.Location = null;
}

## Function invoked when the player attacks something.
## $target: Item being attacked.
## $weapon: Weapon to attack with, which can be null.
## @InitializeWeapon,NewWeapon
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
            $target.DescribeHealthAction($target);
            if ($target.Health < 0)
            {
                DestroyItem($target);
            }
        }

        # Invoke the target's OnAttackedAction delegate.
        if ($target.Health >= 0)
        {
            $target.OnAttackedAction($target);
        }
    }
}
```

## AttackPlayer Function

The `AttackPlayer` function is invoked when a non-player character (e.g.,
monster or enemy) attacks the player.

```text
## Function invoked when a monster or non-player character attacks the player.
## $foe: Item attacking the player.
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
        player.DescribeHealthAction(player);
        if (player.Health < 0)
        {
            Message("Better luck next time!");
            EndGame(false);
        }
    }
}
```

## Weapon Helper Functions

This section contains internal helper functions associated with weapon items.

```text
## UseAction for a weapon item. This selects the weapon as the current weapon.
## $item: Item the action is invoked on.
## @UseAction,InitializeWeapon,NewWeapon
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

## TakeAction for a weapon item.
## $item: Item the action is invoked on.
## @TakeAction,InitializeWeapon,NewWeapon
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

## DropAction for a weapon item.
## $item: Item the action is invoked on.
## @DropAction,InitializeWeapon,NewWeapon
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
## Initializes the properties of a weapon item.
## $item: Item to initialize.
## $adjectives: Space-separated adjectives used to refer to the item.
## $noun: Noun used to refer to the item.
## $damage: AttackDamage property of the weapon.
## $loc: Initial location of the weapon, such as a room or container.
## @NewWeapon
function InitializeWeapon($item:Item, $adjectives:String, $noun:String, $damage:Int, $loc:Item)
{
    SetLabelProperties($item, $adjectives, $noun);
    $item.UseAction = UseWeapon;
    $item.TakeAction = TakeWeapon;
    $item.DropAction = DropWeapon;
    $item.AttackDamage = $damage;
    $item.Location = $loc;
}

## Creates and initializes a weapon item.
## $adjectives: Space-separated adjectives used to refer to the item.
## $noun: Noun used to refer to the item.
## $damage: AttackDamage property of the weapon.
## $loc: Initial location of the weapon, such as a room or container.
## $return: Returns the newly-created item.
## @InitializeWeapon
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
## DescribeAction implementation for the player. This implementation in the Combat
## module replaces the general implementation and adds information about weapons
## and armor.
## $item: Item on which the action is invoked (i.e., the player).
## @DescribeAction
function DescribePlayerWithArms($item:Item)
{
    DescribeCommon($item);

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

## TryFollowPayer Function

The `TryFollowPlayer` function is a helper function used to implement behavior
for monsters or NPCs that follow the player.

```text
## Helper function that may be used to implement the UpdateAction for monsters and
## non-player characters. The function moves the monster to the player's location
## if the monster can follow the player.
## $monster: Monster or non-player character.
## $return: Returns true if the monster moved, or false if not.
## @UpdateAction,$lastRoom
function TryFollowPlayer($monster:Item) : Bool
{
    if (!$isNowDark && $monster.Location == $lastRoom && $lastRoom != player.Location)
    {
        $monster.Location = player.Location;
        AddItemWords($monster);
        Message($"The {Label($monster)} follows you.");
        $return = true;
    }
}
```

## UpdateHostileMonster Function

The `UpdateHostileMonster` function implements the update action for a
hostile monster.

```text
## UpdateAction implementation for a monster in a hostile state.
## $monster: Item the action is invoked on.
## @UpdateAction,UpdateSurprisedMonster,UpdateFriendlyMonster
function UpdateHostileMonster($monster:Item)
{
    if (!$isNowDark)
    {
        if ($monster.Location == player.Location)
        {
            AttackPlayer($monster);
        }
        else
        {
            TryFollowPlayer($monster);
        }
    }
}
```

## UpdateSurprisedMonster Function

The `UpdateSurprisedMonster` is the default initial update action for a
potentially hostile monster. A "surprised" monster becomes hostile after
noticing the player.

```text
## UpdateAction implementation for a monster that has not yet noticed the
## player. This is the typical initial state of a monster.
## $monster: Item the action is invoked on.
## @UpdateAction,UpdateHostileMonster,UpdateFriendlyMonster
function UpdateSurprisedMonster($monster:Item)
{
    if ($monster.Location == player.Location && !$isNowDark)
    {
        Message($"The {Label($monster)} notices you.");
        $monster.UpdateAction = UpdateHostileMonster;
    }
}
```

## UpdateFriendlyMonster Function

The `UpdateFriendlyMonster` function implements the update action for a
friendly monster.

```text
## UpdateAction implementation for a friendly monster, the behavior of which
## is to follow the player.
## $monster: Item the action is invoked on.
## @UpdateAction,UpdateSurprisedMonster,UpdateHostileMonster
function UpdateFriendlyMonster($monster:Item)
{
    TryFollowPlayer($monster);
}
```

## OnMonsterAttacked Function

The `OnMonsterAttacked` function implements the OnAttackedAction delegate
for a monster by changing its behavior to that of a hostile moster.

```text
## OnAttackedAction implementation for a monster.
## $monster: Item the action is invoked on.
## @OnAttackedAction,InitializeMonster,NewMonster
function OnMonsterAttacked($monster:Item)
{
    $monster.UpdateAction = UpdateHostileMonster;
}
```

## InitializeMonster and NewMonster Functions

The `InitializeMonster` function sets the properties of a monster item. The
`NewMonster` function creates and initializes a monster item. The default
behavior of monsters is hostile, but this can be changed by setting the
UpdateAction property.

```text
## Initializes the properties of a monster item.
## $monster: Item to initialize.
## $adjectives: Space-separated adjectives used to refer to the item.
## $noun: Noun used to refer to the item.
## $health: Initial Health and MaxHealth of the monster.
## $damageResistance: DamageResistance of the monster.
## $attackDamage: AttackDamage of the monster.
## $loc: Initial location of the monster, such as a room.
## @NewMonster,DamageResistance,AttackDamage
function InitializeMonster(
    $monster : Item,
    $adjectives : String,
    $noun : String,
    $health : Int,
    $damageResistance : Int,
    $attackDamage : Int,
    $loc : Item
)
{
    SetLabelProperties($monster, $adjectives, $noun);
    $monster.Health = $health;
    $monster.MaxHealth = $health;
    $monster.DescribeHealthAction = DescribeCreatureHealth;
    $monster.DamageResistance = $damageResistance;
    $monster.AttackDamage = $attackDamage;
    $monster.Location = $loc;
    $monster.OnAttackedAction = OnMonsterAttacked;
    $monster.UpdateAction = UpdateSurprisedMonster;
}

## Creates and initializes a monster item.
## $adjectives: Space-separated adjectives used to refer to the item.
## $noun: Noun used to refer to the item.
## $health: Initial Health and MaxHealth of the monster.
## $damageResistance: DamageResistance of the monster.
## $attackDamage: AttackDamage of the monster.
## $loc: Initial location of the monster, such as a room.
## $return: Returns the newly-created monster item.
## @InitializeMonster,DamageResistance,AttackDamage
function NewMonster(
    $adjectives : String,
    $noun : String,
    $health : Int,
    $damageResistance : Int,
    $attackDamage : Int,
    $loc : Item
) : Item
{
    $return = NewItem($"_{$loc}_{$noun}");
    InitializeMonster($return, $adjectives, $noun, $health, $damageResistance, $attackDamage, $loc);
}
```

## Sleep Function

The `Sleep` function sleeps for the specified number of minutes unless interrupted
by being attacked. The return value is true if the player slept for the specified
duration or false if the player was attacked.

```text
## Specifies whether the player is currently sleeping. This is used by the Sleep
## function and the OnPlayerAttacked function.
## @Sleep
var $isSleeping = false;

## Causes the player to sleep for the specified number of minutes or until the
## sleep is interrupted (i.e., if the player is attacked).
## $minutes: Number of minutes to sleep.
## $return: Returns true if the player slept for the full duration, or false if the sleep was interrupted.
## @$isSleeping,OnPlayerAttacked
function Sleep($minutes:Int) : Bool
{
    $isSleeping = true;
    while ($isSleeping && $minutes > 0)
    {
        Tick();
        if (!$isSleeping)
        {
            return false;
        }
        $minutes = $minutes - 1;
    }

    $isSleeping = false;
    return true;
}

## OnAttackedAction implementation for the player item. This causes the player
## to wake up if sleeping.
## $item: Item the action is invoked on.
## @Sleep
function OnPlayerAttacked($item:Item)
{
    $isSleeping = false;
}

game
{
    player.OnAttackedAction = OnPlayerAttacked;
}
```

## InitializeBed and NewBed Functions

The `InitializeBed` function sets the properties of a bed item. The `NewBed`
function creates and initializes a bed item. A bed has the properties of a
table (i.e., you can put things on it) plus the additional "use" of sleeping.

```text
## UseAction implementation for a bed item.
## $item: Item the action is invoked on.
## @UseAction,Sleep,InitializeBed,NewBed
function UseBed($item:Item)
{
    Message($"You go to sleep in the {Label($item)}.");
    if (Sleep(8 * 60))
    {
        Message("You wake up after eight hours.");
    }
    else
    {
        Message("You wake up after being attacked.");
    }
}

## Initializes the properties of a bed item.
## $item: Item to initialize.
## $adjectives: Space-separated adjectives used to refer to the item.
## $noun: Noun used to refer to the item.
## $loc: Initial location of the item, such as a room.
## @Sleep,NewBed
function InitializeBed($item:Item, $adjectives:String, $noun:String, $loc:Item)
{
    # A bed has the properties of a table (you can put things on it), plus
    # the additional "use" action of sleeping.
    InitializeTable($item, $adjectives, $noun, $loc);
    $item.UseAction = UseBed;
}

## Creates and initializes a bed item.
## $adjectives: Space-separated adjectives used to refer to the item.
## $noun: Noun used to refer to the item.
## $loc: Initial location of the item, such as a room.
## $return: Returns the newly-created item.
## @Sleep,InitializeBed
function NewBed($adjectives:String, $noun:String, $loc:Item) : Item
{
    $return = NewItem($"{$noun}_{$loc}");
    InitializeBed($return, $adjectives, $noun, $loc);
}
```

## Commands

This library implements the following commands:

```text
command "attack {$target:Item} with {$weapon:Item}"
{
    AttackItemWith($target, $weapon);
}
command "attack {$target:Item}"
{
    AttackItemWith($target, player.CurrentWeapon);
}
```
