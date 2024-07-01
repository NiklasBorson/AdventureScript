use super::adventure_script_types::*;
use super::lexer::*;
use super::type_def::*;
use super::*;

#[test]
fn test_lexer() {

    let input = "foo() -> 12 $xyz \"hello\" $\"You see an {$obj}.\"".as_bytes().to_vec();

    let expected_tokens = [
        Token::Name("foo"),
        Token::Symbol(SymbolId::LeftParen),
        Token::Symbol(SymbolId::RightParen),
        Token::Symbol(SymbolId::RightArrow),
        Token::Int(12),
        Token::Variable("$xyz"),
        Token::String(String::from("hello")),
        Token::FormatString(String::from("You see an {$obj}.")),
        Token::None
    ];

    let mut lexer = Lexer::new(String::from("filename.txt"), input);

    for i in 0..expected_tokens.len() {
        let token = lexer.read();
        println!("{:?}", token);
        assert!(token == expected_tokens[i]);
    }
}

fn add_enum_type(types : &mut TypeMap, name : &str, value_names : &[& str]) -> Result<TypeRef, ParseErrorCode> {
    let mut v = Vec::new();
    for n in value_names {
        v.push(String::from(*n));
    }
    types.add_enum_type(String::from(name), v)
}

fn add_delegate_type(types : &mut TypeMap, name : &str, return_type : TypeRef, param_type : TypeRef) -> Result<TypeRef, ParseErrorCode> {
    let mut param_types = Vec::new();
    param_types.push(param_type);
    types.add_delegate_type(String::from(name), return_type, param_types)
}

fn is_named_type(types : &TypeMap, type_name : &str, expected_type : &TypeRef) -> bool {
    if let Some(t) = types.get(type_name) {
        eq_type(t, expected_type)
    }
    else {
        false
    }
}

#[test]
fn test_typemap() -> Result<(), ParseErrorCode> {
    let mut types = TypeMap::new();

    // Create an enum type, verify its name and verify name lookup.
    let dir = add_enum_type(&mut types, "Direction", &["North", "South", "East", "West"][..])?;
    assert_eq!(dir.name, "Direction");
    assert!(is_named_type(&types, "Direction", &dir));

    // Create a delegate type, verify its name and verify name lookup.
    let return_type = (&types.bool_type).clone();
    let param_type = (&types.item_type).clone();
    let pred = add_delegate_type(&mut types, "Predicate", return_type, param_type)?;
    assert_eq!(pred.name, "Predicate");
    assert!(is_named_type(&types, "Predicate", &pred));

    // Verify name lookup for the built-in types.
    assert!(is_named_type(&types, "item", &types.item_type));
    assert!(is_named_type(&types, "string", &types.string_type));
    assert!(is_named_type(&types, "int", &types.int_type));
    assert!(is_named_type(&types, "bool", &types.bool_type));
    assert!(is_named_type(&types, "null", &types.null_type));
    assert!(is_named_type(&types, "void", &types.void_type));

    // Make sure different types do not compare equal.
    assert!(!eq_type(&dir, &types.item_type));
    assert!(!eq_type(&types.string_type, &types.item_type));

    Ok(())
}
