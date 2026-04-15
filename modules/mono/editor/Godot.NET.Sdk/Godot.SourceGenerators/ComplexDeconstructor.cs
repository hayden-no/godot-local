using System;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

public readonly struct ComplexDeconstructor
{
    ISymbol Symbol { get; }

    public static bool IsSymbolValid(ISymbol symbol, out DiagnosticDescriptor? failure)
    {
        failure = null;
        var containing = symbol.ContainingType;

        if (symbol is IPropertySymbol propertySymbol)
        {

        }

        if (symbol is IFieldSymbol fieldSymbol)
        {

        }

        throw new NotImplementedException();
    }
}
