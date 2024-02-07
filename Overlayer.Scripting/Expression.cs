using System.Collections.Generic;
using Overlayer.Tags.Attributes;
using Overlayer.Scripting.JS;
using Overlayer.Utils;

namespace Overlayer.Scripting
{
    public static class Expression
    {
        public static readonly Dictionary<string, Result> expressions = new Dictionary<string, Result>();
        [Tag("Expression", Hint = ReturnTypeHint.Object, NotPlaying = true)]
        public static object Expr(string expr)
        {
            if (expressions.TryGetValue(expr, out var res))
                return res.Eval();
            res = MiscUtils.ExecuteSafe(() => JSUtils.CompileSource(expr), out _);
            if (res == null) return null;
            return (expressions[expr] = res).Eval();
        }
    }
}
