using System;

namespace Overlayer.Core.Scripting.JSNet.API;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class IncludeAttribute : Attribute {
}
