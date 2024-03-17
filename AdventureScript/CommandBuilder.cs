using System.Text;
using System.Text.RegularExpressions;

namespace AdventureLib
{
    class CommandBuilder
    {
        string m_commandSpec;
        StringBuilder m_matchString = new StringBuilder();
        List<ParamDef> m_paramList = new List<ParamDef>();
        FunctionVariableFrame? m_frame = null;

        public CommandBuilder(string commandSpec)
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
            if (m_frame != null)
                throw new InvalidOperationException();
        }

        public void FinalizeCommandString(Parser parser)
        {
            CheckNotFinalized();
            m_matchString.Append('$');
            m_frame = new FunctionVariableFrame(parser, m_paramList, Types.Void);
        }

        public VariableFrame GetVariableFrame()
        {
            if (m_frame == null)
            {
                throw new InvalidOperationException();
            }
            return m_frame;
        }

        public CommandDef CreateCommand(Statement body)
        {
            var frame = GetVariableFrame();

            string pattern = m_matchString.ToString().ToLowerInvariant();

            return new CommandDef(
                m_commandSpec,
                new Regex(pattern),
                m_paramList,
                frame.FrameSize,
                body
                );
        }
    }
}
