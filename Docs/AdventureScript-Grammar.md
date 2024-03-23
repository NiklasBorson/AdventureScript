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
