using System;
using System.Threading;

namespace Godot.SourceGenerators;

public interface IWritable
{
    void Write(FormatWriter writer, CancellationToken cancellationToken = default);
}

public interface IWritable<TSettings> : IHaveSettings<TSettings>, IWritable
    where TSettings : class, IEquatable<TSettings>, new()
{
    void Write(FormatWriter writer, TSettings? settings = default, CancellationToken cancellationToken = default);
}

public interface IBeginEndWritable<TSettings> : IHaveSettings<TSettings>, IBeginEndWritable
    where TSettings : class, IEquatable<TSettings>, new()
{
    void Begin(FormatWriter writer, TSettings? settings = default, CancellationToken cancellationToken = default);
    void End(FormatWriter writer, TSettings? settings = default, CancellationToken cancellationToken = default);
}
