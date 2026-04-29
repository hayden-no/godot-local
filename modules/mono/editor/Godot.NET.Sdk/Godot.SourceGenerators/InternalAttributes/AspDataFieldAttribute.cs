using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
internal sealed class AspDataFieldAttribute : Attribute { }
