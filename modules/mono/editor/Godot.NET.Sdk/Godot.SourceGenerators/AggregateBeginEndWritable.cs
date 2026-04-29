using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Godot.SourceGenerators;

public record AggregateBeginEndWritable : IBeginEndWritable
{
    private ImmutableArray<IBeginEndWritable> _array;

    public AggregateBeginEndWritable(ImmutableArray<IBeginEndWritable> array) { _array = array; }

    public AggregateBeginEndWritable(params IEnumerable<IBeginEndWritable> collection)
    {
        _array = collection.ToImmutableArray();
    }

    /// <inheritdoc />
    public void Begin(FormatWriter writer, CancellationToken cancellationToken = default)
    {
        foreach (var writable in _array)
        {
            if (cancellationToken.IsCancellationRequested) return;

            writable.Begin(writer, cancellationToken);
        }
    }

    /// <inheritdoc />
    public void End(FormatWriter writer, CancellationToken cancellationToken = default)
    {
        foreach (var writable in _array)
        {
            if (cancellationToken.IsCancellationRequested) return;

            writable.End(writer, cancellationToken);
        }
    }

    public static implicit operator AggregateBeginEndWritable(IBeginEndWritable[] writables) =>
        new(writables.ToImmutableArray());

    public static implicit operator AggregateBeginEndWritable(ImmutableArray<IBeginEndWritable> writables) =>
        new(writables);
}