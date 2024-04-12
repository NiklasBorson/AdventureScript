using System.Text;

namespace AdventureScript
{
    public enum GameResult
    {
        None,
        Win,
        Loss
    }

    public interface IApiSink
    {
        void AddEnum(EnumTypeDef def);
        void AddDelegate(DelegateTypeDef def);
        void AddFunction(FunctionDef def);
        void AddProperty(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef);
        void AddVariable(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef);
        void AddConstant(SourcePos sourcePos, string[] docComments, string name, TypeDef typeDef);
    }

    public class GameState
    {
        #region Fields
        TypeMap m_typeMap = new TypeMap();
        ItemMap m_itemMap = new ItemMap();
        StringMap m_stringMap = new StringMap();
        PropertyMap m_propMap = new PropertyMap();
        FunctionMap m_funcMap = new FunctionMap();
        CommandMap m_commandMap = new CommandMap();
        CommandMap m_turnCommandMap = new CommandMap();
        bool m_hideGlobalCommands = false;
        GlobalVarMap m_varMap;
        WordMap? m_wordMap = null;
        List<FunctionBody> m_gameBlocks = new List<FunctionBody>();
        List<FunctionBody> m_turnBlocks = new List<FunctionBody>();
        List<string> m_messages = new List<string>();
        List<Drawing> m_drawings = new List<Drawing>();
        Drawing? m_currentDrawing = null;
        GameResult m_result = GameResult.None;
        #endregion

        #region Initialization
        public GameState()
        {
            m_varMap = new GlobalVarMap(m_stringMap);
        }

        public IList<string> LoadGame(string filePath)
        {
            Parser.Parse(filePath, this);

            foreach (var block in this.GameBlocks)
            {
                var frame = new int[block.FrameSize];
                block.Invoke(this, frame);
            }

            // Saving regenerates the game block from item properties.
            this.GameBlocks.Clear();

            // Perform per-turn initialization before the first turn.
            InitializeTurn();

            return m_messages;
        }
        #endregion

        #region Properties
        //
        // Getters
        //
        public GameResult Result => m_result;

        public IList<string> LastOutput => m_messages;

        public bool IsGameOver => m_result != GameResult.None;

        public Drawing? GetDrawing(int id)
        {
            return id > 0 && id <= m_drawings.Count ? m_drawings[id - 1] : null;
        }

        public void GetApis(IApiSink sink)
        {
            foreach (var def in Types)
            {
                var enumDef = def as EnumTypeDef;
                if (enumDef != null)
                {
                    sink.AddEnum(enumDef);
                }
                else
                {
                    var delegateDef = def as DelegateTypeDef;
                    if (delegateDef != null)
                    {
                        sink.AddDelegate(delegateDef);
                    }
                }
            }

            for (int i = 1; i < m_funcMap.Count; i++)
            {
                sink.AddFunction(m_funcMap[i]);
            }

            foreach (var def in m_propMap)
            {
                sink.AddProperty(def.SourcePos, def.DocComments, def.Name, def.Type);
            }

            foreach (var def in m_varMap)
            {
                if (def.IsConstant)
                {
                    sink.AddConstant(def.SourcePos, def.DocComments, def.Name, def.Type);
                }
                else
                {
                    sink.AddVariable(def.SourcePos, def.DocComments, def.Name, def.Type);
                }
            }
        }

        internal TypeMap Types => m_typeMap;
        internal PropertyMap Properties => m_propMap;
        internal FunctionMap Functions => m_funcMap;
        internal CommandMap Commands => m_commandMap;
        internal CommandMap TurnCommands => m_commandMap;
        internal GlobalVarMap GlobalVars => m_varMap;
        internal IntrinsicVars IntrinsicVars => m_varMap.Intrinsics;
        internal ItemMap Items => m_itemMap;
        internal StringMap Strings => m_stringMap;
        internal IList<FunctionBody> GameBlocks => m_gameBlocks;
        internal IList<FunctionBody> TurnBlocks => m_turnBlocks;

