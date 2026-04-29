using System.Collections.Generic;
using System.Threading;

namespace Godot.SourceGenerators;

public abstract record SimpleAbstractBeginEndWritable : IBeginEndWritable
{
    public enum State { Begin, End }
    protected abstract string Seperator { get; }
    protected abstract IEnumerable<string?> GetValues(State state);
    protected virtual bool EndWithNewline => true;
    public abstract bool IsValid();
    /// <inheritdoc />
    public void Begin(FormatWriter writer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return;
        writer.WriteJoinSkipNull(Seperator, GetValues(State.Begin));
        writer.EnsureNewLine();
    }

    /// <inheritdoc />
    public void End(FormatWriter writer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return;
        writer.WriteJoinSkipNull(Seperator, GetValues(State.End));
        writer.EnsureNewLine();
    }
}