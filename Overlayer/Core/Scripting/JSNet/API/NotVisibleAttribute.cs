using System;

namespace Overlayer.Core.Scripting.JSNet.API;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, Inherited = false)]
public class NotVisibleAttribute : Attribute { }