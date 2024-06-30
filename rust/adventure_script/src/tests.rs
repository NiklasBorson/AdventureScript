use super::lexer::*;

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
