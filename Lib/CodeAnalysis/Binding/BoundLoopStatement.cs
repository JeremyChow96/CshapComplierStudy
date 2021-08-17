namespace complier.CodeAnalysis.Binding
{
    internal abstract class LoopStatement : BoundStatement
    {
        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }

        public LoopStatement(BoundLabel breakLabel, BoundLabel continueLabel)
        {
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }
    }
}