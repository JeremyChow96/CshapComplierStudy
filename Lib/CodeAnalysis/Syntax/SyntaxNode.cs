using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Lib.CodeAnalysis.Syntax;

namespace complier.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        protected SyntaxNode(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }
        public abstract SyntaxKind Kind { get; }
        //public abstract IEnumerable<SyntaxNode> GetChildren();
        public virtual TextSpan Span
        {
            get
            {
                var first = GetChildren().First().Span;
                var last = GetChildren().Last().Span;
                return  TextSpan.FromBounds(first.Start, last.End);
            }
        }

        public SyntaxTree SyntaxTree { get; }

        public  SyntaxToken GetLastToken()
        {
            if (this is SyntaxToken token)
            {
                return token;
            }

            return GetChildren().Last().GetLastToken();
        }

        public TextLocation Location => new TextLocation(SyntaxTree.Text, Span);

        public IEnumerable<SyntaxNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
                {
                    var child = (SyntaxNode)property.GetValue(this);
                    if (child!= null)
                    {
                        yield return child;
                    }
                }
                else if (typeof(SeparatedSyntaxList).IsAssignableFrom(property.PropertyType))
                {
                    var separatedSyntaxList = (SeparatedSyntaxList) property.GetValue(this);
                    foreach (var  child  in separatedSyntaxList.GetWithSeparators())
                    {
                        yield return child;
                    }
                        
                }
                else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                {
                    var children = (IEnumerable<SyntaxNode>) property.GetValue(this);
                    foreach (var  child  in children)
                    {
                        if (child!=null)
                        {
                            yield return child;
                        }
                    }
                }
              
            }

        }


        public void WriteTo(TextWriter writer)
        {
            PrettyPrint(writer, this);
        }


        public static void PrettyPrint(TextWriter writer,SyntaxNode node, string indent = "", bool islast = true)
        {
            var isConsoleOut = writer == Console.Out;
        
            //├─
            //└─
            //│
            var marker = islast ? "└───" : "├───";
            if (isConsoleOut)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            
            writer.Write(indent);
            writer.Write(marker);
         

            if (isConsoleOut)
            {
                Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;
                writer.Write(node.Kind);
            }

            if (node is SyntaxToken t && t.Value != null)
            {
                writer.Write(" ");
                writer.Write(t.Value);
            }
            
            if (isConsoleOut)
            {
                Console.ResetColor();
            }
            writer.WriteLine();
            indent += islast ? "    " : "│    ";

         
            
            
            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
            {
                PrettyPrint(writer,child, indent, child == lastChild);
            }
         
        }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                WriteTo(writer);
                return writer.ToString();
            }
        }
    }

}
