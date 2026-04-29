using System.Collections.Generic;

namespace Godot.SourceGenerators;

public static class AggregateBeginEndWritableExtensions
{
    extension(IEnumerable<IBeginEndWritable> items)
    {
        public IBeginEndWritable Aggregate() => new AggregateBeginEndWritable(collection: items);
    }
}