using System;

namespace Overlayer.Tags.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class TagAttribute(string name) : Attribute {
    public string Name { get; } = name;
    public bool NotPlaying { get; set; }
    public bool Hide { get; set; }
    public ValueProcessing ProcessingFlags { get; set; }
    public TagAttribute() : this(null) { }
}
