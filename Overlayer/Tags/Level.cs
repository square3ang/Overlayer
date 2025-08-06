using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using RapidGUI;

namespace Overlayer.Tags {
    public static class Level {
        private static string _title;
        private static string _author;
        private static string _artist;
        private static string _titleRaw;
        private static string _authorRaw;
        private static string _artistRaw;
        public static void LevelInit() {
            if(scnGame.instance != null) {
                _titleRaw = ADOFAI.LevelData?.song;
            } else {
                _titleRaw = ADOBase.sceneName;
            }
            _title = _titleRaw.BreakRichTag();

            if(scnGame.instance != null) {
                _authorRaw = ADOFAI.LevelData?.author;
                _author = _authorRaw.BreakRichTag();
            } else {
                _authorRaw = string.Empty;
                _author = string.Empty;
            }

            if(scnGame.instance != null) {
                _artistRaw = ADOFAI.LevelData?.artist;
                _artist = _artistRaw.BreakRichTag();
            } else {
                _artistRaw = string.Empty;
                _artist = string.Empty;
            }
        }
        [Tag]
        public static string Title(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => _title.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string Author(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => _author.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string Artist(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => _artist.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string TitleRaw(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => _titleRaw.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string AuthorRaw(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => _authorRaw.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string ArtistRaw(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => _artistRaw.Trim(maxLength, afterTrimStr);
    }
}
