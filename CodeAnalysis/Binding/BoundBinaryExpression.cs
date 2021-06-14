using System;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression(BoundExpression left, BoundBinaryOpertor op, BoundExpression right)
        {
            Left = left;
            Op = op;
            Right = right;
        }



        public override Type Type => Left.Type;

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpreesion;

        public BoundExpression Left { get; }
        public BoundBinaryOpertor Op { get; }
        public BoundExpression Right { get; }
    }
}
