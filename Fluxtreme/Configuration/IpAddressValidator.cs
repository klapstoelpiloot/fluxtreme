using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CodeImp.Fluxtreme.Configuration
{
    public class IpAddressValidator : ValidationRule
    {
        private static readonly Regex AddressCheck = new Regex("^[a-z0-9\\-\\.]+(:[0-9]{1,5})?$");

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = Convert.ToString(value);
            if(AddressCheck.IsMatch(str))
            {
                return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, null); //"Must be an IP address or DNS name, optionally with port number");
            }
        }
    }
}
