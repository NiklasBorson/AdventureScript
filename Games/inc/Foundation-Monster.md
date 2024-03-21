# AdventureScript Foundation Library - Monster Module

This optional module implements non-player "monster" items. It depends on the
`Foundation.md` and `Foundation-Combat.md` libraries, both of which must be
included first.

## UpdateAction Property

The `UpdateAction` delegate is invoked each turn. This is used to implement
behavior for "active" items like monsters and non-player characters.

```text
property UpdateAction : ItemDelegate;

turn
{
    foreach (var $item)
    {
        $item.UpdateAction($item);
    }
}
```

## UpdateHostileMonster Function

The `UpdateHostileMonster` function implements the update action for a
hostile monster.

```text
function UpdateHostileMonster($monster:Item)
{
    if (!$isNowDark)
    {
        if ($monster.Location == player.Location)
        {
            AttackPlayer($monster);
        }
        elseif ($monster.Location == $lastRoom)
        {
            $monster.Location = player.Location;
            AddItemWords($monster);
            Message($"The {Label($monster)} follows you.");
        }
    }
}
```

## UpdateSurprisedMonster Function

The `UpdateSurprisedMonster` is the default initial update action for a
potentially hostile monster. A "surprised" monster becomes hostile after
noticing the player.

```text
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
function UpdateFriendlyMonster($monster:Item)
{
    if (!$isNowDark && $monster.Location == $lastRoom)
    {
        $monster.Location = player.Location;
        AddItemWords($monster);
        Message($"The {Label($monster)} follows you.");
    }
}
```

## OnMonsterAttacked Function

The `OnMonsterAttacked` function implements the OnAttackedAction delegate
for a monster by changing its behavior to that of a hostile moster.

```text
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
