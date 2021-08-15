using System;

namespace Lib.CodeAnalysis.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name)
        {
            IsReadOnly = isReadOnly;
            Type = type;
        }

        public bool IsReadOnly { get; }
        public TypeSymbol Type { get; }


        public override SymbolKind Kind => SymbolKind.Variable;
    }
}