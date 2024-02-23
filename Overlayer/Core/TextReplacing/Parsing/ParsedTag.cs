using System.Collections.Generic;
using System.Reflection.Emit;

namespace Overlayer.Core.TextReplacing.Parsing
{
    public class ParsedTag : IParsed
    {
        public Tag tag;
        public List<string> args;
        public ParsedTag(Tag tag, List<string> args)
        {
            this.tag = tag;
            this.args = args;
        }
        public void Emit(ILGenerator il)
        {
            for (int i = 0; i < tag.ArgumentCount; i++)
            {
                if (args.Count - 1 < i)
                    il.Emit(OpCodes.Ldstr, tag.GetterOriginal.GetParameters()[i].DefaultValue?.ToString() ?? string.Empty);
                else il.Emit(OpCodes.Ldstr, args[i]);
            }
            il.Emit(OpCodes.Call, tag.Getter);
        }
    }
}
