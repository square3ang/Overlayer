using System;
using System.Collections.Generic;

namespace Overlayer.Core.Scripting.JSNet;

public static class EnumParser<T> where T : Enum {
    private static readonly string[] Names;

    private static readonly T[] Values;

    private static readonly Dictionary<string, T> NameValues;

    static EnumParser() {
        NameValues = [];
        Names = Enum.GetNames(typeof(T));
        Values = (T[])Enum.GetValues(typeof(T));
        for(int i = 0; i < Names.Length; i++) {
            NameValues[Names[i]] = Values[i];
        }
    }

    public static T Parse(string name) => !NameValues.TryGetValue(name, out var value) ? default(T) : value;

    public static bool TryParse(string name, out T value) => NameValues.TryGetValue(name, out value);

    public static string[] GetNames() => Names;

    public static T[] GetValues() => Values;

    public static int IndexOf(string name) => Array.IndexOf(Names, name);

    public static int IndexOf(T value) => Array.IndexOf(Values, value);
}
