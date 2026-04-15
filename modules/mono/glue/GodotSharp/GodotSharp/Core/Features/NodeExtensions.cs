using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Godot;

public partial class Node : IReadOnlyList<Node>
{
    public virtual partial void _EnterTree()
    {

    }
    public virtual partial void _ExitTree()
    {

    }

    public virtual partial string[] _GetAccessibilityConfigurationWarnings()
    {
        return default;
    }

    public virtual partial string[] _GetConfigurationWarnings()
    {
        return default;
    }

    public virtual partial Rid _GetFocusedAccessibilityElement()
    {
        return default;
    }

    public virtual partial void _Input(InputEvent @event)
    {

    }
    public virtual partial void _PhysicsProcess(double delta)
    {

    }
    public virtual partial void _Process(double delta)
    {

    }
    public virtual partial void _Ready()
    {

    }
    public virtual partial void _ShortcutInput(InputEvent @event)
    {

    }
    public virtual partial void _UnhandledInput(InputEvent @event)
    {

    }
    public virtual partial void _UnhandledKeyInput(InputEvent @event)
    {

    }

    /// <inheritdoc />
    public IEnumerator<Node> GetEnumerator()
    {
        for (int i = 0; i < this.Count; i++)
        {
            yield return this[i];
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public int Count => GetChildCount();

    /// <inheritdoc />
    public Node this[int index] => GetChild(index);

    public Node this[Index index] => GetChild(index.GetOffset(Count));

    public IReadOnlyList<Node> this[System.Range range] => GetChildren()[range];
}
