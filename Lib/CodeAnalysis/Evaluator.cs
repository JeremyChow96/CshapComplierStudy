using complier.CodeAnalysis.Binding;
using System;
using System.Collections.Generic;

namespace complier.CodeAnalysis
{

    internal class Evaluator
    {
        private readonly BoundExpression _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> variables)
        {
            this._root = root;
            this._variables = variables;
        }

        public object Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
            {
                return n.Value;
            }
            if (node is BoundVariableExpression v)
            {
                var value = _variables[v.Variable];
                return value;
            }

            if (node is BoundAssignmentExpression a)
            {
                var value = EvaluateExpression(a.Expresion);
                _variables[a.Variable] = value;
                return value;
            }

            if (node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);

                return u.Op.Kind switch
                {
                    BoundUnaryOperatorKind.Identity => (int)operand,
                    BoundUnaryOperatorKind.Negation => -(int)operand,
                    BoundUnaryOperatorKind.LogicalNegation => !(bool)operand,
                    _ => throw new Exception($"Unexpected unary operator {u.Op}")
                };
            }


            if (node is BoundBinaryExpression b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);


                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => (int)left + (int)right,
                    BoundBinaryOperatorKind.Substraction => (int)left - (int)right,
                    BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
                    BoundBinaryOperatorKind.Division => (int)left / (int)right,
                    BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
                    BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
                    BoundBinaryOperatorKind.Equals =>  Equals(left,right),
                    BoundBinaryOperatorKind.NotEquals => !Equals(left, right),

                    _ => throw new Exception($"Unexpected binary operator {b.Op.Kind}"),
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
