using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class TagDesc : Attribute {
    public string Name { get; }
    public string Value { get; }

    public static readonly Dictionary<string, string> Desc = [];

    public TagDesc(string value) => Value = value;

    public TagDesc(string name, string value) {
        Name = name;
        Value = value;
    }

    public static string GetTagDesc(string tagName) {
        if(string.IsNullOrEmpty(tagName)) {
            return string.Empty;
        }

        if(Desc.TryGetValue(tagName.ToUpperInvariant(), out var desc)) {
            return desc;
        }

        return string.Empty;
    }
}