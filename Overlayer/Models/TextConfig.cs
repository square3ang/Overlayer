using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using Overlayer.Tags;
using Overlayer.Utils;
using TMPro;
using UnityEngine;

namespace Overlayer.Models;

public class TextConfig : ObjectConfig, ICopyable<TextConfig> {
    public TextConfig() => TagManager.OnLoadUnload += ExprApplyConfig;
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
    public ExprValue<float> FontSize = new(44f);
    public ExprValue<float> OutlineWidth = new(0f);
    public ExprValue<float> LineSpacing = new(-25f);
    public ExprValue<float> LineSpacingAdj = new(25f);
    public ExprValue<float> ShadowDilate = new(0f);
    public ExprValue<float> ShadowSoftness = new(0.5f);
    public bool EnableFallbackFonts = false;
    public string[] FallbackFonts = null;
    public ExprValue<GColor> TextColor = new(Color.white);
    public ExprValue<Color> OutlineColor = new(Color.clear);
    public ExprValue<Color> ShadowColor = new(Color.black with { a = 0.5f });
    public ExprValue<Vector2> Scale = new(new Vector2(1f, 1f));
    public ExprValue<Vector2> Position = new(new Vector2(0.5f, 0.0175f));
    public ExprValue<Vector2> Pivot = new(new Vector2(0.5f, 0.5f));
    public ExprValue<Vector2> ShadowOffset = new(new Vector2(0.5f, -0.5f));
    public ExprValue<Vector3> Rotation = new(Vector3.zero);
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
        node[nameof(FontSize)] = FontSize.Serialize(v => new JValue(v));
        node[nameof(OutlineWidth)] = OutlineWidth.Serialize(v => new JValue(v));
        node[nameof(LineSpacing)] = LineSpacing.Serialize(v => new JValue(v));
        node[nameof(LineSpacingAdj)] = LineSpacingAdj.Serialize(v => new JValue(v));
        node[nameof(ShadowDilate)] = ShadowDilate.Serialize(v => new JValue(v));
        node[nameof(ShadowSoftness)] = ShadowSoftness.Serialize(v => new JValue(v));
        node[nameof(EnableFallbackFonts)] = EnableFallbackFonts;
        node[nameof(FallbackFonts)] = FallbackFonts != null ? new JArray(FallbackFonts) : null;
        node[nameof(TextColor)] = TextColor.Serialize(v => v.Serialize());
        node[nameof(OutlineColor)] = OutlineColor.Serialize(ModelUtils.ToNode);
        node[nameof(ShadowColor)] = ShadowColor.Serialize(ModelUtils.ToNode);
        node[nameof(Scale)] = Scale.Serialize(ModelUtils.ToNode);
        node[nameof(Position)] = Position.Serialize(ModelUtils.ToNode);
        node[nameof(Pivot)] = Pivot.Serialize(ModelUtils.ToNode);
        node[nameof(ShadowOffset)] = ShadowOffset.Serialize(ModelUtils.ToNode);
        node[nameof(Rotation)] = Rotation.Serialize(ModelUtils.ToNode);
        node[nameof(Alignment)] = Alignment.ToString();
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
        FontSize.Deserialize(node[nameof(FontSize)], n => n.Value<float>());
        OutlineWidth.Deserialize(node[nameof(OutlineWidth)], n => n.Value<float>());
        LineSpacing.Deserialize(node[nameof(LineSpacing)], n => n.Value<float>());
        LineSpacingAdj.Deserialize(node[nameof(LineSpacingAdj)], n => n.Value<float>());
        ShadowDilate.Deserialize(node[nameof(ShadowDilate)], n => n.Value<float>());
        ShadowSoftness.Deserialize(node[nameof(ShadowSoftness)], n => n.Value<float>());
        EnableFallbackFonts = node[nameof(EnableFallbackFonts)]?.Value<bool>() ?? defaults.EnableFallbackFonts;
        FallbackFonts = node[nameof(FallbackFonts)] != null ? node[nameof(FallbackFonts)].ToObject<string[]>() : defaults.FallbackFonts;
        TextColor.Deserialize(node[nameof(TextColor)], ModelUtils.Unbox<GColor>);
        OutlineColor.Deserialize(node[nameof(OutlineColor)], n => ParseColorNode(n, defaults.OutlineColor.DefaultValue));
        ShadowColor.Deserialize(node[nameof(ShadowColor)], n => ParseColorNode(n, defaults.ShadowColor.DefaultValue));
        Scale.Deserialize(node[nameof(Scale)], ModelUtils.ToVector2);
        Position.Deserialize(node[nameof(Position)], ModelUtils.ToVector2);
        Pivot.Deserialize(node[nameof(Pivot)], ModelUtils.ToVector2);
        ShadowOffset.Deserialize(node[nameof(ShadowOffset)], ModelUtils.ToVector2);
        Rotation.Deserialize(node[nameof(Rotation)], ModelUtils.ToVector3);
        Alignment = node[nameof(Alignment)] != null ? EnumHelper<TextAlignmentOptions>.Parse(node[nameof(Alignment)].Value<string>()) : defaults.Alignment;
    }

    public void Init() {
        if(FontSize.HasExpr) { FontSize.Init(); }
        if(OutlineWidth.HasExpr) { OutlineWidth.Init(); }
        if(LineSpacing.HasExpr) { LineSpacing.Init(); }
        if(LineSpacingAdj.HasExpr) { LineSpacingAdj.Init(); }
        if(ShadowDilate.HasExpr) { ShadowDilate.Init(); }
        if(ShadowSoftness.HasExpr) { ShadowSoftness.Init(); }
        if(TextColor.HasExpr) { TextColor.Init(); }
        if(OutlineColor.HasExpr) { OutlineColor.Init(); }
        if(ShadowColor.HasExpr) { ShadowColor.Init(); }
        if(Scale.HasExpr) { Scale.Init(); }
        if(Position.HasExpr) { Position.Init(); }
        if(Pivot.HasExpr) { Pivot.Init(); }
        if(ShadowOffset.HasExpr) { ShadowOffset.Init(); }
        if(Rotation.HasExpr) { Rotation.Init(); }
    }

    public void ExprApplyConfig() {
        if(FontSize.IsExpr) { FontSize.ApplyConfig(); }
        if(OutlineWidth.IsExpr) { OutlineWidth.ApplyConfig(); }
        if(LineSpacing.IsExpr) { LineSpacing.ApplyConfig(); }
        if(LineSpacingAdj.IsExpr) { LineSpacingAdj.ApplyConfig(); }
        if(ShadowDilate.IsExpr) { ShadowDilate.ApplyConfig(); }
        if(ShadowSoftness.IsExpr) { ShadowSoftness.ApplyConfig(); }
        if(TextColor.IsExpr) { TextColor.ApplyConfig(); }
        if(OutlineColor.IsExpr) { OutlineColor.ApplyConfig(); }
        if(ShadowColor.IsExpr) { ShadowColor.ApplyConfig(); }
        if(Scale.IsExpr) { Scale.ApplyConfig(); }
        if(Position.IsExpr) { Position.ApplyConfig(); }
        if(Pivot.IsExpr) { Pivot.ApplyConfig(); }
        if(ShadowOffset.IsExpr) { ShadowOffset.ApplyConfig(); }
        if(Rotation.IsExpr) { Rotation.ApplyConfig(); }
    }

    public void Release() {
        TagManager.OnLoadUnload -= ExprApplyConfig;
        if(FontSize.IsExpr) { FontSize.Dispose(); }
        if(OutlineWidth.IsExpr) { OutlineWidth.Dispose(); }
        if(LineSpacing.IsExpr) { LineSpacing.Dispose(); }
        if(LineSpacingAdj.IsExpr) { LineSpacingAdj.Dispose(); }
        if(ShadowDilate.IsExpr) { ShadowDilate.Dispose(); }
        if(ShadowSoftness.IsExpr) { ShadowSoftness.Dispose(); }
        if(TextColor.IsExpr) { TextColor.Dispose(); }
        if(OutlineColor.IsExpr) { OutlineColor.Dispose(); }
        if(ShadowColor.IsExpr) { ShadowColor.Dispose(); }
        if(Scale.IsExpr) { Scale.Dispose(); }
        if(Position.IsExpr) { Position.Dispose(); }
        if(Pivot.IsExpr) { Pivot.Dispose(); }
        if(ShadowOffset.IsExpr) { ShadowOffset.Dispose(); }
        if(Rotation.IsExpr) { Rotation.Dispose(); }
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
