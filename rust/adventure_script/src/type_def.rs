use std::rc::Rc;
use std::string::String;
use std::collections::HashMap;
use crate::adventure_script_types::*;

pub type TypeRef = Rc<Type>;

pub struct Type {
    pub name: String,
    pub def: TypeDef
}

pub struct DelegateDef {
    pub return_type: TypeRef,
    pub param_types: Vec<TypeRef>
}

pub enum TypeDef {
    Item,
    String,
    Int,
    Bool,
    Void,
    Null,
    Enum(Vec<String>),
    Delegate(DelegateDef)
}

pub struct TypeMap {
    pub item_type : TypeRef,
    pub string_type : TypeRef,
    pub int_type : TypeRef,
    pub bool_type : TypeRef,
    pub null_type : TypeRef,
    pub void_type : TypeRef,
    hash_map : HashMap<String, TypeRef>,
    delegate_types : Vec<TypeRef>
}

impl TypeMap {
    pub fn new() -> Self {
        let item_type = Rc::new(Type {
            name: String::from("item"), 
            def: TypeDef::Item
        });
        let string_type = Rc::new(Type {
            name: String::from("string"), 
            def: TypeDef::String
        });
        let int_type = Rc::new(Type {
            name: String::from("int"), 
            def: TypeDef::Int
        });
        let bool_type = Rc::new(Type {
            name: String::from("bool"), 
            def: TypeDef::Bool
        });
        let null_type = Rc::new(Type {
            name: String::from("null"), 
            def: TypeDef::Null
        });
        let void_type = Rc::new(Type {
            name: String::from("void"), 
            def: TypeDef::Void
        });

        let mut type_map = Self {
            item_type : item_type.clone(),
            string_type : string_type.clone(),
            int_type : int_type.clone(),
            bool_type : bool_type.clone(),
            null_type : null_type.clone(),
            void_type : void_type.clone(),
            hash_map : HashMap::new(),
            delegate_types : Vec::new()
        };

        type_map.add_type(item_type);
        type_map.add_type(string_type);
        type_map.add_type(int_type);
        type_map.add_type(bool_type);
        type_map.add_type(void_type);
        type_map.add_type(null_type);

        type_map
    }

    fn add_type(&mut self, new_type : TypeRef) {
        self.hash_map.insert(new_type.name.clone(), new_type);
    }

    pub fn add_enum_type(&mut self, name : String, value_names : Vec<String>) -> Result<TypeRef, ParseErrorCode> {
        if self.exists(&name) {
            return Err(ParseErrorCode::DuplicateTypeName);
        }

        if contains_duplicates(value_names.as_slice()) {
            return Err(ParseErrorCode::DuplicateValueName);
        }

        let new_type = Rc::new(Type {
            name, 
            def: TypeDef::Enum(value_names)
        });

        self.add_type(new_type.clone());
        Ok(new_type)
    }

    pub fn add_delegate_type(&mut self, name : String, return_type : TypeRef, param_types : Vec<TypeRef>) -> Result<TypeRef, ParseErrorCode> {
        if self.exists(&name) {
            return Err(ParseErrorCode::DuplicateTypeName);
        }

        let def = DelegateDef{ return_type, param_types };

        // Check if there's an equivalent existing delegate type.
        for other_type in &self.delegate_types {
            if let TypeDef::Delegate(other_delegate) = &other_type.def {
                if eq_delegate(&def, &other_delegate) {

                    // Add the new name as an alias for the existing type.
                    self.hash_map.insert(name, other_type.clone());
                    return Ok(other_type.clone());
                }
            }
        }

        // No existing type, so create a new one.
        let new_type = Rc::new(Type {
            name,
            def: TypeDef::Delegate(def)
        });

        // Add and return the new object.
        self.add_type(new_type.clone());
        self.delegate_types.push(new_type.clone());
        Ok(new_type)
    }

    pub fn get(&self, name : &str) -> Option<&TypeRef> {
        self.hash_map.get(name)
    }

    pub fn exists(&self, name : &str) -> bool {
        self.hash_map.contains_key(name)
    }

}

pub fn eq_type(t1 : &TypeRef, t2 : &TypeRef) -> bool {
    std::ptr::eq((*t1).as_ref(), (*t2).as_ref())
}

fn eq_delegate(t1 : &DelegateDef, t2 : &DelegateDef) -> bool {
    if !eq_type(&t1.return_type, &t2.return_type) {
        false
    }
    else if t1.param_types.len() != t2.param_types.len() {
        false
    }
    else {
        for i in 0..t1.param_types.len() {
            if !eq_type(&t1.param_types[i], &t2.param_types[i]) {
                return false;
            }
        }
        true
    }
}

fn contains_duplicates(names : &[String]) -> bool {
    for i in 1..names.len() {
        let name : &str = names[i].as_ref();
        for j  in 0..i {
            if name == names[j] {
                return true;
            }
        }
    }
    false
}

