using Jint;
using Jint.Native;
using Jint.Native.Function;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Overlayer.Core.Scripting.JSNet.Utils;

public static class JSUtils {
    private static readonly MethodInfo istrue = typeof(JSUtils).GetMethod("IsTrue");

    public static string RemoveImports(string code) {
        StringBuilder stringBuilder = new();
        using(StringReader stringReader = new(code)) {
            string text = null;
            while((text = stringReader.ReadLine()) != null) {
                if(!text.StartsWith("import")) {
                    stringBuilder.AppendLine(text);
                } else {
                    stringBuilder.AppendLine();
                }
            }
        }
        return stringBuilder.ToString();
    }

    public static Options AllowReflection(this Options op) {
        op.Interop.AllowSystemReflection = true;
        return op;
    }

    public static JsValue FromObject(object value, Engine engine) => JsValue.FromObject(engine, value);

    public static object ToObject(object value) => value is not JsValue jsValue ? value : jsValue.ToObject();

    public static MethodInfo Wrap(this Function func, MethodBase target, bool rtIsBool) {
        if(func == null) {
            return null;
        }
        FIWrapper fIWrapper = new(func);
        TypeBuilder typeBuilder = EmitUtils.NewType();
        string[] args = fIWrapper.args;
        ParameterInfo[] array = SelectActualParams(target, target.GetParameters(), args.ToArray());
        if(array == null) {
            return null;
        }
        Type[] parameterTypes = array.Select((ParameterInfo p) => p.ParameterType).ToArray();
        MethodBuilder methodBuilder = typeBuilder.DefineMethod("Wrapper", MethodAttributes.Public | MethodAttributes.Static, rtIsBool ? typeof(bool) : typeof(void), parameterTypes);
        FieldBuilder field = typeBuilder.DefineField("holder", typeof(FIWrapper), FieldAttributes.Public | FieldAttributes.Static);
        ILGenerator iLGenerator = methodBuilder.GetILGenerator();
        LocalBuilder local = iLGenerator.MakeArray<object>(array.Length);
        int num = 1;
        ParameterInfo[] array2 = array;
        foreach(ParameterInfo parameterInfo in array2) {
            EmitUtils.IgnoreAccessCheck(parameterInfo.ParameterType);
            methodBuilder.DefineParameter(num++, ParameterAttributes.None, parameterInfo.Name);
            int arg = num - 2;
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Ldc_I4, arg);
            iLGenerator.Emit(OpCodes.Ldarg, arg);
            iLGenerator.Emit(OpCodes.Stelem_Ref);
        }
        iLGenerator.Emit(OpCodes.Ldsfld, field);
        iLGenerator.Emit(OpCodes.Ldloc, local);
        iLGenerator.Emit(OpCodes.Call, FIWrapper.CallMethod);
        if(rtIsBool) {
            iLGenerator.Emit(OpCodes.Call, istrue);
        } else {
            iLGenerator.Emit(OpCodes.Pop);
        }
        iLGenerator.Emit(OpCodes.Ret);
        Type type = typeBuilder.CreateType();
        type.GetField("holder").SetValue(null, fIWrapper);
        return type.GetMethod("Wrapper");
    }

    public static MethodInfo Wrap(this FIWrapper fi) {
        if(fi == null) {
            return null;
        }
        TypeBuilder typeBuilder = EmitUtils.NewType();
        ParameterInfo[] array = fi.args.Select((string s) => new CustomParameter(typeof(object), s)).ToArray();
        ParameterInfo[] array2 = array;
        if(array2 == null) {
            return null;
        }
        Type[] parameterTypes = array2.Select((ParameterInfo p) => p.ParameterType).ToArray();
        MethodBuilder methodBuilder = typeBuilder.DefineMethod(fi.fi.FunctionDeclaration.Id?.Name ?? "Anonymous", MethodAttributes.Public | MethodAttributes.Static, typeof(object), parameterTypes);
        FieldBuilder field = typeBuilder.DefineField("holder", typeof(FIWrapper), FieldAttributes.Public | FieldAttributes.Static);
        ILGenerator iLGenerator = methodBuilder.GetILGenerator();
        LocalBuilder local = iLGenerator.MakeArray<object>(array2.Length);
        int num = 1;
        array = array2;
        foreach(ParameterInfo parameterInfo in array) {
            methodBuilder.DefineParameter(num++, ParameterAttributes.None, parameterInfo.Name);
            int arg = num - 2;
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Ldc_I4, arg);
            iLGenerator.Emit(OpCodes.Ldarg, arg);
            iLGenerator.Emit(OpCodes.Stelem_Ref);
        }
        iLGenerator.Emit(OpCodes.Ldsfld, field);
        iLGenerator.Emit(OpCodes.Ldloc, local);
        iLGenerator.Emit(OpCodes.Call, FIWrapper.CallMethod);
        iLGenerator.Emit(OpCodes.Ret);
        Type type = typeBuilder.CreateType();
        type.GetField("holder").SetValue(null, fi);
        return type.GetMethod("Wrapper");
    }

    public static ParameterInfo[] SelectActualParams(MethodBase m, ParameterInfo[] p, string[] n, Func<string, CustomParameter> selector = null) {
        Type declaringType = m.DeclaringType;
        List<ParameterInfo> list = [];
        int i;
        for(i = 0; i < n.Length; i++) {
            int num = Array.FindIndex(p, (ParameterInfo pa) => pa.Name == n[i]);
            if(num > 0) {
                list.Add(new CustomParameter(ObjectIfPrivate(p[num].ParameterType), p[num].Name));
                continue;
            }
            string text = n[i];
            switch(text) {
                case "__instance":
                    list.Add(new CustomParameter(ObjectIfPrivate(declaringType), text));
                    continue;
                case "__originalMethod":
                    list.Add(new CustomParameter(typeof(MethodBase), text));
                    continue;
                case "__args":
                    list.Add(new CustomParameter(typeof(MethodBase), text));
                    continue;
                case "__result":
                    list.Add(new CustomParameter((m is MethodInfo methodInfo) ? methodInfo.ReturnType : typeof(object), text));
                    continue;
                case "__exception":
                    list.Add(new CustomParameter(typeof(Exception), text));
                    continue;
                case "__runOriginal":
                    list.Add(new CustomParameter(typeof(bool), text));
                    continue;
                case "il":
                    list.Add(new CustomParameter(typeof(ILGenerator), text));
                    continue;
            }
            if(text.StartsWith("__")) {
                if(!int.TryParse(text.Substring(0, 2), out var result)) {
                    return null;
                }
                if(result < 0 || result >= p.Length) {
                    return null;
                }
                list.Add(new CustomParameter(p[result].ParameterType, text));
            } else if(text.StartsWith("___")) {
                string name = text.Substring(0, 3);
                if(declaringType.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty) == null) {
                    return null;
                }
            } else if(selector != null) {
                list.Add(selector(text));
            }
        }
        return list.ToArray();
    }

    private static Type ObjectIfPrivate(Type t) => !t.IsPublic ? typeof(object) : t;

    [Obsolete("Internal Only!", true)]
    public static bool IsNull(object obj) => obj == null || (obj is JsValue jsValue && (jsValue == JsValue.Undefined || jsValue == JsValue.Null));

    [Obsolete("Internal Only!", true)]
    public static bool IsTrue(object obj) => IsNull(obj) || obj.Equals(true);
}
