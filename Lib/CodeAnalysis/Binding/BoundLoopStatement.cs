namespace complier.CodeAnalysis.Binding
{
    internal abstract class BoundLoopStatement : BoundStatement
    {
        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }

        public BoundLoopStatement(BoundLabel breakLabel, BoundLabel continueLabel)
        {
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }
    }
}