using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventureScript
{
    class CommandBuilder : FunctionBuilder
    {
        string m_commandSpec;
        StringBuilder m_matchString = new StringBuilder();
        List<ParamDef> m_paramList = new List<ParamDef>();

        public CommandBuilder(Parser parser, string commandSpec) : base(parser)
        {
            m_commandSpec = commandSpec;
            m_matchString.Append('^');
        }

        public void AppendString(string text)
        {
            CheckNotFinalized();
            string escapedString = StringHelpers.EscapeRegexSpecialChars(text);
            m_matchString.Append(escapedString);
        }

        public void AppendParam(ParamDef def)
        {
            CheckNotFinalized();
            m_matchString.Append("(.+)");
            m_paramList.Add(def);
        }

        void CheckNotFinalized()
        {
            Debug.Assert(this.FrameSize == 0);
        }

        public void FinalizeCommandString(Parser parser)
        {
            CheckNotFinalized();
            m_matchString.Append('$');
            InitializeFunction(parser, m_paramList, Types.Void);
        }

        public CommandDef CreateCommand()
        {
            string pattern = m_matchString.ToString().ToLowerInvariant();

            return new CommandDef(
                m_commandSpec,
                new Regex(pattern),
                m_paramList,
                CreateFunctionBody()
                );
        }
    }
}
