using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

public readonly struct GodotPropertyData
{
    public GodotPropertyData(IPropertySymbol propertySymbol, MarshalType type)
    {
        PropertySymbol = propertySymbol;
        Type = type;
    }

    public IPropertySymbol PropertySymbol { get; }
    public MarshalType Type { get; }
}