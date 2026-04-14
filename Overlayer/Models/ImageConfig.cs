using Newtonsoft.Json.Linq;
using Overlayer.Core;
using Overlayer.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Overlayer.Models;

public class ImageConfig : ObjectConfig, ICopyable<ImageConfig> {
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

    public List<string> Images = ["{ModDir}ov3_logo.png"];
    public ExprValue<Color> Color = new(UnityEngine.Color.white);

    public ExprValue<Vector2> Scale = new(new Vector2(1, 1));
    public ExprValue<Vector2> Position = new(new Vector2(0.5f, 0.5f));
    public ExprValue<Vector2> Pivot = new(new Vector2(0.5f, 0.5f));
    public ExprValue<Vector3> Rotation = new(Vector3.zero);

    public string PlayingCommand = "0";
    public string NotPlayingCommand = "0";

    public override ObjectConfig Copy() {
        var copy = new ImageConfig {
            Drag = Drag,
            Images = Images.ToList(),
            Color = Color,
            Scale = Scale,
            Position = Position,
            Pivot = Pivot,
            Rotation = Rotation,
            PlayingCommand = PlayingCommand,
            NotPlayingCommand = NotPlayingCommand
        };
        CopyBase(copy);
        return copy;
    }

    ImageConfig ICopyable<ImageConfig>.Copy() => (ImageConfig)Copy();

    public override JToken Serialize() {
        var node = SerializeBase();
        node[nameof(Drag)] = Drag;
        node[nameof(Images)] = new JArray(Images);
        node[nameof(Color)] = Color.Serialize(ModelUtils.ToNode);
        node[nameof(Scale)] = Scale.Serialize(ModelUtils.ToNode);
        node[nameof(Position)] = Position.Serialize(ModelUtils.ToNode);
        node[nameof(Pivot)] = Pivot.Serialize(ModelUtils.ToNode);
        node[nameof(Rotation)] = Rotation.Serialize(ModelUtils.ToNode);
        node[nameof(PlayingCommand)] = PlayingCommand;
        node[nameof(NotPlayingCommand)] = NotPlayingCommand;
        return node;
    }

    public override void Deserialize(JToken node) {
        var defaults = new ImageConfig();
        DeserializeBase(node);
        Drag = node[nameof(Drag)]?.Value<bool>() ?? defaults.Drag;
        Images = node[nameof(Images)] is JArray imagesToken ? imagesToken.Select(t => t.Value<string>()).ToList() : defaults.Images.ToList();
        Color.Deserialize(node[nameof(Color)], n => ModelUtils.ParseColorNode(n, defaults.Color.DefaultValue));
        Scale.Deserialize(node[nameof(Scale)], ModelUtils.ToVector2);
        Position.Deserialize(node[nameof(Position)], ModelUtils.ToVector2);
        Pivot.Deserialize(node[nameof(Pivot)], ModelUtils.ToVector2);
        Rotation.Deserialize(node[nameof(Rotation)], ModelUtils.ToVector3);
        PlayingCommand = node[nameof(PlayingCommand)]?.Value<string>() ?? defaults.PlayingCommand;
        NotPlayingCommand = node[nameof(NotPlayingCommand)]?.Value<string>() ?? defaults.NotPlayingCommand;
    }

    public void Init() {
        Color.Init();
        Scale.Init();
        Position.Init();
        Pivot.Init();
        Rotation.Init();
    }

    public void ExprApplyConfig() {
        Color.ApplyConfig();
        Scale.ApplyConfig();
        Position.ApplyConfig();
        Pivot.ApplyConfig();
        Rotation.ApplyConfig();
    }

    public void Release() {
        TagManager.OnLoadUnload -= ExprApplyConfig;
        Color.Dispose();
        Scale.Dispose();
        Position.Dispose();
        Pivot.Dispose();
        Rotation.Dispose();
    }
}