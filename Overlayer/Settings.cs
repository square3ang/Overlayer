using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using Overlayer.Models;
using Overlayer.Utils;
using System.IO;
using System.Xml.Serialization;

namespace Overlayer;

public class Settings : IModel, ICopyable<Settings> {
    public enum EditorUIMode {
        Simple,
        Advanced
    }

    public bool DisableLogo = false;
    public string Lang = "en-US";
    public float FPSUpdateRate = 100;
    public float FrameTimeUpdateRate = 100;
    public int SystemTagUpdateRate = 100;
    public bool LegacyTheme = false;
    public bool MovingManEditor = true;
    public bool ColorRangeEditor = true;
    public bool EasedValueEditor = true;
    public bool AutoUpdate = false;
    public bool AutoUpdateBeta = false;
    public bool Tooltip = true;
    public bool AutoPivot = true;
    public bool ShowTextNameAsDisplayText = false;
    public EditorUIMode UiMode = EditorUIMode.Simple;
    public bool IncludeReferences = true;
    public bool FileAttempt = false;

    public int PerfStatUpdateRate = 1000;

    public FontMeta AdofaiFont = new();
    public bool ChangeFont = false;
    public bool ShowTrueAutoJudgment = false;

    public bool IsFirstEg = true;
    public JToken Serialize() {
        var node = new JObject {
            [nameof(DisableLogo)] = DisableLogo,
            [nameof(Lang)] = Lang,
            [nameof(FPSUpdateRate)] = FPSUpdateRate,
            [nameof(FrameTimeUpdateRate)] = FrameTimeUpdateRate,
            [nameof(SystemTagUpdateRate)] = SystemTagUpdateRate,
            [nameof(LegacyTheme)] = LegacyTheme,
            [nameof(MovingManEditor)] = MovingManEditor,
            [nameof(ColorRangeEditor)] = ColorRangeEditor,
            [nameof(EasedValueEditor)] = EasedValueEditor,
            [nameof(AutoUpdate)] = AutoUpdate,
            [nameof(AutoUpdateBeta)] = AutoUpdateBeta,
            [nameof(Tooltip)] = Tooltip,
            [nameof(AutoPivot)] = AutoPivot,
            [nameof(ShowTextNameAsDisplayText)] = ShowTextNameAsDisplayText,
            [nameof(UiMode)] = UiMode.ToString(),
            [nameof(IncludeReferences)] = IncludeReferences,
            [nameof(FileAttempt)] = FileAttempt,

            [nameof(PerfStatUpdateRate)] = PerfStatUpdateRate,

            [nameof(AdofaiFont)] = AdofaiFont?.Serialize(),
            [nameof(ChangeFont)] = ChangeFont,
            [nameof(ShowTrueAutoJudgment)] = ShowTrueAutoJudgment,

            [nameof(IsFirstEg)] = IsFirstEg
        };
        return node;
    }
    public void Deserialize(JToken node) {
        var defaultSettings = new Settings();

        DisableLogo = node[nameof(DisableLogo)]?.Value<bool>() ?? defaultSettings.DisableLogo;
        Lang = node[nameof(Lang)]?.Value<string>() ?? defaultSettings.Lang;
        FPSUpdateRate = node[nameof(FPSUpdateRate)]?.Value<float>() ?? defaultSettings.FPSUpdateRate;
        FrameTimeUpdateRate = node[nameof(FrameTimeUpdateRate)]?.Value<float>() ?? defaultSettings.FrameTimeUpdateRate;
        SystemTagUpdateRate = node[nameof(SystemTagUpdateRate)]?.Value<int>() ?? defaultSettings.SystemTagUpdateRate;
        LegacyTheme = node[nameof(LegacyTheme)]?.Value<bool>() ?? defaultSettings.LegacyTheme;
        MovingManEditor = node[nameof(MovingManEditor)]?.Value<bool>() ?? defaultSettings.MovingManEditor;
        ColorRangeEditor = node[nameof(ColorRangeEditor)]?.Value<bool>() ?? defaultSettings.ColorRangeEditor;
        EasedValueEditor = node[nameof(EasedValueEditor)]?.Value<bool>() ?? defaultSettings.EasedValueEditor;
        AutoUpdate = node[nameof(AutoUpdate)]?.Value<bool>() ?? defaultSettings.AutoUpdate;
        AutoUpdateBeta = node[nameof(AutoUpdateBeta)]?.Value<bool>() ?? defaultSettings.AutoUpdateBeta;
        Tooltip = node[nameof(Tooltip)]?.Value<bool>() ?? defaultSettings.Tooltip;
        AutoPivot = node[nameof(AutoPivot)]?.Value<bool>() ?? defaultSettings.AutoPivot;
        ShowTextNameAsDisplayText = node[nameof(ShowTextNameAsDisplayText)]?.Value<bool>() ?? defaultSettings.ShowTextNameAsDisplayText;
        UiMode = EnumHelper<EditorUIMode>.Parse(node[nameof(UiMode)]?.Value<string>() ?? defaultSettings.UiMode.ToString());
        IncludeReferences = node[nameof(IncludeReferences)]?.Value<bool>() ?? defaultSettings.IncludeReferences;
        FileAttempt = node[nameof(FileAttempt)]?.Value<bool>() ?? defaultSettings.FileAttempt;

        PerfStatUpdateRate = node[nameof(PerfStatUpdateRate)]?.Value<int>() ?? defaultSettings.PerfStatUpdateRate;

        ChangeFont = node[nameof(ChangeFont)]?.Value<bool>() ?? defaultSettings.ChangeFont;
        AdofaiFont = node[nameof(AdofaiFont)] != null
            ? ModelUtils.Unbox<FontMeta>(node[nameof(AdofaiFont)])
            : defaultSettings.AdofaiFont;
        ShowTrueAutoJudgment = node[nameof(ShowTrueAutoJudgment)]?.Value<bool>() ?? defaultSettings.ShowTrueAutoJudgment;

        IsFirstEg = node[nameof(IsFirstEg)]?.Value<bool>() ?? defaultSettings.IsFirstEg;
    }
    public Settings Copy() {
        var newSettings = new Settings {
            DisableLogo = DisableLogo,
            Lang = Lang,
            FPSUpdateRate = FPSUpdateRate,
            FrameTimeUpdateRate = FrameTimeUpdateRate,
            SystemTagUpdateRate = SystemTagUpdateRate,
            LegacyTheme = LegacyTheme,
            MovingManEditor = MovingManEditor,
            ColorRangeEditor = ColorRangeEditor,
            EasedValueEditor = EasedValueEditor,
            AutoUpdate = AutoUpdate,
            AutoUpdateBeta = AutoUpdateBeta,
            AutoPivot = AutoPivot,
            ShowTextNameAsDisplayText = ShowTextNameAsDisplayText,
            UiMode = UiMode,
            IncludeReferences = IncludeReferences,
            FileAttempt = FileAttempt,

            ChangeFont = ChangeFont,
            AdofaiFont = AdofaiFont.Copy(),
            ShowTrueAutoJudgment = ShowTrueAutoJudgment,

            IsFirstEg = IsFirstEg
        };
        return newSettings;
    }

