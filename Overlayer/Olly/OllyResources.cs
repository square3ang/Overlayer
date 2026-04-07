using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityModManagerNet;
using static Overlayer.Olly.OllyState;
using static UnityEngine.Object;

namespace Overlayer.Olly;

public static class OllyResources {
    public static Texture2D Base { get; private set; }
    public static Texture2D BG { get; private set; }
    public static Texture2D Hair { get; private set; }
    public static Texture2D HairBG { get; private set; }
    public static Texture2D EyelidUp { get; private set; }
    public static Texture2D EyelidDown { get; private set; }
    public static Texture2D EyelidBG { get; private set; }
    public static Texture2D Nose { get; private set; }
    public static Texture2D[] Eyebrows { get; private set; }
    public static (Texture2D left, Texture2D right)[] Eyes { get; private set; }
    public static Texture2D[] EyeSpecials { get; private set; }
    public static Texture2D EyeHighlightLeft { get; private set; }
    public static Texture2D EyeHighlightRight { get; private set; }
    public static Texture2D[] Mouths { get; private set; }
    public static Texture2D[] Effects { get; private set; }
    public static Texture2D[] EffectForwards { get; private set; }

    public static bool Loaded { get; private set; } = false;
    public static bool LoadAll(UnityModManager.ModEntry modEntry) {
        if(Loaded) {
            return true;
        }
        string path = Path.Combine(modEntry.Path, "eg.res");

        if(!File.Exists(path)) {
            return false;
        }

        using var zip = ZipFile.OpenRead(path);

        Eyebrows = new Texture2D[Enum.GetValues(typeof(Eyebrow)).Length - 1];
        Eyes = new (Texture2D left, Texture2D right)[Enum.GetValues(typeof(Eye)).Length - 1];
        EyeSpecials = new Texture2D[Enum.GetValues(typeof(EyeSpecial)).Length - 1];
        Mouths = new Texture2D[Enum.GetValues(typeof(Mouth)).Length];
        Effects = new Texture2D[Enum.GetValues(typeof(Effect)).Length - 1];
        EffectForwards = new Texture2D[Enum.GetValues(typeof(EffectForward)).Length - 1];

        foreach(var entry in zip.Entries) {
            if(entry.FullName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) {
                using var stream = entry.Open();
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                var bytes = ms.ToArray();

                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if(ImageConversion.LoadImage(tex, bytes)) {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(entry.Name);

                    if(string.Equals(fileNameWithoutExt, "BASE", StringComparison.OrdinalIgnoreCase)) {
                        Base = tex;
                    } else if(string.Equals(fileNameWithoutExt, "BG", StringComparison.OrdinalIgnoreCase)) {
                        BG = tex;
                    } else if(string.Equals(fileNameWithoutExt, "HAIR", StringComparison.OrdinalIgnoreCase)) {
                        Hair = tex;
                    } else if(string.Equals(fileNameWithoutExt, "HAIRBG", StringComparison.OrdinalIgnoreCase)) {
                        HairBG = tex;
                    } else if(string.Equals(fileNameWithoutExt, "ELU", StringComparison.OrdinalIgnoreCase)) {
                        EyelidUp = tex;
                    } else if(string.Equals(fileNameWithoutExt, "ELD", StringComparison.OrdinalIgnoreCase)) {
                        EyelidDown = tex;
                    } else if(string.Equals(fileNameWithoutExt, "ELBG", StringComparison.OrdinalIgnoreCase)) {
                        EyelidBG = tex;
                    } else if(string.Equals(fileNameWithoutExt, "NOSE", StringComparison.OrdinalIgnoreCase)) {
                        Nose = tex;
                    } else if(fileNameWithoutExt.StartsWith("B_", StringComparison.OrdinalIgnoreCase)) {
                        var enumName = fileNameWithoutExt.Substring(2);
                        if(Enum.TryParse(enumName, true, out Eyebrow brow)) {
                            Eyebrows[(int)brow - 1] = tex;
                        } else {
                            DestroyImmediate(tex);
                        }
                    } else if(fileNameWithoutExt.StartsWith("EL_", StringComparison.OrdinalIgnoreCase)) {
                        var enumName = fileNameWithoutExt.Substring(3);
                        if(Enum.TryParse(enumName, true, out Eye eye)) {
                            int index = (int)eye - 1;
                            Eyes[index] = (tex, Eyes[index].right);
                        } else {
                            DestroyImmediate(tex);
                        }
                    } else if(fileNameWithoutExt.StartsWith("ER_", StringComparison.OrdinalIgnoreCase)) {
                        var enumName = fileNameWithoutExt.Substring(3);
                        if(Enum.TryParse(enumName, true, out Eye eye)) {
                            int index = (int)eye - 1;
                            Eyes[index] = (Eyes[index].left, tex);
                        } else {
                            DestroyImmediate(tex);
                        }
                    } else if(fileNameWithoutExt.StartsWith("M_", StringComparison.OrdinalIgnoreCase)) {
                        var enumName = fileNameWithoutExt.Substring(2);
                        if(Enum.TryParse(enumName, true, out Mouth mouth)) {
                            Mouths[(int)mouth - 1] = tex;
                        } else {
                            DestroyImmediate(tex);
                        }
                    } else if(fileNameWithoutExt.StartsWith("ES_", StringComparison.OrdinalIgnoreCase)) {
                        var enumName = fileNameWithoutExt.Substring(3);
                        if(Enum.TryParse(enumName, true, out EyeSpecial special)) {
                            EyeSpecials[(int)special - 1] = tex;
                        } else {
                            DestroyImmediate(tex);
                        }
                    } else if(fileNameWithoutExt.StartsWith("EHL", StringComparison.OrdinalIgnoreCase)) {
                        EyeHighlightLeft = tex;
                    } else if(fileNameWithoutExt.StartsWith("EHR", StringComparison.OrdinalIgnoreCase)) {
                        EyeHighlightRight = tex;
                    } else if(fileNameWithoutExt.StartsWith("FX_", StringComparison.OrdinalIgnoreCase)) {
                        var enumName = fileNameWithoutExt.Substring(3);
                        if(Enum.TryParse(enumName, true, out Effect effect)) {
                            Effects[(int)effect - 1] = tex;
                        } else {
                            DestroyImmediate(tex);
                        }
                    } else if(fileNameWithoutExt.StartsWith("FXF_", StringComparison.OrdinalIgnoreCase)) {
                        var enumName = fileNameWithoutExt.Substring(4);
                        if(Enum.TryParse(enumName, true, out EffectForward forward)) {
                            EffectForwards[(int)forward - 1] = tex;
                        } else {
                            DestroyImmediate(tex);
                        }
                    } else {
                        DestroyImmediate(tex);
                    }
                } else {
                    DestroyImmediate(tex);
                }
            }
        }
        Loaded =
            Base != null&&
            BG != null&&
            Eyes != null && Eyes.Length == Enum.GetValues(typeof(Eye)).Length - 1 &&
            Mouths != null && Mouths.Length == Enum.GetValues(typeof(Mouth)).Length &&
            Hair != null&&
            EyelidUp != null&&
            EyelidDown != null&&
            EyelidBG != null&&
            Nose != null&&
            Eyebrows != null && Eyebrows.Length == Enum.GetValues(typeof(Eyebrow)).Length - 1 &&
            EyeSpecials != null && EyeSpecials.Length == Enum.GetValues(typeof(EyeSpecial)).Length - 1 &&
            EyeHighlightLeft != null&&
            EyeHighlightRight != null&&
            Effects != null && Effects.Length == Enum.GetValues(typeof(Effect)).Length - 1 &&
            EffectForwards != null && EffectForwards.Length == Enum.GetValues(typeof(EffectForward)).Length - 1;
        if(Loaded) {
            return true;
        }

        UnloadAll();
        return false;
    }

