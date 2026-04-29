using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Godot.SourceGenerators.MemberCaching;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Text;

namespace Godot.SourceGenerators;

public class FormatWriter
{
    Stack<int>? indents;
    Stack<IBeginEndWritable>? beginEndWritables;
    string currentIndent = string.Empty;

    StringBuilder? builder;
    bool endsWithNewline;
    CancellationToken cancellationToken;
    private Dictionary<Type, object> settings = new();

    bool Cancelled
    {
        get { return field |= cancellationToken.IsCancellationRequested; }
        set { field = value; }
    }

    Stack<int> Indents
    {
        get
        {
            indents ??= new Stack<int>();

            return indents;
        }
    }

    Stack<IBeginEndWritable> BeginEndWritables
    {
        get
        {
            beginEndWritables ??= new Stack<IBeginEndWritable>();

            return beginEndWritables;
        }
    }

    public TSetting GetSettings<TSetting>()
        where TSetting : class, IEquatable<TSetting>, new()
    {
        var key = typeof(TSetting);
        if (settings.TryGetValue(key, out var settingObj))
        {
            if (settingObj is TSetting setting)
            {
                return setting;
            }
            else
            {
                // invalid type, remove it
                settings.Remove(key);
            }
        }
        settingObj = new TSetting();
        settings[key] = settingObj;
        return (TSetting)settingObj;
    }

    public string NewLine => GetSettings<NewlineConfig>().Newline;

    const string DefaultNullValuePlaceholder = "[null]";
    public string? NullValuePlaceholder { get; set; } = DefaultNullValuePlaceholder;
    static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    public CultureInfo Culture { get; set; } = DefaultCulture;

    public SourceText ToSourceText() { return SourceText.From(ToString(), Encoding.UTF8); }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Cancelled) return string.Empty;

        ClearBlocks();

        return GenerationEnvironment.ToString();
    }

    public void Clear()
    {
        GenerationEnvironment.Clear();
        ClearIndent();
        ClearBlocks(false);
        endsWithNewline = false;
    }

    public FormatWriter() { }
    public FormatWriter(CancellationToken cancellationToken) { this.cancellationToken = cancellationToken; }

    public FormatWriter(StringBuilder builder, CancellationToken cancellationToken = default)
    {
        this.builder = builder;
        this.cancellationToken = cancellationToken;
    }

    public FormatWriter(string baseString, CancellationToken cancellationToken = default) : this(
        new StringBuilder(baseString),
        cancellationToken
    ) { }

    private static ThreadLocal<SingleUse> _cachedSingleUse = new(() => new Instance());

    public static SingleUse Local
    {
        [MustDisposeResource] get { return _cachedSingleUse.Value; }
    }

    public static string Execute(Action<FormatWriter> action, CancellationToken cancellationToken = default)
    {
        var writer = Local;
        writer.cancellationToken = cancellationToken;
        action(writer);

        return writer.ToString();
    }

    public static string Execute<T>(
        Action<FormatWriter, T> action,
        T state,
        CancellationToken cancellationToken = default
    )
    {
        var writer = Local;
        writer.cancellationToken = cancellationToken;
        action(writer, state);

        return writer.ToString();
    }

    public static string Execute<T1, T2>(
        Action<FormatWriter, T1, T2> action,
        T1 state1,
        T2 state2,
        CancellationToken cancellationToken = default
    )
    {
        var writer = Local;
        writer.cancellationToken = cancellationToken;
        action(writer, state1, state2);

        return writer.ToString();
    }

    [MustDisposeResource]
    public class SingleUse : FormatWriter, IDisposable
    {
        protected SingleUse() { }

        protected SingleUse(StringBuilder builder, CancellationToken cancellationToken = default) : base(
            builder,
            cancellationToken
        ) { }

        protected SingleUse(string baseString, CancellationToken cancellationToken = default) : base(
            baseString,
            cancellationToken
        ) { }

        /// <inheritdoc />
        [HandlesResourceDisposal]
        public override string ToString()
        {
            ClearBlocks();
            var output = GenerationEnvironment.ToString();
            Dispose();

            return output;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Clear();
            ClearBlocks(false);
            NullValuePlaceholder = DefaultNullValuePlaceholder;
            Culture = DefaultCulture;
            cancellationToken = default;
            Cancelled = false;
        }
    }

    class Instance : SingleUse
    {
        public Instance() : base() { }

        public Instance(StringBuilder builder, CancellationToken cancellationToken = default) : base(
            builder,
            cancellationToken
        ) { }

        public Instance(string baseString, CancellationToken cancellationToken = default) : base(
            baseString,
            cancellationToken
        ) { }
    }

