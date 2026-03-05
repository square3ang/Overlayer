using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using Overlayer.Models;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Overlayer.Core;

public static class TextConfigImporter {
    public static TextConfig Import(JToken node) {
        var config = ModelUtils.Unbox<TextConfig>(node);
        var refsNode = node["References"] ?? new JArray();
        var refs = ModelUtils.UnwrapList<Reference>((JArray)refsNode);

        if(refs.Any()) {
            var refsDir = Path.Combine(Main.Mod.Path, "References");
            var fontsDir = Path.Combine(refsDir, "Fonts");
            Directory.CreateDirectory(refsDir);

            if(refs.Any(r => r.ReferenceType == Reference.Type.Font)) {
                Directory.CreateDirectory(fontsDir);
            }

            foreach(var @ref in refs) {
                if(@ref.ReferenceType == Reference.Type.Font) {
                    var targetPath = Path.Combine(fontsDir, @ref.Name);
                    File.WriteAllBytes(targetPath, @ref.Raw.Decompress());

                    if((Path.GetFileName(config.Font?.Replace("{ModDir}", Main.Mod.Path)) ?? "") == @ref.Name) {
                        config.Font = targetPath;
                    }
                }
            }
        }

        return config;
    }
    public static JArray GetReferences(TextConfig text) {
        var references = new List<Reference>();

        if(!string.IsNullOrWhiteSpace(text.Font) && text.Font != "Default") {
            references.Add(Reference.GetReference(text.Font, Reference.Type.Font));
        }

        if(text.EnableFallbackFonts) {
            foreach(var fallback in text.FallbackFonts ?? Array.Empty<string>()) {
                if(!string.IsNullOrWhiteSpace(fallback) && fallback != "Default") {
                    references.Add(Reference.GetReference(fallback, Reference.Type.Font));
                }
            }
        }

        return ModelUtils.WrapList(references
            .Where(r => r != null)
            .Distinct()
            .ToList());
    }

    public class Reference : IModel, ICopyable<Reference> {
        public enum Type {
            Font,
        }

        public Type ReferenceType;
        public string From;
        public string Name;
        public byte[] Raw;

        static Dictionary<string, Reference> refCache = new();

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
}