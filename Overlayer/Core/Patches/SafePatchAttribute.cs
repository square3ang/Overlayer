using System;

namespace Overlayer.Core.Patches;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SafePatchAttribute : Attribute {
    public string Id { get; }
    public string TargetType { get; }
    public string TargetMethod { get; }

    public SafePatchAttribute(string id, string targetType, string targetMethod) {
        Id = id!;
        TargetType = targetType!;
        TargetMethod = targetMethod!;
    }
}