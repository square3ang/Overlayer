using System;
using System.Linq;
using System.Reflection;
using Acornima.Ast;
using Jint;
using Jint.Native;
using Jint.Native.Function;

namespace Overlayer.Core.Scripting.JSNet.Utils;

public class FIWrapper {
    public readonly Engine engine;

    public readonly Function fi;

    public readonly string[] args;

    public static readonly MethodInfo CallMethod = typeof(FIWrapper).GetMethod("Call");

    public static readonly MethodInfo CallRawMethod = typeof(FIWrapper).GetMethod("CallRaw");

    public FIWrapper(Function fi) {
        this.fi = fi;
        engine = fi.Engine;
        args = fi.FunctionDeclaration.Params.Select(n => ((Identifier)n).Name).ToArray();
    }

    public object Call(params object[] args) {
        return fi.Call(null, args != null ? Array.ConvertAll(args, o => JsValue.FromObject(engine, o)) : [])
            .ToObject();
    }

    public JsValue CallRaw(params object[] args) {
        return fi.Call(null, args != null ? Array.ConvertAll(args, o => JsValue.FromObject(engine, o)) : []);
    }
}