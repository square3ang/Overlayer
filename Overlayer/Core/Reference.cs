using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace Overlayer.Core;

public class Reference : IModel, ICopyable<Reference> {
    public enum Type {
        Font,
        Image,
    }

    public Type ReferenceType;
    public string From;
    public string Name;
    public byte[] Raw;

    static Dictionary<string, Reference> refCache = [];

    public static Reference GetReference(string path, Type referenceType) {
        var target = path.Replace("{ModDir}", Main.Mod.Path);
        if(refCache.TryGetValue(target, out var reference)) {
            return reference;
        }

        var @ref = new Reference {
            From = target,
            Name = Path.GetFileName(target),
            ReferenceType = referenceType
        };
        if(File.Exists(target)) {
            @ref.Raw = File.ReadAllBytes(target).Compress();
            refCache[target] = @ref;
            return @ref;
        }
        return null;
    }

    public static void Flush() => refCache.Clear();

    public JToken Serialize() {
        var node = new JObject {
            [nameof(ReferenceType)] = ReferenceType.ToString(),
            [nameof(From)] = From,
            [nameof(Name)] = Name,
            [nameof(Raw)] = Raw != null ? Convert.ToBase64String(Raw) : null
        };
        return node;
    }

    public void Deserialize(JToken node) {
        var obj = (JObject)node;
        ReferenceType = EnumHelper<Type>.Parse(obj[nameof(ReferenceType)]?.ToString());
        From = obj[nameof(From)]?.ToString();
        Name = obj[nameof(Name)]?.ToString();
        var rawStr = obj[nameof(Raw)]?.ToString();
        Raw = rawStr != null ? Convert.FromBase64String(rawStr) : null;
    }

    public Reference Copy() {
        return new Reference {
            ReferenceType = ReferenceType,
            From = From,
            Name = Name,
            Raw = Raw
        };
    }
}