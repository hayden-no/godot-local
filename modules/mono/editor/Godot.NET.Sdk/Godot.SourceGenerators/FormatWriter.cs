using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Text;

namespace Godot.SourceGenerators;

public class FormatWriter
{
    Stack<int>? indents;
    string currentIndent = string.Empty;

    StringBuilder? builder;
    bool endsWithNewline;

    Stack<int> Indents
    {
        get
        {
            indents ??= new Stack<int>();

            return indents;
        }
    }

    public SourceText ToSourceText()
    {
        return SourceText.From(GenerationEnvironment.ToString(), Encoding.UTF8);
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

    public void PushIndent(string indent = "    ")
    {
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
        Span<char> stack = stackalloc char[count];
        stack.Fill(c);
        return Write(stack);
    }

    private unsafe FormatWriter Append(scoped ReadOnlySpan<char> text)
    {
        GenerationEnvironment.EnsureCapacity(text.Length);
        fixed(char* p = text)
        {
            GenerationEnvironment.Append(p, text.Length);
        }
        return this;
    }

    public FormatWriter Write(string? textToAppend) => Write(textToAppend.AsSpan());

    public FormatWriter Write(scoped ReadOnlySpan<char> textToAppend)
    {
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
        if (format == null) return this;
        return Write(string.Format(format, args));
    }

    public FormatWriter WriteLine(scoped ReadOnlySpan<char> textToAppend)
    {
        return Write(textToAppend).WriteLine();
    }

    public  FormatWriter WriteLine(char c, int count = 1)
    {
        Span<char> stack = stackalloc char[count];
        stack.Fill(c);
        return WriteLine(stack);
    }

    public FormatWriter WriteLine(string? textToAppend = null)
    {
        Write(textToAppend);
        GenerationEnvironment.AppendLine();
        endsWithNewline = true;

        return this;
    }

    [StringFormatMethod("format")]
    public FormatWriter WriteLine(string? format, params object[] args)
    {
        format ??= string.Empty;
        return WriteLine(string.Format(format, args));
    }

    public FormatWriter WriteJoin<T>(string? separator, IEnumerable<T> items)
    {
        return Write(string.Join(separator, items));
    }

    public FormatWriter WriteJoin<T>(string? separator, params scoped ReadOnlySpan<T> items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (i != 0)
            {
                Write(separator);
            }

            Write(items[i]?.ToString());
        }

        return this;
    }

    public FormatWriter WriteUsingStatement(string? name, string? alias = null)
    {
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
        for (int i = 0; i < names.Length; i++)
        {
            Write("using ").Write(names[i]).WriteLine(';');
        }

        return this;
    }

    public FormatWriter WriteNamespace(string? name) => Write("namespace ").Write(name).WriteLine(';');

    public FormatWriter WriteTypeDeclaration(
        string? typeName,
        ReadOnlySpan<string?> keywords = default,
        string? baseType = null,
        ReadOnlySpan<string?> interfaces = default,
        ReadOnlySpan<string?> genericConstraints = default
    )
    {
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
                    if (hasBase) Write(baseType);
                    else Write(interfaces[0]);
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
        WriteLine("{");
        PushIndent();
        return this;
    }

    public FormatWriter WriteBlockEnd()
    {
        PopIndent();
        WriteLine("}");
        return this;
    }

    public FormatWriter WriteBlock(string? block)
    {
        WriteBlockStart();
        if (!string.IsNullOrEmpty(block))
            WriteLine(block);
        else
            Write(' ');
        WriteBlockEnd();
        return this;
    }

#endregion
}
