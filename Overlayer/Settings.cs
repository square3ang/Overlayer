using JSON;
using Overlayer.Core.Interfaces;
using Overlayer.Models;
using Overlayer.Utils;
using UnityModManagerNet;

namespace Overlayer
{
    public class Settings : UnityModManager.ModSettings, IModel, ICopyable<Settings>
    {
        public bool ChangeFont = false;
        public FontMeta AdofaiFont = new FontMeta();
        public string Lang = "Default";
        public float FPSUpdateRate = 100;
        public float FrameTimeUpdateRate = 100;
        public bool useLegacyTheme = false;
        public bool useMovingManEditor = true;
        public bool useColorRangeEditor = true;
        public JsonNode Serialize()
        {
            var node = JsonNode.Empty;
            node[nameof(ChangeFont)] = ChangeFont;
            node[nameof(AdofaiFont)] = AdofaiFont.Serialize();
            node[nameof(Lang)] = Lang;
            node[nameof(FPSUpdateRate)] = FPSUpdateRate;
            node[nameof(FrameTimeUpdateRate)] = FrameTimeUpdateRate;
            node[nameof(useLegacyTheme)] = useLegacyTheme;
            node[nameof(useMovingManEditor)] = useMovingManEditor;
            node[nameof(useColorRangeEditor)] = useColorRangeEditor;
            return node;
        }
        public void Deserialize(JsonNode node)
        {
            ChangeFont = node[nameof(ChangeFont)];
            AdofaiFont = ModelUtils.Unbox<FontMeta>(node[nameof(AdofaiFont)]);
            Lang = node[nameof(Lang)];
            FPSUpdateRate = node[nameof(FPSUpdateRate)];
            FrameTimeUpdateRate = node[nameof(FrameTimeUpdateRate)];
            useLegacyTheme = node[nameof(useLegacyTheme)];
            useMovingManEditor = node[nameof(useMovingManEditor)];
            useColorRangeEditor = node[nameof(useColorRangeEditor)];
        }
        public Settings Copy()
        {
            var newSettings = new Settings();
            newSettings.ChangeFont = ChangeFont;
            newSettings.AdofaiFont = AdofaiFont.Copy();
            newSettings.Lang = Lang;
            newSettings.FPSUpdateRate = FPSUpdateRate;
            newSettings.FrameTimeUpdateRate = FrameTimeUpdateRate;
            newSettings.useLegacyTheme = useLegacyTheme;
            newSettings.useMovingManEditor = useMovingManEditor;
            newSettings.useColorRangeEditor = useColorRangeEditor;
            return newSettings;
        }
    }
}
