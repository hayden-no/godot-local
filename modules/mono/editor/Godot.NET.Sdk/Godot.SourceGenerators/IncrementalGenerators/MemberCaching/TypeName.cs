using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators.MemberCaching;

public record TypeName : IWritable<TypeName.Settings>
{
    public string Name { get; }
    public string DefaultDisplayString { get; }
    public string FullyQualifiedNameIncludeGlobal { get; }
    public string FullyQualifiedNameOmitGlobal { get; }

    public TypeName(
        string name,
        string defaultDisplayString,
        string fullyQualifiedNameIncludeGlobal,
        string fullyQualifiedNameOmitGlobal
    )
    {
        Name = name;
        DefaultDisplayString = defaultDisplayString;
        FullyQualifiedNameIncludeGlobal = fullyQualifiedNameIncludeGlobal;
        FullyQualifiedNameOmitGlobal = fullyQualifiedNameOmitGlobal;
    }

    public enum Style
    {
        Default,
        NameOnly,
        FullyQualifiedIncludeGlobal,
        FullyQualifiedOmitGlobal,
    }

    public string this[Style style] => style switch
    {
        Style.Default => DefaultDisplayString,
        Style.NameOnly => Name,
        Style.FullyQualifiedIncludeGlobal => FullyQualifiedNameIncludeGlobal,
        Style.FullyQualifiedOmitGlobal => FullyQualifiedNameOmitGlobal,
        _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
    };

    public record Settings
    {
        public Style DefaultStyle;
    }

    /// <inheritdoc />
    public void Write(FormatWriter writer, Settings settings = default, CancellationToken cancellationToken = default)
    {
        writer.Write(this[settings.DefaultStyle]);
    }

    /// <inheritdoc />
    public void Write(FormatWriter writer, CancellationToken cancellationToken = default) => Write(writer, writer.GetSettings<Settings>(), cancellationToken);

    public record Pseudo : TypeName
    {
        public Pseudo(string value) : base(value, value, value, value) { }
    }

    public static TypeName Create(INamespaceOrTypeSymbol symbol)
    {
        return symbol switch
        {
            ITypeSymbol typeSymbol => new(
                typeSymbol.Name,
                typeSymbol.ToDisplayString(),
                typeSymbol.FullQualifiedNameIncludeGlobal(),
                typeSymbol.FullQualifiedNameOmitGlobal()
            ),
            INamespaceSymbol ns => new(
                ns.Name,
                ns.ToDisplayString(),
                ns.FullQualifiedNameIncludeGlobal(),
                ns.FullQualifiedNameOmitGlobal()
            ),
            _ => throw new NotImplementedException()
        };
    }

    public static TypeName? FindOrNull(ISymbol symbol)
    {
        return symbol switch
        {
            IArrayTypeSymbol iArrayTypeSymbol => Create(iArrayTypeSymbol),
            IDiscardSymbol iDiscardSymbol => Create(iDiscardSymbol.Type),
            IDynamicTypeSymbol iDynamicTypeSymbol => Create(iDynamicTypeSymbol),
            INamedTypeSymbol namedTypeSymbol => Create(namedTypeSymbol),
            INamespaceSymbol iNamespaceSymbol => Create(iNamespaceSymbol),
            IPointerTypeSymbol iPointerTypeSymbol => Create(iPointerTypeSymbol.PointedAtType),
            IPropertySymbol propertySymbol => Create(propertySymbol.Type),
            IRangeVariableSymbol iRangeVariableSymbol => new(
                iRangeVariableSymbol.Name,
                iRangeVariableSymbol.ToDisplayString(),
                iRangeVariableSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                iRangeVariableSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .TrimStart("global::")
                    .ToString()
            ),
            IFieldSymbol fieldSymbol => Create(fieldSymbol.Type),
            IFunctionPointerTypeSymbol iFunctionPointerTypeSymbol => Create(iFunctionPointerTypeSymbol),
            ILocalSymbol iLocalSymbol => Create(iLocalSymbol.Type),
            IParameterSymbol parameterSymbol => Create(parameterSymbol.Type),
            IMethodSymbol methodSymbol => Create(methodSymbol.ReturnType),
            IEventSymbol eventSymbol => Create(eventSymbol.Type),
            ITypeParameterSymbol typeParameterSymbol => Create(typeParameterSymbol),
            ITypeSymbol iTypeSymbol => Create(iTypeSymbol),
            INamespaceOrTypeSymbol iNamespaceOrTypeSymbol => Create(iNamespaceOrTypeSymbol),
            IAliasSymbol aliasSymbol => Create(aliasSymbol.Target),
            _ => null
        };
    }

    public static TypeName? CreateOrNull(ITypeSymbol? symbol) => symbol is null ? null : Create(symbol);

    public static IEnumerable<TypeName> CreateRange(IEnumerable<ITypeSymbol?> symbols)
    {
        return symbols.OfType<ITypeSymbol>().Select(Create);
    }

    public static string ChildName(string originalName, string childName) => $"{originalName}.{childName}";

    [Pure]
    public TypeName CreateChild(string name) =>
        new(
            name,
            ChildName(Name, name),
            ChildName(FullyQualifiedNameIncludeGlobal, name),
            ChildName(FullyQualifiedNameOmitGlobal, name)
        );
}