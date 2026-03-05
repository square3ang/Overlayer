using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using Overlayer.Utils;
using TMPro;
using UnityEngine;

namespace Overlayer.Models;

public class TextConfig : IModel, ICopyable<TextConfig> {
    public bool Active = true;
    public delegate void DragChangeHandler(bool state);
    public event DragChangeHandler OnDragChanged;
    private bool _drag;
    public bool Drag {
        get => _drag;
        set {
            if(_drag == value) {
                return;
            }

            _drag = value;
            OnDragChanged?.Invoke(_drag);
        }
    }
    public string Name = string.Empty;
    public string Font = "Default";
    public string PlayingText = "<color=#{FOHex}>{Overloads}</color> <color=#{TEHex}>{CTE}</color> <color=#{VEHex}>{CVE}</color> <color=#{EPHex}>{CEP}</color> <color=#{PHex}>{CP}</color> <color=#{LPHex}>{CLP}</color> <color=#{VLHex}>{CVL}</color> <color=#{TLHex}>{CTL}</color> <color=#{FMHex}>{MissCount}</color>";
    public string NotPlayingText = string.Empty;
    public float FontSize = 44;
    public float OutlineWidth = 0;
    public float LineSpacing = -25f;
    public float LineSpacingAdj = 25f;
    public float ShadowDilate = 0;
    public float ShadowSoftness = 0.5f;
    public bool EnableFallbackFonts = false;
    public string[] FallbackFonts = null;
    public GColor TextColor = Color.white;
    public GColor OutlineColor = Color.clear;
    public GColor ShadowColor = Color.black with { a = 0.5f };
    public Vector2 Scale = new(1, 1);
    public Vector2 Position = new(0.5f, 0.0175f);
    public Vector2 Pivot = new(0.5f, 0.5f);
    public Vector2 ShadowOffset = new(0.5f, -0.5f);
    public Vector3 Rotation = Vector3.zero;
    public TextAlignmentOptions Alignment = TextAlignmentOptions.Center;
    public TextConfig Copy() {
        var newConfig = new TextConfig {
            Active = Active,
            Drag = Drag,
            Name = Name,
            Font = Font,
            PlayingText = PlayingText,
            NotPlayingText = NotPlayingText,
            FontSize = FontSize,
            OutlineWidth = OutlineWidth,
            LineSpacing = LineSpacing,
            LineSpacingAdj = LineSpacingAdj,
            ShadowDilate = ShadowDilate,
            ShadowSoftness = ShadowSoftness,
            TextColor = TextColor,
            OutlineColor = OutlineColor,
            ShadowColor = ShadowColor,
            Scale = Scale,
            Position = Position,
            Pivot = Pivot,
            ShadowOffset = ShadowOffset,
            Rotation = Rotation,
            Alignment = Alignment,
            EnableFallbackFonts = EnableFallbackFonts,
            FallbackFonts = FallbackFonts
        };
        return newConfig;
    }
    public JToken Serialize() {
        var node = new JObject {
            [nameof(Active)] = Active,
            [nameof(Drag)] = Drag,
            [nameof(Name)] = Name,
            [nameof(Font)] = Font,
            [nameof(PlayingText)] = PlayingText,
            [nameof(NotPlayingText)] = NotPlayingText,
            [nameof(FontSize)] = FontSize,
            [nameof(OutlineWidth)] = OutlineWidth,
            [nameof(LineSpacing)] = LineSpacing,
            [nameof(LineSpacingAdj)] = LineSpacingAdj,
            [nameof(ShadowDilate)] = ShadowDilate,
            [nameof(ShadowSoftness)] = ShadowSoftness,
            [nameof(TextColor)] = TextColor.Serialize(),
            [nameof(OutlineColor)] = OutlineColor.Serialize(),
            [nameof(ShadowColor)] = ShadowColor.Serialize(),
            [nameof(Scale)] = ModelUtils.ToNode(Scale),
            [nameof(Position)] = ModelUtils.ToNode(Position),
            [nameof(Pivot)] = ModelUtils.ToNode(Pivot),
            [nameof(ShadowOffset)] = ModelUtils.ToNode(ShadowOffset),
            [nameof(Rotation)] = ModelUtils.ToNode(Rotation),
            [nameof(Alignment)] = Alignment.ToString(),
            [nameof(EnableFallbackFonts)] = EnableFallbackFonts,
            [nameof(FallbackFonts)] = new JArray(FallbackFonts)
        };
        return node;
    }
    public void Deserialize(JToken node) {
        var defaultSettings = new TextConfig();
        Active = node[nameof(Active)]?.Value<bool>() ?? defaultSettings.Active;
        Drag = node[nameof(Drag)]?.Value<bool>() ?? defaultSettings.Drag;
        Name = node[nameof(Name)]?.Value<string>() ?? defaultSettings.Name;
        Font = node[nameof(Font)]?.Value<string>() ?? defaultSettings.Font;
        PlayingText = node[nameof(PlayingText)]?.Value<string>() ?? defaultSettings.PlayingText;
        NotPlayingText = node[nameof(NotPlayingText)]?.Value<string>() ?? defaultSettings.NotPlayingText;
        FontSize = node[nameof(FontSize)]?.Value<float>() ?? defaultSettings.FontSize;
        OutlineWidth = node[nameof(OutlineWidth)]?.Value<float>() ?? defaultSettings.OutlineWidth;
        LineSpacing = node[nameof(LineSpacing)]?.Value<float>() ?? defaultSettings.LineSpacing;
        LineSpacingAdj = node[nameof(LineSpacingAdj)]?.Value<float>() ?? defaultSettings.LineSpacingAdj;
        ShadowDilate = node[nameof(ShadowDilate)]?.Value<float>() ?? defaultSettings.ShadowDilate;
        ShadowSoftness = node[nameof(ShadowSoftness)]?.Value<float>() ?? defaultSettings.ShadowSoftness;
        TextColor = node[nameof(TextColor)] != null
            ? ModelUtils.Unbox<GColor>(node[nameof(TextColor)])
            : defaultSettings.TextColor;
        TextColor.gradientEnabled = node[nameof(TextColor)]?["gradientEnabled"]?.Value<bool>() ?? TextColor.gradientEnabled;
        OutlineColor = node[nameof(OutlineColor)] != null
            ? ModelUtils.Unbox<GColor>(node[nameof(OutlineColor)])
            : defaultSettings.OutlineColor;
        ShadowColor = node[nameof(ShadowColor)] != null
            ? ModelUtils.Unbox<GColor>(node[nameof(ShadowColor)])
            : defaultSettings.ShadowColor;
        Scale = node[nameof(Scale)] != null
            ? ModelUtils.ToVector2(node[nameof(Scale)])
            : defaultSettings.Scale;
        Position = node[nameof(Position)] != null
            ? ModelUtils.ToVector2(node[nameof(Position)])
            : defaultSettings.Position;
        Pivot = node[nameof(Pivot)] != null
            ? ModelUtils.ToVector2(node[nameof(Pivot)])
            : defaultSettings.Pivot;
        ShadowOffset = node[nameof(ShadowOffset)] != null
            ? ModelUtils.ToVector2(node[nameof(ShadowOffset)])
            : defaultSettings.ShadowOffset;
        Rotation = node[nameof(Rotation)] != null
            ? ModelUtils.ToVector3(node[nameof(Rotation)])
            : defaultSettings.Rotation;
        Alignment = node[nameof(Alignment)] != null
            ? EnumHelper<TextAlignmentOptions>.Parse(node[nameof(Alignment)].Value<string>())
            : defaultSettings.Alignment;
        EnableFallbackFonts = node[nameof(EnableFallbackFonts)]?.Value<bool>() ?? defaultSettings.EnableFallbackFonts;
        FallbackFonts = node[nameof(FallbackFonts)] != null
            ? node[nameof(FallbackFonts)].ToObject<string[]>()
            : defaultSettings.FallbackFonts;
    }
}
