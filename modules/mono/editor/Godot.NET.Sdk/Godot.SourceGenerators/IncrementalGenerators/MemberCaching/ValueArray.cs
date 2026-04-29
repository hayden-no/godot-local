using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Godot.SourceGenerators.MemberCaching;

[CollectionBuilder(typeof(ValueArrayBuilder), nameof(ValueArrayBuilder.Create))]
public readonly struct ValueArray<T> : IReadOnlyList<T>,
    IEquatable<ValueArray<T>>,
    IEquatable<IReadOnlyList<T>>,
    IEquatable<IEnumerable<T>>
{


    private readonly ImmutableArray<T> _array;

    public static readonly ValueArray<T> Empty = new ValueArray<T>(ImmutableArray<T>.Empty);

    public bool IsDefault => _array.IsDefault;
    public bool IsEmpty => _array.IsEmpty;
    public bool IsDefaultOrEmpty => _array.IsDefaultOrEmpty;
    public int Length => _array.Length;

    public ValueArray()
    {
        this = Empty;
        ImmutableArray<object> test = [new(), new()];
    }

    public static ValueArray<T> Create(params ReadOnlySpan<T> items)
    {
        return new ValueArray<T>([..items]);
    }

    public ValueArray(IEnumerable<T> collection) { _array = collection.ToImmutableArray(); }

    public ValueArray(ImmutableArray<T> collection) { _array = collection; }

    [Pure]
    public ValueArray<T> Add(T item) => _array.Add(item);

    [Pure]
    public ValueArray<T> AddRange(IEnumerable<T> items) => _array.AddRange(items);

    [Pure]
    public ValueArray<T> Slice(int start, int length) => _array.Slice(start, length);

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_array).GetEnumerator();

    /// <inheritdoc />
    public int Count => _array.Length;

    /// <inheritdoc />
    public T this[int index]
    {
        get { return (_array)[index]; }
        init { _array = _array.SetItem(index, value); }
    }

    /// <inheritdoc />
    public bool Equals(ValueArray<T> other)
    {
        if (_array.Length != other._array.Length) return false;

        return _array.SequenceEqual(other._array);
    }

    /// <inheritdoc />
    public bool Equals(IReadOnlyList<T> other)
    {
        if (other is null) return false;
        if (_array.Length != other.Count) return false;

        return _array.SequenceEqual(other);
    }

    /// <inheritdoc />
    public bool Equals(IEnumerable<T> other) { return _array.SequenceEqual(other); }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            ValueArray<T> other => Equals(other),
            IReadOnlyList<T> onlyList => Equals(onlyList),
            IEnumerable<T> enumerable => Equals(enumerable),
            _ => false
        };
    }

    /// <inheritdoc />
    public override int GetHashCode() { return _array.GetHashCode(); }

    public ImmutableArray<T> ToImmutableArray() => _array;
    public T[] ToArray() => _array.ToArray();

    public static implicit operator ValueArray<T>(ImmutableArray<T> array) => new(array);

    public static implicit operator ImmutableArray<T>(ValueArray<T> array) => array._array;
}