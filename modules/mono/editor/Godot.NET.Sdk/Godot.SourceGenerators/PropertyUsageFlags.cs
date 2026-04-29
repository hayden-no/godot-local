using System;

namespace Godot.SourceGenerators;

[Flags]
internal enum PropertyUsageFlags
{
    None = 0,
    Storage = 2,
    Editor = 4,
    Internal = 8,
    Checkable = 16,
    Checked = 32,
    Group = 64,
    Category = 128,
    Subgroup = 256,
    ClassIsBitfield = 512,
    NoInstanceState = 1024,
    RestartIfChanged = 2048,
    ScriptVariable = 4096,
    StoreIfNull = 8192,
    UpdateAllIfModified = 16384,
    ScriptDefaultValue = 32768,
    ClassIsEnum = 65536,
    NilIsVariant = 131072,
    Array = 262144,
    AlwaysDuplicate = 524288,
    NeverDuplicate = 1048576,
    HighEndGfx = 2097152,
    NodePathFromSceneRoot = 4194304,
    ResourceNotPersistent = 8388608,
    KeyingIncrements = 16777216,
    DeferredSetResource = 33554432,
    EditorInstantiateObject = 67108864,
    EditorBasicSetting = 134217728,
    ReadOnly = 268435456,
    Default = 6,
    NoEditor = 2
}