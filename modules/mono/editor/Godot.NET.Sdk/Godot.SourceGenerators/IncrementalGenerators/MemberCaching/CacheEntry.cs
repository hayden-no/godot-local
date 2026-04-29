using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators.MemberCaching;

public record CacheEntry
{
    public string Name { get; init; }
    public SymbolKind MemberKind { get; init; }
    public TypeName ValueOrReturnType { get; init; }
    public MarshalType? MarshalType { get; init; }
    public XmlComment? XmlComment { get; init; }

    public CacheEntry(string name, SymbolKind memberKind, TypeName type, XmlComment? xmlComment)
    {
        Name = name;
        MemberKind = memberKind;
        ValueOrReturnType = type;
        XmlComment = xmlComment;
    }

    public static CacheEntry Create(
        ISymbol symbol,
        SymbolKind memberKind,
        ITypeSymbol typeSymbol,
        XmlComment? xmlComment = null
    )
    {
        return new(symbol.Name, memberKind, TypeName.Create(typeSymbol), xmlComment);
    }

    public static CacheEntry? FromSymbol(ISymbol symbol, MarshalUtils.TypeCache typeCache, XmlComment? xmlComment = null, bool requireValidMarshal = true)
    {
        var type = TypeName.FindOrNull(symbol);
        if (type is null) return null;

        SourceGenerators.MarshalType? marshalType = null;
        if (requireValidMarshal && !symbol.IsGodotCompatible(typeCache, out marshalType)) return null;
        return new CacheEntry(symbol.Name, symbol.Kind, type, xmlComment) {MarshalType = marshalType};
    }

    public static ValueArray<CacheEntry> FromMembers(
        INamedTypeSymbol typeSymbol, MarshalUtils.TypeCache typeCache,
        IEqualityComparer<ISymbol>? memberComparer = null,
        Predicate<ISymbol>? memberPredicate = null, bool requireValidMarshal = true
    )
    {
        memberPredicate ??= _ => true;
        memberComparer ??= SymbolEqualityComparer.Default;
        return typeSymbol.GetMembers().Distinct(memberComparer)
            .Where(s => memberPredicate(s))
            .Select(s => FromSymbol(s, typeCache, requireValidMarshal: requireValidMarshal)).OfType<CacheEntry>().ToImmutableArray();
    }
}