using Godot.Collections;

namespace Godot.Bridge;

#nullable enable

public readonly struct PropertyInfo
{
    public Variant.Type Type { get; init; }
    public StringName Name { get; init; }
    public PropertyHint Hint { get; init; }
    public string HintString { get; init; }
    public PropertyUsageFlags Usage { get; init; }
    public StringName? ClassName { get; init; }
    public bool Exported { get; init; }

    public PropertyInfo(Variant.Type type, StringName name, PropertyHint hint, string hintString,
        PropertyUsageFlags usage, bool exported)
        : this(type, name, hint, hintString, usage, className: null, exported) { }

    public PropertyInfo(Variant.Type type, StringName name, PropertyHint hint, string hintString,
        PropertyUsageFlags usage, StringName? className, bool exported)
    {
        Type = type;
        Name = name;
        Hint = hint;
        HintString = hintString;
        Usage = usage;
        ClassName = className;
        Exported = exported;
    }

    static readonly StringName _typeKey = new("type");
    static readonly StringName _nameKey = new("name");
    static readonly StringName _hintKey = new("hint");
    static readonly StringName _hintStringKey = new("hint_string");
    static readonly StringName _usageKey = new("usage");
    static readonly StringName _classNameKey = new("class_name");
    static readonly StringName _exportedKey = new("exported");

    public void Apply(Dictionary propertyDict)
    {
        propertyDict[_typeKey] = (int)Type;
        propertyDict[_nameKey] = Name;
        propertyDict[_hintKey] = (int)Hint;
        propertyDict[_hintStringKey] = HintString;
        propertyDict[_usageKey] = (int)Usage;
        propertyDict[_classNameKey] = ClassName;
        propertyDict[_exportedKey] = Exported;
    }

    public static PropertyInfo CreateFrom(Dictionary propertyDict)
    {
        return new PropertyInfo
        {
            Type = propertyDict[_typeKey].As<Variant.Type>(),
            Name = propertyDict[_nameKey].As<StringName>(),
            Hint = propertyDict[_hintKey].As<PropertyHint>(),
            HintString = propertyDict[_hintStringKey].As<string>(),
            Usage = propertyDict[_usageKey].As<PropertyUsageFlags>(),
            ClassName = propertyDict[_classNameKey].As<StringName>(),
            Exported = propertyDict[_exportedKey].As<bool>()
        };
    }
}
