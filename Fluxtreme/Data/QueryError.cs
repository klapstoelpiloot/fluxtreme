using System;

namespace CodeImp.Fluxtreme.Data
{
    public struct QueryError
    {
        public string Description;
        public int StartLine;
        public int StartColumn;
        public int EndLine;
        public int EndColumn;

        public bool HasTextRange => (StartLine != 0) || (StartColumn != 0) || (EndLine != 0) || (EndColumn != 0);

        public QueryError(string description)
        {
            Description = description;
            StartLine = 0;
            StartColumn = 0;
            EndLine = 0;
            EndColumn = 0;
        }

        public QueryError(string description, int startline, int startcolumn, int endline, int endcolumn)
        {
            Description = description;
            StartLine = startline;
            StartColumn = startcolumn;
            EndLine = endline;
            EndColumn = endcolumn;
        }
    }
}
