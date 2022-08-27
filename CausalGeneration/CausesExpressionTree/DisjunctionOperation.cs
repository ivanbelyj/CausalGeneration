﻿using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.CausesExpressionTree
{
    public class DisjunctionOperation : CausesOperation
    {
        public DisjunctionOperation(IEnumerable<CausesExpression> operands) : base(operands) { }
        public override bool Evaluate()
        {
            foreach (CausesExpression term in Operands)
            {
                if (term.Evaluate())
                    return true;
            }
            return false;
        }
    }
}