namespace Overlayer.Tags
{
    public static class Bpm
    {
        [Tag(Dummy=120)] public static double TileBpm;
        [Tag(Dummy=120)] public static double CurBpm;
        [Tag(Dummy=2)] public static double RecKPS;
        [Tag(Dummy=120)] public static double TileBpmWithoutPitch;
        [Tag(Dummy=120)] public static double CurBpmWithoutPitch;
        [Tag(Dummy=2)] public static double RecKPSWithoutPitch;

        public static void Reset()
        {
            TileBpm = CurBpm = RecKPS = 0;
            TileBpmWithoutPitch = CurBpmWithoutPitch = RecKPSWithoutPitch = 0;
        }
    }
}