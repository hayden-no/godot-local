using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators.MemberCaching;

public record InheritanceDeclaration : IReadOnlyList<TypeName>
{
    public TypeName? BaseType { get; }
    public ValueArray<TypeName> Interfaces { get; }
    public bool IsEmpty => BaseType is null && Interfaces.IsEmpty;

    public InheritanceDeclaration(TypeName? baseType, ValueArray<TypeName> interfaces)
    {
        BaseType = baseType;
        Interfaces = interfaces;
    }

    public static InheritanceDeclaration FromType(ITypeSymbol typeSymbol)
    {
        return new(
            TypeName.CreateOrNull(typeSymbol.BaseType),
            TypeName.CreateRange(typeSymbol.Interfaces).ToImmutableArray()
        );
    }

    /// <inheritdoc />
    public IEnumerator<TypeName> GetEnumerator()
    {
        if (BaseType is not null) yield return BaseType;

        if (!Interfaces.IsDefaultOrEmpty)
            foreach (var i in Interfaces)
                yield return i;
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public int Count
    {
        get
        {
            int result = BaseType is not null ? 1 : 0;
            if (!Interfaces.IsDefaultOrEmpty) result += Interfaces.Length;

            return result;
        }
    }

    /// <inheritdoc />
    public TypeName this[int index]
    {
        get
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

            if (BaseType is null)
            {
                return Interfaces[index];
            }
            else
            {
                if (index == 0) return BaseType;

                return Interfaces[index - 1];
            }
        }
    }
}