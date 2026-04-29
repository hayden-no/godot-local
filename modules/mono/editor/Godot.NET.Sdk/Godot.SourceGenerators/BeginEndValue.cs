using System;

namespace Godot.SourceGenerators;

public record BeginEndValue<T>
{
    public T? Value { get; }
    public T? BeginValue { get; }
    public T? EndValue { get; }

    public BeginEndValue(T beginValue, T endValue) => (BeginValue, EndValue) = (beginValue, endValue);
    public BeginEndValue(T value) => Value = value;

    public static implicit operator BeginEndValue<T>(T value) => new(value);

    public bool IsValid(SimpleAbstractBeginEndWritable.State state = SimpleAbstractBeginEndWritable.State.Begin)
    {
        if (Value is not null) return true;

        return state switch
        {
            SimpleAbstractBeginEndWritable.State.Begin => BeginValue is not null,
            SimpleAbstractBeginEndWritable.State.End => EndValue is not null,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    public T Get(SimpleAbstractBeginEndWritable.State state)
    {
        if (Value is not null) return Value;

        return state switch
        {
            SimpleAbstractBeginEndWritable.State.Begin => BeginValue!,
            SimpleAbstractBeginEndWritable.State.End => EndValue!,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}