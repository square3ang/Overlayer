using System;
using System.Reflection;
using Jint.Runtime.Interop.Attributes;

namespace Overlayer.Core.Scripting.JSNet.API;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class ApiAttribute : Attribute {
    public string Name { get; }

    public Type[] RequireTypes { get; set; }

    public string[] RequireTypesAliases { get; set; }

    public string[] Comment { get; set; }

    public string[] ParamComment { get; set; }

    public string ReturnComment { get; set; }

    public ApiAttribute() { }

    public ApiAttribute(string name) {
        Name = name;
    }

    public string GetRequireTypeAlias(int index) {
        if (index >= RequireTypes.Length) return null;
        var type = RequireTypes[index];
        return RequireTypesAliases == null || RequireTypesAliases.Length <= index
            ? type.GetCustomAttribute<AliasAttribute>()?.Name ?? type.Name
            : RequireTypesAliases[index] ?? type.GetCustomAttribute<AliasAttribute>()?.Name ?? type.Name;
    }
}