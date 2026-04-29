using System.Collections.Generic;

namespace Godot.SourceGenerators;

public interface IArgumentWriteable
{
    void Write(FormatWriter writer, params IEnumerable<object?> arguments);
}