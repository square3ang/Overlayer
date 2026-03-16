using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overlayer.Models;

public class ProfileConfig : IModel, ICopyable<ProfileConfig> {
    public bool Active = true;
    public string Name = "Profile NULL";
    public string Path = null;
    public float Opacity = 1f;
    public List<ObjectConfig> Objects = new();

    public ProfileConfig Copy() {
        return new ProfileConfig {
            Active = Active,
            Name = Name,
            Path = Path,
            Opacity = Opacity,
            Objects = Objects.Select(o => o.Copy()).ToList()
        };
    }

    public JToken Serialize() {
        var node = new JObject {
            [nameof(Active)] = Active,
            [nameof(Name)] = Name,
            [nameof(Path)] = Path,
            [nameof(Opacity)] = Opacity,
            [nameof(Objects)] = new JArray(
                Objects.Select(o => {
                    var objNode = (JObject)o.Serialize();
                    var typeName = o.GetType().Name.Replace("Config", "");
                    var newNode = new JObject { ["Type"] = typeName };
                    foreach(var prop in objNode.Properties()) {
                        newNode[prop.Name] = prop.Value;
                    }
                    return newNode;
                })
            )
        };
        return node;
    }

    public void Deserialize(JToken node) {
        var defaults = new ProfileConfig();

        Active = node[nameof(Active)]?.Value<bool>() ?? defaults.Active;
        Name = node[nameof(Name)]?.Value<string>() ?? defaults.Name;
        Path = node[nameof(Path)]?.Value<string>();
        Opacity = node[nameof(Opacity)]?.Value<float>() ?? defaults.Opacity;
        Objects = new List<ObjectConfig>();
        var objectTokens = (node[nameof(Objects)] as JArray)
            ?? (node["Texts"] as JArray)
            ?? new JArray();
        foreach(var obj in objectTokens) {
            string typeName = obj["Type"]?.Value<string>()?.Trim() ?? "";
            ObjectConfig cfg;
            if(!string.IsNullOrEmpty(typeName)) {
                var cfgType = Type.GetType($"Overlayer.Models.{typeName}Config");
                if(cfgType != null && typeof(ObjectConfig).IsAssignableFrom(cfgType)) {
                    cfg = (ObjectConfig)Activator.CreateInstance(cfgType);
                } else {
                    cfg = new TextConfig();
                }
            } else {
                cfg = new TextConfig();
            }

            cfg.Deserialize(obj);
            Objects.Add(cfg);
        }
    }
}