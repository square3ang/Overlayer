using ADOFAI;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;

namespace Overlayer.Tags
{
    public static class Level
    {
        public static LevelData LevelData => scnGame.instance?.levelData ?? scnEditor.instance?.levelData;
        [Tag]
        public static string Title() => ADOBase.isOfficialLevel ? ADOBase.sceneName : TitleRaw()?.BreakRichTag();
        [Tag]
        public static string Author() => AuthorRaw()?.BreakRichTag();
        [Tag]
        public static string Artist() => ArtistRaw()?.BreakRichTag();
        [Tag]
        public static string TitleRaw() => LevelData?.song;
        [Tag]
        public static string AuthorRaw() => LevelData?.author;
        [Tag]
        public static string ArtistRaw() => LevelData?.artist;
        public static void Reset() { }
    }
}
