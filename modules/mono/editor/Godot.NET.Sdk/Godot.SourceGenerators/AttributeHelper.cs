using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

public static class AttributeHelper
{
    public static bool HasAttribute(this ISymbol symbol, string attributeName, bool includeInherited = true)
    {
        return symbol.GetAllAttributes(includeInherited).Any(data => data.AttributeClass?.FullQualifiedNameOmitGlobal() == attributeName);
    }

    public static bool HasAttribute(this ISymbol symbol, string attributeName, out AttributeData? attributeData, bool includeInherited = true)
    {
        attributeData=symbol.GetAllAttributes(includeInherited).FirstOrDefault(data => data.AttributeClass?.FullQualifiedNameOmitGlobal() == attributeName);
        return attributeData != null;
    }

    public static bool HasAttributes(this ISymbol symbol, string attributeName, out AttributeData[]? attributeData, bool includeInherited = true)
    {
        attributeData=symbol.GetAllAttributes(includeInherited).Where(data => data.AttributeClass?.FullQualifiedNameOmitGlobal() == attributeName).ToArray();
        return attributeData.Length > 0;
    }

    public static ImmutableArray<AttributeData> GetAllAttributes(this ISymbol symbol, bool includeInherited = true)
    {
        var attributes = symbol.GetAttributes();
        if (includeInherited && symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.BaseType != null)
        {
            attributes = attributes.AddRange(GetAllAttributes(namedTypeSymbol.BaseType, includeInherited));
        }
        return attributes;
    }

    public static bool TryGetAttributeArg<T>(this AttributeData? attributeData, int constructorIndex, out T? value)
    {
        value = default;
        if (attributeData == null) return false;

        if (attributeData.ConstructorArguments.Length <= constructorIndex) return false;
        if (attributeData.ConstructorArguments[constructorIndex].GetValue() is not T val){
            return false;
        }

        value = val;
        return true;
    }

    public static bool TryGetAttributeArg<T>(this AttributeData? attributeData, string propertyName, out T? value)
    {
        value = default;
        if (attributeData == null) return false;

        if (attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == propertyName) is
            {
                Value.Kind: not TypedConstantKind.Error
            } kvp && kvp.Value.GetValue() is T val)
        {
            value = val;
            return true;
        }

        return false;
    }

    public static bool TryGetValue<T>(this TypedConstant typedConstant, out T? value)
    {
        value = default;

        if (typedConstant.Kind is TypedConstantKind.Error or TypedConstantKind.Array) return false;

        if (typedConstant.Value is T val)
        {
            value = val;
            return true;
        }

        return false;
    }

    public static bool TryGetValues<T>(this TypedConstant typedConstant, out T[]? values)
    {
        values = default;

        if (typedConstant.Kind is TypedConstantKind.Error) return false;

        if (typedConstant.Values is {IsDefaultOrEmpty: true}) return false;

        values = typedConstant.Values.Select(v => v.Value).OfType<T>().ToArray();
        return values is {Length: > 0};
    }

    public static object? GetValue(this TypedConstant typedConstant)
    {
        if (typedConstant.Kind is TypedConstantKind.Error) return null;

        if (typedConstant.Kind is TypedConstantKind.Array)
        {
            return typedConstant.Values.Select(v => v.GetValue()).Where(v => v is not null).ToArray();
        }

        return typedConstant.Value;
    }
}
