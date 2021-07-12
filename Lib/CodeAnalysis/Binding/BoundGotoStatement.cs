namespace complier.CodeAnalysis.Binding
{
    internal sealed class  BoundGotoStatement: BoundStatement
    {
        public LabelSymbol Label { get; }

        public BoundGotoStatement(LabelSymbol label)
        {
            Label = label;
        }
        public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
    }
}