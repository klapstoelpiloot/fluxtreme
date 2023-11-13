using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CodeImp.Fluxtreme.Configuration
{
    internal class NotEmptyValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(str))
            {
                return new ValidationResult(false, null); //"Required value");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
