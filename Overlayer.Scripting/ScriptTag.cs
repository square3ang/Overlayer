using Overlayer.Tags;
using Overlayer.Tags.Attributes;
using System;
using System.IO;
using System.Reflection;

namespace Overlayer.Scripting
{
    public class ScriptTag : OverlayerTag
    {
        public string Script { get; }
        public string Path { get; }
        public ScriptTag(string pathOrScript, MethodInfo method, TagAttribute attr, object target = null) : base(method, attr, target)
        {
            if (File.Exists(pathOrScript))
                Path = pathOrScript;
            else Script = pathOrScript;
        }
        public ScriptTag(string pathOrScript, FieldInfo field, TagAttribute attr, object target = null) : base(field, attr, target)
        {
            if (File.Exists(pathOrScript))
                Path = pathOrScript;
            else Script = pathOrScript;
        }
        public ScriptTag(string pathOrScript, PropertyInfo prop, TagAttribute attr, object target = null) : base(prop, attr, target)
        {
            if (File.Exists(pathOrScript))
                Path = pathOrScript;
            else Script = pathOrScript;
        }
        public ScriptTag(string pathOrScript, string name, Delegate del, bool notPlaying, ValueProcessing flags = ValueProcessing.None) : base(name, del, notPlaying, flags)
        {
            if (File.Exists(pathOrScript))
                Path = pathOrScript;
            else Script = pathOrScript;
        }
    }
}
