using JSON;
using Overlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Overlayer.Core.Translation
{
    public class Language
    {
        private static Language Korean;
        private static Language English;
        public bool Initialized = false;
        public OverlayerLanguage Lang;
        private bool updateMode = false;
        private Dictionary<string, string> pairs = new Dictionary<string, string>();
        public static event Action OnInitialize = delegate { };
        Language(OverlayerLanguage lang)
        {
            Lang = lang;
            Download();
        }
        async void Download()
        {
            if (Initialized) return;
            string json = File.ReadAllText(Path.Combine(Main.Mod.Path,$"{Lang}.json"));
            Main.Logger.Log($"Resolved Language Json From Local File ({Lang})");
            JsonNode node = JsonNode.Parse(json);
            foreach (var pair in node.KeyValues)
                pairs.Add(pair.Key, pair.Value);
            await Task.Delay(100);
            OnInitialize();
            Initialized = true;
        }
        public string this[string key]
        {
            get => pairs.TryGetValue(key, out var value) ? value : key;
            set => pairs[key] =  value;
        }
        /*
        public void ActivateUpdateMode()
        {
            updateMode = true;
            if (Initialized)
            {
                foreach (var key in pairs.Keys.ToList())
                    pairs[key] = update;
            }
        }
        */
        public static Language GetLangauge(OverlayerLanguage lang)
        {
            switch (lang)
            {
                case OverlayerLanguage.Korean:
                    return Korean ??= new Language(OverlayerLanguage.Korean);
                case OverlayerLanguage.English:
                    return English ??= new Language(OverlayerLanguage.English);
                default: return null;
            }
        }
    }
}
