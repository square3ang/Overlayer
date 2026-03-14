using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Overlayer.Models;

public class ProfileConfig : IModel, ICopyable<ProfileConfig> {
    public bool Active = true;
    public string Name = "Profile NULL";
    public string Path = null;
    public float Opacity = 1f;
    public List<TextConfig> Texts = new();

    public ProfileConfig Copy() {
        return new ProfileConfig {
            Active = Active,
            Opacity = Opacity,
            Texts = Texts.Select(tc => tc.Copy()).ToList()
        };
    }

    public JToken Serialize() {
        return new JObject {
            [nameof(Active)] = Active,
            [nameof(Opacity)] = Opacity,
            [nameof(Texts)] = new JArray(Texts.ConvertAll(tc => tc.Serialize()))
        };
    }

    public void Deserialize(JToken node) {
        var defaultSettings = new ProfileConfig();
        Active = node[nameof(Active)]?.ToObject<bool>() ?? defaultSettings.Active;
        Opacity = node[nameof(Opacity)]?.ToObject<float>() ?? defaultSettings.Opacity;
        Texts = node[nameof(Texts)]?.ToObject<List<JToken>>()?.ConvertAll(tc => {
            var textConfig = new TextConfig();
            textConfig.Deserialize(tc);
            return textConfig;
        }) ?? defaultSettings.Texts;
    }
}