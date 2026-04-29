using System;

namespace Godot.SourceGenerators.MemberCaching;

internal static class ValueArrayBuilder
{
    public static ValueArray<T> Create<T>(ReadOnlySpan<T> items) => new ValueArray<T>([..items]);
}