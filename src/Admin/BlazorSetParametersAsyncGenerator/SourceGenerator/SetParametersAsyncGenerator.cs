using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Text;

namespace Microsoft.AspNetCore.Components;

[Generator]
public sealed partial class SetParametersAsyncGenerator : IIncrementalGenerator
{
    private const string m_DoNotGenerateSetParametersAsyncAttribute = """ 
    using System;
    namespace Microsoft.AspNetCore.Components
    {
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
        internal sealed class DoNotGenerateSetParametersAsyncAttribute : Attribute { }
    }
""";

    private const string m_GenerateSetParametersAsyncAttribute = """ 
    using System;
    namespace Microsoft.AspNetCore.Components
    {
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
        internal sealed class GenerateSetParametersAsyncAttribute : Attribute
        {
            public bool RequireExactMatch { get; set; }
        }
    }
""";

    private const string m_GlobalGenerateSetParametersAsyncAttribute = """ 
    using System;
    namespace Microsoft.AspNetCore.Components
    {
        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
        internal sealed class GlobalGenerateSetParametersAsyncAttribute : Attribute
        {
            public bool Enable { get; }
            public GlobalGenerateSetParametersAsyncAttribute(bool enable = true) { Enable = enable; }
        }
    }
""";

    private static readonly DiagnosticDescriptor ParameterNameConflict = new DiagnosticDescriptor(
        id: "TG0001",
        title: "Parameter name conflict",
        messageFormat: "Parameter names are case insensitive. {0} conflicts with {1}.",
        category: "Conflict",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Parameter names must be case insensitive to be usable in routes. Rename the parameter to not be in conflict with other parameters.");

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 注入 attribute 源码
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("DoNotGenerateSetParametersAsyncAttribute.g.cs", SourceText.From(m_DoNotGenerateSetParametersAsyncAttribute, Encoding.UTF8));
            ctx.AddSource("GenerateSetParametersAsyncAttribute.g.cs", SourceText.From(m_GenerateSetParametersAsyncAttribute, Encoding.UTF8));
            ctx.AddSource("GlobalGenerateSetParametersAsyncAttribute.g.cs", SourceText.From(m_GlobalGenerateSetParametersAsyncAttribute, Encoding.UTF8));
        });

        // 筛选 ClassDeclarationSyntax
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(static c => c is not null);

        // 合并 Compilation
        var compilationProvider = context.CompilationProvider;

        var candidateClasses = classDeclarations.Combine(compilationProvider);

        context.RegisterSourceOutput(candidateClasses, static (spc, tuple) =>
        {
            var (classDeclaration, compilation) = tuple;
            Execute(spc, compilation, classDeclaration);
        });
    }

    private static void Execute(SourceProductionContext context, Compilation compilation, ClassDeclarationSyntax classDeclaration)
    {
        var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
        var classSymbol = model.GetDeclaredSymbol(classDeclaration);
        if (classSymbol is null || classSymbol.Name == "_Imports")
            return;

        var positiveAttr = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.GenerateSetParametersAsyncAttribute");
        var negativeAttr = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.DoNotGenerateSetParametersAsyncAttribute");

        if (classSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, negativeAttr)))
            return;

        if (!IsPartial(classSymbol) || !IsComponent(classDeclaration, classSymbol, compilation))
            return;

        var globalEnable = compilation.Assembly.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Microsoft.AspNetCore.Components.GlobalGenerateSetParametersAsyncAttribute")
            ?.ConstructorArguments.FirstOrDefault().Value as bool? ?? false;

        var hasPositive = classSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, positiveAttr));

        if (!globalEnable && !hasPositive)
            return;

        GenerateSetParametersAsyncMethod(context, classSymbol);
    }

    private static void GenerateSetParametersAsyncMethod(SourceProductionContext context, INamedTypeSymbol class_symbol)
    {
        var force_exact_match = class_symbol.GetAttributes().Any(a => a.NamedArguments.Any(na => na.Key == "RequireExactMatch" && na.Value.Value is bool v && v));
        var namespaceName = class_symbol.ContainingNamespace.ToDisplayString();
        var type_kind = class_symbol.TypeKind switch { TypeKind.Class => "class", TypeKind.Interface => "interface", _ => "struct" };
        var type_parameters = string.Join(", ", class_symbol.TypeArguments.Select(t => t.Name));
        type_parameters = string.IsNullOrEmpty(type_parameters) ? type_parameters : "<" + type_parameters + ">";
        context.AddCode(class_symbol.ToDisplayString() + "_override.cs", $@"
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CA2007
#pragma warning disable CS0162
#pragma warning disable CS8632
namespace {namespaceName}
{{
    public partial class {class_symbol.Name}{type_parameters}
    {{
        private bool _initialized;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task SetParametersAsync(ParameterView parameters)
        {{
            Dictionary<string,object?> parameterValues = new();
            foreach (var parameter in parameters)
            {{
              if(BlazorImplementation__WriteSingleParameter(parameter.Name, parameter.Value)==false)
              {{
                  // 如果没有处理参数，则添加到参数列表中
                  parameterValues.Add(parameter.Name, parameter.Value);
              }}
            }}
            
            if(parameterValues.Count > 0)
            {{
                parameters.SetParameterProperties(this);
            }}
            if (!_initialized)
            {{
                _initialized = true;

                return RunInitAndSetParametersAsync();
            }}
            else
            {{
                return CallOnParametersSetAsync();
            }}
        }}

        // We do not want the debugger to consider NavigationExceptions caught by this method as user-unhandled.
    #if NET9_0_OR_GREATER
        [System.Diagnostics.DebuggerDisableUserUnhandledExceptions]
    #endif
        private async Task RunInitAndSetParametersAsync()
        {{
            Task task;

            try
            {{
                OnInitialized();
                task = OnInitializedAsync();
            }}
            catch (Exception ex) when (ex is not NavigationException)
            {{
                throw;
            }}

            if (task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Canceled)
            {{
                // Call state has changed here so that we render after the sync part of OnInitAsync has run
                // and wait for it to finish before we continue. If no async work has been done yet, we want
                // to defer calling StateHasChanged up until the first bit of async code happens or until
                // the end. Additionally, we want to avoid calling StateHasChanged if no
                // async work is to be performed.
                StateHasChanged();

                try
                {{
                    await task;
                }}
                catch // avoiding exception filters for AOT runtime support
                {{
                    // Ignore exceptions from task cancellations.
                    // Awaiting a canceled task may produce either an OperationCanceledException (if produced as a consequence of
                    // CancellationToken.ThrowIfCancellationRequested()) or a TaskCanceledException (produced as a consequence of awaiting Task.FromCanceled).
                    // It's much easier to check the state of the Task (i.e. Task.IsCanceled) rather than catch two distinct exceptions.
                    if (!task.IsCanceled)
                    {{
                        throw;
                    }}
                }}

                // Don't call StateHasChanged here. CallOnParametersSetAsync should handle that for us.
            }}

            await CallOnParametersSetAsync();
        }}

        // We do not want the debugger to consider NavigationExceptions caught by this method as user-unhandled.
    #if NET9_0_OR_GREATER
        [System.Diagnostics.DebuggerDisableUserUnhandledExceptions]
    #endif
        private Task CallOnParametersSetAsync()
        {{
            Task task;

            try
            {{
                OnParametersSet();
                task = OnParametersSetAsync();
            }}
            catch (Exception ex) when (ex is not NavigationException)
            {{
    #if NET9_0_OR_GREATER
                System.Diagnostics.Debugger.BreakForUserUnhandledException(ex);
    #endif
                throw;
            }}

            // If no async work is to be performed, i.e. the task has already ran to completion
            // or was canceled by the time we got to inspect it, avoid going async and re-invoking
            // StateHasChanged at the culmination of the async work.
            var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion &&
                task.Status != TaskStatus.Canceled;

            // We always call StateHasChanged here as we want to trigger a rerender after OnParametersSet and
            // the synchronous part of OnParametersSetAsync has run.
            StateHasChanged();

            return shouldAwaitTask ?
                CallStateHasChangedOnAsyncCompletion(task) :
                Task.CompletedTask;
        }}

        // We do not want the debugger to stop more than once per user-unhandled exception.
    #if NET9_0_OR_GREATER
        [System.Diagnostics.DebuggerDisableUserUnhandledExceptions]
    #endif
        private async Task CallStateHasChangedOnAsyncCompletion(Task task)
        {{
            try
            {{
                await task;
            }}
            catch // avoiding exception filters for AOT runtime support
            {{
                // Ignore exceptions from task cancellations, but don't bother issuing a state change.
                if (task.IsCanceled)
                {{
                    return;
                }}

                throw;
            }}

            StateHasChanged();
        }}

    }}
}}
#pragma warning restore CS8632
#pragma warning restore CS0162
#pragma warning restore CA2007
");
        var bases = class_symbol.GetTypeHierarchy().Where(t => !SymbolEqualityComparer.Default.Equals(t, class_symbol));
        var members = class_symbol.GetMembers() // members of the type itself
            .Concat(bases.SelectMany(t => t.GetMembers().Where(m => m.DeclaredAccessibility != Accessibility.Private))) // plus accessible members of any base
            .Distinct(SymbolEqualityComparer.Default);
        var property_symbols = members.OfType<IPropertySymbol>();
        var writable_property_symbols = property_symbols.Where(ps =>
            !ps.IsReadOnly || ps.GetAttributes().Any(a =>
                a.AttributeClass?.Name is "CascadingParameter" or "CascadingParameterAttribute")
        );

        var parameter_symbols = writable_property_symbols
            .Where(ps => ps.GetAttributes().Any(a => false
            || a.AttributeClass.Name == "Parameter"
            || a.AttributeClass.Name == "ParameterAttribute"
            || a.AttributeClass.Name == "CascadingParameter"
            || a.AttributeClass.Name == "CascadingParameterAttribute"

        ));
        var name_conflicts = parameter_symbols.GroupBy(ps => ps.Name.ToLowerInvariant()).Where(g => g.Count() > 1);
        foreach (var conflict in name_conflicts)
        {
            var key = conflict.Key;
            var conflicting_parameters = conflict.ToList();
            foreach (var parameter in conflicting_parameters)
            {
                var this_name = parameter.Name;
                var conflicting_name = conflicting_parameters.Select(p => p.Name).FirstOrDefault(n => n != this_name);
                foreach (var location in parameter.Locations)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ParameterNameConflict, location, this_name, conflicting_name));
                }
            }
        }
        var all = parameter_symbols.ToList();
        var catch_all_parameter = parameter_symbols.FirstOrDefault(p =>
        {
            var parameter_attr = p.GetAttributes().FirstOrDefault(a => a.AttributeClass!.Name.StartsWith("Parameter"));
            return parameter_attr?.NamedArguments.Any(n => n.Key == "CaptureUnmatchedValues" && n.Value.Value is bool v && v) == true;
        });
        var lower_case_match_cases = parameter_symbols.Except(new[] { catch_all_parameter }).Select(p => $"case \"{p.Name.ToLowerInvariant()}\": this.{p.Name} = ({p.Type.ToDisplayString()}) value; break;");
        var lower_case_match_default = catch_all_parameter == null ? @"default: {return false;}" : $@"
