using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using Overlayer.Utils;
using TMPro;
using UnityEngine;

namespace Overlayer.Models;

public class TextConfig : ObjectConfig, ICopyable<TextConfig> {
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
    public Color OutlineColor = Color.clear;
    public Color ShadowColor = Color.black with { a = 0.5f };
    public Vector2 Scale = new(1, 1);
    public Vector2 Position = new(0.5f, 0.0175f);
    public Vector2 Pivot = new(0.5f, 0.5f);
    public Vector2 ShadowOffset = new(0.5f, -0.5f);
    public Vector3 Rotation = Vector3.zero;
    public TextAlignmentOptions Alignment = TextAlignmentOptions.Center;
    public override ObjectConfig Copy() {
        var copy = new TextConfig {
            Drag = Drag,
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
        CopyBase(copy);
        return copy;
    }
    TextConfig ICopyable<TextConfig>.Copy() => (TextConfig)Copy();
    public override JToken Serialize() {
        var node = SerializeBase();
        node[nameof(Drag)] = Drag;
        node[nameof(Font)] = Font;
        node[nameof(PlayingText)] = PlayingText;
        node[nameof(NotPlayingText)] = NotPlayingText;
        node[nameof(FontSize)] = FontSize;
        node[nameof(OutlineWidth)] = OutlineWidth;
        node[nameof(LineSpacing)] = LineSpacing;
        node[nameof(LineSpacingAdj)] = LineSpacingAdj;
        node[nameof(ShadowDilate)] = ShadowDilate;
        node[nameof(ShadowSoftness)] = ShadowSoftness;
        node[nameof(TextColor)] = TextColor.Serialize();
        node[nameof(OutlineColor)] = ModelUtils.ToNode(OutlineColor);
        node[nameof(ShadowColor)] = ModelUtils.ToNode(ShadowColor);
        node[nameof(Scale)] = ModelUtils.ToNode(Scale);
        node[nameof(Position)] = ModelUtils.ToNode(Position);
        node[nameof(Pivot)] = ModelUtils.ToNode(Pivot);
        node[nameof(ShadowOffset)] = ModelUtils.ToNode(ShadowOffset);
        node[nameof(Rotation)] = ModelUtils.ToNode(Rotation);
        node[nameof(Alignment)] = Alignment.ToString();
        node[nameof(EnableFallbackFonts)] = EnableFallbackFonts;
        node[nameof(FallbackFonts)] = FallbackFonts != null ? new JArray(FallbackFonts) : null;
        return node;
    }

    public override void Deserialize(JToken node) {
        var defaults = new TextConfig();
        DeserializeBase(node);
        Active = node[nameof(Active)]?.Value<bool>() ?? defaults.Active;
        Drag = node[nameof(Drag)]?.Value<bool>() ?? defaults.Drag;
        Name = node[nameof(Name)]?.Value<string>() ?? defaults.Name;
        Font = node[nameof(Font)]?.Value<string>() ?? defaults.Font;
        PlayingText = node[nameof(PlayingText)]?.Value<string>() ?? defaults.PlayingText;
        NotPlayingText = node[nameof(NotPlayingText)]?.Value<string>() ?? defaults.NotPlayingText;
        FontSize = node[nameof(FontSize)]?.Value<float>() ?? defaults.FontSize;
        OutlineWidth = node[nameof(OutlineWidth)]?.Value<float>() ?? defaults.OutlineWidth;
        LineSpacing = node[nameof(LineSpacing)]?.Value<float>() ?? defaults.LineSpacing;
        LineSpacingAdj = node[nameof(LineSpacingAdj)]?.Value<float>() ?? defaults.LineSpacingAdj;
        ShadowDilate = node[nameof(ShadowDilate)]?.Value<float>() ?? defaults.ShadowDilate;
        ShadowSoftness = node[nameof(ShadowSoftness)]?.Value<float>() ?? defaults.ShadowSoftness;
        TextColor = node[nameof(TextColor)] != null
            ? ModelUtils.Unbox<GColor>(node[nameof(TextColor)])
            : defaults.TextColor;
        TextColor.gradientEnabled = node[nameof(TextColor)]?["gradientEnabled"]?.Value<bool>() ?? TextColor.gradientEnabled;
        OutlineColor = ParseColorNode(node[nameof(OutlineColor)], defaults.OutlineColor);
        ShadowColor = ParseColorNode(node[nameof(ShadowColor)], defaults.ShadowColor);
        Scale = node[nameof(Scale)] != null
            ? ModelUtils.ToVector2(node[nameof(Scale)])
            : defaults.Scale;
        Position = node[nameof(Position)] != null
            ? ModelUtils.ToVector2(node[nameof(Position)])
            : defaults.Position;
        Pivot = node[nameof(Pivot)] != null
            ? ModelUtils.ToVector2(node[nameof(Pivot)])
            : defaults.Pivot;
        ShadowOffset = node[nameof(ShadowOffset)] != null
            ? ModelUtils.ToVector2(node[nameof(ShadowOffset)])
            : defaults.ShadowOffset;
        Rotation = node[nameof(Rotation)] != null
            ? ModelUtils.ToVector3(node[nameof(Rotation)])
            : defaults.Rotation;
        Alignment = node[nameof(Alignment)] != null
            ? EnumHelper<TextAlignmentOptions>.Parse(node[nameof(Alignment)].Value<string>())
            : defaults.Alignment;
        EnableFallbackFonts = node[nameof(EnableFallbackFonts)]?.Value<bool>() ?? defaults.EnableFallbackFonts;
        FallbackFonts = node[nameof(FallbackFonts)] != null
            ? node[nameof(FallbackFonts)].ToObject<string[]>()
            : defaults.FallbackFonts;
    }

    private static Color ParseColorNode(JToken token, Color defaultValue) {
        if(token == null) {
            return defaultValue;
        }

        if(token.Type == JTokenType.Object) {
            JObject obj = (JObject)token;
            if(obj.TryGetValue("topLeft", out var legacy)) { // Legacy GColor
                return ModelUtils.ToColor(legacy);
            } else {
                return ModelUtils.ToColor(obj);
            }
        } else if(token.Type == JTokenType.Array) {
            return ModelUtils.ToColor(token);
        }
        return defaultValue;
    }
}
