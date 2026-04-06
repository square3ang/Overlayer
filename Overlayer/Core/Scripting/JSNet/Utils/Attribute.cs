using System;

namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class IgnoresAccessChecksToAttribute(string assemblyName) : Attribute {
        public string AssemblyName { get; } = assemblyName;
    }
}

namespace Jint.Runtime.Interop.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public class RawReturnAttribute : Attribute { }
}

namespace Jint.Runtime.Interop.Attributes {
    public class AliasAttribute(string name) : Attribute {
        public string Name { get; } = name;
    }
}