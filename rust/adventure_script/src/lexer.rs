use std::string::String;
use std::borrow::Cow;

#[derive(Debug)]
#[derive(PartialEq)]
pub enum SymbolId {
    Plus,
    Minus,
    Times,
    Divide,
    Modulo,
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    Period,
    Comma,
    Or,
    And,
    Not,
    Semicolon,
    Colon,
    Assign,
    Less,
    Greater,
    Equals,
    NotEquals,
    LessEquals,
    GreaterEquals,
    QuestionMark,
    Lambda,
    RightArrow,
}

#[derive(Debug)]
#[derive(PartialEq)]
pub enum Token<'a> {
    None,
    Invalid,
    Int(i32),
    Name(&'a str),
    Variable(&'a str),
    String(String),
    FormatString(String),
    Symbol(SymbolId)
}

pub struct Lexer {
    file_name : String,
    input : Vec<u8>,
    token_pos : usize,
    token_end : usize,
    line_number : u32,
    line_start_pos : usize
}

impl Lexer {
    pub fn new(file_name : String, input : Vec<u8>) -> Lexer {
        Lexer {
            file_name,
            input,
            token_pos : 0,
            token_end : 0,
            line_number : 1,
            line_start_pos : 0
        }
    }

    pub fn read(&mut self) -> Token {
        self.skip_whitespace();

        let input : &[u8] = &self.input;
        let i = self.token_pos;

        if i == input.len() {
            return Token::None;
        }

        let ch = input[i];
        let ch2 = if i + 1 < input.len() { input[i + 1] } else { 0 };

        if is_name_start_char(ch) {
            let j = find_if_not(input, i + 1, is_name_char);
            let tok = &input[i..j];
            if let Ok(s) = std::str::from_utf8(tok) {
                self.token_end = j;
                Token::Name(s)
            }
            else {
                Token::Invalid
            }
        }
        else if ch == b'$' && is_name_start_char(ch2) {
            let j = find_if_not(input, i + 2, is_name_char);
            let tok = &input[i..j];
            if let Ok(s) = std::str::from_utf8(tok) {
                self.token_end = j;
                Token::Variable(s)
            }
            else {
                Token::Invalid
            }
        }
        else if is_digit(ch) {
            let j = find_if_not(input, i + 1, is_digit);
            let mut value = (ch - b'0') as i32;
            for digit in &input[i + 1..j] {
                value *= 10;
                value += (digit - b'0') as i32;
            }
            self.token_end = j;
            Token::Int(value)
        }
        else if ch == b'\"' {
            if let Some((s, j)) = parse_string(input, i) {
                self.token_end = j;
                Token::String(s)
            }
            else {
                Token::Invalid
            }
        }
        else if let Some((symbol, len)) = match_symbol(ch, ch2) {
            self.token_end = self.token_pos + len;
            Token::Symbol(symbol)
        }
        else {
            Token::Invalid
        }
    }

    fn skip_whitespace(&mut self) {
        let mut last_ch : u8 = 0;
        for i in self.token_end..self.input.len() {
            let ch = self.input[i];
            if ch == b' ' || ch == b'\t' {
            }
            else if ch == b'\r' || ch == b'\n' {
                // Increment the line number unless this is the second character in a "\r\n" sequence.
                if last_ch != b'\r' || ch != b'\n' {
                    self.line_number += 1;
                }
                self.line_start_pos = i + 1;
            }
            else {
                self.token_pos = i;
                self.token_end = i;
                return;
            }
            last_ch = ch;
        }
        self.token_pos = self.input.len();
        self.token_end = self.input.len();
    }    
}

fn parse_string(input : &[u8], start_pos : usize) -> Option<(String, usize)> {
    assert!(input[start_pos] == b'\"');
    let mut last_char = b'\"';
    let mut v : Vec<u8> = Vec::new();
    for i in start_pos + 1..input.len() {
        let ch = input[i];

        if last_char == b'\\' {
            match ch {
                b'n' => v.push(b'\n'),
                b'\\'|b'\"' => v.push(ch),
                _ => return None
            };
        }
        else {
            match ch {
                b'\"' => {
                    if let Ok(s) = String::from_utf8(v) {
                        return Some((s, i + 1));
                    }
                    else {
                        return None;
                    }
                },
                b'\\' => {
                },
                _ => {
                    v.push(ch);
                }
            }
        }
        last_char = ch;
    }
    None
}

fn is_digit(ch : u8) -> bool {
    ch >= b'0' && ch <= b'9'
}

fn is_name_start_char(ch : u8) -> bool {
    (ch >= b'a' && ch <= b'z') || (ch >= b'A' && ch <= b'Z') || ch == b'_'
}

fn is_name_char(ch : u8) -> bool {
    is_digit(ch) || is_name_start_char(ch)
}

fn find_if_not(input : &[u8], start_pos : usize, pred : fn(u8) -> bool) -> usize {
    for i in start_pos..input.len() {
        if !pred(input[i]) {
            return i;
        }
    }
    return input.len();
}

fn match_symbol(ch : u8, ch2 : u8) -> Option<(SymbolId, usize)> {
    match ch {
        b'+' => Some((SymbolId::Plus, 1)),
        b'-' => if ch2 == b'>' { Some((SymbolId::RightArrow, 2)) } else { Some((SymbolId::Minus, 1)) },
        b'*' => Some((SymbolId::Times, 1)),
        b'/' => Some((SymbolId::Divide, 1)),
        b'%' => Some((SymbolId::Modulo, 1)),
        b'(' => Some((SymbolId::LeftParen, 1)),
        b')' => Some((SymbolId::RightParen, 1)),
        b'{' => Some((SymbolId::LeftBrace, 1)),
        b'}' => Some((SymbolId::RightBrace, 1)),
        b'.' => Some((SymbolId::Period, 1)),
        b',' => Some((SymbolId::Comma, 1)),
        b'|' => if ch2 == b'|' { Some((SymbolId::Or, 2)) } else { None },
        b'&' => if ch2 == b'&' { Some((SymbolId::And, 2)) } else { None },
        b'!' => if ch2 == b'=' { Some((SymbolId::NotEquals, 2)) } else { Some((SymbolId::Not, 1)) },
        b';' => Some((SymbolId::Semicolon, 1)),
        b':' => Some((SymbolId::Colon, 1)),
        b'=' => match ch2 {
            b'=' => Some((SymbolId::Equals, 2)),
            b'>' => Some((SymbolId::Lambda, 2)),
            _ => Some((SymbolId::Assign, 1))
        },
        b'<' => if ch2 == b'=' { Some((SymbolId::LessEquals, 2)) } else { Some((SymbolId::Less, 1)) },
        b'>' => if ch2 == b'=' { Some((SymbolId::GreaterEquals, 2)) } else { Some((SymbolId::Greater, 1)) },
        b'?' => Some((SymbolId::QuestionMark, 1)),
        _ => None
    }
}
