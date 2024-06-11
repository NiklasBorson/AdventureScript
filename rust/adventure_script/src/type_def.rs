use std::rc::Rc;
use std::string::String;
use std::collections::HashMap;

pub trait TypeDef {
    fn name(&self) -> &str;
    fn as_enum(&self) -> Option<&EnumType> {
        None
    }
    fn as_delegate(&self) -> Option<&DelegateType> {
        None
    }
}

pub type TypeRef = Rc<dyn TypeDef>;

pub struct EnumType {
    name : String,
    value_names: Vec<String>
}

impl EnumType {
    pub fn value_names(&self) -> &[String] {
        self.value_names.as_slice()
    }    
}

impl TypeDef for EnumType {
    fn name(&self) -> &str {
        self.name.as_str()
    }
    fn as_enum(&self) -> Option<&EnumType> {
        Some(self)
    }
}

pub struct ParamDef {
    pub name : String,
    pub param_type : TypeRef
}

pub struct DelegateType {
    name : String,
    param_defs : Vec<ParamDef>,
    return_type : TypeRef
}

impl DelegateType {
    pub fn params(&self) -> &[ParamDef] {
        &self.param_defs.as_slice()
    }
    pub fn return_type(&self) -> &TypeRef {
        &self.return_type
    }
}

impl TypeDef for DelegateType {
    fn name(&self) -> &str {
        self.name.as_str()
    }
}

struct ItemType;
impl TypeDef for ItemType {
    fn name(&self) -> &str {
        "item"
    }
}

struct StringType;
impl TypeDef for StringType {
    fn name(&self) -> &str {
        "string"
    }
}

struct IntType;
impl TypeDef for IntType {
    fn name(&self) -> &str {
        "int"
    }
}

struct BoolType;
impl TypeDef for BoolType {
    fn name(&self) -> &str {
        "bool"
    }
}

struct NullType;
impl TypeDef for NullType {
    fn name(&self) -> &str {
        "null"
    }
}

struct VoidType;
impl TypeDef for VoidType {
    fn name(&self) -> &str {
        "void"
    }
}

pub struct TypeMap {
    pub item_type : TypeRef,
    pub string_type : TypeRef,
    pub int_type : TypeRef,
    pub bool_type : TypeRef,
    pub null_type : TypeRef,
    pub void_type : TypeRef,
    hash_map : HashMap<String, TypeRef>,
}

pub enum ParseError {
    DuplicateTypeName,
    DuplicateValueName
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

impl TypeMap {
    pub fn new() -> Self {
        let item_type = Rc::new(ItemType{});
        let string_type = Rc::new(StringType{});
        let int_type = Rc::new(IntType);
        let bool_type = Rc::new(BoolType{});
        let null_type = Rc::new(NullType{});
        let void_type = Rc::new(VoidType{});

        let mut type_map = Self {
            item_type : item_type.clone(),
            string_type : string_type.clone(),
            int_type : int_type.clone(),
            bool_type : bool_type.clone(),
            null_type : null_type.clone(),
            void_type : void_type.clone(),
            hash_map : HashMap::new()
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
        self.hash_map.insert(String::from(new_type.name()), new_type);
    }

    pub fn add_enum_type(&mut self, name : String, value_names : Vec<String>) -> Result<Rc<EnumType>, ParseError> {
        if self.exists(&name) {
            return Err(ParseError::DuplicateTypeName);
        }

        if contains_duplicates(value_names.as_slice()) {
            return Err(ParseError::DuplicateValueName);
        }

        let new_type = Rc::new(EnumType{ name, value_names });
        self.add_type(new_type.clone());

        Ok(new_type)
    }

    pub fn get(&self, name : &str) -> Option<&TypeRef> {
        self.hash_map.get(name)
    }

    pub fn exists(&self, name : &str) -> bool {
        self.hash_map.contains_key(name)
    }

}
