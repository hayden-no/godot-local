using System;

namespace Godot.SourceGenerators;

[Flags]
public enum MethodFlags
{
    Normal = 1,
    Editor = 2,
    Const = 4,
    Virtual = 8,
    Vararg = 16,
    Static = 32,
    ObjectCore = 64,
    Default = 1
}