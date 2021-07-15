namespace complier.CodeAnalysis.Binding
{
    internal sealed class  BoundGotoStatement: BoundStatement
    {
        public BoundLabel Label { get; }

        public BoundGotoStatement(BoundLabel label)
        {
            Label = label;
        }
        public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
    }
}