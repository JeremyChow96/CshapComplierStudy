using System.Collections.Immutable;

namespace Lib.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        internal FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type) : base(name)
        {
            Parameters = parameters;
            Type = type;
        }

        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol Type { get; }


        public override SymbolKind Kind => SymbolKind.Function;
    }
}