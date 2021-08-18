using System.IO;

namespace Lib.CodeAnalysis.Symbols
{
    public abstract class Symbol
    {
        private protected Symbol(string name)
        {
            Name = name;
        }
        public string Name { get; }
        public void WriteTo(TextWriter writer)
        {
            SymbolPrinter.WriteTo(this, writer);
        }
        public abstract SymbolKind Kind { get; }
        public override string ToString() => Name;
     

    }
}