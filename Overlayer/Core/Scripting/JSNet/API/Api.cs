using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop.Attributes;

namespace Overlayer.Core.Scripting.JSNet.API;

public class Api {
    public List<(ApiAttribute, MethodInfo)> Methods { get; } = [];

    public List<(ApiAttribute, Type)> Types { get; } = [];

    public void RegisterMethod(MethodInfo method, ApiAttribute attr = null) {
        attr ??= method.GetCustomAttribute<ApiAttribute>();
        if (attr == null) return;
        Methods.Add((attr, method));
        if (attr.RequireTypes == null) return;
        for (var i = 0; i < attr.RequireTypes.Length; i++) {
            var reqType = attr.RequireTypes[i];
            if (Types.FindIndex(t => t.Item2 == reqType) < 0)
                Types.Add((new ApiAttribute(attr.GetRequireTypeAlias(i)), reqType));
        }
    }

    public void RegisterType(Type type) {
        var customAttribute = type.GetCustomAttribute<ApiAttribute>();
        if (customAttribute != null) {
            if (customAttribute.RequireTypes != null)
                for (var i = 0; i < customAttribute.RequireTypes.Length; i++) {
                    var reqType = customAttribute.RequireTypes[i];
                    if (Types.FindIndex(t => t.Item2 == reqType) < 0)
                        Types.Add((new ApiAttribute(customAttribute.GetRequireTypeAlias(i)), reqType));
                }

            if (Types.FindIndex(tuple => tuple.Item2 == type) < 0) Types.Add((customAttribute, type));
            return;
        }

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                      BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField |
                                      BindingFlags.GetProperty | BindingFlags.SetProperty);
        foreach (var methodInfo in methods) {
            var customAttribute2 = methodInfo.GetCustomAttribute<ApiAttribute>();
            if (customAttribute2 == null) continue;
            Methods.Add((customAttribute2, methodInfo));
            if (customAttribute2.RequireTypes == null) continue;
            for (var k = 0; k < customAttribute2.RequireTypes.Length; k++) {
                var reqType2 = customAttribute2.RequireTypes[k];
                if (Types.FindIndex(t => t.Item2 == reqType2) < 0)
                    Types.Add((new ApiAttribute(customAttribute2.GetRequireTypeAlias(k)), reqType2));
            }
        }

