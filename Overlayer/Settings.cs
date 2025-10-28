using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using Overlayer.Models;
using UnityModManagerNet;

namespace Overlayer
{
    public class Settings : UnityModManager.ModSettings, IModel, ICopyable<Settings>
    {
        public bool disableLogo = false;
        public bool ChangeFont = false;
        public FontMeta AdofaiFont = new FontMeta();
        public string Lang = "Default";
        public float FPSUpdateRate = 100;
        public float FrameTimeUpdateRate = 100;
        public int SystemTagUpdateRate = 100;
        public bool useLegacyTheme = false;
        public bool useLegacyNumberField = false;
        public bool useShowTrueAutoJudgment = false;
        public bool useMovingManEditor = true;
        public bool useColorRangeEditor = true;
        public bool useEasedValueEditor = true;
        public bool useAutoUpdate = false;
        public bool useAutoUpdateBeta = false;
        public bool useTooltip = true;
        public bool isFirstEg = true;
        public bool autoPivot = true;
        public JToken Serialize() {
            var node = new JObject();
            node[nameof(disableLogo)] = disableLogo;
            node[nameof(ChangeFont)] = ChangeFont;
            node[nameof(AdofaiFont)] = AdofaiFont?.Serialize();
            node[nameof(Lang)] = Lang;
            node[nameof(FPSUpdateRate)] = FPSUpdateRate;
            node[nameof(FrameTimeUpdateRate)] = FrameTimeUpdateRate;
            node[nameof(SystemTagUpdateRate)] = SystemTagUpdateRate;
            node[nameof(useLegacyTheme)] = useLegacyTheme;
            node[nameof(useLegacyNumberField)] = useLegacyNumberField;
            node[nameof(useShowTrueAutoJudgment)] = useShowTrueAutoJudgment;
            node[nameof(useMovingManEditor)] = useMovingManEditor;
            node[nameof(useColorRangeEditor)] = useColorRangeEditor;
            node[nameof(useEasedValueEditor)] = useEasedValueEditor;
            node[nameof(useAutoUpdate)] = useAutoUpdate;
            node[nameof(useAutoUpdateBeta)] = useAutoUpdateBeta;
            node[nameof(useTooltip)] = useTooltip;
            node[nameof(isFirstEg)] = isFirstEg;
            node[nameof(autoPivot)] = autoPivot;
            return node;
        }
        public void Deserialize(JToken node) {
            var defaultSettings = new Settings();

            disableLogo = node[nameof(disableLogo)]?.Value<bool>() ?? defaultSettings.disableLogo;
            ChangeFont = node[nameof(ChangeFont)]?.Value<bool>() ?? defaultSettings.ChangeFont;
            AdofaiFont = node[nameof(AdofaiFont)] != null
                ? ModelUtils.Unbox<FontMeta>(node[nameof(AdofaiFont)])
                : defaultSettings.AdofaiFont;
            Lang = node[nameof(Lang)]?.Value<string>() ?? defaultSettings.Lang;
            FPSUpdateRate = node[nameof(FPSUpdateRate)]?.Value<float>() ?? defaultSettings.FPSUpdateRate;
            FrameTimeUpdateRate = node[nameof(FrameTimeUpdateRate)]?.Value<float>() ?? defaultSettings.FrameTimeUpdateRate;
            SystemTagUpdateRate = node[nameof(SystemTagUpdateRate)]?.Value<int>() ?? defaultSettings.SystemTagUpdateRate;
            useLegacyTheme = node[nameof(useLegacyTheme)]?.Value<bool>() ?? defaultSettings.useLegacyTheme;
            useLegacyNumberField = node[nameof(useLegacyNumberField)]?.Value<bool>() ?? defaultSettings.useLegacyNumberField;
            useShowTrueAutoJudgment = node[nameof(useShowTrueAutoJudgment)]?.Value<bool>() ?? defaultSettings.useShowTrueAutoJudgment;
            useMovingManEditor = node[nameof(useMovingManEditor)]?.Value<bool>() ?? defaultSettings.useMovingManEditor;
            useColorRangeEditor = node[nameof(useColorRangeEditor)]?.Value<bool>() ?? defaultSettings.useColorRangeEditor;
            useEasedValueEditor = node[nameof(useEasedValueEditor)]?.Value<bool>() ?? defaultSettings.useEasedValueEditor;
            useAutoUpdate = node[nameof(useAutoUpdate)]?.Value<bool>() ?? defaultSettings.useAutoUpdate;
            useAutoUpdateBeta = node[nameof(useAutoUpdateBeta)]?.Value<bool>() ?? defaultSettings.useAutoUpdateBeta;
            useTooltip = node[nameof(useTooltip)]?.Value<bool>() ?? defaultSettings.useTooltip;
            isFirstEg = node[nameof(isFirstEg)]?.Value<bool>() ?? defaultSettings.isFirstEg;
            autoPivot = node[nameof(autoPivot)]?.Value<bool>() ?? defaultSettings.autoPivot;
        }
        public Settings Copy()
        {
            var newSettings = new Settings();
            newSettings.disableLogo = disableLogo;
            newSettings.ChangeFont = ChangeFont;
            newSettings.AdofaiFont = AdofaiFont.Copy();
            newSettings.Lang = Lang;
            newSettings.FPSUpdateRate = FPSUpdateRate;
            newSettings.FrameTimeUpdateRate = FrameTimeUpdateRate;
            newSettings.SystemTagUpdateRate = SystemTagUpdateRate;
            newSettings.useLegacyTheme = useLegacyTheme;
            newSettings.useLegacyNumberField = useLegacyNumberField;
            newSettings.useShowTrueAutoJudgment = useShowTrueAutoJudgment;
            newSettings.useMovingManEditor = useMovingManEditor;
            newSettings.useColorRangeEditor = useColorRangeEditor;
            newSettings.useEasedValueEditor = useEasedValueEditor;
            newSettings.useAutoUpdate = useAutoUpdate;
            newSettings.useAutoUpdateBeta = useAutoUpdateBeta;
            newSettings.isFirstEg = isFirstEg;
            newSettings.autoPivot = autoPivot;
            return newSettings;
        }
    }
}
