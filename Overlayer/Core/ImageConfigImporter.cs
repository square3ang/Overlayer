using Newtonsoft.Json.Linq;
using Overlayer.Models;
using Overlayer.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Overlayer.Core;

public static class ImageConfigImporter {
    public static ImageConfig Import(JToken node) {
        var config = ModelUtils.Unbox<ImageConfig>(node);
        var refsNode = node["References"] ?? new JArray();
        var refs = ModelUtils.UnwrapList<Reference>((JArray)refsNode);

        if(refs.Any()) {
            var refsDir = Path.Combine(Main.Mod.Path, "References");
            var imagesDir = Path.Combine(refsDir, "Images");
            Directory.CreateDirectory(refsDir);

            if(refs.Any(r => r.ReferenceType == Reference.Type.Image)) {
                Directory.CreateDirectory(imagesDir);
            }

            foreach(var @ref in refs) {
                if(@ref.ReferenceType == Reference.Type.Image) {
                    var targetPath = Path.Combine(imagesDir, @ref.Name);
                    File.WriteAllBytes(targetPath, @ref.Raw.Decompress());

                    var relPath = "{ModDir}References/Images/" + @ref.Name;

                    for(int i = 0; i < config.Images.Count; i++) {
                        if((Path.GetFileName(config.Images[i]?.Replace("{ModDir}", Main.Mod.Path)) ?? "") == @ref.Name) {
                            config.Images[i] = relPath;
                        }
                    }
                }
            }
        }

        return config;
    }

    public static void ImportRef(ImageConfig config, JToken node) {
        var refsNode = node["References"] ?? new JArray();
        var refs = ModelUtils.UnwrapList<Reference>((JArray)refsNode);

        if(!refs.Any()) {
            return;
        }

        var refsDir = Path.Combine(Main.Mod.Path, "References");
        var imagesDir = Path.Combine(refsDir, "Images");
        Directory.CreateDirectory(refsDir);

        if(refs.Any(r => r.ReferenceType == Reference.Type.Image)) {
            Directory.CreateDirectory(imagesDir);
        }

        foreach(var @ref in refs) {
            if(@ref.ReferenceType == Reference.Type.Image) {
                var targetPath = Path.Combine(imagesDir, @ref.Name);
                File.WriteAllBytes(targetPath, @ref.Raw.Decompress());

                var relPath = "{ModDir}References/Images/" + @ref.Name;

                for(int i = 0; i < config.Images.Count; i++) {
                    if((Path.GetFileName(config.Images[i]?.Replace("{ModDir}", Main.Mod.Path)) ?? "") == @ref.Name) {
                        config.Images[i] = relPath;
                    }
                }
            }
        }
    }

    public static JArray GetReferences(ImageConfig config) {
        var references = new List<Reference>();
        foreach(var image in config.Images) {
            if(!string.IsNullOrWhiteSpace(image)) {
                references.Add(Reference.GetReference(image, Reference.Type.Image));
            }
        }
        return ModelUtils.WrapList(references.Where(r => r != null).Distinct().ToList());
    }

    public static JArray GetJustReferences(ImageConfig config) {
        var references = new List<Reference>();

        foreach(var image in config.Images) {
            if(!string.IsNullOrWhiteSpace(image)) {
                references.Add(new Reference {
                    Name = image,
                    ReferenceType = Reference.Type.Image
                });
            }
        }

        var set = new HashSet<string>();
        var result = new List<Reference>();

        foreach(var r in references) {
            if(r == null) {
                continue;
            }

            string key = $"{r.ReferenceType}:{r.Name}";
            if(set.Add(key)) {
                result.Add(r);
            }
        }

        return ModelUtils.WrapList(result);
    }
}