using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CodeImp.Fluxtreme
{
    public class IntegersOnlyTextBox : AutoSelectTextBox
    {
        private static readonly Regex NumericInputRegex = new Regex("^[0-9]{1,2}$");

        public IntegersOnlyTextBox()
        {
            DataObject.AddPastingHandler(this, OnPasting);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!NumericInputRegex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
            else
            {
                base.OnPreviewTextInput(e);
            }
        }

        private void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!NumericInputRegex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
