using System.Collections.Generic;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using JSNet;

namespace Overlayer.Scripting
{
    public static class Expression
    {
        public static readonly Dictionary<string, Script> expressions = new Dictionary<string, Script>();
        [Tag("Expression", Hint = ReturnTypeHint.Object, NotPlaying = true)]
        public static object Expr(string expr)
        {
            if (expressions.TryGetValue(expr, out var res))
                return res.Eval();
            res = MiscUtils.ExecuteSafe(() => Script.InterpretAPI(Main.JSApi, expr), out _);
            if (res == null) return null;
            return (expressions[expr] = res).Eval();
        }
    }
}
