using Overlayer.Tags.Attributes;

namespace Overlayer.Tags;

public static class FailStats {
    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static float OverloadCounter() {
        var controller = scrController.instance;
        return controller == null
            ? float.NaN
            : controller.failbar == null
                ? float.NaN
                : IsImmortal(controller)
                    ? 100f
                    : CalculateFailValue(controller.failbar.overloadCounter);
    }

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static float MultipressCounter() {
        var controller = scrController.instance;
        return !controller
            ? float.NaN
            : !controller.failbar
                ? float.NaN
                : IsImmortal(controller)
                    ? 100f
                    : CalculateFailValue(controller.failbar.multipressCounter);
    }

    public static bool IsImmortal(scrController controller) {
        return ADOBase.isOfficialLevel && controller.gameworld && controller.percentComplete >= 0.96f;
    }

    public static float CalculateFailValue(float value) {
        return value > 1f ? 0f : (1f - value) * 100f;
    }

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static float OverloadCounterRaw => scrController.instance?.failbar?.overloadCounter ?? float.NaN;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static float MultipressCounterRaw => scrController.instance?.failbar?.multipressCounter ?? float.NaN;
}