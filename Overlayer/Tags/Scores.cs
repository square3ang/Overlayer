using Overlayer.Tags.Attributes;

namespace Overlayer.Tags;

public static class Scores {
    [Tag]
    public static int LScore;
    [Tag]
    public static int NScore;
    [Tag]
    public static int SScore;
    [Tag]
    public static int Score;

    public static void SetScores(HitMargin l, HitMargin n, HitMargin s, HitMargin c) {
        switch(c) {
            case HitMargin.VeryEarly:
            case HitMargin.VeryLate:
                Score += 91;
                break;
            case HitMargin.EarlyPerfect:
            case HitMargin.LatePerfect:
                Score += 150;
                break;
            case HitMargin.Perfect:
                Score += 300;
                break;
        }
        switch(l) {
            case HitMargin.VeryEarly:
            case HitMargin.VeryLate:
                LScore += 91;
                break;
            case HitMargin.EarlyPerfect:
            case HitMargin.LatePerfect:
                LScore += 150;
                break;
            case HitMargin.Perfect:
                LScore += 300;
                break;
        }
        switch(n) {
            case HitMargin.VeryEarly:
            case HitMargin.VeryLate:
                NScore += 91;
                break;
            case HitMargin.EarlyPerfect:
            case HitMargin.LatePerfect:
                NScore += 150;
                break;
            case HitMargin.Perfect:
                NScore += 300;
                break;
        }
        switch(s) {
            case HitMargin.VeryEarly:
            case HitMargin.VeryLate:
                SScore += 91;
                break;
            case HitMargin.EarlyPerfect:
            case HitMargin.LatePerfect:
                SScore += 150;
                break;
            case HitMargin.Perfect:
                SScore += 300;
                break;
        }
    }

    public static void Reset() => LScore = NScore = SScore = Score = 0;
}
