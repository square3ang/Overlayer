using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Overlayer.Models;
using Overlayer.Unity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Overlayer.Core;

public static class TextManager {
    public static bool Initialized { get; private set; }
    public static int Count => Texts.Count;
    private static List<OverlayerText> Texts;
    public static void Initialize() {
        if(Initialized) {
            return;
        }

        Texts = new List<OverlayerText>();
        string textsPath = Path.Combine(Main.Mod.Path, "Texts.json");
        List<TextConfig> configs = new();

        if(File.Exists(textsPath)) {
            var content = File.ReadAllText(textsPath);
            if(!string.IsNullOrWhiteSpace(content)) {
                var token = JToken.Parse(content);

                if(token.Type == JTokenType.Array) {
                    foreach(var item in (JArray)token) {
                        if(item.Type == JTokenType.Object) {
                            configs.Add(TextConfigImporter.Import((JObject)item));
                        }
                    }
                } else if(token.Type == JTokenType.Object) {
                    configs.Add(TextConfigImporter.Import((JObject)token));
                }
            }
        }

        foreach(var config in configs) {
            CreateText(config);
        }

        Refresh();
        Initialized = true;
    }
    public static OverlayerText CreateText(TextConfig config) {
        if(string.IsNullOrEmpty(config.Name)) {
            config.Name = (Count + 1).ToString();
        }

        GameObject go = new($"OverlayerText_{config.Name}");
        var text = go.AddComponent<OverlayerText>();
        text.Init(config);
        Texts.Add(text);
        return text;
    }
    public static OverlayerText Get(int index) => index < 0 || index >= Texts.Count ? null : Texts[index];
    public static OverlayerText Find(TextConfig configRef) => Texts.Find(ot => ReferenceEquals(ot.Config, configRef));

    public static bool MoveTextToIndex(int from, int to) {
        if(from < 0 || from >= Texts.Count || to < 0 || to >= Texts.Count || from == to) {
            return false;
        }

        var item = Texts[from];
        Texts.RemoveAt(from);
        Texts.Insert(to, item);

        item.gameObject.transform.SetSiblingIndex(to);
        return true;
    }
    public static bool MoveTextUp(int index) => MoveTextToIndex(index, index - 1);
    public static bool MoveTextDown(int index) => MoveTextToIndex(index, index + 1);
    public static bool MoveTextToTop(int index) => MoveTextToIndex(index, 0);
    public static bool MoveTextToBottom(int index) => MoveTextToIndex(index, Texts.Count - 1);

    public static bool MoveTextByDrag(int fromIndex, int toIndex) {
        if(fromIndex < 0 || fromIndex >= Texts.Count) {
            return false;
        }

        toIndex = Mathf.Clamp(toIndex, 0, Texts.Count);

        if(fromIndex == toIndex || fromIndex == toIndex - 1) {
            return false;
        }

        var item = Texts[fromIndex];
        Texts.RemoveAt(fromIndex);

        if(fromIndex < toIndex) {
            toIndex--;
        }

        Texts.Insert(toIndex, item);
        item.gameObject.transform.SetSiblingIndex(toIndex);
        return true;
    }

    public static void Remove(int index) => DestroyText(Texts[index]);
    public static void DestroyText(OverlayerText text) {
        UnityEngine.Object.Destroy(text.gameObject);
        Texts.Remove(text);
        Refresh();
    }
    public static void Save() {
        var array = ModelUtils.WrapList(Texts.Select(ot => ot.Config).ToList());
        string textsPath = Path.Combine(Main.Mod.Path, "Texts.json");
        File.WriteAllText(textsPath, JsonConvert.SerializeObject(array, Formatting.Indented));
    }
    public static void Refresh() => Texts.ForEach(ot => ot.ApplyConfig());
    public static void Release() {
        if(!Initialized) {
            return;
        }

        Save();
        Texts = null;
        UnityEngine.Object.Destroy(OverlayerText.PCanvasObj);
        Initialized = false;
    }
}
