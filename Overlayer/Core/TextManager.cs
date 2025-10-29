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
        public static void MoveTextUp(int index) {
            if(index <= 0 || index >= Texts.Count) {
                return;
            }
            var tmp = Texts[index - 1];
            Texts[index - 1] = Texts[index];
            Texts[index] = tmp;

            var go = Texts[index].gameObject;
            var goPrev = Texts[index - 1].gameObject;
            int prevSibling = goPrev.transform.GetSiblingIndex();
            goPrev.transform.SetSiblingIndex(go.transform.GetSiblingIndex());
            go.transform.SetSiblingIndex(prevSibling);

            Refresh();
        }
        public static void MoveTextDown(int index) {
            if(index < 0 || index >= Texts.Count - 1) {
                return;
            }
            var tmp = Texts[index + 1];
            Texts[index + 1] = Texts[index];
            Texts[index] = tmp;

            var go = Texts[index].gameObject;
            var goNext = Texts[index + 1].gameObject;
            int nextSibling = goNext.transform.GetSiblingIndex();
            goNext.transform.SetSiblingIndex(go.transform.GetSiblingIndex());
            go.transform.SetSiblingIndex(nextSibling);

            Refresh();
        }
        public static void MoveTextToTop(int index) {
            if(index <= 0 || index >= Texts.Count) {
                return;
            }
            var item = Texts[index];
            Texts.RemoveAt(index);
            Texts.Insert(0, item);

            item.gameObject.transform.SetSiblingIndex(0);

            Refresh();
        }
        public static void MoveTextToBottom(int index) {
            if(index < 0 || index >= Texts.Count - 1) {
                return;
            }

            var item = Texts[index];
            Texts.RemoveAt(index);
            Texts.Add(item);

            item.gameObject.transform.SetSiblingIndex(item.gameObject.transform.parent.childCount - 1);

            Refresh();
        }
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
