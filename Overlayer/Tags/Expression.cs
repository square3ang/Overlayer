using Acornima.Ast;
using Jint;
using Jint.Native;
using Overlayer.Core.Scripting;
using Overlayer.Core.Scripting.JSNet.Utils;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System.Collections.Generic;

namespace Overlayer.Tags;

public static class Expression {
    public static readonly Dictionary<string, ExprContext> expressions = [];

    [Tag("Expression", NotPlaying = true)]
    public static object Expr(string expr) {
        if(expressions.TryGetValue(expr, out var res)) {
            return res.IsFaulted || !res.prepared.IsValid ? null : (object)(res.HasValue ? res.LastValue : res.Run());
        }

        var prepared = Engine.PrepareScript(JSUtils.RemoveImports(expr));

        if(!prepared.IsValid) {
            expressions[expr] = new ExprContext(null, prepared);
            return null;
        }

        var engine = Scripting.JSApi.PrepareInterpreter();
        var ctx = new ExprContext(engine, prepared);
        expressions[expr] = ctx;

        return ctx.Run();
    }

    public class ExprContext {
        public Engine engine;
        public Prepared<Script> prepared;

        public bool IsFaulted;
        public bool HasValue;
        public JsValue LastValue;

        public ExprContext(Engine engine, Prepared<Script> prepared) {
            this.engine = engine;
            this.prepared = prepared;
        }

        public JsValue Run() {
            if(IsFaulted || engine == null || !prepared.IsValid) {
                return JsValue.Null;
            }

            var result = MiscUtils.ExecuteSafe(
                () => engine.Evaluate(prepared),
                out var ex
            );

            if(ex != null) {
                IsFaulted = true;
                return JsValue.Null;
            }

            HasValue = true;
            LastValue = result ?? JsValue.Null;

            return LastValue;
        }
    }
}