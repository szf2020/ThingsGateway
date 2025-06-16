using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorSetParametersAsyncGenerator;

[Generator]
public partial class SetParametersAsyncGenerator : ISourceGenerator
{

    private string m_DoNotGenerateSetParametersAsyncAttribute = """
        
        using System;
        namespace BlazorSetParametersAsyncGenerator
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
            internal sealed class DoNotGenerateSetParametersAsyncAttribute : Attribute
            {
            }
        }


        """
        ;
    private string m_GenerateSetParametersAsyncAttribute = """
        
        using System;
        namespace BlazorSetParametersAsyncGenerator
        {
            [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
            internal sealed class GenerateSetParametersAsyncAttribute : Attribute
            {
                public bool RequireExactMatch { get; set; }
            }
        }


        """
        ;
    private string m_GlobalGenerateSetParametersAsyncAttribute = """
        
        using System;
        namespace BlazorSetParametersAsyncGenerator
        {
            [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
            internal sealed class GlobalGenerateSetParametersAsyncAttribute : Attribute
            {
                public bool Enable { get; }

                public GlobalGenerateSetParametersAsyncAttribute(bool enable = true)
                {
                    Enable = enable;
                }
            }

        }


        """
        ;

    private static readonly DiagnosticDescriptor ParameterNameConflict = new DiagnosticDescriptor(
        id: "TG0001",
        title: "Parameter name conflict",
        messageFormat: "Parameter names are case insensitive. {0} conflicts with {1}.",
        category: "Conflict",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Parameter names must be case insensitive to be usable in routes. Rename the parameter to not be in conflict with other parameters.");

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(a =>
        {
            a.AddSource(nameof(m_DoNotGenerateSetParametersAsyncAttribute), m_DoNotGenerateSetParametersAsyncAttribute);
            a.AddSource(nameof(m_GenerateSetParametersAsyncAttribute), m_GenerateSetParametersAsyncAttribute);
            a.AddSource(nameof(m_GlobalGenerateSetParametersAsyncAttribute), m_GlobalGenerateSetParametersAsyncAttribute);
        });

        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // https://github.com/dotnet/AspNetCore.Docs/blob/1e199f340780f407a685695e6c4d953f173fa891/aspnetcore/blazor/webassembly-performance-best-practices.md#implement-setparametersasync-manually
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
        {
            return;
        }

        var candidate_classes = GetCandidateClasses(receiver, context);

        foreach (var class_symbol in candidate_classes.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>())
        {
            GenerateSetParametersAsyncMethod(context, class_symbol);
        }
    }

    private static void GenerateSetParametersAsyncMethod(GeneratorExecutionContext context, INamedTypeSymbol class_symbol)
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


    private static bool IsPartial(INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .OfType<ClassDeclarationSyntax>()
            .Any(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
    }
    private static bool IsComponent(ClassDeclarationSyntax classDeclarationSyntax, INamedTypeSymbol symbol, Compilation compilation)
    {
        if (HasUserDefinedSetParametersAsync(symbol))
        {
            // 用户自己写了方法，不生成
            return false;
        }

        if (!IsPartial(symbol))
        {
            return false;
        }

        if (classDeclarationSyntax.SyntaxTree.FilePath.EndsWith(".razor") || classDeclarationSyntax.SyntaxTree.FilePath.EndsWith(".razor.cs"))
        {
            return true;
        }



        var iComponent = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.IComponent");
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
        if (iComponent == null || componentBase == null)
            return false;

        if (SymbolEqualityComparer.Default.Equals(symbol, iComponent))
            return true;
        if (SymbolEqualityComparer.Default.Equals(symbol, componentBase))
            return true;

        return false;
    }

    /// <summary>
    /// Enumerate methods with at least one Group attribute
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> GetCandidateClasses(SyntaxReceiver receiver, GeneratorExecutionContext context)
    {
        var compilation = context.Compilation;
        var positiveAttributeSymbol = compilation.GetTypeByMetadataName("BlazorSetParametersAsyncGenerator.GenerateSetParametersAsyncAttribute");
        var negativeAttributeSymbol = compilation.GetTypeByMetadataName("BlazorSetParametersAsyncGenerator.DoNotGenerateSetParametersAsyncAttribute");

        // loop over the candidate methods, and keep the ones that are actually annotated

        // 找特性
        var assemblyAttributes = compilation.Assembly.GetAttributes();

        var enableAttr = assemblyAttributes.FirstOrDefault(attr =>
            attr.AttributeClass?.ToDisplayString() == "BlazorSetParametersAsyncGenerator.GlobalGenerateSetParametersAsyncAttribute");

        var globalEnable = false;
        if (enableAttr != null)
        {
            var arg = enableAttr.ConstructorArguments.FirstOrDefault();
            if (arg.Value is bool b)
                globalEnable = b;
        }

        foreach (ClassDeclarationSyntax class_declaration in receiver.CandidateClasses)
        {
            var model = compilation.GetSemanticModel(class_declaration.SyntaxTree);
            var class_symbol = model.GetDeclaredSymbol(class_declaration);
            if (class_symbol is null)
            {
                continue;
            }
            if (class_symbol.Name == "_Imports")
            {
                continue;
            }

            // 是否拒绝生成
            var hasNegative = class_symbol.GetAttributes().Any(ad =>
                ad.AttributeClass?.Equals(negativeAttributeSymbol, SymbolEqualityComparer.Default) == true);

            if (hasNegative)
                continue;


            if (IsComponent(class_declaration, class_symbol, compilation))
            {
                if (globalEnable)
                {
                    yield return class_symbol;
                }
                else
                {
                    // 必须显式标注 Positive Attribute
                    var hasPositive = class_symbol.GetAttributes().Any(ad =>
                        ad.AttributeClass?.Equals(positiveAttributeSymbol, SymbolEqualityComparer.Default) == true);

                    if (hasPositive)
                        yield return class_symbol;
                }
            }
            else
            {

            }
        }
    }

    /// <summary>
    /// Created on demand before each generation pass
    /// </summary>
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntax_node)
        {
            // any class with at least one attribute is a candidate for property generation
            if (syntax_node is ClassDeclarationSyntax classDeclarationSyntax)
            {
                CandidateClasses.Add(classDeclarationSyntax);
            }
            else
            {

            }
        }
    }
}