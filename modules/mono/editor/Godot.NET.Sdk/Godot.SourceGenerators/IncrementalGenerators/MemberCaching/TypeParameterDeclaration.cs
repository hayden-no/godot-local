using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators.MemberCaching;

public record TypeParameterDeclaration
{
    public VarianceKind Variance { get; } = VarianceKind.None;
    public string Name { get; }
    public InheritanceDeclaration? Inheritance { get; }
    public ValueArray<string> SpecialPrefixes { get; } // such as 'class' or 'struct'
    public ValueArray<string> SpecialSuffixes { get; } // such as 'new()'

    public bool HasConstraints => !SpecialPrefixes.IsEmpty || !SpecialSuffixes.IsEmpty || Inheritance is not null;

    public TypeParameterDeclaration(
        string name,
        VarianceKind variance,
        InheritanceDeclaration? inheritance,
        ValueArray<string> specialPrefixes,
        ValueArray<string> specialSuffixes
    )
    {
        Name = name;
        Variance = variance;
        Inheritance = inheritance;
        SpecialPrefixes = specialPrefixes;
        SpecialSuffixes = specialSuffixes;
    }

    public static IEnumerable<TypeParameterDeclaration> FromType(INamedTypeSymbol typeSymbol)
    {
        if (!typeSymbol.IsGenericType) return [];

        return typeSymbol.TypeParameters.Select(t =>
            {
                return new TypeParameterDeclaration(
                    t.Name,
                    t.Variance,
                    InheritanceDeclaration.FromType(t),
                    GetSpecialPrefixes(t),
                    GetSpecialSuffixes(t)
                );
            }
        );
    }

    public static ValueArray<string> GetSpecialPrefixes(ITypeParameterSymbol parameter)
    {
        var list = new List<string>();
        if (parameter.IsValueType) list.Add("struct");
        if (parameter.IsReferenceType) list.Add("class");
        if (parameter.HasUnmanagedTypeConstraint) list.Add("unmanaged");
        if (parameter.HasNotNullConstraint) list.Add("notnull");

        return list.ToImmutableArray();
    }

    public static ValueArray<string> GetSpecialSuffixes(ITypeParameterSymbol parameter)
    {
        var list = new List<string>();
        if (parameter.HasConstructorConstraint) list.Add("new()");
        if (parameter.AllowsRefLikeType) list.Add("allows ref struct");

        return list.ToImmutableArray();
    }

    public string GetShort()
    {
        return Variance switch
        {
            VarianceKind.None => Name,
            VarianceKind.In => $"in {Name}",
            VarianceKind.Out => $"out {Name}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public IEnumerable<string> GetConstraints(Func<TypeName, string>? transformer = null)
    {
        transformer ??= t => t.FullyQualifiedNameIncludeGlobal;
        return SpecialPrefixes.Concat(Inheritance?.Select(transformer) ?? []).Concat(SpecialSuffixes);
    }
}