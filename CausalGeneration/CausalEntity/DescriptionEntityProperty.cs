﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.CausalEntity
{
    public class DescriptionEntityProperty
    {
        public string? Name { get; set; }
        public string? Text { get; set; }

        public override string ToString() => Name + " " + Text;
    }
}