        internal WordMap WordMap
        {
            get
            {
                // Lazily create or recreate the word map on first access.
                if (m_wordMap == null)
                {
                    m_wordMap = new WordMap(this.Items.Count);
                }
                return m_wordMap;
            }
        }
        #endregion

        #region Game Workflow
        void InitializeTurn()
        {
            // Reset the word map before each turn.
            m_wordMap = null;

            // Reset to the global command map before each turn.
            m_turnCommandMap = new CommandMap();
            m_hideGlobalCommands = false;

            // Execute turn blocks.
            foreach (var block in this.TurnBlocks)
            {
                var frame = new int[block.FrameSize];
                block.Invoke(this, frame);
            }
        }

        internal void HideGlobalCommands()
        {
            m_hideGlobalCommands = true;
        }

        public IList<string> InvokeCommand(string commandLine)
        {
            m_messages.Clear();
            m_drawings.Clear();

            if (m_turnCommandMap.InvokeCommandLine(this, commandLine, m_hideGlobalCommands) ||
                (!m_hideGlobalCommands && m_commandMap.InvokeCommandLine(this, commandLine, true)))
            {
                InitializeTurn();
            }

            return m_messages;
        }

        internal void Tick()
        {
            InitializeTurn();
        }

        public void Save(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                Save(writer);
            }
        }

        public void Save(TextWriter writer)
        {
            var codeWriter = new CodeWriter(writer);

            WriteMessages(codeWriter);            

            this.Types.Save(writer);
            this.Properties.Save(writer);
            this.Items.SaveDefinitions(writer);
            this.GlobalVars.SaveDefinitions(this, codeWriter);
            this.Functions.SaveDefinitions(this, codeWriter);
            this.Commands.SaveDefinitions(this, codeWriter);

            codeWriter.Write("game");
            codeWriter.BeginBlock();
            this.Items.SaveProperties(this, codeWriter);
            codeWriter.EndBlock();

            foreach (var block in this.TurnBlocks)
            {
                codeWriter.Write("turn");
                block.Write(this, codeWriter);
            }
        }

        void WriteMessages(CodeWriter writer)
        {
            writer.Write("game");
            writer.BeginBlock();

            if (m_drawings.Count != 0)
            {
                var sink = new GameDrawingSink(writer);
                foreach (var drawing in m_drawings)
                {
                    writer.Write($"BeginDrawing({drawing.Width}, {drawing.Height});");
                    writer.EndLine();

                    drawing.Draw(sink);

                    writer.Write($"EndDrawing();");
                    writer.EndLine();
                }
            }

            foreach (string message in m_messages)
            {
                writer.Write("RawMessage(");
                writer.Write(StringHelpers.ToStringLiteral(message));
                writer.Write(");");
                writer.EndLine();
            }

            writer.EndBlock();
        }
        #endregion

        #region Item Selection
        string GetNoun(Span<string> list)
        {
            if (list.Length == 0)
            {
                return string.Empty;
            }
            else if (this.IntrinsicVars.IsNounFirst)
            {
                return list[0];
            }
            else
            {
                return list[list.Length - 1];
            }
        }

        Span<string> GetAdjectives(Span<string> list)
        {
            if (list.Length == 0)
            {
                return list;
            }
            else if (this.IntrinsicVars.IsNounFirst)
            {
                return list.Slice(1);
            }
            else
            {
                return list.Slice(0, list.Length - 1);
            }
        }

        internal bool TrySelectItem(string label, out int itemId)
        {
            itemId = 0;

            var words = label.Split().AsSpan();
            string noun = GetNoun(words);
            if (noun == string.Empty)
            {
                OutputNoItem(label);
                return false;
            }

            var adjectives = GetAdjectives(words);

            var matches = this.WordMap.GetMatches(noun, adjectives);

            switch (matches.Count)
            {
                case 0:
                    OutputNoItem(label);
                    return false;

                case 1:
                    itemId = matches[0].ItemId;
                    return true;

                default:
                    OutputAmbiguousItem(label, matches);
                    return false;
            }
        }

