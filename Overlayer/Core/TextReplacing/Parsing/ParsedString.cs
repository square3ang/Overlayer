using System.Reflection.Emit;

namespace Overlayer.Core.TextReplacing.Parsing;

public class ParsedString(string str) : IParsed {
    public string str = str;

    public void Emit(ILGenerator il) => il.Emit(OpCodes.Ldstr, str);
}
