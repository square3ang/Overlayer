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
    [TagDesc("Parses a JavaScript expression and outputs the result.\nThis is one of the most important tags for customization, allowing arithmetic, comparisons, and most basic JS operations on tag values.\nDue to the tag parsing structure, variables are not supported; only simple constructs like the ternary operator are recommended.\nTo access tag values, they must be called as functions (e.g., Tag()).\nFor complex logic, using a separate scripting file is recommended.")]
    public static object Expr(string expr) {
        if(expressions.TryGetValue(expr, out var res)) {
            return res.IsFaulted || !res.prepared.IsValid ? null : (object)res.Run();
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

    public class ExprContext(Engine engine, Prepared<Script> prepared) {
        public Engine engine = engine;
        public Prepared<Script> prepared = prepared;

        public bool IsFaulted;

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

            return result ?? JsValue.Null;
        }
    }
}