#region Indents

    public string PopIndent()
    {
        if (Indents.Count == 0) return "";

        int lastPos = currentIndent.Length - Indents.Pop();
        string last = currentIndent.Substring(lastPos);
        currentIndent = currentIndent.Substring(0, lastPos);

        return last;
    }

    public void PushIndent(string? indent = null)
    {
        indent ??= GetSettings<NewlineConfig>().DefaultIndent;
        if (indent == null) throw new ArgumentNullException(nameof(indent));

        Indents.Push(indent.Length);
        currentIndent += indent;
    }

    public void ClearIndent()
    {
        currentIndent = string.Empty;
        Indents.Clear();
    }

    public string CurrentIndent
    {
        get { return currentIndent; }
    }

#endregion

    public void PushBlock(IBeginEndWritable block)
    {
        if (block is null) throw new ArgumentNullException(nameof(block));

        BeginEndWritables.Push(block);
        WriteStart(block);
    }

    public bool PopBlock()
    {
        if (BeginEndWritables.Count == 0) return false;

        WriteEnd(BeginEndWritables.Pop());

        return true;
    }

    public void ClearBlocks(bool writePoppedBlocks = true)
    {
        if (writePoppedBlocks)
        {
            while (PopBlock()) { }
        }

        BeginEndWritables.Clear();
    }

    public void EnsureNewLine()
    {
        if (endsWithNewline) return;

        WriteLine();
    }

    public void EnsureNewLines(int count)
    {
        if (Cancelled) return;

        if (count <= 0) return;

        if (count == 1)
        {
            EnsureNewLine();

            return;
        }

        int newLines = 0;

        for (int i = GenerationEnvironment.Length - 1; i >= 0; i--)
        {
            if (newLines >= count) return;

            char c = GenerationEnvironment[i];

            if (char.IsWhiteSpace(c))
            {
                if (c == '\n')
                {
                    newLines++;

                    continue;
                }
            }
            else
            {
                break;
            }
        }

        int needed = count - newLines;
        Write('\n', needed);
    }

