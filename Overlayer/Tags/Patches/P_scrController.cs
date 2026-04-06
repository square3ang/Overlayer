using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches;

public class P_scrController : PatchBase<P_scrController> {
    [LazyPatch("Tags.P_scrController.HitTiming__Awake_Rewind", "scrController", "Awake_Rewind", Triggers = [
        nameof(HitTiming.Timing), nameof(HitTiming.TimingAvg)
    ])]
    public static class HitTiming__Awake_Rewind {
        public static void Postfix() {
            HitTiming.Timing = 0;
            HitTiming.Timings = [];
        }
    }

    [LazyPatch("Tags.P_scrController.ProgressStats__FailAction", "scrController", "FailAction", Triggers = [
        nameof(ProgressStats.BestProgress)
    ])]
    public static class ProgressStats__FailAction {
        public static void Postfix() {
            ProgressStats.BestProgress_Update();
        }
    }

    [LazyPatch("Tags.P_scrController.Hit__OnDamage", "scrController", "OnDamage", Triggers = [
        nameof(Hit.Multipress)
    ])]
    public static class Hit__OnDamage {
        public static void Postfix(scrController __instance, bool multipress, bool applyMultipressDamage) {
            if (multipress)
                if (applyMultipressDamage || __instance.consecMultipressCounter > 5)
                    Hit.Multipress++;
        }
    }

    [LazyPatch("Tags.P_scrController.ProgressStats__OnLandOnPortal", "scrController", "OnLandOnPortal", Triggers = [
        nameof(ProgressStats.BestProgress)
    ])]
    public static class ProgressStats__OnLandOnPortal {
        public static void Postfix() {
            ProgressStats.BestProgress_Fix();
        }
    }
}