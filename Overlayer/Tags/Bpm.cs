using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class Bpm
    {
        [Tag(Flags = AdvancedFlags.Round)]
        public static double TileBpm;
        [Tag(Flags = AdvancedFlags.Round)]
        public static double CurBpm;
        [Tag(Flags = AdvancedFlags.Round)]
        public static double RecKPS;
        [Tag(Flags = AdvancedFlags.Round)]
        public static double TileBpmWithoutPitch;
        [Tag(Flags = AdvancedFlags.Round)]
        public static double CurBpmWithoutPitch;
        [Tag(Flags = AdvancedFlags.Round)]
        public static double RecKPSWithoutPitch;
        public static void Reset()
        {
            TileBpm = CurBpm = RecKPS = 0;
            TileBpmWithoutPitch = CurBpmWithoutPitch = RecKPSWithoutPitch = 0;
        }
    }
}
