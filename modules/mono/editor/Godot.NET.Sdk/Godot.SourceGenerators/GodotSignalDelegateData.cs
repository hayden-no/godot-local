using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

public readonly struct GodotSignalDelegateData
{
    public GodotSignalDelegateData(string name, INamedTypeSymbol delegateSymbol, GodotMethodData invokeMethodData)
    {
        Name = name;
        DelegateSymbol = delegateSymbol;
        InvokeMethodData = invokeMethodData;
    }

    public string Name { get; }
    public INamedTypeSymbol DelegateSymbol { get; }
    public GodotMethodData InvokeMethodData { get; }
}