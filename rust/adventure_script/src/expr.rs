use std::string::String;
use crate::adventure_script_types::*;
use crate::type_def::*;

pub struct GameState {

}

#[derive(Debug, PartialEq, PartialOrd)]
pub enum Precedence {
    None,
    Ternary,
    AndOr,
    Compare,
    AddSub,
    MulDiv,
    UnaryNegative,
    Member,
    Atomic
}

pub trait Expr {
    fn get_type(&self) -> &TypeRef;
    fn evaluate(&self, game : &GameState, frame : &mut [i32]) -> i32;
    fn evaluate_const(&self, game : &GameState) -> i32;
    fn set_value(&self, game : &GameState, frame : &mut [i32], value : i32);
    fn can_set_value(&self) -> bool;
    fn has_side_effects(&self) -> bool;
    fn is_constant(&self) -> bool;
    fn get_precedence(&self) -> Precedence;
}

pub struct LiteralExpr {
    return_type : TypeRef,
    value : i32
}

impl Expr for LiteralExpr {
    fn get_type(&self) -> &TypeRef {
        &self.return_type
    }
    fn evaluate(&self, _game : &GameState, _frame : &mut [i32]) -> i32 {
        self.value
    }
    fn evaluate_const(&self, _game : &GameState) -> i32 {
        self.value
    }
    fn set_value(&self, _game : &GameState, _frame : &mut [i32], _value : i32) {
        panic!();
    }
    fn can_set_value(&self) -> bool {
        false
    }
    fn has_side_effects(&self) -> bool {
        false
    }
    fn is_constant(&self) -> bool {
        true
    }
    fn get_precedence(&self) -> Precedence {
        Precedence::Atomic
    }
}
