using System.Threading;

namespace Godot.SourceGenerators;

public interface IBeginEndWritable
{
    void Begin(FormatWriter writer, CancellationToken cancellationToken = default);
    void End(FormatWriter writer, CancellationToken cancellationToken = default);
}