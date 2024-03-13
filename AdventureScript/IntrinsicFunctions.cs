using System.Text;

namespace AdventureLib
{
    sealed class IntrinsicFunctionDef : FunctionDef
    {
        public delegate int Func(GameState game, int[] frame);

        Func m_func;

        public IntrinsicFunctionDef(string name, IList<ParamDef> paramList, TypeDef returnType, Func func) :
            base(name, paramList, returnType)
        {
            m_func = func;
        }

        public override int Invoke(GameState game, int[] frame)
        {
            return m_func(game, frame);
        }

        public override int FrameSize => ParamList.Count + 1;

        public override void SaveDefinition(GameState game, CodeWriter writer)
        {
            // Write nothing for intrinsic functions
        }
    }

    static class IntrinsicFunctions
    {
        public static void Add(FunctionMap funcMap)
        {
            foreach (var func in Intrinsics)
            {
                funcMap.Add(func);
            }
        }

        static int _NewItem(GameState game, int[] frame)
        {
            // NewItem($name:String) : Item
            //  - frame[1] -> $namePrefix
            string namePrefix = game.Strings[frame[1]];

            // Try using the unadorned name prefix.
            string name = namePrefix;

            // Append a suffix if necessary to generate a unique item name.
            for (int suffix = 1;
                game.Items.Exists(name);
                suffix++)
            {
                name = $"{namePrefix}_{suffix}";
            }

            // Create the item.
            var item = game.Items.AddItem(name);
            return item.ID;
        }
        static int _GetItem(GameState game, int[] frame)
        {
            // GetItem($name:String) : Item
            //  - frame[1] -> $name
            string name = game.Strings[frame[1]];
            var item = game.Items[name];
            return item.ID;
        }
        static int _EndGame(GameState game, int[] frame)
        {
            // EndGame($isWon:Bool)
            //  - frame[1] -> $isWon
            game.EndGame(frame[1] != 0);
            return 0;
        }
        static int _Message(GameState game, int[] frame)
        {
            // Message($message:String)
            //  - frame[1] -> $message
            string message = game.Strings[frame[1]];
            game.Message(message);
            return 0;
        }

        static int _ListVariables(GameState game, int[] frame)
        {
            foreach (var varExpr in game.GlobalVars)
            {
                game.Message($"var {varExpr.Name} : {varExpr.Type.Name};");
            }
            return 0;
        }

        static int _ListFunctions(GameState game, int[] frame)
        {
            foreach (var funcDef in game.Functions)
            {
                var b = new StringBuilder();
                b.Append($"function {funcDef.Name}(");
                for (int i = 0; i < funcDef.ParamList.Count; i++)
                {
                    var paramDef = funcDef.ParamList[i];

                    if (i != 0)
                    {
                        b.Append(", ");
                    }

                    b.Append($"{paramDef.Name}:{paramDef.Type.Name}");
                }
                var type = funcDef.ReturnType;
                if (type != Types.Void)
                {
                    b.Append($") : {type.Name};");
                }
                else
                {
                    b.Append(");");
                }
                game.Message(b.ToString());
            }
            return 0;
        }

        static int _ListCommands(GameState game, int[] frame)
        {
            foreach (var def in game.Commands)
            {
                game.Message($"- {def.CommandSpec}");
            }
            return 0;
        }

        static int _ListWords(GameState game, int[] frame)
        {
            game.ListWords();
            return 0;
        }

        static int _AddNoun(GameState game, int[] frame)
        {
            // AddNoun($word:String, $item:Item)
            // - frame[1] -> $word
            // - frame[2] = $item
            game.WordMap.AddNoun(
                /*word*/ game.Strings[frame[1]],
                /*itemId*/ frame[2]
                );
            return 0;
        }

        static int _AddAdjectives(GameState game, int[] frame)
        {
            // AddAdjectives($word:String, $item:Item)
            // - frame[1] -> $word
            // - frame[2] = $item
            var words = game.Strings[frame[1]];
            int itemId = frame[2];

            // Remove leading and trailing spaces and combine multiple spaces.
            words = StringHelpers.NormalizeSpaces(words);

            // Add each word.
            foreach (var word in words.Split())
            {
                game.WordMap.AddAdjective(word, itemId);
            }
            return 0;
        }

        public static readonly IntrinsicFunctionDef[] Intrinsics = new IntrinsicFunctionDef[]
        {
            new IntrinsicFunctionDef(
                "NewItem",
                new ParamDef[] {
                    new ParamDef("$namePrefix", Types.String)
                },
                /*returnType*/ Types.Item,
                _NewItem
                ),
            new IntrinsicFunctionDef(
                "GetItem",
                new ParamDef[] {
                    new ParamDef("$name", Types.String)
                },
                /*returnType*/ Types.Item,
                _GetItem
                ),
            new IntrinsicFunctionDef(
                "EndGame",
                new ParamDef[] {
                    new ParamDef("$isWon", Types.Bool)
                },
                /*returnType*/ Types.Void,
                _EndGame
                ),
            new IntrinsicFunctionDef(
                "Message",
                new ParamDef[] {
                    new ParamDef("$message", Types.String)
                },
                /*returnType*/ Types.Void,
                _Message
                ),
            new IntrinsicFunctionDef(
                "ListVariables",
                new ParamDef[0],
                /*returnType*/ Types.Void,
                _ListVariables
                ),
            new IntrinsicFunctionDef(
                "ListFunctions",
                new ParamDef[0],
                /*returnType*/ Types.Void,
                _ListFunctions
                ),
            new IntrinsicFunctionDef(
                "ListCommands",
                new ParamDef[0],
                /*returnType*/ Types.Void,
                _ListCommands
                ),
            new IntrinsicFunctionDef(
                "ListWords",
                new ParamDef[0],
                /*returnType*/ Types.Void,
                _ListWords
                ),
            new IntrinsicFunctionDef(
                "AddNoun",
                new ParamDef[] {
                    new ParamDef("$word", Types.String),
                    new ParamDef("$item", Types.Item)
                },
                /*returnType*/ Types.Void,
                _AddNoun
                ),
            new IntrinsicFunctionDef(
                "AddAdjectives",
                new ParamDef[] {
                    new ParamDef("$words", Types.String),
                    new ParamDef("$item", Types.Item)
                },
                /*returnType*/ Types.Void,
                _AddAdjectives
                ),
        };
    }
}
