using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;

namespace Overlayer.Models;

public class FontMeta : IModel, ICopyable<FontMeta> {
    public string name;
    public float lineSpacing = 1;
    public float fontScale = 0.5f;

    public JToken Serialize() {
        return new JObject {
            [nameof(name)] = name,
            [nameof(lineSpacing)] = lineSpacing,
            [nameof(fontScale)] = fontScale
        };
    }

    public void Deserialize(JToken obj) {
        var defaultSettings = new FontMeta();

        name = obj.Value<string>(nameof(name)) ?? defaultSettings.name;
        lineSpacing = obj.Value<float?>(nameof(lineSpacing)) ?? defaultSettings.lineSpacing;
        fontScale = obj.Value<float?>(nameof(fontScale)) ?? defaultSettings.fontScale;
    }

    public FontMeta Copy() {
        return new FontMeta {
            name = name,
            lineSpacing = lineSpacing,
            fontScale = fontScale
        };
    }
}
