using System;
using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis.Binding
{
    internal class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression)
        {
            
          
            Variable = variable;
            Expresion = expression;
        }

        public override TypeSymbol Type => Expresion.Type;

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;

        public VariableSymbol Variable { get; }
        public BoundExpression Expresion { get; }
    }
}
