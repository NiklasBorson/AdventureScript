use std::fmt;

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum ParseErrorCode {
    InvalidToken,
    DuplicateTypeName,
    DuplicateValueName
}

fn to_string(error_code : ParseErrorCode) -> &'static str {
    match error_code {
        ParseErrorCode::InvalidToken => "invalid token",
        ParseErrorCode::DuplicateTypeName => "duplicate type name",
        ParseErrorCode::DuplicateValueName => "duplicate value name"
    }
}

#[derive(Debug)]
pub struct ParseError {
    pub file_name : String,
    pub line_number : u32,
    pub column_number : u32,
    pub error_code : ParseErrorCode
}

impl std::fmt::Display for ParseError {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(f, "{}: {},{}: {}.", self.file_name, self.line_number, self.column_number, to_string(self.error_code)) // user-facing output
    }
}
