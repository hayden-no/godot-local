using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

public struct GodotPropertyOrFieldData
{
    public GodotPropertyOrFieldData(ISymbol symbol, MarshalType type)
    {
        Symbol = symbol;
        Type = type;
    }

    public GodotPropertyOrFieldData(GodotPropertyData propertyData)
        : this(propertyData.PropertySymbol, propertyData.Type)
    {
    }

    public GodotPropertyOrFieldData(GodotFieldData fieldData)
        : this(fieldData.FieldSymbol, fieldData.Type)
    {
    }

    public ISymbol Symbol { get; }
    public MarshalType Type { get; }
}