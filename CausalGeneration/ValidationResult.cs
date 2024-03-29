﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class ValidationResult
    {
        public IEnumerable<ValidationError>? Errors { get; set; }
        public bool Succeeded => Errors == null || Errors.Count() == 0;
        public static ValidationResult Success
            => new ValidationResult();

        public ValidationResult() { }
        public static ValidationResult Failed(params ValidationError[] errors)
        {
            return new ValidationResult() { Errors = errors.ToList() };
        }
        public override string ToString()
        {
            if (Errors == null)
                return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("Ошибки валидации входной каузальной модели.\n");
            foreach (var error in Errors)
            {
                sb.Append(error.ToString() + "\n");
            }
            return sb.ToString();
        }
    }
}
