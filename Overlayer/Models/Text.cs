using JSON;
using Overlayer.Core.Interfaces;
using TMPro;
using UnityEngine;
using Overlayer.Utils;

namespace Overlayer.Models
{
    public class Text : IModel, ICopyable<Text>
    {
        public bool Active = true;
        public bool IsExpanded = false;
        public string Name = string.Empty;
        public string Font = "Default";
        public string PlayingText = "<color=#{FOHex}>{Overloads}</color> <color=#{TEHex}>{CurTE}</color> <color=#{VEHex}>{CurVE}</color> <color=#{EPHex}>{CurEP}</color> <color=#{PHex}>{CurP}</color> <color=#{LPHex}>{CurLP}</color> <color=#{VLHex}>{CurVL}</color> <color=#{TLHex}>{CurTL}</color> <color=#{FMHex}>{MissCount}</color>";
        public string NotPlayingText = string.Empty;
        public float FontSize = 44;
        public float OutlineWidth = 0;
        public float LineSpacing = -25f;
        public float LineSpacingAdj = 25f;
        public GColor TextColor = Color.white;
        public GColor OutlineColor = Color.clear;
        public GColor ShadowColor = Color.white with { a = 0.5f };
        public Vector2 Position = Vector2.zero;
        public Vector3 Rotation = Vector3.zero;
        public TextAlignmentOptions Alignment = TextAlignmentOptions.TopLeft;
        public Text Copy()
        {
            var newText = new Text();
            newText.Active = Active;
            newText.IsExpanded = IsExpanded;
            newText.Name = Name;
            newText.Font = Font;
            newText.PlayingText = PlayingText;
            newText.NotPlayingText = NotPlayingText;
            newText.FontSize = FontSize;
            newText.OutlineWidth = OutlineWidth;
            newText.LineSpacing = LineSpacing;
            newText.LineSpacingAdj = LineSpacingAdj;
            newText.TextColor = TextColor;
            newText.OutlineColor = OutlineColor;
            newText.ShadowColor = ShadowColor;
            newText.Position = Position;
            newText.Rotation = Rotation;
            newText.Alignment = Alignment;
            return newText;
        }
        public JsonNode Serialize()
        {
            var node = JsonNode.Empty;
            node[nameof(Active)] = Active;
            node[nameof(IsExpanded)] = IsExpanded;
            node[nameof(Name)] = Name;
            node[nameof(Font)] = Font;
            node[nameof(PlayingText)] = PlayingText;
            node[nameof(NotPlayingText)] = NotPlayingText;
            node[nameof(FontSize)] = FontSize; 
            node[nameof(OutlineWidth)] = OutlineWidth;
            node[nameof(LineSpacing)] = LineSpacing;
            node[nameof(LineSpacingAdj)] = LineSpacingAdj;
            node[nameof(TextColor)] = TextColor.Serialize();
            node[nameof(OutlineColor)] = OutlineColor.Serialize();
            node[nameof(ShadowColor)] = ShadowColor.Serialize();
            node[nameof(Position)] = Position;
            node[nameof(Rotation)] = Rotation;
            node[nameof(Alignment)] = Alignment.ToString();
            return node;
        }
        public void Deserialize(JsonNode node)
        {
            Active = node[nameof(Active)];
            IsExpanded = node[nameof(IsExpanded)];
            Name = node[nameof(Name)];
            Font = node[nameof(Font)];
            PlayingText = node[nameof(PlayingText)];
            NotPlayingText = node[nameof(NotPlayingText)];
            FontSize = node[nameof(FontSize)];
            OutlineWidth = node[nameof(LineSpacing)];
            LineSpacing = node[nameof(LineSpacing)];
            LineSpacingAdj = node[nameof(LineSpacingAdj)];
            TextColor = ModelUtils.Unbox<GColor>(node[nameof(TextColor)]);
            OutlineColor = ModelUtils.Unbox<GColor>(node[nameof(OutlineColor)]);
            ShadowColor = ModelUtils.Unbox<GColor>(node[nameof(ShadowColor)]);
            Position = node[nameof(Position)];
            Rotation = node[nameof(Rotation)];
            Alignment = EnumHelper<TextAlignmentOptions>.Parse(node[nameof(Alignment)]);
        }
    }
}
