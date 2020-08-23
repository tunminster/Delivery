using System;
namespace Delivery.Database.Models
{
    public class CurrencySign
    {
        public static readonly CurrencySign BritishPound = new CurrencySign("1", "£");
        public static readonly CurrencySign US = new CurrencySign("2", "$");

        public CurrencySign(string code, string value)
        {
            Code = code;
            Value = value;
        }

        public string Code { get; set; }
        public string Value { get; set; }
    }
}
