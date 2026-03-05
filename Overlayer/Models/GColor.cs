using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using TMPro;
using UnityEngine;

namespace Overlayer.Models;

public struct GColor : IModel, ICopyable<GColor> {
    internal VertexGradient _color;

    private string _topLeftHex;
    private string _topRightHex;
    private string _bottomLeftHex;
    private string _bottomRightHex;

    public bool gradientEnabled = false;

    public Color topLeft { get => _color.topLeft; set => SetTopLeftColor(value); }
    public Color topRight { get => _color.topRight; set => SetTopRightColor(value); }
    public Color bottomLeft { get => _color.bottomLeft; set => SetBottomLeftColor(value); }
    public Color bottomRight { get => _color.bottomRight; set => SetBottomRightColor(value); }

    public GUIStatus status;
    public GUIStatus topLeftStatus;
    public GUIStatus topRightStatus;
    public GUIStatus bottomLeftStatus;
    public GUIStatus bottomRightStatus;

    public string topLeftHex { get => _topLeftHex; set => SetTopLeftHex(value); }
    public string topRightHex { get => _topRightHex; set => SetTopRightHex(value); }
    public string bottomLeftHex { get => _bottomLeftHex; set => SetBottomLeftHex(value); }
    public string bottomRightHex { get => _bottomRightHex; set => SetBottomRightHex(value); }

    public float r { get => _color.topLeft.r; set => SetTopLeftColor(_color.topLeft with { r = value }); }
    public float g { get => _color.topLeft.g; set => SetTopLeftColor(_color.topLeft with { g = value }); }
    public float b { get => _color.topLeft.b; set => SetTopLeftColor(_color.topLeft with { b = value }); }
    public float a { get => _color.topLeft.a; set => SetTopLeftColor(_color.topLeft with { a = value }); }

    public GColor(Color color) {
        _color = new VertexGradient(color);
        var hex = ColorUtility.ToHtmlStringRGBA(color);
        _topLeftHex = hex;
        _topRightHex = hex;
        _bottomLeftHex = hex;
        _bottomRightHex = hex;
        status = new GUIStatus() {
            Expanded = false,
        };
        topLeftStatus = new GUIStatus() {
            Expanded = false,
        };
        topRightStatus = new GUIStatus() {
            Expanded = false,
        };
        bottomLeftStatus = new GUIStatus() {
            Expanded = false,
        };
        bottomRightStatus = new GUIStatus() {
            Expanded = false,
        };
    }
    public GColor(VertexGradient color) {
        _color = color;
        _topLeftHex = ColorUtility.ToHtmlStringRGBA(color.topLeft);
        _topRightHex = ColorUtility.ToHtmlStringRGBA(color.topRight);
        _bottomLeftHex = ColorUtility.ToHtmlStringRGBA(color.bottomLeft);
        _bottomRightHex = ColorUtility.ToHtmlStringRGBA(color.bottomRight);
        status = new GUIStatus() {
            Expanded = false,
        };
        topLeftStatus = new GUIStatus() {
            Expanded = false,
        };
        topRightStatus = new GUIStatus() {
            Expanded = false,
        };
        bottomLeftStatus = new GUIStatus() {
            Expanded = false,
        };
        bottomRightStatus = new GUIStatus() {
            Expanded = false,
        };

    }
    public GColor Copy() {
        var col = new GColor {
            gradientEnabled = gradientEnabled,
            topLeft = topLeft,
            topRight = topRight,
            bottomLeft = bottomLeft,
            bottomRight = bottomRight,
            status = status.Copy(),
            topLeftStatus = topLeftStatus.Copy(),
            topRightStatus = topRightStatus.Copy(),
            bottomLeftStatus = bottomLeftStatus.Copy(),
            bottomRightStatus = bottomRightStatus.Copy()
        };
        return col;
    }
    public JToken Serialize() {
        return new JObject {
            [nameof(gradientEnabled)] = gradientEnabled,
            [nameof(topLeft)] = ModelUtils.ToNode(topLeft),
            [nameof(topRight)] = ModelUtils.ToNode(topRight),
            [nameof(bottomLeft)] = ModelUtils.ToNode(bottomLeft),
            [nameof(bottomRight)] = ModelUtils.ToNode(bottomRight),
            [nameof(status)] = status?.Serialize(),
            [nameof(topLeftStatus)] = topLeftStatus?.Serialize(),
            [nameof(topRightStatus)] = topRightStatus?.Serialize(),
            [nameof(bottomLeftStatus)] = bottomLeftStatus?.Serialize(),
            [nameof(bottomRightStatus)] = bottomRightStatus?.Serialize()
        };
    }
    public void Deserialize(JToken node) {
        gradientEnabled = node.Value<bool?>(nameof(gradientEnabled)) ?? false;

        topLeft = node[nameof(topLeft)] != null
            ? ModelUtils.ToColor(node[nameof(topLeft)])
            : default;
        topRight = node[nameof(topRight)] != null
            ? ModelUtils.ToColor(node[nameof(topRight)])
            : default;
        bottomLeft = node[nameof(bottomLeft)] != null
            ? ModelUtils.ToColor(node[nameof(bottomLeft)])
            : default;
        bottomRight = node[nameof(bottomRight)] != null
            ? ModelUtils.ToColor(node[nameof(bottomRight)])
            : default;

        topLeftStatus = node[nameof(topLeftStatus)] != null
            ? ModelUtils.Unbox<GUIStatus>(node[nameof(topLeftStatus)])
            : new GUIStatus();
        topRightStatus = node[nameof(topRightStatus)] != null
            ? ModelUtils.Unbox<GUIStatus>(node[nameof(topRightStatus)])
            : new GUIStatus();
        bottomLeftStatus = node[nameof(bottomLeftStatus)] != null
            ? ModelUtils.Unbox<GUIStatus>(node[nameof(bottomLeftStatus)])
            : new GUIStatus();
        bottomRightStatus = node[nameof(bottomRightStatus)] != null
            ? ModelUtils.Unbox<GUIStatus>(node[nameof(bottomRightStatus)])
            : new GUIStatus();

        status = node[nameof(status)] != null
            ? ModelUtils.Unbox<GUIStatus>(node[nameof(status)])
            : new GUIStatus();
    }

