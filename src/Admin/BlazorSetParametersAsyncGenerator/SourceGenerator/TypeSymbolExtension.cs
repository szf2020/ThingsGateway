using Microsoft.CodeAnalysis;

namespace BlazorSetParametersAsyncGenerator
{
    public static class TypeSymbolExtension
    {
        public static IEnumerable<INamedTypeSymbol> GetTypeHierarchy(this INamedTypeSymbol symbol)
        {
            yield return symbol;
            if (symbol.BaseType != null)
            {
                foreach (var type in GetTypeHierarchy(symbol.BaseType))
                {
                    yield return type;
                }
            }
        }
    }
}