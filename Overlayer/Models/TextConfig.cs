using JSON;
using Overlayer.Core.Interfaces;
using Overlayer.Utils;
using TMPro;
using UnityEngine;

namespace Overlayer.Models
{
    public class TextConfig : IModel, ICopyable<TextConfig>
    {
        public bool Active = true;
        public string Name = string.Empty;
        public string Font = "Default";
        public string PlayingText = "<color=#{FOHex}>{Overloads}</color> <color=#{TEHex}>{CTE}</color> <color=#{VEHex}>{CVE}</color> <color=#{EPHex}>{CEP}</color> <color=#{PHex}>{CP}</color> <color=#{LPHex}>{CLP}</color> <color=#{VLHex}>{CVL}</color> <color=#{TLHex}>{CTL}</color> <color=#{FMHex}>{MissCount}</color>";
        public string NotPlayingText = string.Empty;
        public string LexOption = "{}:(),";
        public float FontSize = 44;
        public float OutlineWidth = 0;
        public float LineSpacing = -25f;
        public float LineSpacingAdj = 25f;
        public float ShadowDilate = 0;
        public float ShadowSoftness = 0.5f;
        public GColor TextColor = Color.white;
        public GColor OutlineColor = Color.clear;
        public GColor ShadowColor = Color.black with { a = 0.5f };
        public Vector2 Position = new Vector2(0.5f, 0.0175f);
        public Vector2 Pivot = new Vector2(0.5f, 0.5f);
        public Vector2 ShadowOffset = new Vector2(0.5f, -0.5f);
        public Vector3 Rotation = Vector3.zero;
        public TextAlignmentOptions Alignment = TextAlignmentOptions.Center;
        public TextConfig Copy()
        {
            var newConfig = new TextConfig();
            newConfig.Active = Active;
            newConfig.Name = Name;
            newConfig.Font = Font;
            newConfig.PlayingText = PlayingText;
            newConfig.NotPlayingText = NotPlayingText;
            newConfig.LexOption = LexOption;
            newConfig.FontSize = FontSize;
            newConfig.OutlineWidth = OutlineWidth;
            newConfig.LineSpacing = LineSpacing;
            newConfig.LineSpacingAdj = LineSpacingAdj;
            newConfig.ShadowDilate = ShadowDilate;
            newConfig.ShadowSoftness = ShadowSoftness;
            newConfig.TextColor = TextColor;
            newConfig.OutlineColor = OutlineColor;
            newConfig.ShadowColor = ShadowColor;
            newConfig.Position = Position;
            newConfig.Pivot = Pivot;
            newConfig.ShadowOffset = ShadowOffset;
            newConfig.Rotation = Rotation;
            newConfig.Alignment = Alignment;
            return newConfig;
        }
        public JsonNode Serialize()
        {
            var node = JsonNode.Empty;
            node[nameof(Active)] = Active;
            node[nameof(Name)] = Name;
            node[nameof(Font)] = Font;
            node[nameof(PlayingText)] = PlayingText;
            node[nameof(NotPlayingText)] = NotPlayingText;
            node[nameof(LexOption)] = LexOption;
            node[nameof(FontSize)] = FontSize;
            node[nameof(OutlineWidth)] = OutlineWidth;
            node[nameof(LineSpacing)] = LineSpacing;
            node[nameof(LineSpacingAdj)] = LineSpacingAdj;
            node[nameof(ShadowDilate)] = ShadowDilate;
            node[nameof(ShadowSoftness)] = ShadowSoftness;
            node[nameof(TextColor)] = TextColor.Serialize();
            node[nameof(OutlineColor)] = OutlineColor.Serialize();
            node[nameof(ShadowColor)] = ShadowColor.Serialize();
            node[nameof(Position)] = Position;
            node[nameof(Pivot)] = Pivot;
            node[nameof(ShadowOffset)] = ShadowOffset;
            node[nameof(Rotation)] = Rotation;
            node[nameof(Alignment)] = Alignment.ToString();
            return node;
        }
        public void Deserialize(JsonNode node)
        {
            Active = node[nameof(Active)];
            Name = node[nameof(Name)];
            Font = node[nameof(Font)];
            PlayingText = node[nameof(PlayingText)];
            NotPlayingText = node[nameof(NotPlayingText)];
            LexOption = node[nameof(LexOption)].IfNotExist("{}:(),");
            FontSize = node[nameof(FontSize)];
            OutlineWidth = node[nameof(OutlineWidth)];
            LineSpacing = node[nameof(LineSpacing)];
            LineSpacingAdj = node[nameof(LineSpacingAdj)].IfNotExist(node["LineSpacingAdjustment"]);
            ShadowDilate = node[nameof(ShadowDilate)].IfNotExist(0);
            ShadowSoftness = node[nameof(ShadowSoftness)].IfNotExist(0.5f);
            TextColor = ModelUtils.Unbox<GColor>(node[nameof(TextColor)]);
            TextColor.gradientEnabled = node["GradientText"].IfNotExist(TextColor.gradientEnabled);
            OutlineColor = ModelUtils.Unbox<GColor>(node[nameof(OutlineColor)]);
            ShadowColor = ModelUtils.Unbox<GColor>(node[nameof(ShadowColor)]);
            Position = node[nameof(Position)];
            Pivot = node[nameof(Pivot)];
            ShadowOffset = node[nameof(ShadowOffset)].IfNotExist(new Vector2(0.5f, -0.5f));
            Rotation = node[nameof(Rotation)];
            Alignment = EnumHelper<TextAlignmentOptions>.Parse(node[nameof(Alignment)].IfNotExist(nameof(TextAlignmentOptions.Center)));
        }
    }
}
