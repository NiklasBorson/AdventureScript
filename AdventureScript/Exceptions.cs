using System.Diagnostics.CodeAnalysis;

namespace AdventureScript
{
    public class ParseException : ApplicationException
    {
        public ParseException(SourcePos pos, string message) : base($"{pos}: {message}")
        {
            this.SourcePos = pos;
        }

        public SourcePos SourcePos { get; }
    }

    public struct SourcePos
    {
        public SourcePos()
        {
            FileName = string.Empty;
        }

        public SourcePos(string fileName, int lineNumber, int columnNumber)
        {
            FileName = fileName;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public static readonly SourcePos Empty = new SourcePos();

        public string FileName { get; }
        public int LineNumber { get; }
        public int ColumnNumber { get; }

        public override string ToString()
        {
            return $"{FileName}({LineNumber},{ColumnNumber})";
        }

        [DoesNotReturn]
        public void Fail(string message)
        {
            throw new ParseException(this, message);
        }
    }
}