#region Writing

    protected StringBuilder GenerationEnvironment
    {
        get
        {
            builder ??= new StringBuilder();

            return builder;
        }
        set { builder = value; }
    }

    public FormatWriter Write(char c, int count = 1)
    {
        if (Cancelled) return this;

        Span<char> stack = stackalloc char[count];
        stack.Fill(c);

        return Write(stack);
    }

    private unsafe FormatWriter Append(scoped ReadOnlySpan<char> text)
    {
        if (Cancelled) return this;

        GenerationEnvironment.EnsureCapacity(text.Length);

        fixed (char* p = text)
        {
            GenerationEnvironment.Append(p, text.Length);
        }

        return this;
    }

    public FormatWriter WriteFormat<T>(T? formattable, string? format)
        where T : IFormattable
    {
        if (Cancelled) return this;
        if (formattable is null) return this;

        return Write(formattable.ToString(format, Culture));
    }

    public FormatWriter WriteConvertible<T>(T? convertible)
        where T : IConvertible
    {
        if (Cancelled) return this;
        if (convertible is null) return this;

        return Write(convertible.ToString(Culture));
    }

    public FormatWriter Write(string? textToAppend) => Write(textToAppend.AsSpan());

    public FormatWriter Write(scoped ReadOnlySpan<char> textToAppend)
    {
        const int cancelledCheckInterval = 1000;

        if (Cancelled) return this;
        if (textToAppend.IsEmpty) return this;

        if ((GenerationEnvironment.Length == 0 || endsWithNewline) && CurrentIndent.Length > 0)
        {
            GenerationEnvironment.Append(CurrentIndent);
        }

        endsWithNewline = false;

        char last = textToAppend[textToAppend.Length - 1];

        if (last == '\n' || last == '\r')
        {
            endsWithNewline = true;
        }

        if (CurrentIndent.Length == 0)
        {
            Append(textToAppend);

            return this;
        }

        //insert CurrentIndent after every newline (\n, \r, \r\n)
        //but if there's one at the end of the string, ignore it, it'll be handled next time thanks to endsWithNewline
        int lastNewline = 0;

        for (int i = 0; i < textToAppend.Length - 1; i++)
        {
            if (i % cancelledCheckInterval == 0 && Cancelled) return this;

            char c = textToAppend[i];

            if (c == '\r')
            {
                if (textToAppend[i + 1] == '\n')
                {
                    i++;

                    if (i == textToAppend.Length - 1) break;
                }
            }
            else if (c != '\n')
            {
                continue;
            }

            i++;
            int len = i - lastNewline;

            if (len > 0)
            {
                Append(textToAppend.Slice(lastNewline, i - lastNewline));
            }

            Append(CurrentIndent);
            lastNewline = i;
        }

        if (lastNewline > 0)
            Append(textToAppend.Slice(lastNewline, textToAppend.Length - lastNewline));
        else
            Append(textToAppend);

        return this;
    }

    [StringFormatMethod("format")]
    public FormatWriter Write(string? format, params object[] args)
    {
        if (Cancelled) return this;
        if (format == null) return this;

        return Write(string.Format(format, args));
    }

    public FormatWriter WriteLine(scoped ReadOnlySpan<char> textToAppend)
    {
        if (Cancelled) return this;

        return Write(textToAppend).WriteLine();
    }

    public FormatWriter WriteLine(char c, int count = 1)
    {
        if (Cancelled) return this;

        Span<char> stack = stackalloc char[count];
        stack.Fill(c);

        return WriteLine(stack);
    }

    public FormatWriter WriteLine(string? textToAppend = null)
    {
        if (Cancelled) return this;

        var settings = GetSettings<NewlineConfig>();

        Write(textToAppend);

        if (settings.ApplyIndentToEmptyLines && endsWithNewline)
        {
            // second newline
            GenerationEnvironment.AppendLine(CurrentIndent);
        }
        else
        {
            GenerationEnvironment.AppendLine();
        }
        endsWithNewline = true;

        return this;
    }

    [StringFormatMethod("format")]
    public FormatWriter WriteLine(string? format, params object?[] args)
    {
        if (Cancelled) return this;

        format ??= string.Empty;

        return WriteLine(string.Format(format, args));
    }

    public FormatWriter WriteJoin<T>(string? separator, IEnumerable<T> items)
    {
        if (Cancelled) return this;

        return WriteJoin(separator, items.ToArray().AsSpan());
    }

    public FormatWriter WriteJoin<T>(string? separator, params scoped ReadOnlySpan<T> items)
    {
        if (Cancelled) return this;

        for (int i = 0; i < items.Length; i++)
        {
            if (i != 0)
            {
                Write(separator);
            }

            WriteInternal(this, items[i]);
        }

        return this;


    }

    public FormatWriter WriteJoinSkipNull<T>(string? separator, IEnumerable<T?> items)
    {
        if (Cancelled) return this;

        return WriteJoinSkipNull(separator, items.ToImmutableArray().AsSpan());
    }

    static void WriteInternal<T>(FormatWriter writer, T item)
    {
        switch (item)
        {
            case IWritable writable:
                writer.Write(writable);
                break;
            case IFormattable formattable:
                writer.WriteFormat(formattable, null);
                break;
            case IConvertible convertible:
                writer.WriteConvertible(convertible);
                break;
            case not null:
                writer.Write(item.ToString());
                break;
        }
    }

    public FormatWriter WriteJoinSkipNull<T>(string? separator, params scoped ReadOnlySpan<T?> items)
    {
        if (Cancelled) return this;

        int written = 0;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] is null) continue;

            if (written != 0)
            {
                Write(separator);
            }

            WriteInternal(this, items[i]);
            written++;
        }

        return this;
    }

    public FormatWriter WriteUsingStatement(string? name, string? alias = null)
    {
        if (Cancelled) return this;
        if (name == null) return this;

        Write("using ");

        if (alias != null)
        {
            Write(alias).Write(" = ");
        }

        Write(name);
        Write(';');

        return this;
    }

    public FormatWriter WriteUsingStatements(params scoped ReadOnlySpan<string?> names)
    {
        if (Cancelled) return this;

        for (int i = 0; i < names.Length; i++)
        {
            Write("using ").Write(names[i]).WriteLine(';');
        }

        return this;
    }

    public FormatWriter WriteNamespace(string? name)
    {
        if (Cancelled) return this;

        return Write("namespace ").Write(name).WriteLine(';');
    }

    public FormatWriter WriteTypeDeclaration(
        string? typeName,
        ReadOnlySpan<string?> keywords = default,
        string? baseType = null,
        ReadOnlySpan<string?> interfaces = default,
        ReadOnlySpan<string?> genericConstraints = default
    )
    {
        if (Cancelled) return this;
        if (string.IsNullOrEmpty(typeName)) return this;

        if (!keywords.IsEmpty) WriteJoin(" ", keywords).Write(' ');
        Write(typeName);

        if (!string.IsNullOrEmpty(baseType) || !interfaces.IsEmpty)
        {
            Write(" : ");

            bool hasBase = !string.IsNullOrEmpty(baseType);
            int length = interfaces.Length + (hasBase ? 1 : 0);

            for (int i = 0; i < length; i++)
            {
                if (i == 0)
                {
                    if (hasBase)
                        Write(baseType);
                    else
                        Write(interfaces[0]);
                }
                else
                {
                    Write(", ").Write(interfaces[i - (hasBase ? 0 : 1)]);
                }
            }

            WriteLine();

            if (!genericConstraints.IsEmpty)
            {
                PushIndent();

                for (int i = 0; i < genericConstraints.Length; i++)
                {
                    if (!string.IsNullOrEmpty(genericConstraints[i]))
                    {
                        Write("where ").WriteLine(genericConstraints[i]);
                    }
                }

                PopIndent();
            }
        }

        return this;
    }

    public FormatWriter WriteBlockStart()
    {
        if (Cancelled) return this;

        WriteLine("{");
        PushIndent();

        return this;
    }

    public FormatWriter WriteBlockEnd()
    {
        if (Cancelled) return this;

        PopIndent();
        WriteLine("}");

        return this;
    }

    public FormatWriter WriteBlock(string? block)
    {
        if (Cancelled) return this;

        WriteBlockStart();
        if (!string.IsNullOrEmpty(block))
            WriteLine(block);
        else
            Write(' ');
        WriteBlockEnd();

        return this;
    }

    public FormatWriter Write<T>(T? writable)
        where T : IWritable
    {
        if (Cancelled) return this;

        if (writable is not null) writable.Write(this);

        return this;
    }

    public FormatWriter WriteStart<T>(T? writable)
        where T : IBeginEndWritable
    {
        if (Cancelled) return this;

        if (writable is not null) writable.Begin(this);

        return this;
    }

    public FormatWriter WriteEnd<T>(T? writable)
        where T : IBeginEndWritable
    {
        if (Cancelled) return this;

        if (writable is not null) writable.End(this);

        return this;
    }

    public FormatWriter Write<T>(T? writable, params IEnumerable<object?> arguments)
        where T : IArgumentWriteable
    {
        if (Cancelled) return this;

        if (writable is not null) writable.Write(this, arguments);

        return this;
    }

    public FormatWriter Write<T, TArg>(T? writable, TArg argument)
        where T : IArgumentWriteable<TArg>
    {
        if (Cancelled) return this;

        if (writable is not null) writable.Write(this, argument);

        return this;
    }

    public FormatWriter WriteIfNoneNullOrEmpty(params ICollection<string?> items)
    {
        if (Cancelled) return this;
        if (items.Any(string.IsNullOrEmpty)) return this;

        foreach (string? item in items) Write(item);

        return this;
    }

    public FormatWriter WriteIfNone<T>(InvalidValueKindFlags flags ,params ICollection<T?> items)
    {
        if (Cancelled) return this;
        if (items.Any(flags)) return this;

        foreach(var obj in items) WriteInternal(this, obj);

        return this;
    }

#endregion
}
