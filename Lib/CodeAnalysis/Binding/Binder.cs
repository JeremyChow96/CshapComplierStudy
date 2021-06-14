using complier.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace complier.CodeAnalysis.Binding
{

    internal sealed class Binder
    {
        private readonly List<string> _diagnostics = new List<string>();
        public IEnumerable<string> Diagnostics => _diagnostics;


        public BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");

            }
        }
        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {

            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
            if (boundOperator == null)
            {
                _diagnostics.Add($"Unary operator '{syntax.OperatorToken.Text}' is not defined for type {boundOperand.Type}.");

                return boundOperand;
            }
            return new BoundUnaryExpression(boundOperator, boundOperand);
        }


        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);
            var boundOperator = BoundBinaryOpertor.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
            // BindBinaryOperatorKind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
            if (boundOperator == null)
            {
                _diagnostics.Add($"Binary operator '{syntax.OperatorToken.Text}' is not defined for type {boundLeft.Type} and {boundRight.Type}.");

                return boundLeft;
            }
            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

//        private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type operandType)
//        {
//            if (operandType == typeof(int))
//            {
//                switch (kind)
//                {
//                    case SyntaxKind.PlusToken:
//                        return BoundUnaryOperatorKind.Identity;
//                    case SyntaxKind.MinusToken:
//                        return BoundUnaryOperatorKind.Negation;
//                    //default:
//                    //    throw new Exception($"Unexpected unary operator {kind}");
//                }
//            }

//            if (operandType == typeof(bool))
//            {
//                switch (kind)
//                {

//                    case SyntaxKind.BangToken:
//                        return BoundUnaryOperatorKind.LogicalNegation;
                    
//                }
//            }


//            return null;
//        }
//        private BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind kind, Type leftType, Type rightType)
//        {
//            if (leftType == typeof(int) && rightType == typeof(int))
//            {
//#pragma warning disable CS8509 // switch 表达式不会处理属于其输入类型的所有可能值(它并非详尽无遗)。
//                return kind switch
//#pragma warning restore CS8509 // switch 表达式不会处理属于其输入类型的所有可能值(它并非详尽无遗)。
//                {
//                    SyntaxKind.PlusToken => BoundBinaryOperatorKind.Addition,
//                    SyntaxKind.MinusToken => BoundBinaryOperatorKind.Substraction,
//                    SyntaxKind.StarToken => BoundBinaryOperatorKind.Multiplication,
//                    SyntaxKind.SlashToken => BoundBinaryOperatorKind.Division,
//                 //   _ => throw new Exception($"Unexpected binary operator {kind}"),
//                };
//            }

//            if (leftType == typeof(bool) && rightType == typeof(bool))
//            {
//                switch(kind)
//                {
//                    case SyntaxKind.AmpersandAmpersandToken:
//                        return BoundBinaryOperatorKind.LogicalAnd;
//                    case SyntaxKind.PipePipeToken:
//                        return BoundBinaryOperatorKind.LogicalOr;

//                }
//            }

         
//                return null;
            



//        }


    }
}
