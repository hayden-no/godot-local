using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.SourceGenerators
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ClassPartialModifierAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Common.ClassPartialModifierRule, Common.OuterClassPartialModifierRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classDeclaration)
                return;

            if (context.ContainingSymbol is not INamedTypeSymbol typeSymbol)
                return;

            if (!typeSymbol.InheritsFrom("GodotSharp", GodotClasses.GodotObject))
                return;

            if (!classDeclaration.IsPartial())
                context.ReportDiagnostic(Diagnostic.Create(
                    Common.ClassPartialModifierRule,
                    classDeclaration.Identifier.GetLocation(),
                    typeSymbol.ToDisplayString()));

            var outerClassDeclaration = context.Node.Parent as ClassDeclarationSyntax;
            while (outerClassDeclaration is not null)
            {
                var outerClassTypeSymbol = context.SemanticModel.GetDeclaredSymbol(outerClassDeclaration);
                if (outerClassTypeSymbol == null)
                    return;

                if (!outerClassDeclaration.IsPartial())
                    context.ReportDiagnostic(Diagnostic.Create(
                        Common.OuterClassPartialModifierRule,
                        outerClassDeclaration.Identifier.GetLocation(),
                        outerClassTypeSymbol.ToDisplayString()));

                outerClassDeclaration = outerClassDeclaration.Parent as ClassDeclarationSyntax;
            }
        }
    }
}
