using System;
using System.Collections.Generic;

namespace Overlayer.Utils;

public static class EnumHelper<T> where T : Enum {
    private static readonly string[] Names;
    private static readonly T[] Values;
    private static readonly Dictionary<string, T> NameValues;

    static EnumHelper() {
        NameValues = [];
        Names = Enum.GetNames(typeof(T));
        Values = (T[])Enum.GetValues(typeof(T));
        for (var i = 0; i < Names.Length; i++) NameValues[Names[i]] = Values[i];
    }

    public static T Parse(string name) {
        return NameValues.TryGetValue(name, out var value) ? value : default;
    }

    public static bool TryParse(string name, out T value) {
        return NameValues.TryGetValue(name, out value);
    }

    public static string[] GetNames() {
        return Names;
    }

    public static T[] GetValues() {
        return Values;
    }

    public static int IndexOf(string name) {
        return Array.IndexOf(Names, name);
    }

    public static int IndexOf(T value) {
        return Array.IndexOf(Values, value);
    }
}