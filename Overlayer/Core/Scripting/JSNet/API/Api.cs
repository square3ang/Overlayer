using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Overlayer.Core.Scripting.JSNet.API;

public class Api {
    public List<(ApiAttribute, MethodInfo)> Methods { get; } = [];

    public List<(ApiAttribute, Type)> Types { get; } = [];

    public void RegisterMethod(MethodInfo method, ApiAttribute attr = null) {
        attr ??= method.GetCustomAttribute<ApiAttribute>();
        if(attr == null) {
            return;
        }
        Methods.Add((attr, method));
        if(attr.RequireTypes == null) {
            return;
        }
        for(int i = 0; i < attr.RequireTypes.Length; i++) {
            Type reqType = attr.RequireTypes[i];
            if(Types.FindIndex(((ApiAttribute, Type) t) => t.Item2 == reqType) < 0) {
                Types.Add((new ApiAttribute(attr.GetRequireTypeAlias(i)), reqType));
            }
        }
    }

    public void RegisterType(Type type) {
        ApiAttribute customAttribute = type.GetCustomAttribute<ApiAttribute>();
        if(customAttribute != null) {
            if(customAttribute.RequireTypes != null) {
                for(int i = 0; i < customAttribute.RequireTypes.Length; i++) {
                    Type reqType = customAttribute.RequireTypes[i];
                    if(Types.FindIndex(((ApiAttribute, Type) t) => t.Item2 == reqType) < 0) {
                        Types.Add((new ApiAttribute(customAttribute.GetRequireTypeAlias(i)), reqType));
                    }
                }
            }
            if(Types.FindIndex(((ApiAttribute, Type) tuple) => tuple.Item2 == type) < 0) {
                Types.Add((customAttribute, type));
            }
            return;
        }
        MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty);
        foreach(MethodInfo methodInfo in methods) {
            ApiAttribute customAttribute2 = methodInfo.GetCustomAttribute<ApiAttribute>();
            if(customAttribute2 == null) {
                continue;
            }
            Methods.Add((customAttribute2, methodInfo));
            if(customAttribute2.RequireTypes == null) {
                continue;
            }
            for(int k = 0; k < customAttribute2.RequireTypes.Length; k++) {
                Type reqType2 = customAttribute2.RequireTypes[k];
                if(Types.FindIndex(((ApiAttribute, Type) t) => t.Item2 == reqType2) < 0) {
                    Types.Add((new ApiAttribute(customAttribute2.GetRequireTypeAlias(k)), reqType2));
                }
            }
        }
        Type[] nestedTypes = type.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty);
        foreach(Type type2 in nestedTypes) {
            RegisterType(type2);
        }
    }

    public void RegisterAssembly(Assembly ass) {
        Type[] types = ass.GetTypes();
        foreach(Type type in types) {
            RegisterType(type);
        }
    }

    public void RegisterNamespace(string ns) {
        foreach(Type item in from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly ass) => ass.GetTypes())
                             where t.Namespace == ns
                             select t) {
            RegisterType(item);
        }
    }

    public string Generate() {
        StringBuilder stringBuilder = new();
        foreach(var (apiAttribute, type) in Types) {
            WriteType(type, stringBuilder, apiAttribute.Name);
        }
        foreach(var method in Methods) {
            ApiAttribute item = method.Item1;
            MethodInfo item2 = method.Item2;
            Type returnType = item2.ReturnType;
            ParameterInfo[] parameters = item2.GetParameters();
            string text = item.Name ?? Alias(item2);
            if(parameters.Length != 0) {
                (Type, string, bool)[] source = (from p in parameters
                                                 select (p.ParameterType, p.Name, p.IsDefined(typeof(ParamArrayAttribute), inherit: false)) into t
                                                 where t.ParameterType != typeof(Engine)
                                                 select t).ToArray();
                stringBuilder.AppendLine(GetPRTypeHintComment(returnType, "", item, source.Select(((Type ParameterType, string Name, bool) op) => (op.ParameterType, op.Name)).ToArray()));
                string text2 = source.Aggregate("", (string c, (Type ParameterType, string Name, bool) n) => c + (n.Item3 ? ("..." + n.Name) : n.Name) + ", ");
                stringBuilder.AppendLine("function " + text + "(" + ((text2.Length == 0) ? "" : text2.Remove(text2.Length - 2)) + ") {}");
            } else {
                stringBuilder.AppendLine(GetPRTypeHintComment(returnType, "", item));
                stringBuilder.AppendLine("function " + text + "() {}");
            }
        }
        return stringBuilder.ToString();
    }

    public Engine PrepareInterpreter() {
        var engine = new Engine(op => {
            op.AllowClr(AppDomain.CurrentDomain.GetAssemblies());
            op.Strict(false);
        });

        foreach(var (apiAttribute, type) in Types) {
            var name = apiAttribute.Name ?? type.Name;

            engine.SetValue(name, new TypeWrapper(type));
        }

        foreach(var (apiAttribute, methodInfo) in Methods) {
            var name = apiAttribute.Name ?? methodInfo.Name;

            engine.SetValue(name, CreateFunction(methodInfo, engine));
        }

        return engine;
    }

    private static Delegate CreateFunction(MethodInfo method, Engine engine) {
        return new Func<JsValue, JsValue[], JsValue>((thisObj, jsArgs) => {
            try {
                var parameters = method.GetParameters();
                int pCount = parameters.Length;
                var finalArgs = new object[pCount];
                jsArgs ??= [];

                bool hasParams = pCount > 0 && parameters[pCount - 1].GetCustomAttribute<ParamArrayAttribute>() != null;
                int normalCount = hasParams ? pCount - 1 : pCount;

                for(int i = 0; i < normalCount; i++) {
                    var p = parameters[i];
                    if(p.ParameterType == typeof(Engine)) {
                        finalArgs[i] = engine;
                        continue;
                    }

                    if(i < jsArgs.Length) {
                        finalArgs[i] = engine.TypeConverter.Convert(jsArgs[i], p.ParameterType, null);
                    } else if(p.HasDefaultValue) {
                        finalArgs[i] = p.DefaultValue;
                    } else {
                        finalArgs[i] = p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
                    }
                }

                if(hasParams) {
                    var lastParam = parameters[pCount - 1];
                    var elementType = lastParam.ParameterType.GetElementType();
                    int remainingJsArgs = Math.Max(0, jsArgs.Length - normalCount);

                    var paramArray = Array.CreateInstance(elementType, remainingJsArgs);
                    for(int i = 0; i < remainingJsArgs; i++) {
                        var val = engine.TypeConverter.Convert(jsArgs[normalCount + i], elementType, null);
                        paramArray.SetValue(val, i);
                    }

                    finalArgs[pCount - 1] = paramArray;
                }

                var result = method.Invoke(null, finalArgs);
                return result == null ? JsValue.Null : JsValue.FromObject(engine, result);

            } catch(TargetInvocationException tie) {
                throw tie.InnerException ?? tie;
            } catch(Exception e) {
                throw new JavaScriptException(engine.Intrinsics.Error, e.Message);
            }
        });
    }

    public static ApiAttribute Get(Type t, Api api = null) => api?.Types.Find(((ApiAttribute, Type) tup) => tup.Item2 == t).Item1 ?? t.GetCustomAttribute<ApiAttribute>();

    public static ApiAttribute Get(MethodInfo m, Api api = null) => api?.Methods.Find(((ApiAttribute, MethodInfo) tup) => tup.Item2 == m).Item1 ?? m.GetCustomAttribute<ApiAttribute>();

    private void WriteType(Type type, StringBuilder sb, string alias) {
        sb.Append("class ");
        string text = alias ?? GetTypeName(type);
        sb.Append(text);
        sb.AppendLine(" {");
        ConstructorInfo[] constructors = type.GetConstructors();
        string text2 = type.GetCustomAttribute<ConstructorAttribute>()?.Arguments;
        if(text2 == null && constructors.Length != 0) {
            foreach(ConstructorInfo constructorInfo in constructors) {
                if(constructorInfo.GetParameters().Length != 0) {
                    IEnumerable<ParameterInfo> source = from p in constructorInfo.GetParameters()
                                                        where p.ParameterType != typeof(Engine)
                                                        select p;
                    IEnumerable<(Type, string)> source2 = source.Select((ParameterInfo p) => (p.ParameterType, p.Name));
                    sb.AppendLine(GetPTypeHintComment("  ", null, source2.ToArray()));
                    string text3 = source.Aggregate("", (string c, ParameterInfo n) => c + n.Name + ", ");
                    if(text3.Length > 2) {
                        text3 = text3.Remove(text3.Length - 2);
                    }
                    text2 = text3;
                    break;
                }
            }
        }
        text2 ??= string.Empty;
        sb.AppendLine("  constructor(" + text2 + ") {");
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty);
        foreach(FieldInfo fieldInfo in fields) {
            if(IsVisible(fieldInfo) && !fieldInfo.Name.StartsWith("<") && !fieldInfo.IsStatic) {
                sb.AppendLine("    " + GetTypeHintComment(fieldInfo.FieldType));
                sb.AppendLine("    this." + Alias(fieldInfo) + " = null;");
            }
        }
        sb.AppendLine("  }");
        fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty);
        foreach(FieldInfo fieldInfo2 in fields) {
            if(IsVisible(fieldInfo2) && !fieldInfo2.Name.StartsWith("<")) {
                sb.AppendLine("  " + GetTypeHintComment(fieldInfo2.FieldType, text));
                sb.AppendLine("  static " + Alias(fieldInfo2) + ";");
            }
        }
        if(!type.IsEnum) {
            foreach(MethodInfo item in from x in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty)
                                       orderby x.Name
                                       select x) {
                if(!IsByRef(item) && IsVisible(item) && !(item.DeclaringType == typeof(object)) && !item.Name.StartsWith("<") && (!item.IsSpecialName || item.Name.StartsWith("add_") || item.Name.StartsWith("remove_"))) {
                    IEnumerable<ParameterInfo> source3 = from p in item.GetParameters()
                                                         where p.ParameterType != typeof(Engine)
                                                         select p;
                    sb.AppendLine(GetPRTypeHintComment(parameters: source3.Select((ParameterInfo p) => (p.ParameterType, p.Name)).ToArray(), returnType: item.ReturnType, indent: "  ", attr: Get(item)));
                    string text4 = source3.Aggregate("", (string c, ParameterInfo n) => c + n.Name + ", ");
                    if(text4.Length > 2) {
                        text4 = text4.Remove(text4.Length - 2);
                    }
                    string text5 = Alias(item).Split('.').Last();
                    if(item.IsStatic) {
                        sb.AppendLine("  static " + text5 + "(" + text4 + ") {}");
                    } else {
                        sb.AppendLine("  " + text5 + "(" + text4 + ") {}");
                    }
                }
            }
            foreach(PropertyInfo item2 in from x in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty)
                                          orderby x.Name
                                          select x) {
                if(item2.Name.StartsWith("<")) {
                    continue;
                }
                string text6 = item2.Name.Split('.').Last();
                MethodInfo getMethod = item2.GetGetMethod(nonPublic: true);
                MethodInfo setMethod = item2.GetSetMethod(nonPublic: true);
                if(getMethod != null) {
                    sb.AppendLine("  " + GetTypeHintComment(item2.PropertyType));
                    if(getMethod.IsStatic) {
                        sb.AppendLine("  static get " + text6 + "() {}");
                    } else {
                        sb.AppendLine("  get " + text6 + "() {}");
                    }
                }
                if(setMethod != null) {
                    sb.AppendLine("  " + GetPTypeHintComment("", null, (item2.PropertyType, "value")));
                    if(setMethod.IsStatic) {
                        sb.AppendLine("  static set " + text6 + "(value) {}");
                    } else {
                        sb.AppendLine("  set " + text6 + "(value) {}");
                    }
                }
            }
        }
        sb.AppendLine("}");
    }

    private string GetPRTypeHintComment(Type returnType, string indent, ApiAttribute attr, params (Type, string)[] parameters) {
        int valueOrDefault = ((attr?.ParamComment == null) ? new int?(-1) : attr?.ParamComment.Length).GetValueOrDefault(-1);
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(indent + "/**");
        if(attr?.Comment != null) {
            for(int i = 0; i < attr.Comment.Length; i++) {
                stringBuilder.AppendLine(indent + " * " + attr.Comment[i]);
                if(i < attr.Comment.Length - 1) {
                    stringBuilder.AppendLine(" *");
                }
            }
        }
        for(int j = 0; j < parameters.Length; j++) {
            (Type, string) tuple = parameters[j];
            stringBuilder.AppendLine(indent + " * @param {" + GetTypeHintCode(tuple.Item1) + "} " + tuple.Item2 + ((valueOrDefault - 1 >= j) ? (" " + (attr?.ParamComment[j])) : ""));
        }
        stringBuilder.AppendLine(indent + " * @returns {" + GetTypeHintCode(returnType) + "}" + ((attr != null && attr.ReturnComment != null) ? (" " + attr?.ReturnComment) : ""));
        stringBuilder.Append(indent + " */");
        return stringBuilder.ToString();
    }

    private string GetRTypeHintComment(Type returnType, string indent, ApiAttribute attr) {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(indent + "/**");
        if(attr?.Comment != null) {
            for(int i = 0; i < attr.Comment.Length; i++) {
                stringBuilder.AppendLine(indent + " * " + attr.Comment[i]);
                if(i < attr.Comment.Length - 1) {
                    stringBuilder.AppendLine(" *");
                }
            }
        }
        stringBuilder.AppendLine(indent + " * @returns {" + GetTypeHintCode(returnType) + "}" + ((attr != null && attr.ReturnComment != null) ? (" " + attr?.ReturnComment) : ""));
        stringBuilder.Append(indent + " */");
        return stringBuilder.ToString();
    }

    private string GetPTypeHintComment(string indent, ApiAttribute attr, params (Type, string)[] parameters) {
        int valueOrDefault = ((attr?.ParamComment == null) ? new int?(-1) : attr?.ParamComment.Length).GetValueOrDefault(-1);
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(indent + "/**");
        if(attr?.Comment != null) {
            for(int i = 0; i < attr.Comment.Length; i++) {
                stringBuilder.AppendLine(indent + " * " + attr.Comment[i]);
                if(i < attr.Comment.Length - 1) {
                    stringBuilder.AppendLine(" *");
                }
            }
        }
        for(int j = 0; j < parameters.Length; j++) {
            (Type, string) tuple = parameters[j];
            stringBuilder.AppendLine(indent + " * @param {" + GetTypeHintCode(tuple.Item1) + "} " + tuple.Item2 + ((valueOrDefault - 1 >= j) ? (" " + (attr?.ParamComment[j])) : ""));
        }
        stringBuilder.Append(indent + " */");
        return stringBuilder.ToString();
    }

    private string GetTypeHintCode(Type type) {
        ApiAttribute apiAttribute = Get(type);
        if(apiAttribute != null && apiAttribute.Name != null) {
            return apiAttribute.Name;
        }
        if(type == typeof(void)) {
            return "void";
        }
        if(type == typeof(Array)) {
            return "any[]";
        }
        switch(Type.GetTypeCode(type)) {
            case TypeCode.Empty:
            case TypeCode.DBNull:
                return "null";
            case TypeCode.Boolean:
                return "boolean";
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                return "number";
            case TypeCode.DateTime:
                return "Date";
            case TypeCode.Char:
            case TypeCode.String:
                return "string";
            case TypeCode.Object: {
                ApiAttribute item = Types.Find(((ApiAttribute, Type) t) => t.Item2 == type).Item1;
                return item != null
                    ? item.Name
                    : type.Namespace != null
                ? RemoveAfter(type.FullName?.Replace(type.Namespace + ".", "").Replace('+', '.') ?? GetTypeName(type), "`").Replace("Instance", "")
                : GetTypeName(type).Replace("Instance", "");
            }
            default:
                return "undefined";
        }
    }

    private string GetTypeHintComment(Type type, string originalName = null) => "/**@type {" + ((originalName == null) ? GetTypeHintCode(type) : (type.IsEnum ? originalName : GetTypeHintCode(type))) + "}*/";

    private string RemoveAfter(string str, string after) {
        int num = str.IndexOf(after);
        return num < 0 ? str : str.Remove(num, str.Length - num);
    }

    private static bool IsVisible(MemberInfo member) => member.GetCustomAttribute<NotVisibleAttribute>() == null;

    private static bool IsInclude(FieldInfo f) => (f.IsPublic && f.GetCustomAttribute<ExcludeAttribute>() == null) || (!f.IsPublic && f.GetCustomAttribute<IncludeAttribute>() != null);

    private static bool IsInclude(MethodInfo m) => (m.IsPublic && m.GetCustomAttribute<ExcludeAttribute>() == null) || (!m.IsPublic && m.GetCustomAttribute<IncludeAttribute>() != null);

    private static bool IsInclude(Type t) => ((t.IsPublic || t.IsNestedPublic) && t.GetCustomAttribute<ExcludeAttribute>() == null) || ((!t.IsPublic || t.IsNestedPrivate) && t.GetCustomAttribute<IncludeAttribute>() != null);

    private static string Alias(MemberInfo member) => member.GetCustomAttribute<ApiAttribute>()?.Name ?? member.GetCustomAttribute<AliasAttribute>()?.Name ?? member.Name;

    private static bool IsByRef(MethodInfo method) => method.GetParameters().Any((ParameterInfo p) => p.ParameterType.IsByRef);

    private static string GetTypeName(Type t) {
        StringBuilder stringBuilder = new(t.Name);
        if(t.IsGenericType) {
            stringBuilder.Append('_');
            Type[] genericArguments = t.GetGenericArguments();
            foreach(Type t2 in genericArguments) {
                stringBuilder.Append(GetTypeName(t2) + "_");
            }
        }
        return stringBuilder.ToString().Replace("Single", "Float").RemoveLastAfter("_");
    }
}
