use super::adventure_script_types::*;
use super::lexer::*;
use super::type_def::*;
use std::fs::{self, File};
use std::io::{BufReader,BufRead};
use std::io::Write;
use const_format::formatcp;
use std::error::Error;
use std::fmt;
use std::io::LineWriter;

const INPUT_DIR : &str = "test_files/input";
const BASELINE_DIR : &str = "test_files/baseline";
const OUTPUT_DIR : &str = "target/testout";

#[derive(Debug)]
struct ComparisonError(&'static str, i32);

impl fmt::Display for ComparisonError {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(f, "{} differs from baseline version, line {}.", self.0, self.1)
    }    
}

impl Error for ComparisonError {}

fn compare_test_output(file_name : &'static str) -> Result<(), Box<dyn Error>> {

    let baseline_path = format!("{BASELINE_DIR}/{file_name}");
    let baseline = File::open(baseline_path)?;
    let baseline = BufReader::new(baseline);

    let output_path = format!("{OUTPUT_DIR}/{file_name}");
    let output = File::open(output_path)?;
    let output = BufReader::new(output);

    let mut output_lines = output.lines();
    let mut line_number = 0;

    for baseline_line in baseline.lines() {
        line_number += 1;

        if let Some(output_line) = output_lines.next() {
            if baseline_line? != output_line? {
                return Err(Box::new(ComparisonError(file_name, line_number)));
            }
        }
        else {
            return Err(Box::new(ComparisonError(file_name, line_number)));
        }
    }

    if let Some(_) = output_lines.next() {
        return Err(Box::new(ComparisonError(file_name, line_number)));
    }

    Ok(())
}

#[test]
fn test_lexer() -> Result<(), Box<dyn Error>> {

    const FILE_NAME : &str = "LexerTest.txt";

    let input_path = formatcp!("{INPUT_DIR}/{FILE_NAME}");
    let input  = fs::read_to_string(input_path)?;

    fs::create_dir_all(OUTPUT_DIR)?;
    let output_path = formatcp!("{OUTPUT_DIR}/{FILE_NAME}");
    let output_file = File::create(output_path)?;
    let mut output_file = LineWriter::new(output_file);

    let mut lexer = Lexer::new(String::from(input_path), input.as_bytes().to_vec());

    loop {
        let token = lexer.read();
        writeln!(&mut output_file, "{:?}", token)?;
        if token == Token::None || token == Token::Invalid {
            break;
        }
    }

    compare_test_output(FILE_NAME)
}

fn add_enum_type(types : &mut TypeMap, name : &str, value_names : &[& str]) -> Result<TypeRef, ParseErrorCode> {
    let mut v = Vec::new();
    for n in value_names {
        v.push(String::from(*n));
    }
    types.add_enum_type(String::from(name), v)
}

fn add_delegate_type(types : &mut TypeMap, name : &str, return_type : TypeRef, param_type : TypeRef) -> Result<TypeRef, ParseErrorCode> {
    let mut param_types = Vec::new();
    param_types.push(param_type);
    types.add_delegate_type(String::from(name), return_type, param_types)
}

fn is_named_type(types : &TypeMap, type_name : &str, expected_type : &TypeRef) -> bool {
    if let Some(t) = types.get(type_name) {
        eq_type(t, expected_type)
    }
    else {
        false
    }
}

#[test]
fn test_typemap() -> Result<(), ParseErrorCode> {
    let mut types = TypeMap::new();

    // Create an enum type, verify its name and verify name lookup.
    let dir = add_enum_type(&mut types, "Direction", &["North", "South", "East", "West"][..])?;
    assert_eq!(dir.name, "Direction");
    assert!(is_named_type(&types, "Direction", &dir));

    // Create a delegate type, verify its name and verify name lookup.
    let return_type = (&types.bool_type).clone();
    let param_type = (&types.item_type).clone();
    let pred = add_delegate_type(&mut types, "Predicate", return_type.clone(), param_type.clone())?;
    assert_eq!(pred.name, "Predicate");
    assert!(is_named_type(&types, "Predicate", &pred));

    // Create an equivalent delegate type with the same properties. This should
    // be an alias for the existing type.
    let pred2 = add_delegate_type(&mut types, "Predicate2", return_type, param_type)?;
    assert!(eq_type(&pred, &pred2));
    assert!(is_named_type(&types, "Predicate2", &pred));

    // Verify name lookup for the built-in types.
    assert!(is_named_type(&types, "item", &types.item_type));
    assert!(is_named_type(&types, "string", &types.string_type));
    assert!(is_named_type(&types, "int", &types.int_type));
    assert!(is_named_type(&types, "bool", &types.bool_type));
    assert!(is_named_type(&types, "null", &types.null_type));
    assert!(is_named_type(&types, "void", &types.void_type));

    // Make sure different types do not compare equal.
    assert!(!eq_type(&dir, &types.item_type));
    assert!(!eq_type(&types.string_type, &types.item_type));

    Ok(())
}
