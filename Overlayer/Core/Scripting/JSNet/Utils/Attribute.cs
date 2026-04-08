using System;

namespace Jint.Runtime.Interop.Attributes {
    public class AliasAttribute(string name) : Attribute {
        public string Name { get; } = name;
    }
}