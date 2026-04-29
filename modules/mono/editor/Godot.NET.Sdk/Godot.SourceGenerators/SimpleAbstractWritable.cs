using System.Collections.Generic;
using System.Threading;

namespace Godot.SourceGenerators;

public abstract record SimpleAbstractWritable : IWritable
{
    protected abstract string Seperator { get; }
    protected abstract IEnumerable<string?> GetValues();
    public abstract bool IsValid();
    /// <inheritdoc />
    public void Write(FormatWriter writer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return;
        writer.WriteJoinSkipNull(Seperator, GetValues());
    }
}