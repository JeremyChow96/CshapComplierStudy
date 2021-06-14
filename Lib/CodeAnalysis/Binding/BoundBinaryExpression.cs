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



        public override Type Type => Op.ResultType;

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpreesion;

        public BoundExpression Left { get; }
        public BoundBinaryOpertor Op { get; }
        public BoundExpression Right { get; }
    }

    // internal sealed class BoundParentheiszedExpression : BoundExpression
    // {
    //     public BoundParentheiszedExpression(Bound)
    //     {
    //     }

    //     public override Type Type => throw new NotImplementedException();

    //     public override BoundNodeKind Kind => throw new NotImplementedException();
    // }
}
