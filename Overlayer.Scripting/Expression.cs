using Acornima.Ast;
using Jint;
using Jint.Native;
using JSNet.Utils;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System.Collections.Generic;

namespace Overlayer.Scripting
{
    public static class Expression
    {
        public static readonly Dictionary<string, ExprContext> expressions = new Dictionary<string, ExprContext>();
        [Tag("Expression", NotPlaying = true)]
        public static object Expr(string expr)
        {
            if (expressions.TryGetValue(expr, out var res))
                return res.Run();
            res = MiscUtils.ExecuteSafe(() => new ExprContext(Main.JSExpressionApi.PrepareInterpreter(), Engine.PrepareScript(JSUtils.RemoveImports(expr))), out _);
            if (!res.prepared.IsValid) return null;
            return (expressions[expr] = res).Run();
        }
        public class ExprContext
        {
            public Engine engine;
            public Prepared<Script> prepared;
            public ExprContext(Engine engine, Prepared<Script> prepared)
            {
                this.engine = engine;
                this.prepared = prepared;
            }
            public JsValue Run() => engine.Evaluate(prepared); 
        }
    }
}
