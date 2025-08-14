using JSON;
using Overlayer.Core.Interfaces;
using Overlayer.Models;
using Overlayer.Utils;
using UnityEngine;
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
        public int SystemTagUpdateRate = 100;
        public bool useLegacyTheme = false;
        public bool useShowTrueAutoJudgment = false;
        public bool useMovingManEditor = true;
        public bool useColorRangeEditor = true;
        public bool useAutoUpdate = false;
        public bool useAutoUpdateBeta = false;
        public bool isFirstEg = true;
        public JsonNode Serialize()
        {
            var node = JsonNode.Empty;
            node[nameof(ChangeFont)] = ChangeFont;
            node[nameof(AdofaiFont)] = AdofaiFont.Serialize();
            node[nameof(Lang)] = Lang;
            node[nameof(FPSUpdateRate)] = FPSUpdateRate;
            node[nameof(FrameTimeUpdateRate)] = FrameTimeUpdateRate;
            node[nameof(SystemTagUpdateRate)] = SystemTagUpdateRate;
            node[nameof(useLegacyTheme)] = useLegacyTheme;
            node[nameof(useShowTrueAutoJudgment)] = useShowTrueAutoJudgment;
            node[nameof(useMovingManEditor)] = useMovingManEditor;
            node[nameof(useColorRangeEditor)] = useColorRangeEditor;
            node[nameof(useAutoUpdate)] = useAutoUpdate;
            node[nameof(useAutoUpdateBeta)] = useAutoUpdateBeta;
            node[nameof(isFirstEg)] = isFirstEg;
            return node;
        }
        public void Deserialize(JsonNode node)
        {
            ChangeFont = node[nameof(ChangeFont)];
            AdofaiFont = ModelUtils.Unbox<FontMeta>(node[nameof(AdofaiFont)]);
            Lang = node[nameof(Lang)];
            FPSUpdateRate = node[nameof(FPSUpdateRate)];
            FrameTimeUpdateRate = node[nameof(FrameTimeUpdateRate)];
            SystemTagUpdateRate = node[nameof(SystemTagUpdateRate)];
            useLegacyTheme = node[nameof(useLegacyTheme)];
            useShowTrueAutoJudgment = node[nameof(useShowTrueAutoJudgment)];
            useMovingManEditor = node[nameof(useMovingManEditor)];
            useColorRangeEditor = node[nameof(useColorRangeEditor)];
            useAutoUpdate = node[nameof(useAutoUpdate)];
            useAutoUpdateBeta = node[nameof(useAutoUpdateBeta)];
            isFirstEg = node[nameof(isFirstEg)];
        }
        public Settings Copy()
        {
            var newSettings = new Settings();
            newSettings.ChangeFont = ChangeFont;
            newSettings.AdofaiFont = AdofaiFont.Copy();
            newSettings.Lang = Lang;
            newSettings.FPSUpdateRate = FPSUpdateRate;
            newSettings.FrameTimeUpdateRate = FrameTimeUpdateRate;
            newSettings.SystemTagUpdateRate = SystemTagUpdateRate;
            newSettings.useLegacyTheme = useLegacyTheme;
            newSettings.useShowTrueAutoJudgment = useShowTrueAutoJudgment;
            newSettings.useMovingManEditor = useMovingManEditor;
            newSettings.useColorRangeEditor = useColorRangeEditor;
            newSettings.useAutoUpdate = useAutoUpdate;
            newSettings.useAutoUpdateBeta = useAutoUpdateBeta;
            newSettings.isFirstEg = isFirstEg;
            return newSettings;
        }
    }
}