        public void OutputInvalidCommand()
        {
            string message = this.IntrinsicVars.InvalidCommandString;
            Message(message);
        }

        public void OutputInvalidCommandArg(string arg)
        {
            string formatString = this.IntrinsicVars.InvalidArgFormatString;
            Message(string.Format(formatString, arg));
        }

        void OutputNoItem(string label)
        {
            string formatString = this.IntrinsicVars.NoItemFormatString;
            Message(string.Format(formatString, label));
        }

        void OutputAmbiguousItem(string label, IReadOnlyList<WordMapEntry> matches)
        {
            string formatString = this.IntrinsicVars.AmbiguousItemFormatString;
            Message(string.Format(formatString, label));

            bool isNounFirst = this.IntrinsicVars.IsNounFirst;
            var b = new StringBuilder();

            foreach (var match in matches)
            {
                b.Clear();
                b.Append("- ");

                if (isNounFirst)
                {
                    b.Append(match.Noun);
                    foreach (var adj in match.Adjectives)
                    {
                        b.Append(' ');
                        b.Append(adj);
                    }
                }
                else
                {
                    foreach (var adj in match.Adjectives)
                    {
                        b.Append(adj);
                        b.Append(' ');
                    }
                    b.Append(match.Noun);
                }
                Message(b.ToString());
            }
        }

        internal void ListWords()
        {
            var wordList = new List<string>();

            var b = new StringBuilder();
            bool isNounFirst = this.IntrinsicVars.IsNounFirst;

            foreach (var itemWords in WordMap.GetAllWords())
            {
                b.Clear();
                if (isNounFirst)
                {
                    b.Append(itemWords.Noun);
                    foreach (var word in itemWords.Adjectives)
                    {
                        b.Append(' ');
                        b.Append(word);
                    }
                }
                else 
                {
                    foreach (var word in itemWords.Adjectives)
                    {
                        b.Append(word);
                        b.Append(' ');
                    }
                    b.Append(itemWords.Noun);
                }
                wordList.Add(b.ToString());
            }

            wordList.Sort();

            foreach (string s in wordList)
            {
                Message(s);
            }
        }

        #endregion

        #region Intrinsic Methods
        internal void EndGame(bool isWon)
        {
            m_result = isWon ? GameResult.Win : GameResult.Loss;
        }

        void AddNormalizedMessage(string message)
        {
            message = StringHelpers.NormalizeSpaces(message);
            if (message.Length != 0)
            {
                m_messages.Add(message);
            }
        }

        internal void Message(string message)
        {
            if (!IsGameOver)
            {
                if (message.Contains('\n'))
                {
                    foreach (var line in message.Split('\n'))
                    {
                        AddNormalizedMessage(line);
                    }
                }
                else
                {
                    AddNormalizedMessage(message);
                }
            }
        }

        internal void RawMessage(string message)
        {
            if (!IsGameOver)
            {
                m_messages.Add(message);
            }
        }

        internal void BeginDrawing(int width, int height)
        {
            m_currentDrawing = new Drawing(width, height);
        }

        internal int EndDrawing()
        {
            if (m_currentDrawing == null)
                return 0;

            m_drawings.Add(m_currentDrawing);
            m_currentDrawing = null;

            return m_drawings.Count;
        }

        internal void DrawRectangle(
            int left,
            int top,
            int width,
            int height,
            int fillColor,
            int strokeColor,
            int strokeThickness
            )
        {
            if (m_currentDrawing != null)
            {
                m_currentDrawing.AddShape(new Rectangle(
                    left,
                    top,
                    width,
                    height,
                    fillColor,
                    strokeColor,
                    strokeThickness
                    ));
            }
        }

        internal void DrawEllipse(
            int left,
            int top,
            int width,
            int height,
            int fillColor,
            int strokeColor,
            int strokeThickness
            )
        {
            if (m_currentDrawing != null)
            {
                m_currentDrawing.AddShape(new Ellipse(
                    left,
                    top,
                    width,
                    height,
                    fillColor,
                    strokeColor,
                    strokeThickness
                    ));
            }
        }
        #endregion
    }
}
