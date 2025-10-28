using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Overlayer.Models;
using Overlayer.Unity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Overlayer.Core
{
    public static class TextManager
    {
        public static bool Initialized { get; private set; }
        public static int Count => Texts.Count;
        private static List<OverlayerText> Texts;
        public static void Initialize() {
            if(Initialized) {
                return;
            }

            Texts = new List<OverlayerText>();
            string textsPath = Path.Combine(Main.Mod.Path, "Texts.json");
            List<TextConfig> configs = new List<TextConfig>();

            if(File.Exists(textsPath)) {
                var content = File.ReadAllText(textsPath);
                if(!string.IsNullOrWhiteSpace(content)) {
                    var token = JToken.Parse(content);

                    if(token.Type == JTokenType.Array) {
                        foreach(var item in (JArray)token) {
                            if(item.Type == JTokenType.Object)
                                configs.Add(TextConfigImporter.Import((JObject)item));
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
        public static OverlayerText CreateText(TextConfig config)
        {
            if (string.IsNullOrEmpty(config.Name))
                config.Name = (Count + 1).ToString();
            GameObject go = new GameObject($"OverlayerText_{config.Name}");
            var text = go.AddComponent<OverlayerText>();
            text.Init(config);
            Texts.Add(text);
            return text;
        }
        public static OverlayerText Get(int index) {
            if(index < 0 || index >= Texts.Count) {
                return null;
            }
            return Texts[index];
        }
        public static OverlayerText Find(TextConfig configRef) => Texts.Find(ot => ReferenceEquals(ot.Config, configRef));
        public static void Remove(int index) => DestroyText(Texts[index]);
        public static void DestroyText(OverlayerText text)
        {
            UnityEngine.Object.Destroy(text.gameObject);
            Texts.Remove(text);
            Refresh();
        }
        public static void Save() {
            var array = ModelUtils.WrapList(Texts.Select(ot => ot.Config).ToList());
            string textsPath = Path.Combine(Main.Mod.Path, "Texts.json");
            File.WriteAllText(textsPath, JsonConvert.SerializeObject(array, Formatting.Indented));
        }
        public static void Refresh()
        {
            Texts.ForEach(ot => ot.ApplyConfig());
        }
        public static void Release()
        {
            if (!Initialized) return;
            Save();
            Texts = null;
            UnityEngine.Object.Destroy(OverlayerText.PCanvasObj);
            Initialized = false;
        }
    }
}
