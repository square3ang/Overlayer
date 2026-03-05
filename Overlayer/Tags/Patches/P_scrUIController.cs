using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches;

public class P_scrUIController : PatchBase<P_scrUIController> {
    [LazyPatch("Tags.P_scrUIController.Status__WipeFromBlack", "scrUIController", "WipeFromBlack", Triggers = new string[] {
        nameof(Status.Attempts),
    })]
    public static class Status__WipeFromBlack {
        public static void Prefix() => Status.Attempts_Update();
    }
}
