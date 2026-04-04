using System;

namespace Overlayer.Core.Scripting.JSNet.API;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class ConstructorAttribute : Attribute {
    public string Arguments { get; }

    public ConstructorAttribute(string ctorArgument) => Arguments = ctorArgument;
}
