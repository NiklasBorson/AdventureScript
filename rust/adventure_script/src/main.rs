mod adventure_script_types;
mod type_def;
mod lexer;

#[cfg(test)]
mod tests;

use crate::type_def::*;
use crate::lexer::*;

fn compare_type(t1 : &Type, t2 : &Type) {
    if std::ptr::eq(t1, t2) {
        println!("{} == {}", t1.name, t2.name);
    }
    else {
        println!("{} != {}", t1.name, t2.name);
    }
}

fn dump_type(types : &TypeMap, type_name : &str) {
    if let Some(t) = types.get(type_name) {
        println!("Type {}", t.name);

        match &t.def {
            TypeDef::Enum { value_names } => {
                for name in value_names.iter() {
                    println!("    {name}");
                }
            },
            TypeDef::Delegate { return_type, param_types } => {
                for t in param_types.iter() {
                    println!("    {}", t.name);
                }
                println!("    -> {}", return_type.name);
            },
            _ => {}
        }
    }
    else {
        println!("Type {type_name} does not exist.");
    }
}

fn main() {
    let mut types = TypeMap::new();

    let mut dir_names = Vec::new();
    dir_names.push(String::from("North"));
    dir_names.push(String::from("South"));

    if let Ok(dir) = types.add_enum_type(String::from("Direction"), dir_names) {
        println!("{} added.\n", dir.name);
    }

    compare_type(types.bool_type.as_ref(), types.bool_type.as_ref());
    compare_type(types.bool_type.as_ref(), types.item_type.as_ref());

    dump_type(&types, "bool");
    dump_type(&types, "string");
    dump_type(&types, "item");
    dump_type(&types, "Direction");

    let input = "foo() + 12 * $xyz \"hello\" $\"You see an {$obj}.\"".as_bytes().to_vec();
    let mut lexer = Lexer::new(String::from("foo.txt"), input);
    let mut token = lexer.read();
    while token != Token::None && token != Token::Invalid {
        println!("{:?}", token);
        token = lexer.read();
    }
    println!("{:?}", token);
}
