# AdventureScript Grammar

## Grammar Notation

This documentation describes the AdventureScript grammar using a variation on
[Backus-Naur form (BNF)](https://en.wikipedia.org/wiki/Backus%E2%80%93Naur_form).

There are many variations on BNF, but in general they consist of _production rules_
that define how _non terminal symbols_ may be expressed in terms of other symbols.

### Production Rule Example

The following production rule defines the _EnumDef_ non-terminal symbol in terms of
other symbols. It means an _EnumDef_ comprises the keyword "enum" followed by a _Name_,
which is in turn followed by a _NameList_ enclosed in parentheses. The non-terminal
symbols _Name_ and _NameList_ are defined elsewhere in the grammar.

```text
EnumDef = "enum" Name "(" NameList ")"
```

### Non-Terminal Symbols

A _non-terminal symbol_ is an abstract element of the grammar that does not appear
in the source text. In the previous example, _EnumDef_, _Name_, and _NameList_ are
non-terminal symbols.

As another example, when diagramming an English sentence, you might identify a
_noun phrase_ and a _verb phrase_. These terms refer to grammatical constructs rather
than literal text, so they are non-terminal symbols.

### Terminal Symbols

A _terminal symbol_ is literal text. In this document, terminal symbols are represented
in one of two ways:

- As literal strings in quotation marks.
- As character sets enclosed in square brackets (as in regular expressions).

### Grouping, Quantifiers, and Alternates

A sequence of symbols in the right-hand side of a production rule may be grouped together
by enclosing them in parentheses. 

The `*` symbol means the previous symbol or group occurs zero or more times.

The `+` symbol means the previous symbol or group occurs one or more times.

The `?` symbol means the previous symbol or group occurs zero or one time.

The `|` symbol means _either_ the symbols to the left or to the right.

### More Production Rule Examples

Following are additional production rules that demonstrate grouping, quantifiers, and
alternates.

The following production rule means a _Name_ comprises a _NameStartChar_ followed by zero
or more _NameChar_ symbols:

```text
Name = NameStartChar NameChar*
```

Thus, a _NameStartChar_ is a character that is valid as the first character of a _Name_.
It is defined by the following production rule:

```text
NameStartChar = [A-Za-z_]
```

A _NameChar_ is a character that may appear elsewhere in a _Name_ after the first character.
The following production rule means a _NameChar_ can be any _NameStartChar_ or any decimal
digit. Thus, decimal digits can be used in a _Name_ but not as the first character.

```text
NameChar = NameStartChar | [0-9]
```

The following production rule demonstrates grouping and the `*` quantifier. It means a
_NameList_ comprises one or more _Name_ symbols separated by commas:

```text
NameList = Name ( "," Name )*
```

## Top-Level Elements

An AdventureScript file comprises zero or more _definitions_ defined as follows:

```text
AdventureScriptFile = Definition*

Definition = "include" String ";"
           | "enum" Name "(" NameList ")" ";"
           | "delegate" Name ParamList TypeDecl? ";"
           | "property" NameList TypeDecl? ";"
           | "item" ( Name | String )
           | "var" VarName TypeDecl? ( = Expr )? ";"
           | "function" Name ParamList TypeDecl? StatementBLock
           | "function" Name ParamList "=>" Expression ";"
           | "map" Name Name -> Name "{" MapEntry ( "," MapEntry )* "}"
           | "command" String StatementBlock
           | "game" StatementBlock
           | "turn" StatementBlock
```

## Basic Building Blocks

A _String_ is zero or more characters enclosed in double quotation marks.

A _FormatString_ is a string preceded by the '$' character. A format string may have
expressions embedded in it enclosed in curly braces.

An _Int_ is an integer literal, represented as sequence of decimal digits.

A _Name_ comprises a name-start character followed by zero or more name characters:

```text
NameStartChar = [A-Za-z_]
NameChar = NameStartChar | [0-9]
Name = NameStartChar NameChar*
```

A _VarName_ (variable name) is a _Name_ preceded by a '$' symbol:

```text
VarName = "$" Name
```

A _NameList_ is one or more names separated by commas:

```text
NameList = Name ( "," Name )*
```

A _TypeDecl_ is a ":" followed by the name of a previously-defined type:

```text
TypeDecl = ":" Name
```

A _ParamList_ is a comma-separated list of parameter definitions enclosed in
parentheses. Each parameter definition comprises a _VarName_ and _TypeDecl_:

```text
ParamDef = VarName TypeDecl
ParamList = "(" ( ParamDef ( "," ParamDef )* )? ")"
```

A _StatementBlock_ comprises zero or more _statements_ enclosed in curly braces.
Statements are described in a later section.

```text
StatementBlock = "{" Statement* "}"
```

A _MapEntry_ maps an enum value name to either another enum value name or to a
constant expression. Expressions are described in a later section.

```text
MapEntry = Name "->" ( Name | Expr )
```

## Statements

Functions, commands, game blocks, and turn blocks are defined in terms of
_statements_, which are defined as follows:

```text
Statement = "var" VarName TypeDecl? ( "=" Expr )? ";"
          | Expr "=" Expr ";"
          | Expr ";"
          | IfBlock ElseifBlock* ElseBlock?
          | "switch" "(" Expr ")" "{" CaseBlock+ DefaultCaseBlock? "}"
          | "while" "(" Expr ")" StatementBlock
          | "foreach" "(" "var" VarName TypeDecl? ")" WhereClause? StatementBlock
          | "break" ";"
          | "continue" ";"
          | StatementBlock

IfBlock = "if" "(" Expr ")" StatementBlock
ElseifBlock = "if" "(" Expr ")" StatementBlock
ElseBlock = "else" StatementBlock

CaseBlock = "case" Expr StatementBlock
DefaultCaseBlock "default" StatementBlock

WhereClause = "where" Name BinaryOp UnaryExpr
```

## Expressions

Expressions are the building blocks of statements.

```text
Expr      = UnaryExpr
          | Expr BinaryOp Expr              # uses operator precedence
          | Expr "?" Expr ":" Expr

UnaryExpr = String | FormatString | Int     # literal value
          | "true" | "false" | "null"       # literal value
          | Name                            # item name
          | VarName                         # variable
          | Name "." Name                   # enum value
          | UnaryExpr ( "." Name )+         # property
          | Name "(" ArgList ")"            # function call
          | Expr "(" ArgList ")"            # delegate call
          | UnaryOp UnaryExpr
          | "(" Expr ")"

BinaryOp  = "==" | "!=" | "<" | "<=" | ">" | ">="   # comparison
          | "*" | "/" | "%" | "+" | "-"             # arithmetic
          | "&&" | "||"                             # logical

UnaryOp   = "!" | "-"                       # not or negative
```
