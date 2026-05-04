using Overlayer.Core;
using Overlayer.Tags.Attributes;
using UnityEngine;

namespace Overlayer.Tags;

public static class InternalProfile {
    [Tag(Hide = true)]
    public static string IT_Santokki_Combo() {
        return null;
    }

    [Tag(Hide = true)]
    public static string IT_Santokki_XAccuracy() {
        if(AccuracyStats.XAccuracy != 100) {
            return string.Join(",", [
                1f, 0.8588f, 0.9137f, 1f,
            1f, 0.9490f, 0.9765f, 1f,
            1f, 1f, 1f, 1f,
            0.8392f, 0.8392f, 0.8392f, 1f
            ]);
        }

        long ticks = FastDateTime.Now.Ticks;

        float baseT = ticks / 29314f;

        float t0 = baseT * 1.0f % 1f;
        float t1 = baseT * 1.7f % 1f;
        float t2 = baseT * 2.3f % 1f;
        float t3 = baseT * 3.1f % 1f;

        Color c0 = Color.HSVToRGB(t0, 1f, 1f);
        Color c1 = Color.HSVToRGB(t1, 1f, 1f);
        Color c2 = Color.HSVToRGB(t2, 1f, 1f);
        Color c3 = Color.HSVToRGB(t3, 1f, 1f);

        return string.Join(",", [
            c0.r, c0.g, c0.b, 1f,
            c1.r, c1.g, c1.b, 1f,
            c2.r, c2.g, c2.b, 1f,
            c3.r, c3.g, c3.b, 1f
        ]);
    }

    [Tag(Hide = true)]
    public static string IT_Santokki_Timing() {
        return null;
    }
}
