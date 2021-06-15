using System;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(string name, Type type)
        {
            Name = name;
            Type = type;
        }
        public override Type Type { get; }

        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;

        public string Name { get; }
    }
}
