//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#if !NET45_OR_GREATER

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.Text;

namespace ThingsGateway.Foundation;

/// <summary>
/// 增量源生成器：VariableObject
/// </summary>
[Generator]
public sealed class VariableObjectSourceGenerator : IIncrementalGenerator
{
    private const string AttributeSource = @"
using System;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 使用源生成变量写入方法的调用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class GeneratorVariableAttribute : Attribute
    {
    }
}
";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("GeneratorVariableAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8)));

        var variableObjectTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: VariableObjectSyntaxFilter.IsCandidate,
                transform: VariableObjectSyntaxFilter.GetVariableObjectType)
            .Where(static t => t is not null)
            .Select(static (t, _) => (INamedTypeSymbol)t!)  // 明确转成 INamedTypeSymbol
            .Collect();

        var compilationAndTypes = context.CompilationProvider.Combine(variableObjectTypes);

        context.RegisterSourceOutput(compilationAndTypes, (spc, source) =>
        {
            var (compilation, types) = source;

            foreach (var typeSymbol in types.ToList().Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>())
            {
                var builder = new VariableCodeBuilder(typeSymbol);
                if (builder.TryToSourceText(out var sourceText))
                {
                    var tree = CSharpSyntaxTree.ParseText(sourceText);
                    var root = tree.GetRoot().NormalizeWhitespace();
                    var ret = root.ToFullString();
                    spc.AddSource($"{builder.GetFileName()}.g.cs", SourceText.From(ret, Encoding.UTF8));
                }
            }
        });
    }
}

#endif
