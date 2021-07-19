using Lib.CodeAnalysis.Symbols;

namespace complier.CodeAnalysis.Binding
{
    internal sealed class Conversion
    {

        public static readonly Conversion None = new Conversion(exist: false, isIdentity: false, isImplicit: false);
        public static readonly Conversion Identity = new Conversion(exist: true, isIdentity: true, isImplicit: true);
        public static readonly Conversion Implicit = new Conversion(exist: true, isIdentity: false, isImplicit: true);
        public static readonly Conversion Explicit = new Conversion(exist: true, isIdentity: false, isImplicit: false);
  
        
        public bool Exist { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => Exist && !IsImplicit;
        
        private Conversion(bool exist, bool isIdentity, bool isImplicit)
        {
            Exist = exist;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }
        
        public static Conversion Classify(TypeSymbol from, TypeSymbol to)
        {
            //Identity

            if (from ==to)
            {
                return Identity;
            }
            
            if (from ==TypeSymbol.Bool|| from==TypeSymbol.Int)
            {
                if (to ==TypeSymbol.String)
                {
                    return Explicit;
                }
            }

            if (from == TypeSymbol.String)
            {
                if (to ==TypeSymbol.Bool|| to==TypeSymbol.Int)
                {
                    return Explicit;
                }
            }

            return Conversion.None;
        }
    }
}