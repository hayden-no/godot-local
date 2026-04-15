using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Godot.SourceGenerators
{
    [Generator]
    public class ScriptPathAttributeGenerator : IIncrementalGenerator
    {
        const string GENERATOR_NAME = "ScriptPathAttribute";

        public class Options
        {
            public bool isGeneratorEnabled { get; }
            public bool isToolsProject { get; }
            public string projectDir { get; }
            public bool isValid { get; }

            public Options(bool isGeneratorEnabled, bool isToolsProject, string projectDir, bool isValid)
            {
                this.isGeneratorEnabled = isGeneratorEnabled;
                this.isToolsProject = isToolsProject;
                this.projectDir = projectDir;
                this.isValid = isValid;
            }

            public override string ToString()
            {
                return
                    $"{{ isGeneratorEnabled = {isGeneratorEnabled}, isToolsProject = {isToolsProject}, projectDir = {projectDir}, isValid = {isValid} }}";
            }

            public override bool Equals(object? value)
            {
                return value is Options other &&
                    EqualityComparer<bool>.Default.Equals(other.isGeneratorEnabled, isGeneratorEnabled) &&
                    EqualityComparer<bool>.Default.Equals(other.isToolsProject, isToolsProject) &&
                    EqualityComparer<string>.Default.Equals(other.projectDir, projectDir) &&
                    EqualityComparer<bool>.Default.Equals(other.isValid, isValid);
            }

            public override int GetHashCode()
            {
                var hash = 0x7a2f0b42;
                hash = (-1521134295 * hash) + EqualityComparer<bool>.Default.GetHashCode(isGeneratorEnabled);
                hash = (-1521134295 * hash) + EqualityComparer<bool>.Default.GetHashCode(isToolsProject);
                hash = (-1521134295 * hash) + EqualityComparer<string>.Default.GetHashCode(projectDir);

                return (-1521134295 * hash) + EqualityComparer<bool>.Default.GetHashCode(isValid);
            }
        }

        private static readonly IEqualityComparer<SyntaxReference> SyntaxReferenceSyntaxTreeComparer =
            EqualityComparer<SyntaxReference>.Create(
                (left, right) => Equals(left?.SyntaxTree, right?.SyntaxTree),
                reference => reference?.SyntaxTree.GetHashCode() ?? 0
            );

        /// <inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // var scriptClasses = context.GodotScriptClassProvider().Combine(context.AnalyzerConfigOptionsProvider);
            //
            // context.RegisterSourceOutput(scriptClasses, Action);

            var unique = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform)
                .Where(x => x.IsValid)
                .WithComparer(
                    EqualityComparer<ExtensionMethods.Context>.Create(
                        (context1, context2) =>
                            SymbolEqualityComparer.Default.Equals(context1?.Symbol, context2?.Symbol),
                        context1 => SymbolEqualityComparer.Default.GetHashCode(context1?.Symbol)
                    )
                );

            Options Selector(AnalyzerConfigOptionsProvider o, CancellationToken ct)
            {
                var isGeneratorEnabled = o.IsSourceGenEnabled(GENERATOR_NAME);
                var isToolsProject = o.IsToolsProject();
                string projectDir = string.Empty;
                bool isValid = true;

                if (!o.GlobalOptions.TryGetValue("GodotProjectDirBase64", out string? pd) || string.IsNullOrEmpty(pd))
                {
                    if (!o.GlobalOptions.TryGetValue("GodotProjectDir", out pd) || string.IsNullOrEmpty(pd))
                    {
                        // throw new InvalidOperationException("Property 'GodotProjectDir' is null or empty.");
                        isValid = false;
                    }
                }
                else
                {
                    projectDir = Encoding.UTF8.GetString(Convert.FromBase64String(pd));
                }

                return new Options(isGeneratorEnabled, isToolsProject, projectDir, isValid);
            }

            var options = context.AnalyzerConfigOptionsProvider.Select(Selector);

            context.RegisterSourceOutput(
                unique.Combine(options),
                (productionContext, tuple) =>
                {
                    var (context, options) = tuple;

                    if (!options.isValid) return;

                    var filePaths = context.Symbol.DeclaringSyntaxReferences
                        .Select(s => RelativeToDir(s.SyntaxTree.FilePath, options.projectDir))
                        .ToImmutableHashSet();
                    string namespaceName =
                        context.Symbol.ContainingNamespace != null &&
                        !context.Symbol.ContainingNamespace.IsGlobalNamespace
                            ? context.Symbol.ContainingNamespace.FullQualifiedNameOmitGlobal()
                            : string.Empty;

                    string uniqueHint =
                        context.Symbol.FullQualifiedNameOmitGlobal().SanitizeQualifiedNameForUniqueHint() +
                        "_ScriptPath.generated";

                    productionContext.AddSource(uniqueHint, GetClassDocument(context.Symbol, filePaths, namespaceName));
                }
            );

            context.RegisterSourceOutput(
                unique.Collect().Combine(options),
                (productionContext, tuple) =>
                {
                    var (contexts, options) = tuple;

                    if (!options.isValid) return;

                    var symbols = contexts.Select(x => x.Symbol).ToImmutableArray();
                    productionContext.AddSource("AssemblyScriptTypes.generated", GetAssemblyDocument(symbols));

                    // check for duplicate paths

                    HashSet<string> usedPaths = new HashSet<string>();

                    foreach (var (syntaxes, name) in symbols.Select(s => (
                            s.DeclaringSyntaxReferences.Distinct(SyntaxReferenceSyntaxTreeComparer).ToImmutableArray(),
                            s.Name)
                        ))

                    {
                        foreach (var filePath in syntaxes.Select(s => RelativeToDir(
                                    s.SyntaxTree.FilePath,
                                    options.projectDir
                                )
                            ))
                        {
                            if (!usedPaths.Add(filePath))
                            {
                                foreach (var syntax in syntaxes.Select(s => s.GetSyntax()))
                                {
                                    productionContext.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Common.MultipleClassesInGodotScriptRule,
                                            syntax.GetLocation(),
                                            name
                                        )
                                    );
                                }
                            }
                        }
                    }
                }
            );

            ExtensionMethods.Context Transform(GeneratorSyntaxContext context, CancellationToken arg2)
            {
                var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, arg2);

                if (symbol is not INamedTypeSymbol typeSymbol) return new();
                if (typeSymbol.IsStatic) return new();
                if (typeSymbol.BaseType is null ||
                    !typeSymbol.BaseType.InheritsFrom("GodotSharp", GodotClasses.GodotObject))
                    return new();

                return new(typeSymbol, context.Node as ClassDeclarationSyntax);
            }

            bool Predicate(SyntaxNode syntax, CancellationToken arg2)
            {
                if (syntax is not ClassDeclarationSyntax cds) return false;
                if (cds.IsNested()) return false;
                if (!cds.IsPartial()) return false;
                if (Path.GetFileNameWithoutExtension(cds.SyntaxTree.FilePath)
                    .Equals(cds.Identifier.ValueText, StringComparison.Ordinal))
                    return true;

                return false;
            }

            // void Action(
            //     SourceProductionContext source,
            //     (ExtensionMethods.Context Left, AnalyzerConfigOptionsProvider Right) arg2
            // )
            // {
            //     var (context, options) = arg2;
            //
            //     if (!options.IsSourceGenEnabled(GENERATOR_NAME) || options.IsToolsProject()) return;
            //
            //     if (!options.GlobalOptions.TryGetValue("GodotProjectDirBase64", out string? godotProjectDir) ||
            //         godotProjectDir!.Length == 0)
            //     {
            //         if (!options.GlobalOptions.TryGetValue("GodotProjectDir", out godotProjectDir) ||
            //             godotProjectDir.Length == 0)
            //         {
            //             throw new InvalidOperationException("Property 'GodotProjectDir' is null or empty.");
            //         }
            //     }
            //     else
            //     {
            //         godotProjectDir = Encoding.UTF8.GetString(Convert.FromBase64String(godotProjectDir));
            //     }
            //
            //     if (context.Syntax.IsNested()) return;
            //
            //     if (!Path.GetFileNameWithoutExtension(context.Syntax.SyntaxTree.FilePath)
            //         .Equals(context.Symbol.Name, StringComparison.Ordinal))
            //         return;
            // }
        }

        static string GetPathAttribute(string path) { return $"""[ScriptPathAttribute("res://{path})]"""; }

        static SourceText GetClassDocument(
            INamedTypeSymbol symbol,
            IEnumerable<string> paths,
            string? namespaceName = null
        )
        {
            var text = $"""
                        using Godot;

                        {(string.IsNullOrWhiteSpace(namespaceName) ?
                                string.Empty :
                                $"namespace {namespaceName};"
                            )}

                        {(string.Join("\n", paths.Select(GetPathAttribute)))}
                        partial class {symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {"{ }"}
                        """;

            return SourceText.From(text, Encoding.UTF8);
        }

        static SourceText GetAssemblyDocument(IEnumerable<INamedTypeSymbol> symbols)
        {
            var sb = new StringBuilder();

            sb.Append($"""[assembly: {GodotClasses.AssemblyHasScriptsAttr}(new System.Type[] """).Append("\n\t{");

            sb.Append(
                    string.Join(
                        "\n\t\t",
                        symbols.Select(sym =>
                            {
                                var qn = sym.ToDisplayString(
                                    NullableFlowState.NotNull,
                                    SymbolDisplayFormat.FullyQualifiedFormat.WithGenericsOptions(
                                        SymbolDisplayGenericsOptions.None
                                    )
                                );
                                var gn = sym.IsGenericType
                                    ? $"<{new string(',', sym.TypeParameters.Count() - 1)}>"
                                    : string.Empty;

                                return $"typeof({qn}{gn})";
                            }
                        )
                    )
                )
                .Append("\n\t})]\n");

            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }

        // public void Execute(GeneratorExecutionContext context)
        // {
        //     if (context.IsGodotSourceGeneratorDisabled("ScriptPathAttribute")) return;
        //
        //     if (context.IsGodotToolsProject()) return;
        //
        //     // NOTE: NotNullWhen diagnostics don't work on projects targeting .NET Standard 2.0
        //     // ReSharper disable once ReplaceWithStringIsNullOrEmpty
        //     if (!context.TryGetGlobalAnalyzerProperty("GodotProjectDirBase64", out string? godotProjectDir) ||
        //         godotProjectDir!.Length == 0)
        //     {
        //         if (!context.TryGetGlobalAnalyzerProperty("GodotProjectDir", out godotProjectDir) ||
        //             godotProjectDir!.Length == 0)
        //         {
        //             throw new InvalidOperationException("Property 'GodotProjectDir' is null or empty.");
        //         }
        //     }
        //     else
        //     {
        //         // Workaround for https://github.com/dotnet/roslyn/issues/51692
        //         godotProjectDir = Encoding.UTF8.GetString(Convert.FromBase64String(godotProjectDir));
        //     }
        //
        //     Dictionary<INamedTypeSymbol, IEnumerable<ClassDeclarationSyntax>> godotClasses = context.Compilation
        //         .SyntaxTrees.SelectMany(tree => tree.GetRoot()
        //             .DescendantNodes()
        //             .OfType<ClassDeclarationSyntax>()
        //             // Ignore inner classes
        //             .Where(cds => !cds.IsNested())
        //             .SelectGodotScriptClasses(context.Compilation)
        //             // Report and skip non-partial classes
        //             .Where(x =>
        //                 {
        //                     if (x.cds.IsPartial()) return true;
        //
        //                     return false;
        //                 }
        //             )
        //         )
        //         .Where(x =>
        //             // Ignore classes whose name is not the same as the file name
        //             Path.GetFileNameWithoutExtension(x.cds.SyntaxTree.FilePath) == x.symbol.Name
        //         )
        //         .GroupBy<(ClassDeclarationSyntax cds, INamedTypeSymbol symbol),
        //             INamedTypeSymbol>(x => x.symbol, SymbolEqualityComparer.Default)
        //         .ToDictionary<IGrouping<INamedTypeSymbol, (ClassDeclarationSyntax cds, INamedTypeSymbol symbol)>,
        //             INamedTypeSymbol, IEnumerable<ClassDeclarationSyntax>>(
        //             g => g.Key,
        //             g => g.Select(x => x.cds),
        //             SymbolEqualityComparer.Default
        //         );
        //
        //     var usedPaths = new HashSet<string>();
        //
        //     foreach (var godotClass in godotClasses)
        //     {
        //         VisitGodotScriptClass(
        //             context,
        //             godotProjectDir,
        //             usedPaths,
        //             symbol: godotClass.Key,
        //             classDeclarations: godotClass.Value
        //         );
        //     }
        //
        //     if (godotClasses.Count <= 0) return;
        //
        //     AddScriptTypesAssemblyAttr(context, godotClasses);
        // }
        //
        // private static void VisitGodotScriptClass(
        //     GeneratorExecutionContext context,
        //     string godotProjectDir,
        //     HashSet<string> usedPaths,
        //     INamedTypeSymbol symbol,
        //     IEnumerable<ClassDeclarationSyntax> classDeclarations
        // )
        // {
        //     var attributes = new StringBuilder();
        //
        //     // Remember syntax trees for which we already added an attribute, to prevent unnecessary duplicates.
        //     var attributedTrees = new List<SyntaxTree>();
        //
        //     foreach (var cds in classDeclarations)
        //     {
        //         if (attributedTrees.Contains(cds.SyntaxTree)) continue;
        //
        //         attributedTrees.Add(cds.SyntaxTree);
        //
        //         if (attributes.Length != 0) attributes.Append("\n");
        //
        //         string scriptPath = RelativeToDir(cds.SyntaxTree.FilePath, godotProjectDir);
        //
        //         if (!usedPaths.Add(scriptPath))
        //         {
        //             context.ReportDiagnostic(
        //                 Diagnostic.Create(
        //                     Common.MultipleClassesInGodotScriptRule,
        //                     cds.Identifier.GetLocation(),
        //                     symbol.Name
        //                 )
        //             );
        //
        //             return;
        //         }
        //
        //         attributes.Append(@"[ScriptPathAttribute(""res://");
        //         attributes.Append(scriptPath);
        //         attributes.Append(@""")]");
        //     }
        //
        //     INamespaceSymbol namespaceSymbol = symbol.ContainingNamespace;
        //     string classNs = namespaceSymbol != null && !namespaceSymbol.IsGlobalNamespace
        //         ? namespaceSymbol.FullQualifiedNameOmitGlobal()
        //         : string.Empty;
        //     bool hasNamespace = classNs.Length != 0;
        //
        //     string uniqueHint = symbol.FullQualifiedNameOmitGlobal().SanitizeQualifiedNameForUniqueHint() +
        //         "_ScriptPath.generated";
        //
        //     var source = new StringBuilder();
        //
        //     // using Godot;
        //     // namespace {classNs} {
        //     //     {attributesBuilder}
        //     //     partial class {className} { }
        //     // }
        //
        //     source.Append("using Godot;\n");
        //
        //     if (hasNamespace)
        //     {
        //         source.Append("namespace ");
        //         source.Append(classNs);
        //         source.Append(" {\n\n");
        //     }
        //
        //     source.Append(attributes);
        //     source.Append("\npartial class ");
        //     source.Append(symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        //     source.Append("\n{\n}\n");
        //
        //     if (hasNamespace)
        //     {
        //         source.Append("\n}\n");
        //     }
        //
        //     context.AddSource(uniqueHint, SourceText.From(source.ToString(), Encoding.UTF8));
        // }
        //
        // private static void AddScriptTypesAssemblyAttr(
        //     GeneratorExecutionContext context,
        //     Dictionary<INamedTypeSymbol, IEnumerable<ClassDeclarationSyntax>> godotClasses
        // )
        // {
        //     var sourceBuilder = new StringBuilder();
        //
        //     sourceBuilder.Append("[assembly:");
        //     sourceBuilder.Append(GodotClasses.AssemblyHasScriptsAttr);
        //     sourceBuilder.Append("(new System.Type[] {");
        //
        //     bool first = true;
        //
        //     foreach (var godotClass in godotClasses)
        //     {
        //         var qualifiedName = godotClass.Key.ToDisplayString(
        //             NullableFlowState.NotNull,
        //             SymbolDisplayFormat.FullyQualifiedFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
        //         );
        //         if (!first) sourceBuilder.Append(", ");
        //         first = false;
        //         sourceBuilder.Append("typeof(");
        //         sourceBuilder.Append(qualifiedName);
        //         if (godotClass.Key.IsGenericType)
        //             sourceBuilder.Append($"<{new string(',', godotClass.Key.TypeParameters.Count() - 1)}>");
        //         sourceBuilder.Append(")");
        //     }
        //
        //     sourceBuilder.Append("})]\n");
        //
        //     context.AddSource(
        //         "AssemblyScriptTypes.generated",
        //         SourceText.From(sourceBuilder.ToString(), Encoding.UTF8)
        //     );
        // }
        //
        // public void Initialize(GeneratorInitializationContext context) { }

        private static string RelativeToDir(string path, string dir)
        {
            // Make sure the directory ends with a path separator
            dir = Path.Combine(dir, " ").TrimEnd();

            if (Path.DirectorySeparatorChar == '\\') dir = dir.Replace("/", "\\") + "\\";

            var fullPath = new Uri(Path.GetFullPath(path), UriKind.Absolute);
            var relRoot = new Uri(Path.GetFullPath(dir), UriKind.Absolute);

            // MakeRelativeUri converts spaces to %20, hence why we need UnescapeDataString
            return Uri.UnescapeDataString(relRoot.MakeRelativeUri(fullPath).ToString());
        }
    }
}
