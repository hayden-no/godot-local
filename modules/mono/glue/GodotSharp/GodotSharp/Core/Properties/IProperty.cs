#nullable  enable

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Godot.Properties;

public interface IProperty
{
    StringName Name { get; }
    bool IsReadOnly { get; }
    event PropertyChangedHandler? PropertyUpdated;
    public object? Obj { get; }
}

public interface IReadOnlyProperty<T> : IProperty
{
    T Value { get; }
}

public interface IProperty<T> : IReadOnlyProperty<T>
{
    new T Value { get; set; }

    /// <inheritdoc />
    T IReadOnlyProperty<T>.Value => Value;
}

public interface IPropertyOwner
{
    IPropertyCollection Properties { get; }
}

public interface IPropertyCollection : IReadOnlyCollection<IProperty>
{

}

public delegate void PropertyChangedHandler(IPropertyOwner sender, IProperty property);

public delegate void PropertyUpdatedHandler<T>(IPropertyOwner sender, IProperty<T> property);
