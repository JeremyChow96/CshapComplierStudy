using complier.CodeAnalysis.Binding;
using complier.CodeAnalysis.Syntax;
using System;

namespace complier.CodeAnalysis
{
    internal class Evaluator
    {
        private readonly BoundExpression _root;

        public Evaluator(BoundExpression root)
        {
            this._root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
            {
                return (int)n.Value;
            }

            if (node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);

                return u.OperatorKind switch
                {
                    BoundUnaryOperatorKind.Identity => operand,
                    BoundUnaryOperatorKind.Negation => -operand,
                    _ => throw new Exception($"Unexpected unary operator {u.OperatorKind}")
                };
            }


            if (node is BoundBinaryExpression b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);


                return b.OperatorKind switch
                {
                    BoundBinaryOperatorKind.Addition => left + right,
                    BoundBinaryOperatorKind.Substraction => left - right,
                    BoundBinaryOperatorKind.Multiplication => left * right,
                    BoundBinaryOperatorKind.Division => left / right,
                    _ => throw new Exception($"Unexpected binary operator {b.OperatorKind}"),
                };
            }

            //if (node is ParenthesizedExpressionSyntax p)
            //{
            //    return EvaluateExpression(p.Expression);
            //}


            throw new Exception($"Unexpected node {node.Kind}");

        }
    }
}