    public void Save() {
        var path = Path.Combine(Main.Mod.Path, "Settings.json");
        var json = Serialize().ToString();
        File.WriteAllText(path, json);
    }

    public void Load() {
        if(MigratefromLegacyXmlSettings()) {
            return;
        }

        var path = Path.Combine(Main.Mod.Path, "Settings.json");
        if(File.Exists(path)) {
            var json = File.ReadAllText(path);
            var node = JToken.Parse(json);
            Deserialize(node);
        }
    }

    private bool MigratefromLegacyXmlSettings() {
        var jsonPath = Path.Combine(Main.Mod.Path, "Settings.json");
        var xmlPath = Path.Combine(Main.Mod.Path, "Settings.xml");

        if(File.Exists(jsonPath)) {
            return false;
        }

        if(!File.Exists(xmlPath)) {
            return false;
        }

        var serializer = new XmlSerializer(typeof(LegacyXmlSettings));
        using(var stream = File.OpenRead(xmlPath)) {
            var legacy = (LegacyXmlSettings)serializer.Deserialize(stream);

            DisableLogo = legacy.disableLogo;
            ChangeFont = legacy.ChangeFont;
            AdofaiFont = legacy.AdofaiFont?.Copy() ?? new FontMeta();
            Lang = legacy.Lang;
            FPSUpdateRate = legacy.FPSUpdateRate;
            FrameTimeUpdateRate = legacy.FrameTimeUpdateRate;
            SystemTagUpdateRate = legacy.SystemTagUpdateRate;
            LegacyTheme = legacy.useLegacyTheme;
            ShowTrueAutoJudgment = legacy.useShowTrueAutoJudgment;
            MovingManEditor = legacy.useMovingManEditor;
            ColorRangeEditor = legacy.useColorRangeEditor;
            EasedValueEditor = legacy.useEasedValueEditor;
            AutoUpdate = legacy.useAutoUpdate;
            AutoUpdateBeta = legacy.useAutoUpdateBeta;
            Tooltip = legacy.useTooltip;
            AutoPivot = legacy.autoPivot;
            ShowTextNameAsDisplayText = legacy.showTextNameAsDisplayText;
            IsFirstEg = legacy.isFirstEg;
        }

        Save();

        File.Delete(xmlPath);

        return true;
    }

    [XmlRoot("Settings")]
    public class LegacyXmlSettings {
        public bool disableLogo;
        public bool ChangeFont;
        public FontMeta AdofaiFont;
        public string Lang;
        public float FPSUpdateRate;
        public float FrameTimeUpdateRate;
        public int SystemTagUpdateRate;
        public bool useLegacyTheme;
        public bool useShowTrueAutoJudgment;
        public bool useMovingManEditor;
        public bool useColorRangeEditor;
        public bool useEasedValueEditor;
        public bool useAutoUpdate;
        public bool useAutoUpdateBeta;
        public bool useTooltip;
        public bool autoPivot;
        public bool showTextNameAsDisplayText;
        public bool isFirstEg;
    }
}
