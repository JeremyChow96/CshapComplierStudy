using complier.CodeAnalysis.Binding;
using System;
using System.Collections.Generic;
using complier.CodeAnalysis.Syntax;
using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis
{
    internal class Evaluator
    {
        private readonly BoundBlockStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;
        private Random _random;

        private object _lastValue;

        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();
            for (int i = 0; i < _root.Statements.Length; i++)
            {
                if (_root.Statements[i] is BoundLabelStatement l)
                {
                    labelToIndex.Add(l.Label, i + 1);
                }
            }

            var index = 0;
            while (index < _root.Statements.Length)
            {
                var s = _root.Statements[index];

                switch (s.Kind)
                {
                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement) s);
                        index++;
                        break;
                    case BoundNodeKind.VariableDeclaration:
                        EvaluateVariableDeclaration((BoundVariableDeclaration) s);
                        index++;
                        break;
                    case BoundNodeKind.GotoStatement:
                        var gs = (BoundGotoStatement) s;
                        index = labelToIndex[gs.Label];
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        var cgs = (BoundConditionalGotoStatement) s;
                        var condtion = (bool) EvaluateExpression(cgs.Condition);
                        if (condtion == cgs.JumpIfTrue)
                        {
                            index = labelToIndex[cgs.Label];
                        }
                        else
                        {
                            index++;
                        }

                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;
                    default:
                        throw new Exception($"Unexpected node {s.Kind}");
                }
            }


            return _lastValue;
        }


        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            _variables[node.Variable] = value;
            _lastValue = value;
        }


        private object EvaluateExpression(BoundExpression node)
        {
            return node.Kind switch
            {
                BoundNodeKind.LiteralExpression => EvaluateLiteralExpression((BoundLiteralExpression) node),
                BoundNodeKind.VariableExpression => EvaluateVariableExpression((BoundVariableExpression) node),
                BoundNodeKind.AssignmentExpression => EvaluateAssignmentExpression((BoundAssignmentExpression) node),
                BoundNodeKind.UnaryExpression => EvaluateUnaryExpression((BoundUnaryExpression) node),
                BoundNodeKind.BinaryExpression => EvaluateBinaryExpression((BoundBinaryExpression) node),
                BoundNodeKind.CallExpression => EvaluateCallExpression((BoundCallExpression) node),
                _ => throw new Exception($"Unexpected node {node.Kind}")
            };
        }

        private object EvaluateCallExpression(BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Input)
            {
                return Console.ReadLine();
            }
            else if (node.Function == BuiltinFunctions.Print)
            {
                var message = (string) EvaluateExpression(node.Arguments[0]);
                Console.WriteLine(message);
                return null;
            }
            else if (node.Function == BuiltinFunctions.Rnd)
            {
                var max = (int) EvaluateExpression(node.Arguments[0]);
                if (_random== null)
                {
                    _random = new Random();
                }

                return _random.Next(max);
            }
            else
            {
                throw new Exception($"Unexpected function '{node.Function}'");
            }
        }


        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            var left = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            switch (b.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (b.Type == TypeSymbol.Int)
                    {
                        return (int) left + (int) right;
                    }

                    return (string) left + (string) right;
                case BoundBinaryOperatorKind.Substraction:
                    return (int) left - (int) right;
                case BoundBinaryOperatorKind.Multiplication:
                    return (int) left * (int) right;
                case BoundBinaryOperatorKind.Division:
                    return (int) left / (int) right;
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool) left && (bool) right;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool) left || (bool) right;
                case BoundBinaryOperatorKind.BitwiseAnd when b.Type == TypeSymbol.Bool:
                    return (bool) left & (bool) right;
                case BoundBinaryOperatorKind.BitwiseAnd:
                    return (int) left & (int) right;
                case BoundBinaryOperatorKind.BitwiseOr when b.Type == TypeSymbol.Bool:
                    return (bool) left | (bool) right;
                case BoundBinaryOperatorKind.BitwiseOr:
                    return (int) left | (int) right;
                case BoundBinaryOperatorKind.BitwiseXor when b.Type == TypeSymbol.Bool:
                    return (bool) left ^ (bool) right;
                case BoundBinaryOperatorKind.BitwiseXor:
                    return (int) left ^ (int) right;
                case BoundBinaryOperatorKind.LessThan:
                    return (int) left < (int) right;
                case BoundBinaryOperatorKind.LessThanOrEquals:
                    return (int) left <= (int) right;
                case BoundBinaryOperatorKind.GreaterThan:
                    return (int) left > (int) right;
                case BoundBinaryOperatorKind.GreaterThanOrEquals:
                    return (int) left >= (int) right;
                case BoundBinaryOperatorKind.Equals:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.NotEquals:
                    return !Equals(left, right);
                default:
                    throw new Exception($"Unexpected binary operator {b.Op.Kind}");
            }
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            var operand = EvaluateExpression(u.Operand);

            switch (u.Op.Kind)
            {
                case BoundUnaryOperatorKind.Identity:
                    return (int) operand;
                case BoundUnaryOperatorKind.Negation:
                    return -(int) operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool) operand;
                case BoundUnaryOperatorKind.OnesComplement:
                    return ~(int) operand;
                default:
                    throw new Exception($"Unexpected unary operator {u.Op}");
            }
        }

        private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expresion);
            _variables[a.Variable] = value;
            return value;
        }

        private object EvaluateVariableExpression(BoundVariableExpression v)
        {
            return _variables[v.Variable];
        }

        private static object EvaluateLiteralExpression(BoundLiteralExpression n)
        {
            return n.Value;
        }
    }
}