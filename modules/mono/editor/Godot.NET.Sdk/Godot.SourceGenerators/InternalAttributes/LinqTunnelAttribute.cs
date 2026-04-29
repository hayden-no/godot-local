using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the method is a pure LINQ method with postponed enumeration (like <c>Enumerable.Select</c> or
/// <c>Enumerable.Where</c>). This annotation allows inference of the <c>[InstantHandle]</c> annotation for parameters
/// of delegate type by analyzing LINQ method chains.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class LinqTunnelAttribute : Attribute { }
