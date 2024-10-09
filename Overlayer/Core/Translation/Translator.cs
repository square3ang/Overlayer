using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Overlayer.Core.Translatior
{
    public class Translator
    {
        // Readonly fields for KTL key and its expected value, which can be customized at instantiation.
        private readonly string KTLKey;
        private readonly string ExpectedKTLValue;

        private Dictionary<string,Dictionary<string,string>> translations = new Dictionary<string,Dictionary<string,string>>();
        public string CurrentLanguage = "Default";
        private int Fail = 0;

        // Failure states:
        // 0: No failure occurred; language pack can be loaded successfully.
        // -1: Fail to load JSON: Unknown cause.
        // 1: Fail to load JSON: The file exists, but no valid translation was found.
        // 2: Fail to load JSON: Error reading directory.
        // 3: Fail to load JSON: Error loading file.
        // 4: Fail to load JSON: The file does not exist.
        private bool IsLoading = true;

        // Static event to signal when the language initialization is complete.
        public event Action OnInitialize = delegate { };

        /// <summary>
        /// Initializes a new instance of the Translator class and starts loading translations asynchronously.
        /// </summary>
        /// <param name="jsonFilePath">The path to the directory containing the JSON translation files.</param>
        /// <param name="ktlKey">The key for the KTL value (default: "KTL").</param>
        /// <param name="expectedKtlValue">The expected value for the KTL key (default: "For translation file verification").</param>
        public Translator(string ktlKey = "KTL",string expectedKtlValue = "For translation file verification")
        {
            KTLKey = ktlKey; // Assign the KTL key.
            ExpectedKTLValue = expectedKtlValue; // Assign the expected KTL value.
        }

        /// <summary>
        /// Loads translations from JSON files in the specified directory asynchronously.
        /// </summary>
        /// <param name="baseLangFolderPath">The path to the folder containing the language JSON files.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        internal async Task LoadTranslationsAsync(string baseLangFolderPath)
        {
            try
            {
                IsLoading = true;
                string[] files = Array.Empty<string>();

                try
                {
                    // Retrieve all JSON files from the specified directory.
                    files = Directory.GetFiles(baseLangFolderPath,"*.json");
                }
                catch
                {
                    // If there's an error reading the directory, set failure state to 2.
                    Fail = 2;
                    return;
                }
                if(files.Count() == 0)
                {
                    Fail = 4;
                    return;
                }
                foreach(var file in files)
                {
                    try
                    {
                        using var reader = new StreamReader(file);
                        // Read the JSON content asynchronously.
                        var jsonString = await reader.ReadToEndAsync();
                        var jsonObject = JObject.Parse(jsonString);
                        var validTranslations = new Dictionary<string,Dictionary<string,string>>();
                        // Iterate through each property in the JSON object.
                        foreach(var property in jsonObject.Properties())
                        {
                            string blockName = property.Name;
                            var blockValue = property.Value;

                            // Check if the block contains the KTL key.
                            if(blockValue[KTLKey] != null)
                            {
                                string ktValue = blockValue[KTLKey].ToString();
                                // Verify if the KTL value matches the expected value.
                                if(ktValue == ExpectedKTLValue)
                                {
                                    // Add valid translations to the dictionary.
                                    validTranslations[blockName] = blockValue.ToObject<Dictionary<string,string>>();
                                }
                            }
                        }
                        // Add the valid translations to the main translations dictionary.
                        foreach(var validTranslation in validTranslations)
                        {
                            translations[validTranslation.Key] = validTranslation.Value;
                        }
                    }
                    catch
                    {
                        // If there's an error loading the file, set failure state to 3.
                        Fail = 3;
                    }
                }

                // Determine the overall failure state after processing all files.
                if(translations.Count == 0)
                {
                    Fail = 1; // No valid translations found.
                }
                else
                {
                    Fail = 0; // Translations loaded successfully.
                }
            }
            catch
            {
                // Reset translations on an unknown failure.
                translations = new Dictionary<string,Dictionary<string,string>>();
                Fail = -1;
            }
            finally
            {
                IsLoading = false; // Set loading state to false when finished.
                OnInitialize?.Invoke(); // Trigger the OnInitialize event when loading is complete.
            }
        }

        /// <summary>
        /// Gets the loading state of the translator.
        /// </summary>
        /// <returns>True if loading is in progress; otherwise, false.</returns>
        public bool GetLoading() => IsLoading;

        /// <summary>
        /// Checks if there was any failure during translation loading.
        /// </summary>
        /// <returns>True if there was a failure; otherwise, false.</returns>
        public bool GetFail() => Fail != 0;

        /// <summary>
        /// Retrieves the current failure state code.
        /// </summary>
        /// <returns>The failure state code.</returns>
        public int GetFailAdvence() => Fail;

        /// <summary>
        /// Retrieves the list of available languages for translation.
        /// </summary>
        /// <returns>An array of language codes.</returns>
        public string[] GetLanguages()
        {
            var languages = translations.Keys.ToList();
            // If no languages are found, add "Default".
            if(languages.Count <= 0 || Fail != 0 || IsLoading || CurrentLanguage == "Default")
            {
                languages.Add("Default");
            }

            return languages.ToArray(); // Return the list of languages as an array.
        }

        /// <summary>
        /// Retrieves the translation for a specified key in the current language.
        /// </summary>
        /// <param name="key">The key for the translation.</param>
        /// <param name="defaultValue">The default value to return if translation is not found.</param>
        /// <returns>The translated value or the default value if not found.</returns>
        public string Get(string key,string defaultValue)
        {
            // If loading is in progress or there's a failure, return the default value.
            if(Fail != 0 || IsLoading || CurrentLanguage == "Default")
            {
                return defaultValue;
            }

            string languageCode = CurrentLanguage.ToString();

            // Check if the translations contain the current language.
            if(translations.TryGetValue(languageCode,out var languageTranslations))
            {
                // Attempt to retrieve the translated value using the provided key.
                if(languageTranslations.TryGetValue(key,out var translatedValue))
                {
                    return translatedValue; // Return the translated value if found.
                }
            }
            //Main.Logger.Log(defaultValue);

            return defaultValue; // Return the default value if translation is not found.
        }
    }
}
