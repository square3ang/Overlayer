using Overlayer.Tags.Attributes;
using Overlayer.Utils;

namespace Overlayer.Tags {
    public static class Level {
        [Tag]
        public static string Title(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => ADOBase.isOfficialLevel ? ADOBase.sceneName.Trim(maxLength, afterTrimStr) : ADOFAI.LevelData?.song?.BreakRichTag()?.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string Author(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) {
            if(scnGame.instance != null) {
                return ADOFAI.LevelData?.author?.BreakRichTag()?.Trim(maxLength, afterTrimStr);
            } else {
                return string.Empty;
            }
        }
        [Tag]
        public static string Artist(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) {
            if(scnGame.instance != null) {
                return ADOFAI.LevelData?.artist.BreakRichTag()?.Trim(maxLength, afterTrimStr);
            } else {
                return string.Empty;
            }
        }
        [Tag]
        public static string TitleRaw(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => ADOBase.isOfficialLevel ? ADOBase.sceneName.Trim(maxLength, afterTrimStr) : ADOFAI.LevelData?.song?.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string AuthorRaw(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) {
            if(scnGame.instance != null) {
                return ADOFAI.LevelData?.author?.Trim(maxLength, afterTrimStr);
            } else {
                return string.Empty;
            }
        }
        [Tag]
        public static string ArtistRaw(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) {
            if(scnGame.instance != null) {
                return ADOFAI.LevelData?.artist?.Trim(maxLength, afterTrimStr);
            } else {
                return string.Empty;
            }
        }
        public static void Reset() { }
    }
}
