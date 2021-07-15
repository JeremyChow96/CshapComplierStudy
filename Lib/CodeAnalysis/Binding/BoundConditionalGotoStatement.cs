namespace complier.CodeAnalysis.Binding
{
    internal sealed class BoundConditionalGotoStatement: BoundStatement
    {
        public BoundLabel Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfTrue { get; }

        public BoundConditionalGotoStatement(BoundLabel label,BoundExpression condition,bool jumpIfTrue=true)
        {
            Label = label;
            Condition = condition;
            JumpIfTrue = jumpIfTrue;
        }
        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
    }
}