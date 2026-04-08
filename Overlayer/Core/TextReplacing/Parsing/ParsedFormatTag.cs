using System.Reflection;
using System.Reflection.Emit;

namespace Overlayer.Core.TextReplacing.Parsing;

public class ParsedFormatTag(Tag tag, Tag.FormatType formatType, string format) : IParsed {
    public Tag tag = tag;
    public Tag.FormatType formatType = formatType;
    public string format = format;

    public void Emit(ILGenerator il) {
        il.Emit(OpCodes.Call, tag.GetterRaw);
        il.Emit(OpCodes.Ldstr, format);
        il.Emit(OpCodes.Call, formatType == Tag.FormatType.Float ? ToStringFloat : ToStringDouble);
    }

    static ParsedFormatTag() {
        ToStringFloat = typeof(ParsedFormatTag).GetMethod(nameof(FloatToString));
        ToStringDouble = typeof(ParsedFormatTag).GetMethod(nameof(DoubleToString));
    }
    public static MethodInfo ToStringFloat;
    public static MethodInfo ToStringDouble;
    public static string FloatToString(float value, string format) => value.ToString(format);
    public static string DoubleToString(double value, string format) => value.ToString(format);
}