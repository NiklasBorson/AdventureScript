# AdventureScript Grammar

## Grammar Notation

This documentation describes the AdventureScript grammar using a variation on
[Backus-Naur form (BNF)](https://en.wikipedia.org/wiki/Backus%E2%80%93Naur_form).

There are many variations on BNF, but in general they consist of _production rules_
that define how _non-terminal symbols_ may be expressed in terms of other symbols.
A _non-terminal symbol_ is an abstract element of a grammar that does not appear
in the source text. For example, when diagramming an English sentence, you might
identify a _noun phrase_ and a _verb phrase_. These terms refer to grammatical
constructs rather than literal text, so they are non-terminal symbols.

A _terminal symbol_ is literal text that may appear in the input. A non-terminal
symbol may be defined in terms of terminal symbols as well as other non-terminal
symbols. In this document, terminal symbols are represented in two ways:

- As literal strings in quotation marks.
- As character sets enclosed in square brackets (as in regular expressions).

For example, the following production rule defines the non-terminal symbol
_EnumDef_. It specifies that an _EnumDef_ comprises the symbols on the right side
of the equal sign in order, where _Name_ and _NameList_ are other non-terminal
symbols defined elsewhere in the grammar.

### Grouping, Quantifiers, and Alternates

A sequence of symbols in the right-hand side of a production rule may be grouped
together by enclosing them in parentheses.

The `*` symbol means the previous symbol or group occurs zero or more times.

The `+` symbol means the previous symbol or group occurs one or more times.

The `?` symbol means the previous symbol or group occurs zero or one time.

The `|` symbol means _either_ the symbols to the left or to the right.

### Production Rule Examples

The following production rules demonstrate grouping, quantifiers, and alternates.

A _Name_ comprises a _NameStartChar_ followed by zero or more _NameChar_ symbols.

```text
Name = NameStartChar NameChar*
```

A _NameStartChar_ is a an uppercase or lowercase English letter or an underscore.

```text
NameStartChar = [A-Za-z_]
```

A _NameChar_ is any _NameStartChar_ or any digit.

```text
NameChar = NameStartChar | [0-9]
```

A _NameList_ is a commma-separated sequence of names. More precisely, it is a _Name_
followed by zero or more additional _Name_ productions preceded by commas.

```text
NameList = Name ( "," Name )*
```

### White Space and Comments

Spaces are not allowed with a _Name_ or _VarName_ production. In all other cases,
spaces between symbols are ignored. For brevity, this is not spelled out explicitly
in the grammar.

The '#' symbol (outside of a quoted string) comments out the remainder of the
current line. This is also not represented in the grammar.

Newlines are treated the same as spaces (except for ending '#' comments).

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
          | "return" Expr? ";"
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
