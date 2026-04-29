using System.Collections.Generic;
using System.Threading;

namespace Godot.SourceGenerators;

public record Pragma : SimpleAbstractBeginEndWritable
{
    public string Kind { get; }
    public BeginEndValue<string>? Operator { get; }
    public string Value { get; }
    public BeginEndValue<string>? Comment { get; }

    /// <inheritdoc />
    public override bool IsValid() => !string.IsNullOrWhiteSpace(Kind) && !string.IsNullOrWhiteSpace(Value);

    /// <inheritdoc />
    protected override string Seperator => " ";

    /// <inheritdoc />
    protected override IEnumerable<string?> GetValues(State state)
    {
        yield return "#pragma";
        yield return Kind;
        if (Operator is not null) yield return Operator.Get(state);
        yield return Value;
        if (Comment is not null) yield return Comment.Get(state);
    }

    public Pragma(string kind, string value, BeginEndValue<string>? comment = null)
    {
        Kind = kind;
        Value = value;
        Comment = comment;
    }

    public Pragma(string kind, BeginEndValue<string> @operator, string value, BeginEndValue<string>? comment = null)
    {
        Kind = kind;
        Operator = @operator;
        Value = value;
        Comment = comment;
    }

    public static class Warnings
    {
        private static readonly BeginEndValue<string> Operator = new("disable", "restore");

        static Pragma New(string name, string? comment = null) =>
            new("warning", Operator, name, new($"// Disable {comment}", string.Empty));

        public static readonly Pragma CS0109 = New("CS0109", "redundant 'new' keyword");
    }
}

public interface IArgumentWriteable<T> : IArgumentWriteable
{
    void Write(FormatWriter writer, T argument, CancellationToken cancellationToken = default);
}
