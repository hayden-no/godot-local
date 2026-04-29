using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

public readonly struct GodotFieldData
{
    public GodotFieldData(IFieldSymbol fieldSymbol, MarshalType type)
    {
        FieldSymbol = fieldSymbol;
        Type = type;
    }

    public IFieldSymbol FieldSymbol { get; }
    public MarshalType Type { get; }
}