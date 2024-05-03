using Overlayer.Core;
using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class TUF
    {
        [Tag]
        public static bool TUFRequestCompleted = false;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double TUFDifficulty => TUFDifficulties.pguDiffNum;
        [Tag(ProcessingFlags = ValueProcessing.AccessMember)]
        public static OverlayerWebAPI.TUFDifficulties TUFDifficulties = new OverlayerWebAPI.TUFDifficulties();
        public static void Reset()
        {
            TUFRequestCompleted = false;
        }
    }
}