default:
{{
    this.{catch_all_parameter.Name} ??= new System.Collections.Generic.Dictionary<string, object>();
    var writable_dict = this.{catch_all_parameter.Name};
    if (!writable_dict.TryAdd(name, value))
    {{
        writable_dict[name] = value;
    }}
    break;
}}";

        var exact_match_cases = parameter_symbols.Except(new[] { catch_all_parameter }).Select(p => $"case \"{p!.Name}\": this.{p.Name} = ({p.Type.ToDisplayString()}) value; break;");
        string exact_match_default;
        if (force_exact_match)
        {
            if (catch_all_parameter == null) // exact matches are forced, and we do not have a catch-all parameter, therefore we need to throw on unmatched parameter
            {
                exact_match_default = @"default: { return false;";
            }
            else // exact matches are forced, and we DO have a catch-all parameter, therefore we simply add that unmatched parameter to the dictionary
            {
                exact_match_default = $@"
default:
{{
    this.{catch_all_parameter.Name} ??= new System.Collections.Generic.Dictionary<string, object>();
    var writable_dict = this.{catch_all_parameter.Name};
    if (!writable_dict.TryAdd(name, value))
    {{
        writable_dict[name] = value;
    }}
    break;
}}";
            }
        }
        else
        {
            // exact matches are not forced, so if there is no exact match, we fall back to compare it in lower case
            exact_match_default = $@"
default:
{{
    switch (name.ToLowerInvariant())
    {{
        {string.Join("\n", lower_case_match_cases)}
        {lower_case_match_default}
    }}
    break;
}}
";
        }
        context.AddCode(class_symbol.ToDisplayString() + "_implementation.cs", $@"
using System;

#pragma warning disable CS0162
#pragma warning disable CS0618
#pragma warning disable CS8632
namespace {namespaceName}
{{
    public partial class {class_symbol.Name}{type_parameters}
    {{

        private bool BlazorImplementation__WriteSingleParameter(string name, object value)
        {{
            if(name != ""Body"")
            {{

                switch (name)
                {{
                    {string.Join("\n", exact_match_cases)}
                    {exact_match_default}
                }}
                return true;
            }}
           return false;
         }}
    }}
}}
#pragma warning restore CS8632
#pragma warning restore CS0618
#pragma warning restore CS0162");
    }

    private static bool IsPartial(INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .OfType<ClassDeclarationSyntax>()
            .Any(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
    }

    private static bool IsComponent(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol symbol, Compilation compilation)
    {
        if (HasUserDefinedSetParametersAsync(symbol))
            return false;

        if (classDeclaration.SyntaxTree.FilePath.EndsWith(".razor") || classDeclaration.SyntaxTree.FilePath.EndsWith(".razor.cs"))
            return true;

        var iComponent = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.IComponent");
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");

        if (iComponent == null || componentBase == null)
            return false;

        return symbol.AllInterfaces.Contains(iComponent) || SymbolEqualityComparer.Default.Equals(symbol.BaseType, componentBase);
    }

    private static bool HasUserDefinedSetParametersAsync(INamedTypeSymbol classSymbol)
    {
        return classSymbol
            .GetMembers("SetParametersAsync")
            .OfType<IMethodSymbol>()
            .Any(m =>
                m.Parameters.Length == 1 &&
                m.Parameters[0].Type.ToDisplayString() == "Microsoft.AspNetCore.Components.ParameterView" &&
                m.DeclaredAccessibility == Accessibility.Public &&
                !m.IsStatic);
    }

}