        var nestedTypes = type.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                              BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField |
                                              BindingFlags.GetProperty | BindingFlags.SetProperty);
        foreach (var type2 in nestedTypes) RegisterType(type2);
    }

    public void RegisterAssembly(Assembly ass) {
        var types = ass.GetTypes();
        foreach (var type in types) RegisterType(type);
    }

    public void RegisterNamespace(string ns) {
        foreach (var item in from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(ass => ass.GetTypes())
                 where t.Namespace == ns
                 select t)
            RegisterType(item);
    }

    public string Generate() {
        StringBuilder stringBuilder = new();
        foreach (var (apiAttribute, type) in Types) WriteType(type, stringBuilder, apiAttribute.Name);
        foreach (var method in Methods) {
            var item = method.Item1;
            var item2 = method.Item2;
            var returnType = item2.ReturnType;
            var parameters = item2.GetParameters();
            var text = item.Name ?? Alias(item2);
            if (parameters.Length != 0) {
                (Type, string, bool)[] source = (from p in parameters
                    select (p.ParameterType, p.Name, p.IsDefined(typeof(ParamArrayAttribute), false))
                    into t
                    where t.ParameterType != typeof(Engine)
                    select t).ToArray();
                stringBuilder.AppendLine(GetPRTypeHintComment(returnType, "", item,
                    source.Select(((Type ParameterType, string Name, bool) op) => (op.ParameterType, op.Name))
                        .ToArray()));
                var text2 = source.Aggregate("",
                    (string c, (Type ParameterType, string Name, bool) n) =>
                        c + (n.Item3 ? "..." + n.Name : n.Name) + ", ");
                stringBuilder.AppendLine("function " + text + "(" +
                                         (text2.Length == 0 ? "" : text2.Remove(text2.Length - 2)) + ") {}");
            }
            else {
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

        foreach (var (apiAttribute, type) in Types) {
            var name = apiAttribute.Name ?? type.Name;

            engine.SetValue(name, new TypeWrapper(type));
        }

        foreach (var (apiAttribute, methodInfo) in Methods) {
            var name = apiAttribute.Name ?? methodInfo.Name;

            engine.SetValue(name, CreateFunction(methodInfo, engine));
        }

        return engine;
    }

    private static Delegate CreateFunction(MethodInfo method, Engine engine) {
        return new Func<JsValue, JsValue[], JsValue>((thisObj, jsArgs) => {
            try {
                var parameters = method.GetParameters();
                var pCount = parameters.Length;
                var finalArgs = new object[pCount];
                jsArgs ??= [];

                var hasParams = pCount > 0 && parameters[pCount - 1].GetCustomAttribute<ParamArrayAttribute>() != null;
                var normalCount = hasParams ? pCount - 1 : pCount;

                for (var i = 0; i < normalCount; i++) {
                    var p = parameters[i];
                    if (p.ParameterType == typeof(Engine)) {
                        finalArgs[i] = engine;
                        continue;
                    }

                    if (i < jsArgs.Length)
                        finalArgs[i] = engine.TypeConverter.Convert(jsArgs[i], p.ParameterType, null);
                    else if (p.HasDefaultValue)
                        finalArgs[i] = p.DefaultValue;
                    else
                        finalArgs[i] = p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
                }

                if (hasParams) {
                    var lastParam = parameters[pCount - 1];
                    var elementType = lastParam.ParameterType.GetElementType();
                    var remainingJsArgs = Math.Max(0, jsArgs.Length - normalCount);

                    var paramArray = Array.CreateInstance(elementType, remainingJsArgs);
                    for (var i = 0; i < remainingJsArgs; i++) {
                        var val = engine.TypeConverter.Convert(jsArgs[normalCount + i], elementType, null);
                        paramArray.SetValue(val, i);
                    }

                    finalArgs[pCount - 1] = paramArray;
                }

                var result = method.Invoke(null, finalArgs);
                return result == null ? JsValue.Null : JsValue.FromObject(engine, result);
            }
            catch (TargetInvocationException tie) {
                throw tie.InnerException ?? tie;
            }
            catch (Exception e) {
                throw new JavaScriptException(engine.Intrinsics.Error, e.Message);
            }
        });
    }

    public static ApiAttribute Get(Type t, Api api = null) {
        return api?.Types.Find(tup => tup.Item2 == t).Item1 ??
               t.GetCustomAttribute<ApiAttribute>();
    }

    public static ApiAttribute Get(MethodInfo m, Api api = null) {
        return api?.Methods.Find(tup => tup.Item2 == m).Item1 ??
               m.GetCustomAttribute<ApiAttribute>();
    }

    private void WriteType(Type type, StringBuilder sb, string alias) {
        sb.Append("class ");
        var text = alias ?? GetTypeName(type);
        sb.Append(text);
        sb.AppendLine(" {");
        var constructors = type.GetConstructors();
        var text2 = type.GetCustomAttribute<ConstructorAttribute>()?.Arguments;
        if (text2 == null && constructors.Length != 0)
            foreach (var constructorInfo in constructors)
                if (constructorInfo.GetParameters().Length != 0) {
                    var source = from p in constructorInfo.GetParameters()
                        where p.ParameterType != typeof(Engine)
                        select p;
                    IEnumerable<(Type, string)> source2 = source.Select(p => (p.ParameterType, p.Name));
                    sb.AppendLine(GetPTypeHintComment("  ", null, source2.ToArray()));
                    var text3 = source.Aggregate("", (c, n) => c + n.Name + ", ");
                    if (text3.Length > 2) text3 = text3.Remove(text3.Length - 2);
                    text2 = text3;
                    break;
                }

        text2 ??= string.Empty;
        sb.AppendLine("  constructor(" + text2 + ") {");
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                    BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty |
                                    BindingFlags.SetProperty);
        foreach (var fieldInfo in fields)
            if (IsVisible(fieldInfo) && !fieldInfo.Name.StartsWith("<") && !fieldInfo.IsStatic) {
                sb.AppendLine("    " + GetTypeHintComment(fieldInfo.FieldType));
                sb.AppendLine("    this." + Alias(fieldInfo) + " = null;");
            }

        sb.AppendLine("  }");
        fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                                BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty |
                                BindingFlags.SetProperty);
        foreach (var fieldInfo2 in fields)
            if (IsVisible(fieldInfo2) && !fieldInfo2.Name.StartsWith("<")) {
                sb.AppendLine("  " + GetTypeHintComment(fieldInfo2.FieldType, text));
                sb.AppendLine("  static " + Alias(fieldInfo2) + ";");
            }

        if (!type.IsEnum) {
            foreach (var item in from x in type.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                                           BindingFlags.Public | BindingFlags.NonPublic |
                                                           BindingFlags.GetField | BindingFlags.SetField |
                                                           BindingFlags.GetProperty | BindingFlags.SetProperty)
                     orderby x.Name
                     select x)
                if (!IsByRef(item) && IsVisible(item) && !(item.DeclaringType == typeof(object)) &&
                    !item.Name.StartsWith("<") && (!item.IsSpecialName || item.Name.StartsWith("add_") ||
                                                   item.Name.StartsWith("remove_"))) {
                    var source3 = from p in item.GetParameters()
                        where p.ParameterType != typeof(Engine)
                        select p;
                    sb.AppendLine(GetPRTypeHintComment(
                        parameters: source3.Select(p => (p.ParameterType, p.Name)).ToArray(),
                        returnType: item.ReturnType, indent: "  ", attr: Get(item)));
                    var text4 = source3.Aggregate("", (c, n) => c + n.Name + ", ");
                    if (text4.Length > 2) text4 = text4.Remove(text4.Length - 2);
                    var text5 = Alias(item).Split('.').Last();
                    if (item.IsStatic)
                        sb.AppendLine("  static " + text5 + "(" + text4 + ") {}");
                    else
                        sb.AppendLine("  " + text5 + "(" + text4 + ") {}");
                }

            foreach (var item2 in from x in type.GetProperties(BindingFlags.Instance | BindingFlags.Static |
                                                               BindingFlags.Public | BindingFlags.NonPublic |
                                                               BindingFlags.GetField | BindingFlags.SetField |
                                                               BindingFlags.GetProperty | BindingFlags.SetProperty)
                     orderby x.Name
                     select x) {
                if (item2.Name.StartsWith("<")) continue;
                var text6 = item2.Name.Split('.').Last();
                var getMethod = item2.GetGetMethod(true);
                var setMethod = item2.GetSetMethod(true);
                if (getMethod != null) {
                    sb.AppendLine("  " + GetTypeHintComment(item2.PropertyType));
                    if (getMethod.IsStatic)
                        sb.AppendLine("  static get " + text6 + "() {}");
                    else
                        sb.AppendLine("  get " + text6 + "() {}");
                }

                if (setMethod != null) {
                    sb.AppendLine("  " + GetPTypeHintComment("", null, (item2.PropertyType, "value")));
                    if (setMethod.IsStatic)
                        sb.AppendLine("  static set " + text6 + "(value) {}");
                    else
                        sb.AppendLine("  set " + text6 + "(value) {}");
                }
            }
        }

        sb.AppendLine("}");
    }

    private string GetPRTypeHintComment(Type returnType, string indent, ApiAttribute attr,
        params (Type, string)[] parameters) {
        var valueOrDefault = (attr?.ParamComment == null ? -1 : attr?.ParamComment.Length).GetValueOrDefault(-1);
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(indent + "/**");
        if (attr?.Comment != null)
            for (var i = 0; i < attr.Comment.Length; i++) {
                stringBuilder.AppendLine(indent + " * " + attr.Comment[i]);
                if (i < attr.Comment.Length - 1) stringBuilder.AppendLine(" *");
            }

        for (var j = 0; j < parameters.Length; j++) {
            var tuple = parameters[j];
            stringBuilder.AppendLine(indent + " * @param {" + GetTypeHintCode(tuple.Item1) + "} " + tuple.Item2 +
                                     (valueOrDefault - 1 >= j ? " " + attr?.ParamComment[j] : ""));
        }

        stringBuilder.AppendLine(indent + " * @returns {" + GetTypeHintCode(returnType) + "}" +
                                 (attr != null && attr.ReturnComment != null ? " " + attr?.ReturnComment : ""));
        stringBuilder.Append(indent + " */");
        return stringBuilder.ToString();
    }

    private string GetRTypeHintComment(Type returnType, string indent, ApiAttribute attr) {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(indent + "/**");
        if (attr?.Comment != null)
            for (var i = 0; i < attr.Comment.Length; i++) {
                stringBuilder.AppendLine(indent + " * " + attr.Comment[i]);
                if (i < attr.Comment.Length - 1) stringBuilder.AppendLine(" *");
            }

        stringBuilder.AppendLine(indent + " * @returns {" + GetTypeHintCode(returnType) + "}" +
                                 (attr != null && attr.ReturnComment != null ? " " + attr?.ReturnComment : ""));
        stringBuilder.Append(indent + " */");
        return stringBuilder.ToString();
    }

    private string GetPTypeHintComment(string indent, ApiAttribute attr, params (Type, string)[] parameters) {
        var valueOrDefault = (attr?.ParamComment == null ? -1 : attr?.ParamComment.Length).GetValueOrDefault(-1);
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(indent + "/**");
        if (attr?.Comment != null)
            for (var i = 0; i < attr.Comment.Length; i++) {
                stringBuilder.AppendLine(indent + " * " + attr.Comment[i]);
                if (i < attr.Comment.Length - 1) stringBuilder.AppendLine(" *");
            }

        for (var j = 0; j < parameters.Length; j++) {
            var tuple = parameters[j];
            stringBuilder.AppendLine(indent + " * @param {" + GetTypeHintCode(tuple.Item1) + "} " + tuple.Item2 +
                                     (valueOrDefault - 1 >= j ? " " + attr?.ParamComment[j] : ""));
        }

        stringBuilder.Append(indent + " */");
        return stringBuilder.ToString();
    }

    private string GetTypeHintCode(Type type) {
        var apiAttribute = Get(type);
        if (apiAttribute != null && apiAttribute.Name != null) return apiAttribute.Name;
        if (type == typeof(void)) return "void";
        if (type == typeof(Array)) return "any[]";
        switch (Type.GetTypeCode(type)) {
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
                var item = Types.Find(t => t.Item2 == type).Item1;
                return item != null
                    ? item.Name
                    : type.Namespace != null
                        ? RemoveAfter(
                            type.FullName?.Replace(type.Namespace + ".", "").Replace('+', '.') ?? GetTypeName(type),
                            "`").Replace("Instance", "")
                        : GetTypeName(type).Replace("Instance", "");
            }
            default:
                return "undefined";
        }
    }

    private string GetTypeHintComment(Type type, string originalName = null) {
        return "/**@type {" +
               (originalName == null ? GetTypeHintCode(type) : type.IsEnum ? originalName : GetTypeHintCode(type)) +
               "}*/";
    }

    private string RemoveAfter(string str, string after) {
        var num = str.IndexOf(after);
        return num < 0 ? str : str.Remove(num, str.Length - num);
    }

    private static bool IsVisible(MemberInfo member) {
        return member.GetCustomAttribute<NotVisibleAttribute>() == null;
    }

    private static bool IsInclude(FieldInfo f) {
        return (f.IsPublic && f.GetCustomAttribute<ExcludeAttribute>() == null) ||
               (!f.IsPublic && f.GetCustomAttribute<IncludeAttribute>() != null);
    }

    private static bool IsInclude(MethodInfo m) {
        return (m.IsPublic && m.GetCustomAttribute<ExcludeAttribute>() == null) ||
               (!m.IsPublic && m.GetCustomAttribute<IncludeAttribute>() != null);
    }

    private static bool IsInclude(Type t) {
        return ((t.IsPublic || t.IsNestedPublic) && t.GetCustomAttribute<ExcludeAttribute>() == null) ||
               ((!t.IsPublic || t.IsNestedPrivate) && t.GetCustomAttribute<IncludeAttribute>() != null);
    }

    private static string Alias(MemberInfo member) {
        return member.GetCustomAttribute<ApiAttribute>()?.Name ??
               member.GetCustomAttribute<AliasAttribute>()?.Name ?? member.Name;
    }

    private static bool IsByRef(MethodInfo method) {
        return method.GetParameters().Any(p => p.ParameterType.IsByRef);
    }

    private static string GetTypeName(Type t) {
        StringBuilder stringBuilder = new(t.Name);
        if (t.IsGenericType) {
            stringBuilder.Append('_');
            var genericArguments = t.GetGenericArguments();
            foreach (var t2 in genericArguments) stringBuilder.Append(GetTypeName(t2) + "_");
        }

        return stringBuilder.ToString().Replace("Single", "Float").RemoveLastAfter("_");
    }
}