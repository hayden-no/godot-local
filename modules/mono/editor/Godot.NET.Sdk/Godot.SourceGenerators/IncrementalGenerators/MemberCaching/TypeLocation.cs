using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators.MemberCaching;

public record struct TypeLocation : IBeginEndWritable
{
    public string? Namespace { get; init; }
    public ValueArray<string> ContainingTypes { get; init; }
    public bool IsEmpty => Namespace is null && ContainingTypes.IsEmpty;
    public bool IsDefault => Namespace is null && ContainingTypes.IsDefault;
    public bool IsDefaultOrEmpty => Namespace is null && ContainingTypes.IsDefaultOrEmpty;

    public TypeLocation(string? @namespace, ValueArray<string> containingTypes)
    {
        Namespace = @namespace;
        ContainingTypes = containingTypes;
    }

    public static TypeLocation FromType(INamedTypeSymbol? namedTypeSymbol, int skip = 1) =>
        FromType((ISymbol?)namedTypeSymbol, skip);

    public static TypeLocation FromType(ISymbol? symbol, int skip = 0)
    {
        if (symbol is null) return new(null, ImmutableArray<string>.Empty);

        var builder = ImmutableArray<string>.Empty.ToBuilder();
        INamedTypeSymbol? current = symbol as INamedTypeSymbol ?? symbol.ContainingType;
        string namespaceStr;

        var ns = symbol.ContainingNamespace;
        if (ns.IsGlobalNamespace)
            namespaceStr = string.Empty;
        else
            namespaceStr = ns.FullQualifiedNameOmitGlobal();

        while (current is not null)
        {
            if (skip > 0)
            {
                skip--;
            }
            else
            {
                builder.Insert(
                    0,
                    string.Join(
                        " ",
                        "partial",
                        current.GetDeclarationKeyword(),
                        current.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                    )
                );
            }

            current = current.ContainingType;
        }

        return new(namespaceStr, builder.ToImmutableArray());
    }

    [Pure]
    public TypeLocation NewChild(string modifiers, string declarationKeyword, string name)
    {
        return this with {ContainingTypes = ContainingTypes.Add(string.Join(" ", modifiers, declarationKeyword, name))};
    }

    [Pure]
    public TypeLocation NewChild(string fullDeclaration)
    {
        return this with { ContainingTypes = [..ContainingTypes, fullDeclaration] };
    }

    /// <inheritdoc />
    public void Begin(FormatWriter writer, CancellationToken cancellationToken = default)
    {
        writer.WriteNamespace(Namespace);
        writer.EnsureNewLines(2);

        foreach (var containingType in ContainingTypes)
        {
            if (cancellationToken.IsCancellationRequested) return;
            writer.WriteLine(containingType);
            writer.WriteBlockStart();
        }
    }

    /// <inheritdoc />
    public void End(FormatWriter writer, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < ContainingTypes.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested) return;
            writer.WriteBlockEnd();
        }
    }
}