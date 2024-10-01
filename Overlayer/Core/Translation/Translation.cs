using Newtonsoft.Json;
using Overlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Overlayer.Core.Translation
{
    public class Translator
    {
        private Dictionary<string,Dictionary<string,string>> translations = new Dictionary<string,Dictionary<string,string>>();
        private List<string> languages = new List<string>();
        public string currentLanguage = "English";
        public bool Fail { get; private set; } = false;
        public bool IsLoading { get; private set; } = false;

        public Translator(string jsonFilePath)
        {
            Task.Run(async () => await LoadTranslationsAsync(jsonFilePath));
        }

        private async Task LoadTranslationsAsync(string jsonFilePath)
        {
            try
            {
                IsLoading = true;

                using(var reader = new StreamReader(jsonFilePath))
                {
                    var json = await reader.ReadToEndAsync();
                    translations = JsonConvert.DeserializeObject<Dictionary<string,Dictionary<string,string>>>(json);

                    languages = new List<string> { "English" };
                    languages.AddRange(translations.Keys);
                }

                Fail = false;
            }
            catch
            {
                translations = new Dictionary<string,Dictionary<string,string>>();
                Fail = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void SetLanguage(string language)
        {
            if(languages.Contains(language))
            {
                currentLanguage = language;
            }
        }

        public string[] GetLanguages()
        {
            return languages.ToArray();
        }

        public string Get(string key,string defaultValue)
        {
            if(Fail || IsLoading || currentLanguage == "English")
            {
                return defaultValue;
            }

            if(translations.TryGetValue(currentLanguage,out var languageTranslations))
            {
                if(languageTranslations.TryGetValue(key,out var translatedValue))
                {
                    return translatedValue;
                }
            }

            return defaultValue;
        }
    }
}
