using System.Linq;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class SymbolExtensions
{
    extension(ISymbol symbol)
    {
        public ITypeSymbol? GetValueType()
        {
            return symbol switch
            {
                IArrayTypeSymbol arrayType => arrayType.ElementType,
                IDiscardSymbol discardSymbol => discardSymbol.Type,
                IPointerTypeSymbol pointerType => pointerType.PointedAtType,
                IPropertySymbol propertySymbol => propertySymbol.Type,
                IFieldSymbol fieldSymbol => fieldSymbol.Type,
                ILocalSymbol localSymbol => localSymbol.Type,
                IParameterSymbol parameterSymbol => parameterSymbol.Type,
                IMethodSymbol methodSymbol => methodSymbol.ReturnType,
                IEventSymbol eventSymbol => eventSymbol.Type,
                IAliasSymbol { Target: ITypeSymbol } aliasSymbol => (ITypeSymbol)aliasSymbol.Target,
                _ => null
            };
        }

        public bool IsGodotCompatible(MarshalUtils.TypeCache typeCache, out MarshalType? marshalType)
        {
            switch (symbol)
            {
                case ITypeSymbol typeSymbol:
                    marshalType = typeSymbol.GetMarshalType(typeCache);
                    return marshalType != null;

                case IMethodSymbol methodSymbol:
                    marshalType = null;
                    if (methodSymbol.IsGenericMethod) return false;
                    if (!methodSymbol.ReturnsVoid &&
                        (marshalType = methodSymbol.ReturnType.GetMarshalType(typeCache)) is null)
                        return false;
                    if (!methodSymbol.Parameters.All(p => p.IsValidGodotMethodParameter(typeCache))) return false;

                    return true;
                default:
                    marshalType = symbol.GetValueType()?.GetMarshalType(typeCache);
                    return marshalType != null;
            }
        }

        public bool IsValidGodotMethodParameter(MarshalUtils.TypeCache typeCache)
        {
            if (symbol is not IParameterSymbol parameterSymbol) return false;
            if (parameterSymbol.RefKind != RefKind.None) return false;

            if (!parameterSymbol.Type.IsMarshalled(typeCache))
                return false;

            return true;
        }
    }

    extension(ITypeSymbol typeSymbol)
    {
        public bool IsValidGodotMethodReturnType(MarshalUtils.TypeCache typeCache)
        {
            return typeSymbol.IsMarshalled(typeCache);
        }

        public MarshalType? GetMarshalType(MarshalUtils.TypeCache typeCache)
        {
            return MarshalUtils.ConvertManagedTypeToMarshalType(typeSymbol, typeCache);
        }

        public bool IsMarshalled(MarshalUtils.TypeCache typeCache)
        {
            return typeSymbol.GetMarshalType(typeCache) is not null;
        }

        VariantType? GetVariantType(MarshalUtils.TypeCache typeCache)
        {
            if (MarshalUtils.ConvertManagedTypeToMarshalType(typeSymbol, typeCache) is { } marshalType)
            {
                return MarshalUtils.ConvertMarshalTypeToVariantType(marshalType);
            }

            return null;
        }

        public bool IsVariantType(MarshalUtils.TypeCache typeCache)
        {
            return typeSymbol.GetVariantType(typeCache) is not null;
        }
    }
}