    public static void UnloadAll() {
        if(!Loaded) {
            return;
        }
        Loaded = false;
        if(Base != null) {
            Destroy(Base);
            Base = null;
        }
        if(Hair != null) {
            Destroy(Hair);
            Hair = null;
        }
        if(BG != null) {
            Destroy(BG);
            BG = null;
        }
        if(EyelidUp != null) {
            Destroy(EyelidUp);
            EyelidUp = null;
        }
        if(EyelidDown != null) {
            Destroy(EyelidDown);
            EyelidDown = null;
        }
        if(EyelidBG != null) {
            Destroy(EyelidBG);
            EyelidBG = null;
        }
        for(int i = 0; i < Eyebrows.Length; i++) {
            if(Eyebrows[i] != null) {
                Destroy(Eyebrows[i]);
                Eyebrows[i] = null;
            }
        }

        for(int i = 0; i < Eyes.Length; i++) {
            if(Eyes[i].left != null) {
                Destroy(Eyes[i].left);
                Eyes[i].left = null;
            }
            if(Eyes[i].right != null) {
                Destroy(Eyes[i].right);
                Eyes[i].right = null;
            }
        }

        for(int i = 0; i < EyeSpecials.Length; i++) {
            if(EyeSpecials[i] != null) {
                Destroy(EyeSpecials[i]);
                EyeSpecials[i] = null;
            }
        }

        if(EyeHighlightLeft != null) {
            Destroy(EyeHighlightLeft);
            EyeHighlightLeft = null;
        }
        if(EyeHighlightRight != null) {
            Destroy(EyeHighlightRight);
            EyeHighlightRight = null;
        }

        for(int i = 0; i < Mouths.Length; i++) {
            if(Mouths[i] != null) {
                Destroy(Mouths[i]);
                Mouths[i] = null;
            }
        }

        for(int i = 0; i < Effects.Length; i++) {
            if(Effects[i] != null) {
                Destroy(Effects[i]);
                Effects[i] = null;
            }
        }

        for(int i = 0; i < EffectForwards.Length; i++) {
            if(EffectForwards[i] != null) {
                Destroy(EffectForwards[i]);
                EffectForwards[i] = null;
            }
        }
    }
}
