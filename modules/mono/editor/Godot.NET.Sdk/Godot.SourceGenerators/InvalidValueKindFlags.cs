using System;

namespace Godot.SourceGenerators;

[Flags]
public enum InvalidValueKindFlags
{
    None = 0,
    Null = 1 << 0,
    Empty = 1 << 1,
    WhiteSpace = 1 << 2,
    DefaultValue = 1 << 3,
}