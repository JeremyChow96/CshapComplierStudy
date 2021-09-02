﻿using complier.CodeAnalysis.Binding;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using complier.CodeAnalysis.Syntax;
using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis
{
    internal class Evaluator
    {
        private readonly BoundProgram _program;
        private readonly Dictionary<VariableSymbol, object> _globalVariables;
        private readonly Dictionary<FunctionSymbol, BoundBlockStatement> _functions = new Dictionary<FunctionSymbol, BoundBlockStatement>();

        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new();
        private Random _random;

        private object _lastValue;

        public Evaluator(BoundProgram program  ,Dictionary<VariableSymbol, object> globalVariables)
        {
            _program = program;
            _globalVariables = globalVariables;
            _locals.Push(new Dictionary<VariableSymbol, object>());

            var current = program;
            while (current != null) 
            {
                foreach (var kv in current.Functions) 
                {
                    var function = kv.Key;
                    var body = kv.Value;
                    _functions.Add(function, body);
                }
                current = current.Previous;
            }
        }

     
        public object Evaluate()
        {
            var function = _program.MainFunction ?? _program.ScriptFunction;
            if (function==null)
            {
                return null;
            }
            var body = _functions[function];
            return EvaluateStatement(body);
        }

        private object EvaluateStatement(BoundBlockStatement body)
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();
            for (int i = 0; i < body.Statements.Length; i++)
            {
                if (body.Statements[i] is BoundLabelStatement l)
                {
                    labelToIndex.Add(l.Label, i + 1);
                }
            }

            var index = 0;
            while (index < body.Statements.Length)
            {
                var s = body.Statements[index];

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
                    case BoundNodeKind.ReturnStatement:
                        var rs = (BoundReturnStatement)s;
                        _lastValue= rs.Expression == null ? null : EvaluateExpression(rs.Expression);
                        return _lastValue;
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
            _lastValue = value;
            Assign(node.Variable, value);
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
                BoundNodeKind.ConversionExpression => EvaluateConversionExpression((BoundConversionExpression) node),
                _ => throw new Exception($"Unexpected node {node.Kind}")
            };
        }

        private static object EvaluateLiteralExpression(BoundLiteralExpression n)
        {
            return n.Value;
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

        private object EvaluateVariableExpression(BoundVariableExpression v)
        {
            
            if (v.Variable.Kind == SymbolKind.GlobalVariable)
                return _globalVariables[v.Variable];
            else
            {
                var locals = _locals.Peek();
                return locals[v.Variable];
            }
        }

        private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expresion);
            Assign(a.Variable, value);

            return value;
        }


        private object EvaluateCallExpression(BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Input)
            {
                return Console.ReadLine();
            }
            else if (node.Function == BuiltinFunctions.Print)
            {
                var message =  EvaluateExpression(node.Arguments[0]);
                Console.WriteLine(message);
                return null;
            }
            else if (node.Function == BuiltinFunctions.Rnd)
            {
                var max = (int) EvaluateExpression(node.Arguments[0]);
                if (_random == null)
                {
                    _random = new Random();
                }

                return _random.Next(max);
            }
            else
            {
                var locals = new Dictionary<VariableSymbol, object>();
                for (int i = 0; i < node.Arguments.Length; i++)
                {
                    var parameter = node.Function.Parameters[i];
                    var value = EvaluateExpression(node.Arguments[i]);
                    locals.Add(parameter, value);
                }

                _locals.Push(locals);

                var statement =_functions[node.Function];

                var result = EvaluateStatement(statement);
                _locals.Pop();

                return result;

                //  throw new Exception($"Unexpected function '{node.Function}'");
            }
        }


        private object EvaluateConversionExpression(BoundConversionExpression node)
        {
            var value = EvaluateExpression(node.Expression);
            if (node.Type == TypeSymbol.Any)
            {
                return value;
            }
            else if (node.Type == TypeSymbol.Bool)
            {
                return Convert.ToBoolean(value);
            }
            else if (node.Type == TypeSymbol.Int)
            {
                return Convert.ToInt32(value);
            }
            else if (node.Type == TypeSymbol.String)
            {
                return Convert.ToString(value);
            }
            else
            {
                throw new Exception($"Unexpected type '{node.Type}'");
            }
        }



        private void Assign(VariableSymbol v, object value)
        {
            if (v.Kind == SymbolKind.GlobalVariable)
            {
                _globalVariables[v] = value;
            }
            else
            {
                var locals = _locals.Peek();
                locals[v] = value;
            }
        }
    }
}