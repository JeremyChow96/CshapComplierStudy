namespace complier.CodeAnalysis.Binding
{

    internal  sealed  class  BoundLabelStatement: BoundStatement
    {
        public BoundLabel Label { get; }

        public BoundLabelStatement(BoundLabel label)
        {
            Label = label;
        }
        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
    }
}