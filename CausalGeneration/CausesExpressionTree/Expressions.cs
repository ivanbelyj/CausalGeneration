using CausalGeneration.Edges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.CausesExpressionTree
{
    public static class Expressions
    {
        public static ConjunctionOperation And(params CausesExpression[] expressions)
            => new ConjunctionOperation(expressions);
        public static ConjunctionOperation And(params ProbabilityEdge[] edges)
            => And(edges.Select(edge => new EdgeLeaf(edge)).ToArray());

        public static DisjunctionOperation Or(params CausesExpression[] expressions)
            => new DisjunctionOperation(expressions);
        public static DisjunctionOperation Or(params ProbabilityEdge[] edges)
            => Or(edges.Select(edge => new EdgeLeaf(edge)).ToArray());

        public static InversionOperation Not(CausesExpression expr)
            => new InversionOperation(expr);
        public static InversionOperation Not(ProbabilityEdge edge)
            => new InversionOperation(new EdgeLeaf(edge));
    }
}
