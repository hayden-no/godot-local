using System;

namespace Godot.SourceGenerators;

public interface IHaveSettings<TSettings>
    where TSettings : class, IEquatable<TSettings>, new()
{

}