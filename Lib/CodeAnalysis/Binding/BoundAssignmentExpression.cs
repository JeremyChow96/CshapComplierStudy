using System;

namespace complier.CodeAnalysis.Binding
{
    internal class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression(string name, BoundExpression expression)
        {
            
            Name = name;
            Expresion = expression;
        }

        public override Type Type => Expresion.Type;

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;

        public string Name { get; }
        public BoundExpression Expresion { get; }
    }
}
