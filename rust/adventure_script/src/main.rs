mod adventure_script_types;
mod type_def;
mod lexer;

#[cfg(test)]
mod tests;

use crate::type_def::*;
use crate::lexer::*;

fn eq_type(t1 : &TypeRef, t2 : &TypeRef) -> bool {
    std::ptr::eq((*t1).as_ref(), (*t2).as_ref())
}

fn main() {
}
