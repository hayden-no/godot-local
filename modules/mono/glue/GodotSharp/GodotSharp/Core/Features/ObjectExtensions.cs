#nullable enable

using System;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace Godot;

[VirtualMethodImpliesOverride(nameof(GodotObject._Notification), nameof(Test))]
public partial class GodotObject : IEquatable<GodotObject>
{
    public virtual partial Variant _Get(StringName property)
    {
        return default;
    }

    public virtual partial Array<Dictionary> _GetPropertyList()
    {
        return default;
    }

    public virtual partial Variant _IterGet(Variant iter)
    {
        return default;
    }

    public virtual partial bool _IterInit(Array iter)
    {
        return default;
    }

    public virtual partial bool _IterNext(Array iter)
    {
        return default;
    }

    public virtual partial void _Notification(int what)
    {

    }

    protected virtual void Test(int what){}

    public virtual partial bool _PropertyCanRevert(StringName property)
    {
        return default;
    }

    public virtual partial Variant _PropertyGetRevert(StringName property)
    {
        return default;
    }

    public virtual partial bool _Set(StringName property, Variant value)
    {
        return default;
    }

    public virtual partial void _ValidateProperty(Dictionary property)
    {

    }

    /// <inheritdoc />
    public bool Equals(GodotObject? other)
    {
        if (other is null) return false;
        return GetPtr(this) == GetPtr(other);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as GodotObject);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return GetPtr(this).GetHashCode();
    }

    public static bool operator==(GodotObject? left, GodotObject? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator!=(GodotObject? left, GodotObject? right)
    {
        return !(left == right);
    }
}
