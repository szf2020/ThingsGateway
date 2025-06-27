using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class VariableObjectSyntaxFilter
{
    public const string GeneratorVariableAttributeTypeName = "ThingsGateway.Foundation.GeneratorVariableAttribute";
    public const string VariableRuntimeAttributeTypeName = "ThingsGateway.Foundation.VariableRuntimeAttribute";

    /// <summary>
    /// 语法筛选器：只筛选 ClassDeclarationSyntax
    /// </summary>
    public static bool IsCandidate(SyntaxNode syntaxNode, CancellationToken _)
    {
        return syntaxNode is ClassDeclarationSyntax;
    }

    /// <summary>
    /// 语义分析：判断是否是带 GeneratorVariableAttribute 的类
    /// </summary>
    public static INamedTypeSymbol? GetVariableObjectType(GeneratorSyntaxContext context, CancellationToken _)
    {
        var classSyntax = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
        if (classSymbol == null)
            return null;

        if (classSymbol.IsAbstract)
            return null;

        var generatorAttr = context.SemanticModel.Compilation.GetTypeByMetadataName(GeneratorVariableAttributeTypeName);
        if (generatorAttr == null)
            return null;

        if (HasAttribute(classSymbol, generatorAttr))
            return classSymbol;

        return null;
    }

    /// <summary>
    /// 判断 symbol 是否声明了指定 Attribute
    /// </summary>
    public static bool HasAttribute(INamedTypeSymbol symbol, INamedTypeSymbol attributeSymbol)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol))
                return true;
        }
        return false;
    }
}
