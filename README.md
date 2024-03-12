# CsAdventure

CsAdventure is a text adventure game implemented in C#. This solution includes the following projects:

- **AdventureEngine** implements the game engine as a shared library.

- **EngineTest** implements a set of tests for the game engine.

- **CsAdventure** is the game program.

## Game Definition

A game definition is an XML file loaded by the game engine that defines the game content and rules. It includes items, properties, commands, and rules.

An _item_ is any object in the game. Each item has a unique name and a set of properties. The player is a special item named "player". Rooms are items, as are doors, passages, and so on, as well as portable items that may be collected and used by the player. If a game includes monsters or non-player characters, those would also be items.

Each item has a set of _properties_. The game engine has an open-ended property system. Concepts like location and movement are not fundamental features but can be implemented in terms of properties. For example, the player might have a "Location" property that references an item (i.e., a room).

## Game Definition Schema

### GameDefinition Element

Top-level element of the game definition.

Contains the following child elements in order:

- ( `Definitions` )?
- ( `Rules` )?
- ( `Commands` )?

### Definitions Element

Contains the following child elements in any order:

- `Template`
- `ActionTemplate`
- `ApplyTemplate`
- `EnumType`
- `PropertyDef`
- `DerivedProperty`
- `Item`

### Template Element

Contains definition elements with placeholder variables, which are later replaced with template parameters when the template is instantiated.

Attributes:

- `Name` specifies the name of the tamplate.
- `Params` specifies a comma-spearated list of template parameter names.

Contains the following child elements in order:

- ( `Description` )?
- ( `PropertyDef` | `DerivedProperty` | `Item` )*

The optional `Description` element contains text describing the template. It is used only for documentation purposes and is ignored by the game engine.

### Action Elements

An _Action Element_ is any of the following:

- `ForEachItem`
- `SelectItem`
- `If` ( `ElseIf` )* ( `Else` )?
- `SetProperty`
- `Message`
- `EndGame`
- `ApplyActionTemplate`

### EnumType Element

Defines an enum type, which can later be referenced as the type of a `PropertyDef` element.

Attributes:

- `Name` specifies the type name.
- `Values` specifies a comma-separated list of property values names for this type.

This element has no child elements.

Example:

```xml
<EnumType Name="Direction" Values="North,South,East,West"/>
```

### PropertyDef Element

Defines a named property, which can subsequently be assigned to items.

Attributes:

- `Name` specifies the unique name of the property.
- `Type` must be `Int`, `Item`, `String`, or a previously-defined enum type

This element has no child elements.

Example:

```xml
<PropertyDef Name="Location" Type="Item"/>
```

### DerivedProperty Element

TODO

