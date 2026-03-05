using DG.Tweening;
using Overlayer.Core;
using Overlayer.Core.TextReplacing;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Overlayer.Tags;

public static class Effect {
    [JSImplementedBy("Discord@kkitut")]
    [Tag(NotPlaying = true)]
    public static string ColorRange(string rawFunc, double valueMin, double valueMax, string colorMinHex, string colorMaxHex, Ease ease = Ease.Linear, int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) {
        OverlayerTag ovTag = TagManager.GetTag(rawFunc);
        if(ovTag == null) {
            return "Tag Not Found!";
        }
        if(!ovTag.NotPlaying && !Main.IsPlaying) {
            return "Tag Not Playing!";
        }
        Tag tag = ovTag.Tag;
        Delegate getter = tag.GetterDelegate;

        double val = 0;
        if(getter is Func<string> fs) {
            val = StringConverter.ToDouble(fs());
        } else if(getter is Func<string, string> fss) {
            val = StringConverter.ToDouble(fss("6"));
        } else {
            return "Not Supported Tag!";
        }
        val = Clamp(val, valueMin, valueMax);

        float eased = DOVirtual.EasedValue(0, 1, Mathf.InverseLerp((float)valueMin, (float)valueMax, (float)val), ease);
        string hexMin = NormalizeHex(colorMinHex, out int fmtMin);
        string hexMax = NormalizeHex(colorMaxHex, out int fmtMax);
        if(fmtMin == -1 || fmtMax == -1) {
            return "Hex length must be 3, 4, 6, or 8!";
        }
        if(fmtMin != fmtMax) {
            return "Min/Max hex lengths must match!";
        }

        ColorUtility.TryParseHtmlString("#" + hexMin, out Color min);
        ColorUtility.TryParseHtmlString("#" + hexMax, out Color max);
        Color newColor = new(
            ((1 - eased) * min.r) + (eased * max.r),
            ((1 - eased) * min.g) + (eased * max.g),
            ((1 - eased) * min.b) + (eased * max.b),
            ((1 - eased) * min.a) + (eased * max.a)
        );

        return ColorUtility.ToHtmlStringRGBA(newColor).Trim(maxLength, afterTrimStr);
    }
    public static double Clamp(double value, double min, double max) => value < min ? min : value > max ? max : value;
    public static string NormalizeHex(string hex, out int fmt) {
        if(hex[0] == '#') {
            hex = hex.Substring(1);
        }
        if(hex.Length == 8) {
            fmt = 8;
            return hex;
        }
        if(hex.Length == 6) {
            fmt = 6;
            return hex;
        }
        if(hex.Length == 3) {
            fmt = 3;
            return $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }
        if(hex.Length == 4) {
            fmt = 4;
            return $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
        }

        fmt = -1;
        return null;
    }
    public static string FormatOutput(Color c, int fmt) {
        string h = ColorUtility.ToHtmlStringRGBA(c);
        if(fmt == 8) {
            return h;
        }
        if(fmt == 6) {
            return h.Substring(0, 6);
        }
        return fmt == 3 ? $"{h[0]}{h[2]}{h[4]}" : fmt == 4 ? h : h;
    }

    static Dictionary<string, double> movingMan_tagValueCache = new();
    static Dictionary<string, long> movingMan_tagStartTimeCache = new();
    [JSImplementedBy("Discord@kkitut")]
    [Tag(NotPlaying = true)]
    public static double MovingMan(string rawFunc = nameof(ComboStats.Combo), double startSize = 30, double endSize = 80, double defaultSize = 30, double speed = 800, bool invert = false, Ease ease = Ease.OutExpo) {
        OverlayerTag ovTag = TagManager.GetTag(rawFunc);
        if(ovTag == null || (!ovTag.NotPlaying && !Main.IsPlaying)) {
            return defaultSize;
        }
        Tag tag = ovTag.Tag;
        Delegate getter = tag.GetterDelegate;

        double val = 0;
        if(getter is Func<string> fs) {
            val = StringConverter.ToDouble(fs());
        } else if(getter is Func<string, string> fss) {
            val = StringConverter.ToDouble(fss("6"));
        } else {
            return defaultSize;
        }

        movingMan_tagValueCache.TryGetValue(rawFunc, out double vCache);
        movingMan_tagStartTimeCache.TryGetValue(rawFunc, out long stCache);
        long mills = FastDateTime.Now.Ticks / 10000;
        if(val != vCache) {
            movingMan_tagStartTimeCache[rawFunc] = stCache = mills;
            movingMan_tagValueCache[rawFunc] = val;
        }
        float elapsed = mills - stCache;
        if(elapsed < speed) {
            float lifetime = (float)(elapsed / speed);
            float eased = DOVirtual.EasedValue(0, 1, lifetime, ease);
            if(invert) {
                eased = 1 - eased;
            }

            float changed = (float)(endSize - startSize) * eased;
            return startSize + changed;
        }

        return defaultSize;
    }

    [JSImplementedBy("Discord@wsbimango")]
    [Tag(NotPlaying = true)]
    public static double EasedValue(string rawFunc = nameof(Bpm.TileBpm), int digits = -1, double speed = 500, Ease ease = Ease.Linear) {
        OverlayerTag ovTag = TagManager.GetTag(rawFunc);
        if(ovTag == null || (!ovTag.NotPlaying && !Main.IsPlaying)) {
            return 0;
        }
        Tag tag = ovTag.Tag;

        Delegate getter = tag.GetterDelegate;
        EventEase ee = new(
            getter is Func<string> fs ?
            () => StringConverter.ToDouble(fs()) :
            getter is Func<string, string> fss ?
            () => StringConverter.ToDouble(fss("6")) :
            null, ease, speed, false);
        if(ee.Getter == null) {
            return 0;
        }

        var easedValue = ee.Compute(rawFunc);
        var prev = ee.GetPrevValue(rawFunc);

        return (prev + ((ee.Value - prev) * easedValue)).Round(digits);
    }

    [Tag(NotPlaying = true)]
    public static string Rainbow(double speed = 18) {
        double hue = Environment.TickCount % (int)(360 * speed) / speed;

        double c = 1;
        double x = 1 - Math.Abs((hue / 60 % 2) - 1);
        double m = 0;
        double r, g = 0, b;

        if(hue < 60) { r = c; g = x; b = 0; } else if(hue < 120) { r = x; g = c; b = 0; } else if(hue < 180) { r = 0; g = c; b = x; } else if(hue < 240) { r = 0; g = x; b = c; } else if(hue < 300) { r = x; g = 0; b = c; } else { r = c; g = 0; b = x; }

        int R = (int)((r + m) * 255);
        int G = (int)((g + m) * 255);
        int B = (int)((b + m) * 255);

        return $"{R:X2}{G:X2}{B:X2}";
    }
}
