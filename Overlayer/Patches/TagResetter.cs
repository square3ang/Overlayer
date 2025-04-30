using Overlayer.Tags;

namespace Overlayer.Patches
{
    public static class TagResetter
    {
        public static void Postfix()
        {
            OverlayerAPI.Reset();
            Bpm.Reset();
            Tags.FrameRate.Reset();
            Hex.Reset();
            Hit.Reset();
            HitTiming.Reset();
            Tags.Level.Reset();
            Song.Reset();
            Status.Reset();
            Tile.Reset();
            Time.Reset();
        }
    }
}
