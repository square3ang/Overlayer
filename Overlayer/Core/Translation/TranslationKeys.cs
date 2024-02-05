namespace Overlayer.Core.Translation
{
    public static class TranslationKeys
    {
        public static class Settings
        {
            public const string Prefix = "SETTINGS_";
            internal static readonly string Langauge = Prefix + "LANGUAGE";
            internal static readonly string ChangeFont = Prefix + "CHANGE_FONT";
            internal static readonly string FpsUpdateRate = Prefix + "FPS_UPDATE_RATE";
            internal static readonly string FrameTimeUpdateRate = Prefix + "FRAMETIME_UPDATE_RATE";
            internal static readonly string AdofaiFont_Font = Prefix + "ADOFAIFONT_FONT";
            internal static readonly string AdofaiFont_FontScale = Prefix + "ADOFAIFONT_FONT_SCALE";
            internal static readonly string AdofaiFont_LineSpacing = Prefix + "ADOFAIFONT_LINE_SPACING";
            internal static readonly string AdofaiFont_Apply = Prefix + "ADOFAIFONT_APPLY";
            internal static readonly string EditThisText = Prefix + "EDIT_THIS_TEXT";
            internal static readonly string Edit = Prefix + "EDIT";
            internal static readonly string NewText = Prefix + "NEW_TEXT";
        }
        public static class TextConfig
        {
            public const string Prefix = "TEXT_CONFIG_";
        }
        public static class Misc
        {
            public const string Prefix = "MISC_";
            internal static readonly string Text = Prefix + "TEXT";
        }
    }
}
