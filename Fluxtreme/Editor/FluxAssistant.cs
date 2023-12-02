using CodeImp.Fluxtreme.Tools;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeImp.Fluxtreme.Editor
{
    public class FluxAssistant : IAssistant
    {
        private enum Images : int
        {
            Function = 0,
            Parameter = 1,
            Variable = 2
        };

        private Scintilla editor;

        // Constructor
        public FluxAssistant(Scintilla editor)
        {
            this.editor = editor;
            editor.AutoCIgnoreCase = true;
            editor.CharAdded += Editor_CharAdded;
            editor.AutoCCompleted += Editor_AutoCCompleted;
            editor.RegisterRgbaImage((int)Images.Function, Properties.Resources.Function);
            editor.RegisterRgbaImage((int)Images.Parameter, Properties.Resources.Label);
            editor.RegisterRgbaImage((int)Images.Variable, Properties.Resources.Variable);
        }

        private void Editor_AutoCCompleted(object sender, AutoCSelectionEventArgs e)
        {
            if (e.Text.EndsWith(":"))
            {
                editor.InsertText(editor.CurrentPosition, " ");
                editor.AnchorPosition++;
                editor.CurrentPosition++;
            }
        }

        private void Editor_CharAdded(object sender, CharAddedEventArgs e)
        {
            if ((editor.GetStyleAt(editor.CurrentPosition) != (int)FluxStyles.String) &&
                (editor.GetStyleAt(editor.CurrentPosition) != (int)FluxStyles.Comment) &&
                (editor.GetStyleAt(editor.CurrentPosition) != (int)FluxStyles.RegEx))
            {
                // When a ( or , is typed and we are in the scope of a function call,
                // we want to list the function parameters for insertion...
                if ((e.Char == '(') || (e.Char == ','))
                {
                    if (e.Char == ',')
                    {
                        editor.InsertText(editor.CurrentPosition, " ");
                        editor.CurrentPosition++;
                    }

                    string func = FluxLexer.FunctionFromPosition(editor, editor.CurrentPosition);
                    if (func != null)
                    {
                        ShowAutoComplete(false, false, FluxLexer.Functions[func]);
                    }
                }
                
                // When identifier characters are typed and we are not inside
                // a string, comment or regex, then show all identifiers...
                if (FluxLexer.IdentifierChars.Contains((char)e.Char) || (e.Char == '.'))
                {
                    ShowAutoComplete(true, true, null);
                }
            }
        }

        private void ShowAutoComplete(bool functions, bool variables, IReadOnlyList<string> parameters)
        {
            List<string> items = new List<string>();
            string functionicon = ((int)Images.Function).ToString(CultureInfo.InvariantCulture);
            string parametericon = ((int)Images.Parameter).ToString(CultureInfo.InvariantCulture);
            string variableicon = ((int)Images.Variable).ToString(CultureInfo.InvariantCulture);

            if (parameters != null)
            {
                items.AddRange(parameters.Select(p => p + ":?" + parametericon));
            }

            if (functions)
            {
                items.AddRange(FluxLexer.Functions.Keys.Select(f => f + "?" + functionicon));
            }

            if (variables)
            {
                items.AddRange(FluxLexer.FindAllVariables(editor, true).Select(v => v + "?" + variableicon));
            }

            TextRange r = FluxLexer.IdentifierFromPosition(editor, editor.CurrentPosition);
            string firstchars = editor.GetTextRange(r.Start, editor.CurrentPosition - r.Start);

            // Filter the items by the characters already typed
            List<string> filtered = items.Where(i => i.IndexOf(firstchars, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            filtered.Sort();

            editor.AutoCShow(editor.CurrentPosition - r.Start, string.Join(" ", filtered));
        }
    }
}
