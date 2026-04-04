namespace Overlayer.Core.Scripting.JSNet.API;

public static class StringUtils {
    public static string RemoveAfter(this string str, string after) {
        int num = str.IndexOf(after);
        return num < 0 ? str : str.Remove(num, str.Length - num);
    }

    public static string RemoveLastAfter(this string str, string after) {
        int num = str.LastIndexOf(after);
        return num < 0 ? str : str.Remove(num, str.Length - num);
    }

    public static string TrimBetween(this string str, string start, string end) {
        int num = str.IndexOf(start);
        int num2 = str.LastIndexOf(end);
        return num < 0 || num2 < 0 ? str : str.Remove(num, num2 - num + 1);
    }
}
