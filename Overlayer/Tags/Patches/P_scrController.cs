using Overlayer.Core.Patches;
using System.Collections.Generic;

namespace Overlayer.Tags.Patches {
    public class P_scrController : PatchBase<P_scrController> {
        [LazyPatch("Tags.P_scrController.HitTiming__Awake_Rewind", "scrController", "Awake_Rewind", Triggers = new string[] {
            nameof(HitTiming.Timing), nameof(HitTiming.TimingAvg),
        })]
        public static class HitTiming__Awake_Rewind {
            public static void Postfix() {
                HitTiming.Timing = 0;
                HitTiming.Timings = new List<double>();
            }
        }

        [LazyPatch("Tags.P_scrController.Status__FailAction", "scrController", "FailAction", Triggers = new string[] {
            nameof(Status.BestProgress)
        })]
        public static class Status__FailAction {
            public static void Postfix() {
                Status.BestProgress_Update();
            }
        }

        [LazyPatch("Tags.P_scrController.Hit__OnDamage", "scrController", "OnDamage", Triggers = new string[] {
            nameof(Hit.Multipress),
        })]
        public static class Hit__OnDamage {
            public static void Postfix(scrController __instance, bool multipress, bool applyMultipressDamage) {
                if(multipress) {
                    if(applyMultipressDamage || __instance.consecMultipressCounter > 5) {
                        Hit.Multipress++;
                    }
                }
            }
        }

        [LazyPatch("Tags.P_scrController.Status__OnLandOnPortal", "scrController", "OnLandOnPortal", Triggers = new string[] {
            nameof(Status.BestProgress)
        })]
        public static class Status__OnLandOnPortal {
            public static void Postfix() {
                Status.BestProgress_Fix();
            }
        }
    }
}
