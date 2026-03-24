using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using Overlayer.Core.TextReplacing;
using Overlayer.Tags;
using System;
using System.Linq;

namespace Overlayer.Models;

public class ExprValue<T> : ICopyable<ExprValue<T>> {
    public ExprValue(T defaultValue) {
        DefaultValue = defaultValue;
        Value = defaultValue;
    }
    public readonly T DefaultValue;
    public T Value;
    private bool ReplacersInitialized = false;
    public bool IsExpr => ReplacersInitialized;
    public bool HasExpr { get; private set; } = false;
    public Replacer PlayingReplacer;
    public Replacer NotPlayingReplacer;
    public string Playing = "";
    public string NotPlaying = "";

    public ExprValue<T> Copy() {
        return new ExprValue<T>(DefaultValue) {
            Value = Value,
            PlayingReplacer = PlayingReplacer?.Copy(),
            NotPlayingReplacer = NotPlayingReplacer?.Copy(),
            Playing = Playing,
            NotPlaying = NotPlaying
        };
    }

    private static JToken SerializeValue(T value, Func<T, JToken> serializer) {
        try {
            return serializer(value);
        } catch {
            return JValue.CreateNull();
        }
    }

    private T DeserializeValue(Func<T> parser) {
        try {
            return parser();
        } catch {
            return DefaultValue;
        }
    }

    public JToken Serialize(Func<T, JToken> serializer) {
        if(ReplacersInitialized) {
            JObject obj = new() {
                [nameof(Playing)] = Playing,
                [nameof(NotPlaying)] = NotPlaying
            };
            return obj;
        }

        return SerializeValue(Value, serializer);
    }

    public bool Deserialize(JToken node, Func<JToken, T> parser) {
        if(node == null) {
            Value = DefaultValue;
            HasExpr = false;
            return false;
        }

        if(node.Type == JTokenType.Object && node[nameof(Playing)] != null) {
            Playing = node[nameof(Playing)].Value<string>();
            NotPlaying = node[nameof(NotPlaying)]?.Value<string>() ?? "";
            HasExpr = true;
            return true;
        }

        Value = DeserializeValue(() => parser(node));
        HasExpr = false;
        return false;
    }

    public void Init() {
        if(ReplacersInitialized) {
            return;
        }

        PlayingReplacer = new Replacer(Playing, TagManager.All.Select(ot => ot.Tag));
        NotPlayingReplacer = new Replacer(NotPlaying, TagManager.NP.Select(ot => ot.Tag));
        ApplyConfig();
        ReplacersInitialized = true;
    }

    public T Update(Func<string, T> parser) {
        string raw = Main.IsPlaying ? PlayingReplacer?.Replace() : NotPlayingReplacer?.Replace();

        if(string.IsNullOrEmpty(raw)) {
            return default;
        }
        if(true) {
            try {
                return parser(raw);
            } catch {
                return default;
            }
        } else {
            return parser(raw);
        }
    }

    public void ApplyConfig() {
        PlayingReplacer.Source = Playing;
        NotPlayingReplacer.Source = NotPlaying;
        PlayingReplacer.UpdateTags(TagManager.All.Select(ot => ot.Tag));
        NotPlayingReplacer.UpdateTags(TagManager.NP.Select(ot => ot.Tag));
        PlayingReplacer.Compile();
        NotPlayingReplacer.Compile();
        TagManager.UpdatePatch();
    }

    public void Dispose() {
        if(!ReplacersInitialized) {
            return;
        }

        PlayingReplacer?.Dispose();
        NotPlayingReplacer?.Dispose();
        ReplacersInitialized = false;
    }

    public bool GetNormalValue(out T value) {
        if(!ReplacersInitialized) {
            value = Value;
            return true;
        }

        value = default;
        return false;
    }

    public bool GetExprValue(Func<string, T> parser, out T expr) {
        if(ReplacersInitialized) {
            expr = Update(parser);
            return true;
        }

        expr = default;
        return false;
    }
}