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

            editor.LexerName = "python";

            editor.Styles[ScintillaNET.Style.Default].BackColor = GetColorResource("AColour.Tone1.Background.Static");
            editor.Styles[ScintillaNET.Style.Default].ForeColor = GetColorResource("AColour.Glyph.Static");
            editor.CaretForeColor = Color.White;
            editor.StyleClearAll();
            editor.Styles[ScintillaNET.Style.Default].BackColor = GetColorResource("AColour.Tone1.Background.Static");
            editor.Styles[ScintillaNET.Style.Default].ForeColor = GetColorResource("AColour.Glyph.Static");
            editor.Styles[ScintillaNET.Style.LineNumber].ForeColor = GetColorResource("AColour.Tone8.Border.Static");
            editor.Styles[ScintillaNET.Style.LineNumber].BackColor = GetColorResource("AColour.Tone4.Background.Static");
            editor.SetFoldMarginColor(false, GetColorResource("AColour.Tone1.Background.Static"));
            editor.EdgeColor = GetColorResource("AColour.Tone1.Background.Static");
            editor.SetWhitespaceBackColor(false, GetColorResource("AColour.Tone1.Background.Static"));
            editor.Margins[0].BackColor = GetColorResource("AColour.Tone1.Background.Static");
            editor.Margins[1].BackColor = GetColorResource("AColour.Tone1.Background.Static");
            editor.Margins[2].BackColor = GetColorResource("AColour.Tone1.Background.Static");
            editor.Styles[ScintillaNET.Style.Python.Character].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            //editor.Styles[ScintillaNET.Style.Python.ClassName].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            editor.Styles[ScintillaNET.Style.Python.CommentBlock].ForeColor = Color.FromArgb(100, 100, 100);
            editor.Styles[ScintillaNET.Style.Python.CommentLine].ForeColor = Color.FromArgb(100, 100, 100);
            //editor.Styles[ScintillaNET.Style.Python.Decorator].ForeColor = Color.FromKnownColor(KnownColor.LightBlue);
            //editor.Styles[ScintillaNET.Style.Python.DefName].ForeColor = Color.FromKnownColor(KnownColor.LightBlue);
            //editor.Styles[ScintillaNET.Style.Python.Identifier].ForeColor = Color.FromKnownColor(KnownColor.Purple);
            editor.Styles[ScintillaNET.Style.Python.Number].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            editor.Styles[ScintillaNET.Style.Python.Operator].ForeColor = Color.FromKnownColor(KnownColor.LightGray);    // Braces and pipe symbols
            editor.Styles[ScintillaNET.Style.Python.String].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            editor.Styles[ScintillaNET.Style.Python.StringEol].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            editor.Styles[ScintillaNET.Style.Python.Triple].ForeColor = Color.FromKnownColor(KnownColor.Red);
            //editor.Styles[ScintillaNET.Style.Python.TripleDouble].ForeColor = Color.FromKnownColor(KnownColor.LightGreen);
            editor.Styles[ScintillaNET.Style.Python.Word].ForeColor = Color.FromKnownColor(KnownColor.DeepSkyBlue);
            editor.Styles[ScintillaNET.Style.Python.Word2].ForeColor = Color.FromKnownColor(KnownColor.PeachPuff);

            List<string> functions = new List<string>();
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream stream = asm.GetManifestResourceStream("Fluxtreme.FluxFunctions.txt"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        functions.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }

            string allfunctions = string.Join(" ", functions);

            editor.SetKeywords(0, allfunctions);
            //editor.SetKeywords(1, "bucket start stop fn or");

        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }

        private Color GetColorResource(string resourcename)
        {
            System.Windows.Media.Color c = (System.Windows.Media.Color)FindResource(resourcename);
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }
}
