using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.SourceGenerators;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class ClassPartialModifierCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Common.ClassPartialModifierRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Get the syntax root of the document.
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        // Get the diagnostic to fix.
        var diagnostic = context.Diagnostics.First();

        // Get the location of code issue.
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Use that location to find the containing class declaration.
        var classDeclaration = root?.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .First();

        if (classDeclaration == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Add partial modifier",
                cancellationToken => AddPartialModifierAsync(context.Document, classDeclaration, cancellationToken),
                classDeclaration.ToFullString()),
            context.Diagnostics);
    }

    private static async Task<Document> AddPartialModifierAsync(Document document,
        ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
    {
        // Create a new partial modifier.
        var partialModifier = SyntaxFactory.Token(SyntaxKind.PartialKeyword);
        var modifiedClassDeclaration = classDeclaration.AddModifiers(partialModifier);
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        // Replace the old class declaration with the modified one in the syntax root.
        var newRoot = root!.ReplaceNode(classDeclaration, modifiedClassDeclaration);
        var newDocument = document.WithSyntaxRoot(newRoot);
        return newDocument;
    }
}