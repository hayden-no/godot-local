using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked parameter or property contains HTTP verbs provided by ASP.NET routing attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
internal sealed class AspRouteVerbsAttribute : Attribute { }
