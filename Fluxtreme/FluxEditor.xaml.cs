using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Keys = System.Windows.Forms.Keys;

namespace Fluxtreme
{
    /// <summary>
    /// Interaction logic for FluxEditor.xaml
    /// </summary>
    public partial class FluxEditor : UserControl
    {
        public event EventHandler TextChanged;

        public string Text { get { return editor.Text; } set { editor.Text = value; } }

        public FluxEditor()
        {
            InitializeComponent();

            // Symbol margin
            editor.Margins[0].Type = MarginType.Symbol;
            editor.Margins[0].Width = 20;
            //editor.Margins[0].Mask = 1 << (int)ImageIndex.ScriptError; // Error marker only
            editor.Margins[0].Cursor = MarginCursor.Arrow;
            editor.Margins[0].Sensitive = true;

            // Line numbers margin
            editor.Margins[1].Type = MarginType.Number;
            editor.Margins[1].Width = 16;
            editor.Margins[1].Mask = 0; // No markers here

            // Spacing margin
            editor.Margins[2].Type = MarginType.Color;
            editor.Margins[2].Width = 5;
            editor.Margins[2].Cursor = MarginCursor.Arrow;
            editor.Margins[2].Mask = 0; // No markers here
            editor.Margins[2].BackColor = GetColorResource("AColour.Tone1.Background.Static");

            // These key combinations put odd characters in the script. Let's disable them
            editor.AssignCmdKey(Keys.Control | Keys.Q, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.W, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.E, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.R, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.I, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.P, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.G, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.H, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.K, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.B, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.N, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.Q, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.W, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.E, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.R, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.Y, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.O, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.P, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.A, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.S, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.D, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.F, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.G, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.H, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.K, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.Z, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.X, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.C, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.V, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.B, Command.Null);
            editor.AssignCmdKey(Keys.Control | Keys.Shift | Keys.N, Command.Null);

            editor.Font = new Font("Consolas", 11.0f);
            editor.TextChanged += Editor_TextChanged;

            editor.Styles[ScintillaNET.Style.Default].BackColor = GetColorResource("AColour.Tone1.Background.Static");
            editor.Styles[ScintillaNET.Style.Default].ForeColor = GetColorResource("AColour.Glyph.Static");
            editor.CaretForeColor = Color.White;
            editor.StyleClearAll();
            editor.Styles[ScintillaNET.Style.Default].BackColor = GetColorResource("AColour.Tone1.Background.Static");
            editor.Styles[ScintillaNET.Style.Default].ForeColor = GetColorResource("AColour.Glyph.Static");
            editor.Styles[ScintillaNET.Style.LineNumber].BackColor = GetColorResource("AColour.Tone4.Background.Static");
            editor.Styles[ScintillaNET.Style.LineNumber].ForeColor = GetColorResource("AColour.Tone8.Border.Static");
            editor.SetFoldMarginColor(false, GetColorResource("AColour.Tone1.Background.Static"));
            editor.EdgeColor = GetColorResource("AColour.Tone1.Background.Static");
            editor.SetWhitespaceBackColor(false, GetColorResource("AColour.Tone4.Background.Static"));
            editor.Margins[0].BackColor = GetColorResource("AColour.Tone4.Background.Static");
            editor.Margins[1].BackColor = GetColorResource("AColour.Tone4.Background.Static");
            editor.Margins[2].BackColor = GetColorResource("AColour.Tone4.Background.Static");
        }

        public void Setup()
        {
            editor.LexerName = "python";
            editor.Styles[ScintillaNET.Style.Python.Character].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            //editor.Styles[ScintillaNET.Style.Python.ClassName].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            editor.Styles[ScintillaNET.Style.Python.CommentBlock].ForeColor = Color.FromArgb(100, 100, 100);
            editor.Styles[ScintillaNET.Style.Python.CommentLine].ForeColor = Color.FromArgb(100, 100, 100);
            //editor.Styles[ScintillaNET.Style.Python.Decorator].ForeColor = Color.FromKnownColor(KnownColor.LightBlue);
            //editor.Styles[ScintillaNET.Style.Python.DefName].ForeColor = Color.FromKnownColor(KnownColor.LightBlue);
            editor.Styles[ScintillaNET.Style.Python.Identifier].ForeColor = Color.FromKnownColor(KnownColor.White);
            editor.Styles[ScintillaNET.Style.Python.Number].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            editor.Styles[ScintillaNET.Style.Python.Operator].ForeColor = Color.FromKnownColor(KnownColor.LightGray);    // Braces and pipe symbols
            editor.Styles[ScintillaNET.Style.Python.String].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            editor.Styles[ScintillaNET.Style.Python.StringEol].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            //editor.Styles[ScintillaNET.Style.Python.Triple].ForeColor = Color.FromKnownColor(KnownColor.Red);
            //editor.Styles[ScintillaNET.Style.Python.TripleDouble].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            editor.Styles[ScintillaNET.Style.Python.Word].ForeColor = Color.FromKnownColor(KnownColor.DeepSkyBlue);
            editor.Styles[ScintillaNET.Style.Python.Word2].ForeColor = Color.FromKnownColor(KnownColor.PeachPuff);

            string[] functions = ReadResourceStrings("Fluxtreme.FluxFunctions.txt");
            Dictionary<string, string> func_dict = new Dictionary<string, string>();
            Dictionary<string, string> ident_dict = new Dictionary<string, string>();
            foreach (string f in functions)
            {
                int argspos = f.IndexOf('(');
                string fname = f.Substring(0, argspos);
                string fargs = f.Substring(argspos + 1).TrimEnd(')');

                // Because the python lexer doesn't understand the "library.function" format, we separate these.
                int pointindex = fname.IndexOf('.');
                if (pointindex > 0)
                {
                    string libname = fname.Substring(0, pointindex);
                    fname = fname.Substring(pointindex + 1);
                    func_dict[libname] = libname;
                }

                func_dict[fname] = fargs;

                // Collect arguments
                string[] args = fargs.Split(',');
                foreach (string a in args)
                {
                    ident_dict[a.Trim()] = string.Empty;
                }
            }

            editor.SetKeywords(0, string.Join(" ", func_dict.Keys));
            editor.SetKeywords(1, string.Join(" ", ident_dict.Keys));
            editor.SetProperty("lexer.python.keywords2.no.sub.identifiers", "1");
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            UpdateScrollbar();
            TextChanged?.Invoke(this, e);
        }

        private Color GetColorResource(string resourcename)
        {
            System.Windows.Media.Color c = (System.Windows.Media.Color)FindResource(resourcename);
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        private static string[] ReadResourceStrings(string resourcename)
        {
            List<string> lines = new List<string>();
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream stream = asm.GetManifestResourceStream(resourcename))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        lines.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            return lines.ToArray();
        }

        private void ScrollBar_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            editor.FirstVisibleLine = (int)scrollbar.Value;
        }

        private void editor_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            UpdateScrollbar();
        }

        private void UpdateScrollbar()
        {
            scrollbar.Minimum = 0;
            scrollbar.Maximum = editor.Lines.Count - editor.LinesOnScreen;
            scrollbar.LargeChange = editor.LinesOnScreen;
            scrollbar.ViewportSize = editor.LinesOnScreen;
            scrollbar.Value = editor.FirstVisibleLine;
            scrollbar.IsEnabled = (editor.Lines.Count > editor.LinesOnScreen);
            scrollbar.Opacity = scrollbar.IsEnabled ? 1.0 : 0.2;
        }

        private void editor_Resize(object sender, EventArgs e)
        {
            UpdateScrollbar();
        }
    }
}
