using System.Collections.Generic;
using System.Reflection;
namespace complier.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract IEnumerable<SyntaxNode> GetChildren();


        //public  IEnumerable<SyntaxNode> GetChildren()
        //{
        //    var properties = GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance);
        //    foreach (var property in properties)
        //    {
        //        if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
        //        {
        //            var child = (SyntaxNode)property.GetValue(this);
        //            yield return child;
        //        }
        //        // IEnumerable<T>  =>
        //    }

        //}


    }

}
