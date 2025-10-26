using Overlayer.Tags.Attributes;

namespace Overlayer.Tags {
    public static class FailStats {

        [Tag]
        public static float OverloadCounter() {
            var controller = scrController.instance;
            if(controller == null) {
                return float.NaN;
            }
            if(controller.failbar == null) {
                return float.NaN;
            }
            if(IsImmortal(controller)) {
                return 100f;
            }
            return CalculateFailValue(controller.failbar.overloadCounter);
        }

        [Tag]
        public static float MultipressCounter() {
            var controller = scrController.instance;
            if(controller == null) {
                return float.NaN;
            }
            if(controller.failbar == null) {
                return float.NaN;
            }
            if(IsImmortal(controller)) {
                return 100f;
            }
            return CalculateFailValue(controller.failbar.multipressCounter);
        }

        public static bool IsImmortal(scrController controller)
            => (ADOBase.isOfficialLevel && controller.gameworld && controller.percentComplete >= 0.96f);

        public static float CalculateFailValue(float value)
            => value > 1f ? 0f : (1f - value) * 100f;

        [Tag]
        public static float OverloadCounterRaw() => scrController.instance?.failbar?.overloadCounter ?? float.NaN;
        [Tag]
        public static float MultipressCounterRaw() => scrController.instance?.failbar?.multipressCounter ?? float.NaN;
    }
}
