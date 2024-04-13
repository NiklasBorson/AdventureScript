using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AdventureScript
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
                    ParseGlobalVarDefinition(/*isConst*/ false);
                }
                else if (MatchName("const"))
                {
                    ParseGlobalVarDefinition(/*isConst*/ true);
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
                    var commandDef = ParseCommandDefinition();
                    this.Game.Commands.Add(commandDef);
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
            var docComments = m_lexer.DocComments;
            var sourcePos = m_lexer.SourcePos;

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
            var def = new EnumTypeDef(sourcePos, docComments, name, valueNames);

            this.Game.Types.Add(def);
        }

        void ParseDelegateDefinition()
        {
            var docComments = m_lexer.DocComments;
            var sourcePos = m_lexer.SourcePos;

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
            this.Game.Types.AddDelegateType(sourcePos, docComments, name, paramList, returnType);
        }

        void ParsePropertyDefinition()
        {
            var docComments = m_lexer.DocComments;
            var sourcePos = m_lexer.SourcePos;

            // Advance past the property keyword.
            Advance();

            var names = ParseNameList();
            ReadSymbol(SymbolId.Colon);

            var typeDef = ParseTypeName();

            foreach (var name in names)
            {
                if (!this.Game.Properties.TryAdd(sourcePos, docComments, name, typeDef))
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
                string name = ReadName();
                AddItem(name);
            }
            else if (this.IsStringToken)
            {
                string name = ReadString();
                AddItem(name);
            }
            else
            {
                Fail("Expected name or string after 'item'.");
            }

            ReadSymbol(SymbolId.Semicolon);
        }
        void ParseGlobalVarDefinition(bool isConst)
        {
            var docComments = m_lexer.DocComments;
            var sourcePos = m_lexer.SourcePos;

            // Advance past "var" <varName> "="
            Advance();
            string varName = ReadVariableName();
            var type = ParseOptionalTypeDeclaration();
            int value = 0;

            // Is there an initializer expression?
            if (MatchSymbol(SymbolId.Assign))
            {
                Advance();

                // Parse the right-hand expression.
                var frame = new VariableFrame(this);
                var rightExpr = ParseExpression(frame);

                // Make sure we have a type.
                type = DeriveAssignedTypeAllowVoid(type, rightExpr);
                if (type == Types.Void)
                {
                    Fail($"The initializer for {varName} does not return a value.");
                }

                // Compute the value.
                value = rightExpr.Evaluate(
                    this.Game,
                    new int[frame.FrameSize]
                    );
            }
            else if (isConst)
            {
                Fail($"No initializer for constant {varName}.");
            }
            else if (type == Types.Void)
            {
                Fail($"No type specified for variable {varName}.");
            }

            // Try adding the variable.
            var newVar = this.Game.GlobalVars.TryAdd(sourcePos, docComments, varName, type, isConst);
            if (newVar == null)
            {
                Fail($"The variable {varName} is already defined.");
            }

            newVar.Value = value;

            // Read the final semicolon.
            ReadSymbol(SymbolId.Semicolon);
        }

        void AddItem(string name)
        {
            ReserveName(name, "item");
            this.Game.Items.AddItem(name);
        }

        FunctionBody ParseCodeBlock()
        {
            var builder = new FunctionBuilder(this);
            ParseStatementBlock(builder);
            return builder.CreateFunctionBody();
        }

        void ParseFunctionDefinition()
        {
            var docComments = m_lexer.DocComments;
            var sourcePos = m_lexer.SourcePos;

            // Advance past the "function" keyword.
            Advance();

            // Parse the function name.
            var functionName = ReadName();
            ReserveName(functionName, "function");

            // Parse the parameter list and return type.
            var paramList = ParseParamList();
            var returnType = ParseOptionalTypeDeclaration();

            FunctionDef functionDef;

            if (MatchSymbol(SymbolId.Lambda))
            {
                // Create a variable frame for this function.
                var frame = new VariableFrame(this);

                // Add variables for the return type and parameters.
                frame.InitializeFunction(this, paramList, returnType);

                // Parse the lambda expression.
                Advance();
                var expr = ParseExpression(frame);

                // Determine the actual return type based on the declared return
                // type (if any) and the expression type.
                returnType = DeriveAssignedTypeAllowVoid(returnType, expr);

                // Create the function.
                functionDef = new LambdaFunctionDef(
                    functionName,
                    paramList,
                    frame.FrameSize,
                    expr
                    );

                // Advance past the terminating semicolon.
                ReadSymbol(SymbolId.Semicolon);
            }
            else
            {
                // Create a function builder.
                var builder = new FunctionBuilder(this);

                // Add variables for the return type and parameters.
                builder.InitializeFunction(this, paramList, returnType);

                // Parse the function body.
                ParseStatementBlock(builder);

                // Create the function.
                functionDef = new UserFunctionDef(
                    functionName,
                    paramList,
                    returnType,
                    builder.CreateFunctionBody()
                    );
            }

            this.Game.Functions.Add(functionDef);

            functionDef.SourcePos = sourcePos;
            functionDef.DocComments = docComments;
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

        CommandDef ParseCommandDefinition()
        {
            // Advance past "command" keyword.
            Advance();

            string inputString = ReadString();
            var builder = new CommandBuilder(this, inputString);

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

            ParseStatementBlock(builder);

            return builder.CreateCommand();
        }

        void ParseMapDefinition()
        {
            var docComments = m_lexer.DocComments;
            var sourcePos = m_lexer.SourcePos;

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

            functionDef.SourcePos = sourcePos;
            functionDef.DocComments = docComments;
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

        void ParseStatementBlock(FunctionBuilder builder)
        {
            // Advance past the opening brace for the block.
            ReadSymbol(SymbolId.LeftBrace);
            builder.PushScope();

            // Parse statements until we get to the closing brace.
            while (!this.MatchSymbol(SymbolId.RightBrace))
            {
                ParseStatement(builder);
            }

            // Advance past the closing brace.
            Advance();
            builder.PopScope();
        }

        void ParseStatement(FunctionBuilder builder)
        {
            if (MatchName("var"))
            {
                ParseVarStatement(builder);
            }
            else if (MatchName("switch"))
            {
                ParseSwitchStatement(builder);
            }
            else if (MatchName("if"))
            {
                ParseIfStatement(builder);
            }
            else if (MatchName("while"))
            {
                ParseWhileStatement(builder);
            }
            else if (MatchName("foreach"))
            {
                ParseForeachStatement(builder);
            }
            else if (MatchName("return"))
            {
                ParseReturnStatement(builder);
            }
            else if (MatchName("break"))
            {
                var loop = builder.CurrentLoop;
                if (loop == null)
                {
                    Fail("break can only be inside a loop.");
                }
                builder.AddStatement(new BreakStatement(loop));
                Advance();
                ReadSymbol(SymbolId.Semicolon);
            }
            else if (MatchName("continue"))
            {
                var loop = builder.CurrentLoop;
                if (loop == null)
                {
                    Fail("continue can only be inside a loop.");
                }
                builder.AddStatement(new ContinueStatement(loop));
                Advance();
                ReadSymbol(SymbolId.Semicolon);
            }
            else if (MatchName("command"))
            {
                var commandDef = ParseCommandDefinition();
                builder.AddStatement(new CommandStatement(commandDef));
            }
            else if (MatchSymbol(SymbolId.LeftBrace))
            {
                builder.AddStatement(new BlockStartStatement());
                ParseStatementBlock(builder);
                builder.AddStatement(new BlockEndStatement());
            }
            else
            {
                var expr = ParseExpression(builder);

                if (MatchSymbol(SymbolId.Assign))
                {
                    if (!expr.CanSetValue)
                    {
                        Fail("Expression to left of '=' cannot be assigned to.");
                    }
                    Advance();

                    var right = ParseExpression(builder);
                    ReadSymbol(SymbolId.Semicolon);

                    if (!AssignStatement.CanAssignTypes(expr.Type, right.Type))
                    {
                        Fail($"Cannot convert expression of type {right.Type.Name} to {expr.Type.Name}.");
                    }

                    builder.AddStatement(new AssignStatement(expr, right));
                }
                else
                {
                    ReadSymbol(SymbolId.Semicolon);
                    if (!expr.HasSideEffects)
                    {
                        Fail("The expression has no effect.");
                    }
                    builder.AddStatement(new ExpressionStatement(expr));
                }
            }
        }

        void ParseVarStatement(FunctionBuilder builder)
        {
            // Advance past the var keyword.
            Advance();

            // Read the variable name and type.
            string varName = ReadVariableName();
            var type = ParseOptionalTypeDeclaration();

            // Parse the right-hand express if specified.
            Expr? rightExpr = null;
            if (MatchSymbol(SymbolId.Assign))
            {
                Advance();
                rightExpr = ParseExpression(builder);
                type = DeriveAssignedTypeAllowVoid(type, rightExpr);
                if (type == Types.Void)
                {
                    Fail("The expression does not return a value.");
                }
            }
            else if (type == Types.Void)
            {
                Fail($"No type specified for variable {varName}.");
            }

            // Add the variable and create the statement.
            var newVar = builder.AddVariable(this, varName, type);
            builder.AddStatement(new VarStatement(newVar, rightExpr));

            // Advance past the semicolon.
            ReadSymbol(SymbolId.Semicolon);
        }

        void ParseSwitchStatement(FunctionBuilder builder)
        {
            // Advance past the switch keyword.
            Advance();

            // Parse the test expression.
            ReadSymbol(SymbolId.LeftParen);
            var expr = ParseExpression(builder);
            var type = expr.Type;
            if (type == Types.Void)
            {
                Fail("Expression does not return a value.");
            }
            ReadSymbol(SymbolId.RightParen);
            ReadSymbol(SymbolId.LeftBrace);

            // Create and add the switch statement.
            var switchStatement = new SwitchStatement(expr);
            builder.AddStatement(switchStatement);

            // Remember the ends of all the nested blocks.
            List<BlockEndStatement> blockEndList = new List<BlockEndStatement>();

            // Parse all the case blocks.
            while (MatchName("case"))
            {
                Advance();

                // Parse the constant case expression.
                var caseExpr = ParseExpression(builder);
                if (caseExpr.Type != type)
                {
                    Fail("Case expression is the wrong type.");
                }
                if (!caseExpr.IsConstant)
                {
                    Fail("Case expression must be constant.");
                }
                int value = caseExpr.EvaluateConst(this.Game);

                // Create and add the case statement.
                var caseStatement = switchStatement.TryCreateCase(value);
                if (caseStatement == null)
                {
                    Fail("Duplicate case.");
                }
                builder.AddStatement(caseStatement);

                // Parse the body of the case block.
                ParseStatementBlock(builder);

                // Add a block end statement for the end of the block.
                var blockEnd = new BlockEndStatement();
                builder.AddStatement(blockEnd);
                blockEndList.Add(blockEnd);
            }

            if (switchStatement.CaseCount == 0)
            {
                Fail("'case' expected in switch block.");
            }

            // Check for optional "default" case.
            if (MatchName("default"))
            {
                Advance();

                // Create and add the default case statement.
                builder.AddStatement(switchStatement.CreateDefaultCase());

                // Parse the body of the default case block.
                ParseStatementBlock(builder);

                // Add a block end statement for the end of the block.
                var blockEnd = new BlockEndStatement();
                builder.AddStatement(blockEnd);
                blockEndList.Add(blockEnd);
            }

            // Read the final closing brace for the switch statement.
            ReadSymbol(SymbolId.RightBrace);

            // Add the final block-end for the switch statement.
            builder.AddStatement(switchStatement.BlockEnd);

            // Make the block-end for each nested block point to the final block end.
            int nextIndex = switchStatement.BlockEnd.NextStatementIndex;
            foreach (var blockEnd in blockEndList)
            {
                blockEnd.NextStatementIndex = nextIndex;
            }
        }

        void ParseIfStatement(FunctionBuilder builder)
        {
            // Advance past the if keyword.
            Advance();

            var blockEndList = new List<BlockEndStatement>();

            // Create and add the if statement.
            var statement = new IfStatement(ParseIfCondition(builder), "if");
            builder.AddStatement(statement);

            // Parse the body of the if block.
            ParseStatementBlock(builder);

            // Add the end block, and remember it for later.
            builder.AddStatement(statement.BlockEnd);
            blockEndList.Add(statement.BlockEnd);

            // Remember the last statement in the if/elseif chain.
            var lastStatement = statement;

            while (MatchName("elseif"))
            {
                Advance();

                // Create and add the elseif statement.
                statement = new IfStatement(ParseIfCondition(builder), "elseif");
                builder.AddStatement(statement);

                // Parse the body of the elseif block.
                ParseStatementBlock(builder);

                // Add the end block, and remember it for later.
                builder.AddStatement(statement.BlockEnd);
                blockEndList.Add(statement.BlockEnd);

                // Let the last if/elseif statement point to the new one as its "else" branch.
                lastStatement.ElseBranch = statement;
                lastStatement = statement;
            }

            if (MatchName("else"))
            {
                Advance();

                // Create and add the "else" statement.
                var elseStatement = new ElseStatement();
                builder.AddStatement(elseStatement);

                // Parse the body of the else block.
                ParseStatementBlock(builder);

                // Add the end block and remember it for later.
                var blockEnd = new BlockEndStatement();
                builder.AddStatement(blockEnd);
                blockEndList.Add(blockEnd);

                // Let the last if/elseif statement point to the else block as its
                // "else" branch.
                lastStatement.ElseBranch = elseStatement;
            }

            // Make the block-end for each block jump to the final block end.
            int blockCount = blockEndList.Count;
            if (blockCount != 0)
            {
                int nextIndex = blockEndList[--blockCount].NextStatementIndex;
                for (int i = 0; i < blockCount; i++)
                {
                    blockEndList[i].NextStatementIndex = nextIndex;
                }
            }
        }

        Expr ParseIfCondition(VariableFrame frame)
        {
            ReadSymbol(SymbolId.LeftParen);
            var expr = ParseExpression(frame);
            ReadSymbol(SymbolId.RightParen);
            return expr;
        }

        void ParseWhileStatement(FunctionBuilder builder)
        {
            // Advance past the while keyword.
            Advance();

            var loop = new WhileStatement(this, ParseIfCondition(builder));
            builder.BeginLoop(loop);

            ParseStatementBlock(builder);

            builder.EndLoop(loop);
        }

        void ParseForeachStatement(FunctionBuilder builder)
        {
            // Begin a new scope for the loop variable.
            builder.PushScope();

            // Advance past "foreach" "(" "var"
            Advance();
            ReadSymbol(SymbolId.LeftParen);
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
            var loopVar = builder.AddVariable(this, varName, type);
            ReadSymbol(SymbolId.RightParen);

            // Is there a where clause?
            if (MatchName("where"))
            {
                if (type != Types.Item)
                {
                    Fail("Loop variable used with 'where' must have type Item.");
                }
                Advance();

                // Read the property name, which is the left argument.
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
                var rightArg = ParseUnaryExpression(builder);

                // Determine the type of the binary expression.
                var exprType = op.DeriveType(propDef.Type, rightArg.Type);
                if (exprType != Types.Bool)
                {
                    if (exprType == Types.Void)
                    {
                        Fail($"Invalid argument types for '{op.SymbolText}' operator.");
                    }
                    else
                    {
                        Fail("Expected Boolean operator in 'where' clause.");
                    }
                }

                // Add an unnamed variable to store the value of the right-hand expression.
                var hiddenVar = builder.AddHiddenVariable(rightArg.Type);

                // Create the loop statement.
                var loop = new ForEachItemWhereStatement(
                    loopVar,
                    propDef,
                    op,
                    rightArg,
                    hiddenVar.FrameIndex
                    );

                builder.BeginLoop(loop);

                // Parse the loop body.
                ParseStatementBlock(builder);

                builder.EndLoop(loop);
            }
            else if (type == Types.Item)
            {
                // Create the loop statement.
                var loop = new ForEachItemStatement(loopVar);

                builder.BeginLoop(loop);

                // Parse the loop body.
                ParseStatementBlock(builder);

                builder.EndLoop(loop);
            }
            else if (type.IsEnumType)
            {
                // Create the loop statement.
                var loop = new ForEachEnumStatement(loopVar, type);

                builder.BeginLoop(loop);

                // Parse the loop body.
                ParseStatementBlock(builder);

                builder.EndLoop(loop);
            }
            else
            {
                Fail("Only Item and enum types can be used with foreach.");
            }

            builder.PopScope();
        }

        void ParseReturnStatement(FunctionBuilder builder)
        {
            // Advance past return keyword
            Advance();

            if (MatchSymbol(SymbolId.Semicolon))
            {
                builder.AddStatement(new ReturnStatement());
                Advance();
            }
            else if (builder.ReturnType != Types.Void)
            {
                Expr expr = ParseExpression(builder);
                if (!AssignStatement.CanAssignTypes(builder.ReturnType, expr.Type))
                {
                    Fail($"Cannot convert expression of type {expr.Type.Name} to the return type.");
                }
                builder.AddStatement(new ReturnValueStatement(expr));
                ReadSymbol(SymbolId.Semicolon);
            }
            else
            {
                Fail("A return value is not expected here.");
            }
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

            if (expr.IsConstant)
            {
                int value = expr.EvaluateConst(this.Game);
                return new LiteralExpr(expr.Type, value);
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
            var input = m_lexer.StringValue;
            var pos = m_lexer.SourcePos;
            Advance();

            // Return a string literal if no embedded expressions.
            if (input.IndexOf('{') < 0)
            {
                return new LiteralExpr(
                    Types.String,
                    this.Game.Strings[input]
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
                        input.Substring(start, i - start),
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
            "break",
            "case",
            "continue",
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
            "return",
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

            string value = m_lexer.StringValue;
            Advance();

            if (!this.IsStringToken)
            {
                return value;
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
