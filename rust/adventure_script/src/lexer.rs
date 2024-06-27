use std::string::String;

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

pub enum Token {
    None,
    Error,
    Int,
    Name,
    Variable,
    String(String),
    FormatString(String),
    Symbol(SymbolId)
}

pub struct Lexer {
    file_name : String,
    data : Vec<u8>,
    token_pos : usize,
    token_end : usize,
    line_number : u32,
    line_start_pos : usize,
    token : Token
}

const SPACE : u8 = ' ' as u8;
const TAB : u8 = '\t' as u8;
const RETURN : u8 = '\r' as u8;
const NEWLINE : u8 = '\n' as u8;

impl Lexer {
    pub fn new(file_name : String, data : Vec<u8>) -> Lexer {
        Lexer {
            file_name,
            data,
            token_pos : 0,
            token_end : 0,
            line_number : 1,
            line_start_pos : 0,
            token : Token::None
        }
    }

    fn skip_whitespace(&mut self) {
        let mut last_ch : u8 = 0;
        for i in [self.token_end, self.data.len()] {
            let ch = self.data[i];
            if ch == SPACE || ch == TAB {
            }
            else if ch == RETURN || ch == NEWLINE {
                if last_ch != RETURN || ch != NEWLINE {
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
        self.token_pos = self.data.len();
        self.token_end = self.data.len();
    }    
}
