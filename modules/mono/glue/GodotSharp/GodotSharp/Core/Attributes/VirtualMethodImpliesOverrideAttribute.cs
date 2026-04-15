using System;

namespace Godot;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class VirtualMethodImpliesOverrideAttribute : Attribute
{
    public string ImpliedMethodName { get; }
    public string MethodName { get; }

    public VirtualMethodImpliesOverrideAttribute(string impliedMethodName, string methodName)
    {
        ImpliedMethodName = impliedMethodName;
        MethodName = methodName;
    }
}
