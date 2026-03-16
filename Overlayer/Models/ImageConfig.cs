using Newtonsoft.Json.Linq;
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
            if(_drag == value)
                return;
            _drag = value;
            OnDragChanged?.Invoke(_drag);
        }
    }

    public List<string> Images = new();
    public Color Color = Color.white;

    public Vector2 Scale = new(1, 1);
    public Vector2 Position = new(0.5f, 0.0175f);
    public Vector2 Pivot = new(0.5f, 0.5f);
    public Vector3 Rotation = Vector3.zero;

    public string PlayingCommand = string.Empty;
    public string NotPlayingCommand = string.Empty;

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
        node[nameof(Color)] = ModelUtils.ToNode(Color);
        node[nameof(Scale)] = ModelUtils.ToNode(Scale);
        node[nameof(Position)] = ModelUtils.ToNode(Position);
        node[nameof(Pivot)] = ModelUtils.ToNode(Pivot);
        node[nameof(Rotation)] = ModelUtils.ToNode(Rotation);
        node[nameof(PlayingCommand)] = PlayingCommand;
        node[nameof(NotPlayingCommand)] = NotPlayingCommand;
        return node;
    }

    public override void Deserialize(JToken node) {
        Main.Logger.Log("Deserializing ImageConfig...");
        var defaults = new ImageConfig();
        DeserializeBase(node);
        Drag = node[nameof(Drag)]?.Value<bool>() ?? defaults.Drag;
        if(node[nameof(Images)] is JArray imagesToken) {
            Images = imagesToken.Select(t => t.Value<string>()).ToList();
        } else {
            Images = defaults.Images.ToList();
        }
        Color = node[nameof(Color)] != null ? ModelUtils.ToColor(node[nameof(Color)]) : defaults.Color;
        Scale = node[nameof(Scale)] != null ? ModelUtils.ToVector2(node[nameof(Scale)]) : defaults.Scale;
        Position = node[nameof(Position)] != null ? ModelUtils.ToVector2(node[nameof(Position)]) : defaults.Position;
        Pivot = node[nameof(Pivot)] != null ? ModelUtils.ToVector2(node[nameof(Pivot)]) : defaults.Pivot;
        Rotation = node[nameof(Rotation)] != null ? ModelUtils.ToVector3(node[nameof(Rotation)]) : defaults.Rotation;
        PlayingCommand = node[nameof(PlayingCommand)]?.Value<string>() ?? defaults.PlayingCommand;
        NotPlayingCommand = node[nameof(NotPlayingCommand)]?.Value<string>() ?? defaults.NotPlayingCommand;
    }
}