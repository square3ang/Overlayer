using ADOFAI;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;

namespace Overlayer.Tags
{
    public static class Level
    {
        public static LevelData LevelData => scnGame.instance?.levelData ?? scnEditor.instance?.levelData;
        [Tag(Hint = ReturnTypeHint.String)]
        public static string Title() => ADOBase.isOfficialLevel ? ADOBase.sceneName : TitleRaw()?.BreakRichTag();
        [Tag(Hint = ReturnTypeHint.String)]
        public static string Author() => AuthorRaw()?.BreakRichTag();
        [Tag(Hint = ReturnTypeHint.String)]
        public static string Artist() => ArtistRaw()?.BreakRichTag();
        [Tag(Hint = ReturnTypeHint.String)]
        public static string TitleRaw() => LevelData?.song;
        [Tag(Hint = ReturnTypeHint.String)]
        public static string AuthorRaw() => LevelData?.author;
        [Tag(Hint = ReturnTypeHint.String)]
        public static string ArtistRaw() => LevelData?.artist;
        public static void Reset() { }
    }
}
