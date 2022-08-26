using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class ValidationError
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public override string ToString()
        {
            string str = "";
            if (Code != null)
            {
                str += $"Код: {Code}";
            }
            if (Code != null && Description != null)
            {
                str += "; ";
            }
            if (Description != null)
            {
                str += $"{Description}";
            }
            return str;
        }

        public ValidationError(string code, string description)
        {
            Code = code;
            Description = description;
        }
    }
}
