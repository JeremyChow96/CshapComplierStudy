using complier.CodeAnalysis.Binding;
using System;
using System.Collections.Generic;
using complier.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis
{
    internal class Evaluator
    {
        private readonly BoundBlockStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        private object _lastValue;

        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            var labelToIndex = new Dictionary<LabelSymbol, int>();
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
                        var condtion = (bool)EvaluateExpression(cgs.Condition);
                        if (condtion==cgs.JumpIfTrue)
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

        // private void EvaluateStatement(BoundStatement node)
        // {
        //     switch (node.Kind)
        //     {
        //         case BoundNodeKind.BlockStatement:
        //             EvaluateBlockStatement((BoundBlockStatement) node);
        //             break;
        //         case BoundNodeKind.ExpressionStatement:
        //             EvaluateExpressionStatement((BoundExpressionStatement) node);
        //             break;
        //         case BoundNodeKind.VariableDeclaration:
        //             EvaluateVariableDeclaration((BoundVariableDeclaration) node);
        //             break;
        //         case BoundNodeKind.IfStatement:
        //             EvaluateIfStatement((BoundIfStatement) node);
        //             break;
        //         case BoundNodeKind.WhileStatement:
        //             EvaluateWhileStatement((BoundWhileStatement) node);
        //             break;
        //
        //         // case BoundNodeKind.ForStatement:
        //         //     EvaluateForStatement((BoundForStatement) node);
        //         //     break;
        //         default:
        //             throw new Exception($"Unexpected node {node.Kind}");
        //     }
        // }

        // private void EvaluateForStatement(BoundForStatement node)
        // {
        //     var lowerBound = (int) EvaluateExpression(node.LowerBound);
        //     var upperBound = (int) EvaluateExpression(node.UpperBound);
        //     _variables[node.Variable] = lowerBound;
        //     for (int i = lowerBound; i <= upperBound; i++)
        //     {
        //         _variables[node.Variable] = i;
        //         EvaluateStatement(node.Body);
        //     }
        // }

        // private void EvaluateBlockStatement(BoundBlockStatement node)
        // {
        //     foreach (var statement in node.Statements)
        //     {
        //         EvaluateStatement(statement);
        //     }
        // }


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


        // private void EvaluateIfStatement(BoundIfStatement node)
        // {
        //     var condition = (bool) EvaluateExpression(node.Condition);
        //     if (condition)
        //     {
        //         EvaluateStatement(node.ThenStatement);
        //     }
        //     else if (node.ElseStatement != null)
        //     {
        //         EvaluateStatement(node.ElseStatement);
        //     }
        // }

        // private void EvaluateWhileStatement(BoundWhileStatement node)
        // {
        //     // var condition = (bool)EvaluateExpression(node.Condition);
        //     while ((bool) EvaluateExpression(node.Condition))
        //     {
        //         EvaluateStatement(node.Body);
        //     }
        // }

        private object EvaluateExpression(BoundExpression node)
        {
            return node.Kind switch
            {
                BoundNodeKind.LiteralExpression => EvaluateLiteralExpression((BoundLiteralExpression) node),
                BoundNodeKind.VariableExpression => EvaluateVariableExpression((BoundVariableExpression) node),
                BoundNodeKind.AssignmentExpression => EvaluateAssignmentExpression((BoundAssignmentExpression) node),
                BoundNodeKind.UnaryExpression => EvaluateUnaryExpression((BoundUnaryExpression) node),
                BoundNodeKind.BinaryExpression => EvaluateBinaryExpression((BoundBinaryExpression) node),
                _ => throw new Exception($"Unexpected node {node.Kind}")
            };
        }


        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            var left = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            return b.Op.Kind switch
            {
                BoundBinaryOperatorKind.Addition => (int) left + (int) right,
                BoundBinaryOperatorKind.Substraction => (int) left - (int) right,
                BoundBinaryOperatorKind.Multiplication => (int) left * (int) right,
                BoundBinaryOperatorKind.Division => (int) left / (int) right,
                BoundBinaryOperatorKind.LogicalAnd => (bool) left && (bool) right,
                BoundBinaryOperatorKind.LogicalOr => (bool) left || (bool) right,
                BoundBinaryOperatorKind.BitwiseAnd when b.Type == typeof(bool) => (bool) left & (bool) right,
                BoundBinaryOperatorKind.BitwiseAnd => (int) left & (int) right,
                BoundBinaryOperatorKind.BitwiseOr when b.Type == typeof(bool) => (bool) left | (bool) right,
                BoundBinaryOperatorKind.BitwiseOr => (int) left | (int) right,
                BoundBinaryOperatorKind.BitwiseXor when b.Type == typeof(bool) => (bool) left ^ (bool) right,
                BoundBinaryOperatorKind.BitwiseXor => (int) left ^ (int) right,
                BoundBinaryOperatorKind.LessThan => (int) left < (int) right,
                BoundBinaryOperatorKind.LessThanOrEquals => (int) left <= (int) right,
                BoundBinaryOperatorKind.GreaterThan => (int) left > (int) right,
                BoundBinaryOperatorKind.GreaterThanOrEquals => (int) left >= (int) right,
                BoundBinaryOperatorKind.Equals => Equals(left, right),
                BoundBinaryOperatorKind.NotEquals => !Equals(left, right),
                _ => throw new Exception($"Unexpected binary operator {b.Op.Kind}")
            };
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            var operand = EvaluateExpression(u.Operand);

            return u.Op.Kind switch
            {
                BoundUnaryOperatorKind.Identity => (int) operand,
                BoundUnaryOperatorKind.Negation => -(int) operand,
                BoundUnaryOperatorKind.LogicalNegation => !(bool) operand,
                BoundUnaryOperatorKind.OnesComplement => ~(int) operand,
                _ => throw new Exception($"Unexpected unary operator {u.Op}")
            };
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