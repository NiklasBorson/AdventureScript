#[derive(Debug)]
pub enum ParseError {
    InvalidToken,
    DuplicateTypeName,
    DuplicateValueName
}
