using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Godot.SourceGenerators.MemberCaching;

namespace Godot.SourceGenerators;

public record DerivedArgumentWrapper<TWritable, T> : IArgumentWriteable<T> where TWritable : IArgumentWriteable
{
    public TWritable Writable { get; }
    public ValueArray<Func<T?, object?>> Transformers { get; }

    public DerivedArgumentWrapper(TWritable writable, ValueArray<Func<T?, object?>> transformers)
    {
        Writable = writable;
        Transformers = transformers;
    }

    public DerivedArgumentWrapper(TWritable writable, params IEnumerable<Func<T?, object?>> transformers) : this(writable, transformers.ToImmutableArray())
    {
    }

    public void Write(FormatWriter writer, T argument, CancellationToken cancellationToken = default) => Writable.Write(writer, Transformers.Select(t => t(argument)), cancellationToken);

    /// <inheritdoc />
    public void Write(FormatWriter writer, params IEnumerable<object?> arguments)
    {
        if (arguments is null) throw new ArgumentNullException(nameof(arguments));
        if (Transformers.Length == 0) Writable.Write(writer, arguments);
        else Writable.Write(writer, Transformers.Select(t => t(arguments.Cast<T?>().FirstOrDefault())));
    }
}