using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Godot.SourceGenerators.MemberCaching;

public class ValueDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEquatable<ValueDictionary<TKey, TValue>>, IEquatable<IReadOnlyDictionary<TKey, TValue>>, IEquatable<IEnumerable<KeyValuePair<TKey, TValue>>> where TKey : notnull
{
    private readonly ImmutableDictionary<TKey, TValue> _dictionary;

    public static readonly ValueDictionary<TKey, TValue> Empty = new(ImmutableDictionary<TKey, TValue>.Empty);

    public ValueDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) { _dictionary = collection.ToImmutableDictionary(); }
    public ValueDictionary(ImmutableDictionary<TKey, TValue> collection) { _dictionary = collection; }
    public ValueDictionary()
    {
        _dictionary = ImmutableDictionary<TKey, TValue>.Empty;
    }

    [Pure]
    public ValueDictionary<TKey, TValue> Add(TKey key, TValue value) => _dictionary.Add(key, value);
    [Pure]
    public ValueDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items) => _dictionary.AddRange(items);
    [Pure]
    public ValueDictionary<TKey, TValue> Remove(TKey key) => _dictionary.Remove(key);
    [Pure]
    public ValueDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys) => _dictionary.RemoveRange(keys);
    [Pure]
    public ValueDictionary<TKey, TValue> Clear() => _dictionary.Clear();
    [Pure]
    public ValueDictionary<TKey, TValue> SetItem(TKey key, TValue value) => _dictionary.SetItem(key, value);
    [Pure]
    public ValueDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items) => _dictionary.SetItems(items);

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionary).GetEnumerator();

    /// <inheritdoc />
    public int Count => _dictionary.Count;

    /// <inheritdoc />
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

    /// <inheritdoc />
    public TValue this[TKey key] => _dictionary[key];

    /// <inheritdoc />
    public IEnumerable<TKey> Keys => _dictionary.Keys;

    /// <inheritdoc />
    public IEnumerable<TValue> Values => _dictionary.Values;

    public ImmutableDictionary<TKey, TValue> ToImmutableDictionary() => _dictionary;
    public KeyValuePair<TKey, TValue>[] ToArray() => _dictionary.ToArray();
    public ImmutableArray<KeyValuePair<TKey, TValue>> ToImmutableArray() => _dictionary.ToImmutableArray();
    public ValueArray<KeyValuePair<TKey, TValue>> ToValueArray() => _dictionary.ToImmutableArray();


    static readonly EqualityComparer<TKey> KeyComparer = EqualityComparer<TKey>.Default;
    static readonly EqualityComparer<TValue> ValueComparer = EqualityComparer<TValue>.Default;

    /// <inheritdoc />
    public bool Equals(ValueDictionary<TKey, TValue> other)
    {
        if (Count != other.Count) return false;

        foreach (var key in _dictionary.Keys)
        {
            if (!other._dictionary.TryGetKey(key, out var otherKey)) return false;
            if (!KeyComparer.Equals(key, otherKey)) return false;
        }

        foreach (var kvp in this._dictionary)
        {
            if (!other._dictionary.TryGetValue(kvp.Key, out var otherValue)) return false;
            if (!ValueComparer.Equals(kvp.Value, otherValue)) return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(IReadOnlyDictionary<TKey, TValue> other)
    {
        if (other is null) return false;
        if (Count != other.Count) return false;

        foreach (var key in _dictionary.Keys)
        {
            if (!other.ContainsKey(key)) return false;
        }

        foreach (var kvp in this._dictionary)
        {
            if (!other.TryGetValue(kvp.Key, out var otherValue)) return false;
            if (!ValueComparer.Equals(kvp.Value, otherValue)) return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(IEnumerable<KeyValuePair<TKey, TValue>> other)
    {
        if (other is null) return false;
        if (other is IReadOnlyCollection<KeyValuePair<TKey, TValue>> collection && collection.Count != Count) return false;

        HashSet<TKey> visited = new();

        foreach (var kvp in other)
        {
            if (visited.Count > Count) return false;
            if (!_dictionary.TryGetKey(kvp.Key, out var key)) return false;
            if (!visited.Add(key)) return false;
            if (!_dictionary.TryGetValue(kvp.Key, out var value)) return false;
            if (!ValueComparer.Equals(value, kvp.Value)) return false;
        }

        if (visited.Count != Count) return false;

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            ValueDictionary<TKey, TValue> other => Equals(other),
            IReadOnlyDictionary<TKey, TValue> other => Equals(other),
            IEnumerable<KeyValuePair<TKey, TValue>> other => Equals(other),
            _ => false
        };
    }

    /// <inheritdoc />
    public override int GetHashCode() => _dictionary.GetHashCode();

    public static bool operator ==(ValueDictionary<TKey, TValue> left, ValueDictionary<TKey, TValue> right) => left.Equals(right);
    public static bool operator ==(ValueDictionary<TKey, TValue> left, IReadOnlyDictionary<TKey, TValue> right) => left.Equals(right);
    public static bool operator !=(ValueDictionary<TKey, TValue> left, ValueDictionary<TKey, TValue> right) => !left.Equals(right);
    public static bool operator !=(ValueDictionary<TKey, TValue> left, IReadOnlyDictionary<TKey, TValue> right) => !left.Equals(right);

    public static implicit operator ValueDictionary<TKey, TValue>(ImmutableDictionary<TKey, TValue> dictionary) => new(dictionary);
    public static implicit operator ImmutableDictionary<TKey, TValue>(ValueDictionary<TKey, TValue> dictionary) => dictionary._dictionary;
}