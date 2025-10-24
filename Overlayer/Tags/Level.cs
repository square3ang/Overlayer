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
        private static string _defaultTextColor;
        private static string _defaultTextShadowColor;

        public static void Update() {
            if(scnGame.instance != null) {
                _titleRaw = ADOFAI.LevelData?.song;
                _authorRaw = ADOFAI.LevelData?.author;
                _author = _authorRaw.BreakRichTag();
                _artistRaw = ADOFAI.LevelData?.artist;
                _artist = _artistRaw.BreakRichTag();
            } else {
                _titleRaw = ADOBase.sceneName;
                _authorRaw = string.Empty;
                _author = string.Empty;
                _artistRaw = string.Empty;
                _artist = string.Empty;
            }
            _defaultTextColor = ADOFAI.LevelData?.defaultTextColor.ToHex() ?? "#ffffff";
            _defaultTextShadowColor = ADOFAI.LevelData?.defaultTextShadowColor.ToHex() ?? "#000000";
            _title = _titleRaw.BreakRichTag();
        }

        [Tag]
        public static string Title(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr)
            => string.IsNullOrEmpty(_title) ? "" : _title.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string Author(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr)
            => string.IsNullOrEmpty(_author) ? "" : _author.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string Artist(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr)
            => string.IsNullOrEmpty(_artist) ? "" : _artist.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string TitleRaw(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr)
            => string.IsNullOrEmpty(_titleRaw) ? "" : _titleRaw.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string AuthorRaw(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr)
            => string.IsNullOrEmpty(_authorRaw) ? "" : _authorRaw.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string ArtistRaw(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr)
            => string.IsNullOrEmpty(_artistRaw) ? "" : _artistRaw.Trim(maxLength, afterTrimStr);
        [Tag]
        public static string DefaultTextColor() => _defaultTextColor;
        [Tag]
        public static string DefaultTextShadowColor() => _defaultTextShadowColor;
    }
}
