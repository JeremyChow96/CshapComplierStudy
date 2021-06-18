using System.Collections.Generic;
using System.IO;
using System.Linq;
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




        public static TextWriter PrettyPrint(TextWriter writer,SyntaxNode node, string indent = "", bool islast = true)
        {
            //├─
            //└─
            //│
            var marker = islast ? "└───" : "├───";
            writer.Write(indent);
            writer.Write(marker);
            writer.Write(node.Kind);

            if (node is SyntaxToken t && t.Value != null)
            {
                writer.Write(" ");
                writer.Write(t.Value);
            }
            writer.WriteLine();

            indent += islast ? "    " : "│    ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
            {
                PrettyPrint(writer,child, indent, child == lastChild);
            }
            return writer;
        }
    }

}
