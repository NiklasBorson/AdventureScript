using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventureScript
{
    record class CommandDef(
        string CommandSpec,
        Regex MatchExpr,
        IList<ParamDef> Params,
        FunctionBody Body
        );

    class CommandMap : IEnumerable<CommandDef>
    {
        List<CommandDef> m_commandList = new List<CommandDef>();

        public void Add(CommandDef def)
        {
            m_commandList.Add(def);
        }

        public bool InvokeCommandLine(GameState game, string commandLine, bool warnIfInvalid)
        {
            string input = StringHelpers.NormalizeInputString(
                commandLine,
                game.IntrinsicVars.IgnoreWords
                );

            foreach (var def in m_commandList)
            {
                var match = def.MatchExpr.Match(input);
                if (match.Success)
                {
                    return InvokeCommand(game, def, match.Groups);
                }
            }

            if (warnIfInvalid)
            {
                game.OutputInvalidCommand();
            }
            return false;
        }

        bool InvokeCommand(GameState game, CommandDef def, IReadOnlyList<Group> groups)
        {
            // Captures 1..N map to parameters 0..N-1.
            Debug.Assert(groups.Count - 1 == def.Params.Count);
            if (groups.Count - 1 < def.Params.Count)
            {
                game.OutputInvalidCommand();
                return false;
            }

            // Try mapping each of the parameters to values.
            // These are stored in the "stack" frame at indices 1..N.
            var frame = new int[def.Body.FrameSize];
            for (int i = 0; i < def.Params.Count; i++)
            {
                if (!TryMapParamValue(
                    game, 
                    groups[i + 1].Value, 
                    def.Params[i].Type, 
                    out frame[i + 1]
                    ))
                {
                    return false;
                }
            }

            def.Body.Invoke(game, frame);
            return true;
        }

        bool TryMapParamValue(GameState game, string input, TypeDef type, out int value)
        {
            if (type == Types.Item)
            {
                return game.TrySelectItem(input, out value);
            }
            else if (type.IsEnumType)
            {
                var valueNames = type.ValueNames;
                for (int i = 0; i < valueNames.Count; i++)
                {
                    if (string.Compare(
                        input,
                        valueNames[i],
                        /*ignoreCase*/ true
                        ) == 0)
                    {
                        value = i;
                        return true;
                    }
                }
            }
            else if (type == Types.String)
            {
                value = game.Strings[input];
                return true;
            }
            else if (type == Types.Int)
            {
                if (Int32.TryParse(input, out value))
                {
                    return true;
                }
            }
            else if (type == Types.Bool)
            {
                switch (input)
                {
                    case "false":
                    case "no":
                    case "0":
                        value = 0;
                        return true;

                    case "true":
                    case "yes":
                    case "1":
                        value = 1;
                        return true;
                }
            }

            game.OutputInvalidCommandArg(input);
            value = 0;
            return false;
        }

        public void SaveDefinitions(GameState game, CodeWriter writer)
        {
            foreach (var def in m_commandList)
            {

                writer.Write("command \"");
                writer.Write(def.CommandSpec);
                writer.Write("\"");
                def.Body.Write(game, writer);
            }
        }

        public IEnumerator<CommandDef> GetEnumerator()
        {
            return m_commandList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_commandList.GetEnumerator();
        }
    }
}
