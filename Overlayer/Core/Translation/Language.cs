using JSON;
using Overlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var json = await OverlayerWebAPI.GetLanguageJson(Lang);
            JsonNode node = JsonNode.Parse(json);
            foreach (var pair in node.KeyValues)
                pairs.Add(pair.Key, pair.Value);
            OnInitialize();
            Initialized = true;
            if (updateMode) ActivateUpdateMode();
        }
        public string this[string key]
        {
            get => pairs.TryGetValue(key, out var value) ? value : key;
            set => pairs[key] = value;
        }
        public void ActivateUpdateMode()
        {
            updateMode = true;
            if (Initialized)
            {
                foreach (var key in pairs.Keys.ToList())
                    pairs[key] = "Update!Update!Update!";
            }
        }
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
