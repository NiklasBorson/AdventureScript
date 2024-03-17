using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AdventureLib
{
    class Parser
    {
        Lexer m_lexer;
        Stack<string> m_fileStack = new Stack<string>();

        public static void Parse(string filePath, GameState game)
        {
            using (var lexer = new FileLexer(filePath))
            {
                var parser = new Parser(game, lexer);
                try
                {
                    parser.Parse();
                }
                catch (Exception e)
                {
                    parser.Fail(e.Message);
                }
            }
        }

        Parser(GameState game, Lexer lexer)
        {
            this.Game = game;
            m_lexer = lexer;
            m_fileStack.Push(Path.GetFullPath(lexer.FilePath));
            InitializeReservedNames();
        }

        void Parse()
        {
            // Advance to the first token.
            Advance();

            while (m_lexer.HaveToken)
            {
                if (MatchName("include"))
                {
                    ParseIncludeDeclaration();
                }
                else if (MatchName("enum"))
                {
                    ParseEnumDefinition();
                }
                else if (MatchName("delegate"))
                {
                    ParseDelegateDefinition();
                }
                else if (MatchName("property"))
                {
                    ParsePropertyDefinition();
                }
                else if (MatchName("item"))
                {
                    ParseItemDefinition();
                }
                else if (MatchName("var"))
                {
                    ParseGlobalVarDefinition();
                }
                else if (MatchName("function"))
                {
                    ParseFunctionDefinition();
                }
                else if (MatchName("map"))
                {
                    ParseMapDefinition();
                }
                else if (MatchName("command"))
                {
                    ParseCommandDefinition();
                }
                else if (MatchName("game"))
                {
                    Advance();
                    this.Game.GameBlocks.Add(ParseCodeBlock());
                }
                else if (MatchName("turn"))
                {
                    Advance();
                    this.Game.TurnBlocks.Add(ParseCodeBlock());
                }
                else
                {
                    Fail("Syntax error.");
                }
            }
        }

        void ParseIncludeDeclaration()
        {
            // Advance past "include" keyword.
            Advance();

            // Get and advance past the file path.
            var filePath = ReadString();

            // Advance past the expected semicolon.
            ReadSymbol(SymbolId.Semicolon);

            // Parse the include file.
            ParseIncludeFile(filePath);
        }

        void ParseIncludeFile(string filePath)
        {
            string? path = Path.GetDirectoryName(m_lexer.FilePath);
            if (path == null)
            {
                Fail($"Cannot cannot resolve path: {filePath}.");
            }

            path = Path.GetFullPath(Path.Combine(path, filePath));
            if (!File.Exists(path))
            {
                Fail($"File does not exist: {filePath}.");
            }

            if (m_fileStack.Contains(path))
            {
                Fail($"File is already included: {path}.");
            }

            m_fileStack.Push(path);

            path = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);

            using (var lexer = new FileLexer(path))
            {
                var oldLexer = m_lexer;

                m_lexer = lexer;

                Parse();

                m_lexer = oldLexer;
            }

            m_fileStack.Pop();
        }

        void ParseEnumDefinition()
        {
            // Advance past the enum keyword.
            Advance();

            // Get and advance past the type name.
            var name = ReadName();

            // Make sure the name is not already used.
            ReserveName(name, "type");

            // Parse the rest of the statement.
            ReadSymbol(SymbolId.LeftParen);
            var valueNames = ParseNameList();
            ReadSymbol(SymbolId.RightParen);
            ReadSymbol(SymbolId.Semicolon);

            // Add the new type.
            this.Game.Types.AddEnumType(name, valueNames);
        }

        void ParseDelegateDefinition()
        {
            // Advance past the delegate keyword.
            Advance();

            var name = ReadName();

            // Make sure the name is not already used.
            ReserveName(name, "type");

            // Parse the parameter list.
            var paramList = ParseParamList();

            // Parse the return type.
            var returnType = ParseOptionalTypeDeclaration();

            // Read the terminating semicolon.
            ReadSymbol(SymbolId.Semicolon);

            // Add the delegate.
            this.Game.Types.AddDelegateType(name, paramList, returnType);
        }

        void ParsePropertyDefinition()
        {
            // Advance past the property keyword.
            Advance();

            var names = ParseNameList();
            ReadSymbol(SymbolId.Colon);

            var typeDef = ParseTypeName();

            foreach (var name in names)
            {
                if (!this.Game.Properties.TryAdd(name, typeDef))
                {
                    Fail($"Property {name} is already defined.");
                }
            }

            ReadSymbol(SymbolId.Semicolon);
        }
        void ParseItemDefinition()
        {
            // Advance past item keyword.
            Advance();

            if (this.IsNameToken)
            {
                AddItem(ReadName());
            }
            else if (this.IsStringToken)
            {
                AddItem(ReadString());
            }
            else
            {
                Fail("Expected name or string after 'item'.");
            }

            ReadSymbol(SymbolId.Semicolon);
        }
        void ParseGlobalVarDefinition()
        {
            // Advance past "var" <varName> "="
            Advance();
            string varName = ReadVariableName();
            var declaredType = ParseOptionalTypeDeclaration();
            ReadSymbol(SymbolId.Assign);

            // Parse the right-hand expression.
            var frame = new VariableFrame(this);
            var rightExpr = ParseExpression(frame);

            // Make sure we have a type.
            var type = DeriveAssignedTypeAllowVoid(declaredType, rightExpr);
            if (type == Types.Void)
            {
                Fail($"The initializer for {varName} does not return a value.");
            }

            // Try adding the variable.
            var newVar = this.Game.GlobalVars.TryAdd(varName, type);
            if (newVar == null)
            {
                Fail($"The variable {varName} is already defined.");
            }

            // Compute the value.
            newVar.Value = rightExpr.Evaluate(
                this.Game, 
                new int[frame.FrameSize]
                );

            // Read the final semicolon.
            ReadSymbol(SymbolId.Semicolon);
        }

        void AddItem(string name)
        {
            ReserveName(name, "item");
            this.Game.Items.AddItem(name);
        }

        void ParseFunctionDefinition()
        {
            // Advance past the "function" keyword.
            Advance();

            // Parse the function name.
            var functionName = ReadName();
            ReserveName(functionName, "function");

            // Parse the parameter list and return type.
            var paramList = ParseParamList();
            var returnType = ParseOptionalTypeDeclaration();

            // Create a variable frame for this function.
            var frame = new FunctionVariableFrame(this, paramList, returnType);

            if (MatchSymbol(SymbolId.Lambda))
            {
                // Parse the lambda expression.
                Advance();
                var expr = ParseExpression(frame);

                // Determine the actual return type based on the declared return
                // type (if any) and the expression type.
                returnType = DeriveAssignedTypeAllowVoid(returnType, expr);

                // Create the function.
                var functionDef = new LambdaFunctionDef(
                    functionName,
                    paramList,
                    frame.FrameSize,
                    expr
                    );
                this.Game.Functions.Add(functionDef);

                // Advance past the terminating semicolon.
                ReadSymbol(SymbolId.Semicolon);
            }
            else
            {
                // Parse the function body.
                var statement = ParseStatementBlock(frame);

                // Create the function.
                var functionDef = new UserFunctionDef(
                    functionName,
                    paramList,
                    returnType,
                    frame.FrameSize,
                    statement
                    );
                this.Game.Functions.Add(functionDef);
            }
        }

        TypeDef ParseOptionalTypeDeclaration()
        {
            if (MatchSymbol(SymbolId.Colon))
            {
                Advance();
                return ParseTypeName();
            }
            else
            {
                return Types.Void;
            }
        }

        TypeDef DeriveAssignedTypeAllowVoid(TypeDef declaredType, Expr expr)
        {
            // The declared type can be void but should never be null.
            Debug.Assert(declaredType != Types.Null);

            var exprType = expr.Type;

            // If the declared and expression type are the same then return it.
            if (declaredType == exprType)
            {
                return declaredType;
            }

            // If the declared type is void then use the expression type,
            // which must be a real type (not null).
            if (declaredType == Types.Void)
            {
                if (exprType == Types.Null)
                {
                    Fail("The expression type is ambiguous. Use an explicit type declaration.");
                }
                return exprType;
            }

            // The expression type does not match the declared type. This is an error unless
            // the expression type is null. The null type is only used for the null value,
            // which can be converted to any type.
            if (exprType != Types.Null)
            {
                Fail($"Cannot convert an expression of type {exprType.Name} to {declaredType.Name}.");
            }

            // Convert null expression to the declared type.
            return declaredType;
        }

        void ParseCommandDefinition()
        {
            // Advance past "command" keyword.
            Advance();

            string inputString = ReadString();
            var builder = new CommandBuilder(inputString);

            // Scan the input string for embedded parameters.
            int inputPos = 0;
            while (inputPos < inputString.Length)
            {
                // Is there another embedded ParamDef?
                int i = inputString.IndexOf('{', inputPos);
                if (i < 0)
                    break;

                // Append text before the ParamDef.
                if (i > inputPos)
                {
                    builder.AppendString(inputString.Substring(inputPos, i - inputPos));
                }

                // Let start..i exclusive be the ParamDef.
                int start = i + 1;
                i = inputString.IndexOf('}', start);
                inputPos = i + 1;

                // Create an embedded lexer.
                var oldLexer = m_lexer;
                m_lexer = new StringLexer(
                    oldLexer.FilePath,
                    inputString.Substring(start, i - start),
                    oldLexer.SourcePos.LineNumber,
                    oldLexer.SourcePos.ColumnNumber
                    );

                // Parse the param def.
                m_lexer.Advance();
                builder.AppendParam(ParseParamDef());
                if (m_lexer.TokenType != TokenType.None)
                {
                    Fail("Unexpected characters after parameter definition.");
                }

                // Restore the original lexer.
                m_lexer = oldLexer;
            }

            // Add any remaining text at the end.
            if (inputPos < inputString.Length)
            {
                builder.AppendString(inputString.Substring(inputPos, inputString.Length - inputPos));
            }

            builder.FinalizeCommandString(this);

            // Parse the body of the command.
            var body = ParseStatementBlock(
                builder.GetVariableFrame()
                );

            // Add the command.
            this.Game.Commands.Add(builder.CreateCommand(body));
        }

        void ParseMapDefinition()
        {
            // "map" <name> <fromType> "-> <toType>
            Advance();
            var name = ReadName();
            var fromType = ParseTypeName();
            if (!fromType.IsEnumType)
            {
                Fail("Input to 'map' must be an enum type.");
            }
            ReadSymbol(SymbolId.RightArrow);
            var toType = ParseTypeName();

            // Get the number of values and allocate the value map.
            int valueCount = fromType.ValueNames.Count;
            if (valueCount > 63)
            {
                Fail("Too many enum values for 'map'.");
            }
            int[] valueMap = new int[valueCount];
            UInt64 valueMaskAll = (((UInt64)1) << valueCount) - 1;
            UInt64 valueMask = 0;

            // Read the mappings.
            ReadSymbol(SymbolId.LeftBrace);

            var pair = ParseMapEntry(fromType, toType);
            valueMap[pair.Key] = pair.Value;
            valueMask |= ((UInt64)1) << pair.Key;

            while (MatchSymbol(SymbolId.Comma))
            {
                Advance();
                pair = ParseMapEntry(fromType, toType);

                var flag = ((UInt64)1) << pair.Key;
                if ((valueMask & flag) != 0)
                {
                    Fail($"A mapping for {fromType.ValueNames[pair.Key]} was already specified.");
                }

                valueMap[pair.Key] = pair.Value;
                valueMask |= flag;
            }

            ReadSymbol(SymbolId.RightBrace);

            if (valueMask != valueMaskAll)
            {
                Fail($"Not all values of {fromType.Name} are specified.");
            }

            var functionDef = new MapFunctionDef(
                name,
                fromType,
                toType,
                valueMap
                );

            this.Game.Functions.Add(functionDef);
        }

        KeyValuePair<int, int> ParseMapEntry(TypeDef fromType, TypeDef toType)
        {
            int fromValue = ParseShortEnumValue(fromType);
            ReadSymbol(SymbolId.RightArrow);

            int toValue = toType.IsEnumType ?
                ParseShortEnumValue(toType) :
                ParseConstantExpression(toType);

            return new KeyValuePair<int, int>(fromValue, toValue);
        }

        int ParseShortEnumValue(TypeDef type)
        {
            var name = ReadName();
            int value = type.ValueNames.IndexOf(name);
            if (value < 0)
            {
                Fail($"Type {type.Name} does not have a {name} value.");
            }
            return value;
        }

        int ParseConstantExpression(TypeDef type)
        {
            var expr = ParseExpression(new VariableFrame(this));
            if (expr.Type != type)
            {
                Fail($"Expected expression of type {type.Name}.");
            }

            if (!expr.IsConstant)
            {
                Fail("Expected constant expression.");
            }

            return expr.EvaluateConst(this.Game);
        }

        List<ParamDef> ParseParamList()
        {
            var paramList = new List<ParamDef>();

            ReadSymbol(SymbolId.LeftParen);

            if (this.IsVariableToken)
            {
                paramList.Add(ParseParamDef());

                while (MatchSymbol(SymbolId.Comma))
                {
                    Advance();
                    paramList.Add(ParseParamDef());
                }
            }

            ReadSymbol(SymbolId.RightParen);

            return paramList;
        }

        ParamDef ParseParamDef()
        {
            string varName = ReadVariableName();
            ReadSymbol(SymbolId.Colon);
            var type = ParseTypeName();
            return new ParamDef(varName, type);
        }

        CodeBlock ParseCodeBlock()
        {
            var frame = new VariableFrame(this);

            var statement = ParseStatementBlock(frame);

            return new CodeBlock(frame.FrameSize, statement);
        }

        Statement ParseStatementBlock(VariableFrame frame)
        {
            // Advance past the opening brace for the block.
            ReadSymbol(SymbolId.LeftBrace);
            frame.BeginBlock();

            // Parse statements until we get to the closing brace.
            List<Statement> statements = new List<Statement>();
            while (!this.MatchSymbol(SymbolId.RightBrace))
            {
                statements.Add(ParseStatement(frame));
            }

            // Advance past the closing brace.
            Advance();

            frame.EndBlock();

            return StatementBlock.Create(statements);
        }

        Statement ParseStatement(VariableFrame frame)
        {
            if (MatchName("var"))
            {
                return ParseVarStatement(frame);
            }
            else if (MatchName("switch"))
            {
                return ParseSwitchStatement(frame);
            }
            else if (MatchName("if"))
            {
                return ParseIfStatement(frame);
            }
            else if (MatchName("while"))
            {
                return ParseWhileStatement(frame);
            }
            else if (MatchName("foreach"))
            {
                return ParseForeachStatement(frame);
            }
            else if (MatchSymbol(SymbolId.LeftBrace))
            {
                return ParseStatementBlock(frame);
            }
            else
            {
                var expr = ParseExpression(frame);
                Statement? statement;

                if (MatchSymbol(SymbolId.Assign))
                {
                    if (!expr.CanSetValue)
                    {
                        Fail("Expression to left of '=' cannot be assigned to.");
                    }
                    Advance();

                    var right = ParseExpression(frame);
                    ReadSymbol(SymbolId.Semicolon);

                    if (!AssignStatement.CanAssignTypes(expr.Type, right.Type))
                    {
                        Fail($"Cannot convert expression of type {right.Type.Name} to {expr.Type.Name}.");
                    }

                    statement = new AssignStatement(expr, right, /*isNewVar*/ false);
                }
                else
                {
                    ReadSymbol(SymbolId.Semicolon);
                    if (!expr.HasSideEffects)
                    {
                        Fail("The expression has no effect.");
                    }
                    statement = new ExpressionStatement(expr);
                }
                return statement;
            }
        }

        Statement ParseVarStatement(VariableFrame frame)
        {
            // Advance past the var keyword.
            Advance();

            // Read $varName=
            string varName = ReadVariableName();
            var declaredType = ParseOptionalTypeDeclaration();
            ReadSymbol(SymbolId.Assign);

            // Parse the right-hand expression and get its type.
            var rightExpr = ParseExpression(frame);
            var type = DeriveAssignedTypeAllowVoid(declaredType, rightExpr);
            if (type == Types.Void)
            {
                Fail("The expression does not return a value.");
            }

            // Add the variable and create an assignment statement.
            var leftExpr = frame.AddVar(this, varName, type);
            var statement = new AssignStatement(leftExpr, rightExpr, /*isNewVar*/ true);

            // Advance past the semicolon.
            ReadSymbol(SymbolId.Semicolon);

            return statement;
        }
        Statement ParseSwitchStatement(VariableFrame frame)
        {
            // Advance past the switch keyword.
            Advance();

            // Parse the test expression.
            ReadSymbol(SymbolId.LeftParen);
            var expr = ParseExpression(frame);
            var type = expr.Type;
            if (type == Types.Void)
            {
                Fail("Expression does not return a value.");
            }
            ReadSymbol(SymbolId.RightParen);

            Dictionary<int, Statement> cases = new Dictionary<int, Statement>();
            Statement? defaultCase = null;

            ReadSymbol(SymbolId.LeftBrace);

            while (MatchName("case"))
            {
                Advance();

                // Parse the constant case expression.
                var caseExpr = ParseExpression(frame);
                if (caseExpr.Type != type)
                {
                    Fail("Case expression is the wrong type.");
                }
                if (!caseExpr.IsConstant)
                {
                    Fail("Case expression must be constant.");
                }
                int value = caseExpr.EvaluateConst(this.Game);

                if (cases.ContainsKey(value))
                {
                    Fail("Duplicate case.");
                }

                // Parse the statement block.
                cases.Add(value, ParseStatementBlock(frame));
            }

            if (cases.Count == 0)
            {
                Fail("'case' expected in switch block.");
            }

            if (MatchName("default"))
            {
                Advance();
                defaultCase = ParseStatementBlock(frame);
            }

            ReadSymbol(SymbolId.RightBrace);

            return new SwitchStatement(expr, type, cases, defaultCase);
        }

        Statement ParseIfStatement(VariableFrame frame)
        {
            // Advance past the if keyword.
            Advance();

            var result = new IfStatement(
                this, 
                ParseIfCondition(frame), 
                ParseStatementBlock(frame)
                );

            while (MatchName("elseif"))
            {
                Advance();
                result.AddBlock(
                    this,
                    ParseIfCondition(frame),
                    ParseStatementBlock(frame)
                    );
            }

            if (MatchName("else"))
            {
                Advance();
                result.SetElseBlock(ParseStatementBlock(frame));
            }

            return result;
        }

        Expr ParseIfCondition(VariableFrame frame)
        {
            ReadSymbol(SymbolId.LeftParen);
            var expr = ParseExpression(frame);
            ReadSymbol(SymbolId.RightParen);
            return expr;
        }

        Statement ParseWhileStatement(VariableFrame frame)
        {
            // Advance past the while keyword.
            Advance();

            return new IfStatement(
                this,
                ParseIfCondition(frame),
                ParseStatementBlock(frame)
                );
        }

        Statement ParseForeachStatement(VariableFrame frame)
        {
            // Advance past "foreach" "("
            Advance();
            ReadSymbol(SymbolId.LeftParen);

            // Advance past the 'var' keyword.
            ReadName("var");

            // Get the loop variable name.
            string varName = ReadVariableName();

            // Parse the optional type declaration.
            var type = ParseOptionalTypeDeclaration();
            if (type == Types.Void)
            {
                // Item type is the default.
                type = Types.Item;
            }
            else if (type != Types.Item && !type.IsEnumType)
            {
                Fail("Only Item and enum types can be used with foreach.");
            }
            ReadSymbol(SymbolId.RightParen);

            // Parse the where clause if specified.
            WhereClause? whereClause = null;
            if (type == Types.Item)
            {
                whereClause = ParseOptionalWhereClause(frame);
            }

            // Add the loop variable, which is scoped to the block.
            frame.BeginBlock();
            var loopVar = frame.AddVar(this, varName, type);

            var body = ParseStatementBlock(frame);
            frame.EndBlock();

            return new ForEachStatement(loopVar, type, whereClause, body);
        }

        WhereClause? ParseOptionalWhereClause(VariableFrame frame)
        {
            // Check for 'where' keyword and advance past it.
            if (!MatchName("where"))
            {
                return null;
            }
            Advance();

            // Read the property name, which the left argument.
            string propName = ReadName();
            var propDef = this.Game.Properties.TryGet(propName);
            if (propDef == null)
            {
                Fail($"Undefined property: {propName}.");
            }

            // Read the binary operator.
            var op = BinaryOperators.GetOp(m_lexer.SymbolValue);
            if (op == null)
            {
                Fail("Expected binary operator.");
            }
            Advance();

            // Parse the right-hand expression.
            var rightArg = ParseUnaryExpression(frame);

            // Determine the type of the binary expression.
            var type = op.DeriveType(propDef.Type, rightArg.Type);
            if (type != Types.Bool)
            {
                if (type == Types.Void)
                {
                    Fail($"Invalid argument types for '{op.SymbolText}' operator.");
                }
                else
                {
                    Fail("Expected Boolean operator in 'where' clause.");
                }
            }

            return new WhereClause(propDef, op, rightArg);
        }

        Expr ParseExpression(VariableFrame frame)
        {
            Expr expr = ParseBinaryExpression(frame);

            if (MatchSymbol(SymbolId.QuestionMark))
            {
                if (expr.Type != Types.Bool)
                {
                    Fail("Expected Boolean expression before '?'.");
                }

                Advance();
                var first = ParseBinaryExpression(frame);
                ReadSymbol(SymbolId.Colon);
                var second = ParseExpression(frame);

                expr = new TernaryExpr(this, expr, first, second);
            }

            return expr;
        }

        Expr ParseBinaryExpression(VariableFrame frame)
        {
            // expr -> unary (BinOp expr)*

            var expr = ParseUnaryExpression(frame);

            var op = BinaryOperators.GetOp(m_lexer.SymbolValue);
            if (op == null)
                return expr;

            return ParseBinaryExpression(frame, expr, op, Precedence.None);
        }

        Expr ParseBinaryExpression(VariableFrame frame, Expr left, BinaryOp? op, Precedence minPrecedence)
        {
            while (op != null && op.Precedence >= minPrecedence)
            {
                // Advance past the operator.
                Advance();

                // Parse the right-hand operand.
                var right = ParseUnaryExpression(frame);

                // If the next token is a binary operator of higher precedence than the
                // current operator, recursively parse it and let the right-hand expression
                // be the result.
                var nextOp = BinaryOperators.GetOp(m_lexer.SymbolValue);
                while (nextOp != null && nextOp.Precedence > op.Precedence)
                {
                    right = ParseBinaryExpression(frame, right, nextOp, nextOp.Precedence);
                    nextOp = BinaryOperators.GetOp(m_lexer.SymbolValue);
                }

                // Replace the left-hand expression with the binary expression.
                left = BinaryExpr.Create(this, op, left, right);
                op = nextOp;
            }
            return left;
        }

        Expr ParseUnaryExpression(VariableFrame frame)
        {
            Expr? expr = null;

            switch (m_lexer.TokenType)
            {
                case TokenType.Int:
                    expr = new LiteralExpr(
                        Types.Int,
                        m_lexer.IntValue
                        );
                    Advance();
                    return expr;

                case TokenType.String:
                    expr = new LiteralExpr(
                        Types.String,
                        this.Game.Strings[ReadString()]
                        );
                    return expr;

                case TokenType.FormatString:
                    return ParseFormatStringExpr(frame);

                case TokenType.Name:
                    expr = ParseNameExpr(frame);
                    break;

                case TokenType.Variable:
                    expr = ParseVariableExpr(frame);
                    break;

                case TokenType.Symbol:
                    if (m_lexer.SymbolValue == SymbolId.LeftParen)
                    {
                        // '(' expr ')'
                        Advance();
                        expr = ParseExpression(frame);
                        ReadSymbol(SymbolId.RightParen);
                    }
                    else
                    {
                        // '!' or '-' unary prefix operator
                        var op = UnaryOperators.GetOp(m_lexer.SymbolValue);
                        if (op != null)
                        {
                            Advance();
                            expr = UnaryExpr.Create(
                                this,
                                op,
                                ParseUnaryExpression(frame)
                                );
                        }
                    }
                    break;
            }

            if (expr == null)
            {
                Fail("Syntax error.");
            }

            for (;;)
            {
                if (MatchSymbol(SymbolId.Period))
                {
                    Advance();

                    string propName = ReadName();
                    var propDef = this.Game.Properties.TryGet(propName);
                    if (propDef == null)
                    {
                        Fail($"Undefined property: {propName}.");
                    }

                    expr = new PropertyExpr(this, expr, propDef);
                }
                else if (MatchSymbol(SymbolId.LeftParen))
                {
                    // Make sure the expression to the left is a delegate.
                    var delegateType = expr.Type as DelegateTypeDef;
                    if (delegateType == null)
                    {
                        Fail("Expected delegate to the left of '('.");
                    }

                    // Parse the argument list including parentheses.
                    var argList = ParseArgListIncludingParentheses(frame);

                    return new DelegateExpr(this, expr, delegateType, argList);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        Expr ParseVariableExpr(VariableFrame frame)
        {
            string varName = ReadVariableName();

            var expr = frame.TryGetVar(varName);
            if (expr == null)
            {
                Fail($"Undefined variable: {varName}.");
            }

            return expr;
        }

        Expr ParseNameExpr(VariableFrame frame)
        {
            string name = ReadName();

            // Check for keywords.
            if (name == "true")
            {
                return new LiteralExpr(Types.Bool, 1);
            }
            if (name == "false")
            {
                return new LiteralExpr(Types.Bool, 0);
            }
            if (name == "null")
            {
                return new LiteralExpr(Types.Null, 0);
            }

            // Is the name a function?
            var func = this.Game.Functions.TryGetFunction(name);
            if (func != null)
            {
                // If followed by a '(' then it's a function call.
                if (MatchSymbol(SymbolId.LeftParen))
                {
                    // Parse the argument list including parentheses.
                    var argList = ParseArgListIncludingParentheses(frame);

                    // Special case for "GetItem" function where the
                    // argument is constant.
                    if (name == "GetItem" &&
                        argList.Count == 1 &&
                        argList[0].Type == Types.String &&
                        argList[0].IsConstant)
                    {
                        int stringId = argList[0].EvaluateConst(this.Game);
                        string itemName = this.Game.Strings[stringId];
                        var matchingItem = this.Game.Items.TryGetItem(itemName);
                        if (matchingItem == null)
                        {
                            Fail($"Item '{itemName}' is not defined.");
                        }
                        return new LiteralExpr(Types.Item, matchingItem.ID);
                    }

                    return new FunctionExpr(this, func, argList);
                }

                // There's no argument list so convert the function name to a delegate.
                var delegateType = this.Game.Types.FindDelegateType(func.ParamList, func.ReturnType);
                if (delegateType == null)
                {
                    Fail($"No matching delegate type for function {name}.");
                }
                return new LiteralExpr(delegateType, func.ID);
            }

            var type = this.Game.Types.TryGet(name);
            if (type != null)
            {
                if (!type.IsEnumType)
                {
                    Fail($"Type '{name}' cannot be used as an expression.");
                }

                // It's a delegate type, so we expect an expression of the form:
                // <typeName>.valueName>
                ReadSymbol(SymbolId.Period);
                string valueName = ReadName();

                // Map the value name to an integer.
                int i = type.ValueNames.IndexOf(valueName);
                if (i < 0)
                {
                    Fail($"Undefined enum value: {name}.{valueName}.");
                }
                return new LiteralExpr(type, i);
            }

            // Otherwise we expect this to be an item.
            var item = this.Game.Items.TryGetItem(name);
            if (item == null)
            {
                Fail($"Unresolved name: {name}.");
            }

            return new LiteralExpr(Types.Item, item.ID);
        }

        List<Expr> ParseArgListIncludingParentheses(VariableFrame frame)
        {
            // Parse the argument list.
            var argList = new List<Expr>();
            Advance();
            if (!MatchSymbol(SymbolId.RightParen))
            {
                argList.Add(ParseExpression(frame));
                while (MatchSymbol(SymbolId.Comma))
                {
                    Advance();
                    argList.Add(ParseExpression(frame));
                }
            }
            ReadSymbol(SymbolId.RightParen);
            return argList;
        }

        Expr ParseFormatStringExpr(VariableFrame frame)
        {
            var input = m_lexer.FormatStringValue;
            var pos = m_lexer.SourcePos;
            Advance();

            // Return a string literal if no embedded expressions.
            if (input.IndexOf('{') < 0)
            {
                return new LiteralExpr(
                    Types.String,
                    this.Game.Strings[new string(input)]
                    );
            }

            // List of embedded expressions.
            var exprList = new List<Expr>();

            // Allocate a buffer for the format string, which cannot
            // be longer than the input string.
            var formatChars = new char[input.Length];
            int formatLen = 0;

            // Iterate over each input character.
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if (ch != '{')
                {
                    // Append ordinary character to format string.
                    formatChars[formatLen++] = ch;
                }
                else 
                {
                    // Seek the end of the embedded expression.
                    int start = ++i;
                    while (i < input.Length && input[i] != '}')
                    {
                        i++;
                    }
                    if (i == input.Length)
                    {
                        Fail("Unmatched '{' in format string");
                    }

                    // Add a format specifier to the format string.
                    int exprIndex = exprList.Count;
                    if (exprIndex > 9)
                    {
                        Fail("Too many embedded expressions.");
                    }
                    formatChars[formatLen++] = '{';
                    formatChars[formatLen++] = (char)('0' + exprIndex);
                    formatChars[formatLen++] = '}';

                    // Parse the embedded expression.
                    exprList.Add(ParseEmbeddedExpression(
                        frame,
                        new string(input.Slice(start, i - start)),
                        pos.LineNumber,
                        pos.ColumnNumber + start
                        ));
                }
            }

            return new FormatStringExpr(
                this,
                new string(formatChars, 0, formatLen),
                exprList
                );
        }

        Expr ParseEmbeddedExpression(VariableFrame frame, string inputExpr, int lineNumber, int colOffset)
        {
            // Create a new lexer for the embedded expression.
            var oldLexer = m_lexer;
            m_lexer = new StringLexer(m_lexer.FilePath, inputExpr, lineNumber, colOffset);

            // Advance to the first token.
            Advance();

            // Parse the expression.
            var expr = ParseExpression(frame);

            // We don't expect any extra characters after the expression.
            if (m_lexer.TokenType != TokenType.None)
            {
                Fail("Syntax error.");
            }

            // Restore the original lexer.
            m_lexer = oldLexer;
            return expr;
        }

        IList<string> ParseNameList()
        {
            var names = new List<string>();

            names.Add(ReadName());

            while (MatchSymbol(SymbolId.Comma))
            {
                Advance();
                names.Add(ReadName());
            }

            return names;
        }

        TypeDef ParseTypeName()
        {
            string typeName = ReadName();
            var typeDef = this.Game.Types.TryGet(typeName);
            if (typeDef == null)
            {
                Fail($"Undefined type name: {typeName}.");
            }
            return typeDef;
        }

        void Advance()
        {
            m_lexer.Advance();

            if (m_lexer.TokenType == TokenType.Error)
            {
                Fail("Invalid token.");
            }
        }

        static readonly string[] m_keywords = new string[]
        {
            "case",
            "else",
            "elseif",
            "enum",
            "false",
            "foreach",
            "function",
            "game",
            "if",
            "include",
            "null",
            "property",
            "switch",
            "true",
            "turn",
            "var",
        };

        Dictionary<string, string> m_reservedNames = new Dictionary<string, string>();
        
        void InitializeReservedNames()
        {
            foreach (var keyword in m_keywords)
            {
                m_reservedNames.Add(keyword, "keyword");
            }

            foreach (var func in IntrinsicFunctions.Intrinsics)
            {
                m_reservedNames.Add(func.Name, "function");
            }
        }

        void ReserveName(string name, string nameType)
        {
            if (!m_reservedNames.TryAdd(name, nameType))
            {
                Fail($"Cannot add {nameType} because '{name}' is already a {m_reservedNames[name]}.");
            }
        }

        bool MatchName(ReadOnlySpan<char> name)
        {
            return this.IsNameToken && 
                MemoryExtensions.Equals(name, m_lexer.NameValue, StringComparison.Ordinal);  
        }

        void ReadName(string name)
        {
            if (!MatchName(name))
            {
                Fail($"Expected '{name}'");
            }
            Advance();
        }

        string ReadName()
        {
            if (!this.IsNameToken)
            {
                Fail("Expected name.");
            }

            string result = new string(m_lexer.NameValue);

            Advance();

            return result;
        }
        string ReadVariableName()
        {
            if (!this.IsVariableToken)
            {
                Fail("Expected variable.");
            }

            string result = new string(m_lexer.NameValue);

            Advance();

            return result;
        }

        string ReadString()
        {
            if (!this.IsStringToken)
            {
                Fail("Expected string.");
            }

            var value = m_lexer.StringValue;
            Advance();

            if (!this.IsStringToken)
            {
                return new string(value);
            }

            // Concatenate consecutive string tokens.
            var builder = new StringBuilder();
            builder.Append(value);
            while (this.IsStringToken)
            {
                builder.Append(m_lexer.StringValue);
                Advance();
            }

            return builder.ToString();
        }

        bool MatchSymbol(SymbolId symbolId)
        {
            return m_lexer.SymbolValue == symbolId;
        }

        void ReadSymbol(SymbolId symbolId)
        {
            if (!MatchSymbol(symbolId))
            {
                Fail($"Expected {symbolId}.");
            }
            Advance();
        }

        bool IsNameToken => m_lexer.TokenType == TokenType.Name;
        bool IsVariableToken => m_lexer.TokenType == TokenType.Variable;
        bool IsStringToken => m_lexer.TokenType == TokenType.String;

        public GameState Game { get; }

        [DoesNotReturn]
        public void Fail(string message)
        {
            m_lexer.SourcePos.Fail(message);
        }
    }
}
