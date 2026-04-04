using System;

namespace Overlayer.Core.Scripting.JSNet.API;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class NotVisibleAttribute : Attribute {
}
