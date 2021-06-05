using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace BlazorInteractive
{
    public static class SyntaxNodeExtensions
    {
        public static string RemoveNamespace(this SyntaxNode root) =>
            root
            .ChildNodes()
            .Where(node => node is not QualifiedNameSyntax)
            .Aggregate(new StringBuilder(), (sb, node) =>
            {
                if (node is NamespaceDeclarationSyntax @namespace)
                {
                    return sb.Append(RemoveNamespace(@namespace));
                }

                return sb.Append(node.ToFullString());
            })
            .ToString();
    }
}