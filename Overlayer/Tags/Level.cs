using ADOFAI;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using UnityEngine.UI;

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
        private static string _defaultTextColorAlpha;
        private static string _defaultTextShadowColorAlpha;

        private static Shadow LevelNameTextColorShadowComponent;

        public static void Init() {
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
            _title = _titleRaw.BreakRichTag();

            LevelNameTextColorShadowComponent = ADOBase.controller.txtLevelName.GetComponent<Shadow>();

            LevelData levelData = ADOFAI.LevelData;
            if(levelData == null) {
                _defaultTextColor = LevelNameTextColor(true);
                _defaultTextShadowColor = LevelNameTextShadowColor(true);
                _defaultTextColorAlpha = LevelNameTextColor(false);
                _defaultTextShadowColorAlpha = LevelNameTextShadowColor(false);
            } else {
                _defaultTextColor = ADOFAI.LevelData.defaultTextColor.ToHex(false);
                _defaultTextShadowColor = ADOFAI.LevelData.defaultTextShadowColor.ToHex(false);
                _defaultTextColorAlpha = ADOFAI.LevelData.defaultTextColor.ToHex(true);
                _defaultTextShadowColorAlpha = ADOFAI.LevelData.defaultTextShadowColor.ToHex(true);
            }

                

           
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
        public static string DefaultTextColor(bool noAlpha = false)
            => noAlpha ? _defaultTextColor : _defaultTextColorAlpha;
        [Tag]
        public static string DefaultTextShadowColor(bool noAlpha = false)
            => noAlpha ? _defaultTextShadowColor : _defaultTextShadowColorAlpha;
        [Tag]
        public static string LevelNameText() => ADOBase.controller.txtLevelName.text.BreakRichTag();
        [Tag]
        public static string LevelNameTextRaw() => ADOBase.controller.txtLevelName.text;
        [Tag]
        public static string LevelNameTextColor(bool noAlpha = false)
            => ADOBase.controller.txtLevelName.color.ToHex(!noAlpha);
        [Tag]
        public static string LevelNameTextShadowColor(bool noAlpha = false) {
            if(LevelNameTextColorShadowComponent == null) {
                return noAlpha ? "#000000" : "#00000050";
            } else {
                return LevelNameTextColorShadowComponent?.effectColor.ToHex(!noAlpha);
            }
        }
    }
}