    private void SetTopLeftColor(Color color) {
        if(color == _color.topLeft) {
            return;
        }

        _color.topLeft = color;
        _topLeftHex = ColorUtility.ToHtmlStringRGBA(color);
    }
    private void SetTopLeftHex(string hex) {
        if(hex == _topLeftHex) {
            return;
        }

        if(ColorUtility.TryParseHtmlString($"#{hex}", out var parsed)) {
            _color.topLeft = parsed;
            _topLeftHex = hex;
        }
    }

    private void SetTopRightColor(Color color) {
        if(color == _color.topRight) {
            return;
        }

        _color.topRight = color;
        _topRightHex = ColorUtility.ToHtmlStringRGBA(color);
    }
    private void SetTopRightHex(string hex) {
        if(hex == _topRightHex) {
            return;
        }

        if(ColorUtility.TryParseHtmlString($"#{hex}", out var parsed)) {
            _color.topRight = parsed;
            _topRightHex = hex;
        }
    }

    private void SetBottomLeftColor(Color color) {
        if(color == _color.bottomLeft) {
            return;
        }

        _color.bottomLeft = color;
        _bottomLeftHex = ColorUtility.ToHtmlStringRGBA(color);
    }
    private void SetBottomLeftHex(string hex) {
        if(hex == _bottomLeftHex) {
            return;
        }

        if(ColorUtility.TryParseHtmlString($"#{hex}", out var parsed)) {
            _color.bottomLeft = parsed;
            _bottomLeftHex = hex;
        }
    }

    private void SetBottomRightColor(Color color) {
        if(color == _color.bottomRight) {
            return;
        }

        _color.bottomRight = color;
        _bottomRightHex = ColorUtility.ToHtmlStringRGBA(color);
    }
    private void SetBottomRightHex(string hex) {
        if(hex == _bottomRightHex) {
            return;
        }

        if(ColorUtility.TryParseHtmlString($"#{hex}", out var parsed)) {
            _color.bottomRight = parsed;
            _bottomRightHex = hex;
        }
    }

    public static implicit operator Color(GColor color) => color.topLeft;
    public static implicit operator GColor(Color color) => new(color);

    public static implicit operator VertexGradient(GColor color) => color.gradientEnabled ? new VertexGradient(color.topLeft, color.topRight, color.bottomLeft, color.bottomRight) : new VertexGradient(color);
    public static implicit operator GColor(VertexGradient color) => new(color);

    public static GColor operator +(GColor a, GColor b) {
        return new VertexGradient(
            a.topLeft + b.topLeft,
            a.topRight + b.topRight,
            a.bottomLeft + b.bottomLeft,
            a.bottomRight + b.bottomRight);
    }
    public static GColor operator -(GColor a, GColor b) {
        return new VertexGradient(
            a.topLeft - b.topLeft,
            a.topRight - b.topRight,
            a.bottomLeft - b.bottomLeft,
            a.bottomRight - b.bottomRight);
    }
}
