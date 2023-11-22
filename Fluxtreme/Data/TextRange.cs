using System;

namespace CodeImp.Fluxtreme.Data
{
    public struct TextRange : IEquatable<TextRange>
    {
        public static readonly TextRange Empty = new TextRange();

        public int StartLine;
        public int StartColumn;
        public int EndLine;
        public int EndColumn;

        public TextRange(int startline, int startcolumn, int endline, int endcolumn)
        {
            StartLine = startline;
            StartColumn = startcolumn;
            EndLine = endline;
            EndColumn = endcolumn;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is TextRange))
            {
                return false;
            }

            TextRange other = (TextRange)obj;

            return (this.StartLine == other.StartLine) &&
                   (this.StartColumn == other.StartColumn) &&
                   (this.EndLine == other.EndLine) &&
                   (this.EndColumn == other.EndColumn);
        }

        public bool Equals(TextRange other)
        {
            return (this.StartLine == other.StartLine) &&
                   (this.StartColumn == other.StartColumn) &&
                   (this.EndLine == other.EndLine) &&
                   (this.EndColumn == other.EndColumn);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartLine, StartColumn, EndLine, EndColumn);
        }
    }
}
