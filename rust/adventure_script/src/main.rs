use crate::type_def::*;

mod type_def;

fn compare_type(t1 : &dyn TypeDef, t2 : &dyn TypeDef) {
    if std::ptr::eq(t1, t2) {
        println!("{} == {}", t1.name(), t2.name());
    }
    else {
        println!("{} != {}", t1.name(), t2.name());
    }
}

fn dump_type(types : &TypeMap, type_name : &str) {
    if let Some(t) = types.get(type_name) {
        println!("Type {}", t.name());

        if let Some(e) = t.as_enum() {
            for name in e.value_names().iter() {
                println!("    {name}");
            }
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
        println!("{} added.\n", dir.name());
    }

    compare_type(types.bool_type.as_ref(), types.bool_type.as_ref());
    compare_type(types.bool_type.as_ref(), types.item_type.as_ref());

    dump_type(&types, "bool");
    dump_type(&types, "string");
    dump_type(&types, "item");
    dump_type(&types, "Direction");
}
