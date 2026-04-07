using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches;

public class P_Persistence : PatchBase<P_Persistence> {
    [LazyPatch("Tags.P_Persistence.Status__SetCustomWorldAttempts", "Persistence", "SetCustomWorldAttempts", Triggers =
    [
        nameof(Status.Attempts)
    ])]
    public static class Status_AttemptsGame {
        public static void Postfix() => Status.Attempts_UpdateGame();
    }

    [LazyPatch("Tags.P_Persistence.Status__IncrementWorldAttempts", "Persistence", "IncrementWorldAttempts", Triggers =
    [
        nameof(Status.Attempts)
    ])]
    [LazyPatch("Tags.P_Persistence.Status__IncrementWorldAttemptsWithoutNewBest", "Persistence", "IncrementWorldAttemptsWithoutNewBest", Triggers =
    [
        nameof(Status.Attempts)
    ])]
    [LazyPatch("Tags.P_Persistence.Status__SetWorldAttempts", "Persistence", "SetWorldAttempts", Triggers =
    [
        nameof(Status.Attempts)
    ])]
    [LazyPatch("Tags.P_Persistence.Status__SetWorldAttemptsWithoutNewBest", "Persistence", "SetWorldAttemptsWithoutNewBest", Triggers =
    [
        nameof(Status.Attempts)
    ])]
    public static class Status_AttemptsOfficial {
        public static void Postfix() => Status.Attempts_UpdateOfficial();
    }
}
