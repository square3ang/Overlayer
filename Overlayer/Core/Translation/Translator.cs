using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Overlayer.Core.Translatior {
    // Enum to represent various translation loading states.
    public enum TranslationFailState {
        // No failure.
        Success = 0,
        // Unknown error occurred.
        UnknownCause = 1,
        // No valid translation was found.
        NoValidTranslationFound = 2,
        // Error reading the directory.
        ErrorReadingDirectory = 3,
        // Error loading the file.
        ErrorLoadingFile = 4,
        // The file does not exist.
        FileDoesNotExist = 5
    }

    public class Translator {
        // Readonly fields for KTL key and its expected value.
        private readonly string KTLKey;
        private readonly string ExpectedKTLValue;

        private Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string[]>> translationsArr = new Dictionary<string, Dictionary<string, string[]>>();

        // Private backing field for current language.
        private string currentLanguage = "Default";
        /// <summary>   
        /// Gets or sets the current language.
        /// </summary>
        public string CurrentLanguage {
            get { return currentLanguage; }
            set { currentLanguage = value; }
        }

        // Field to store current failure state.
        private TranslationFailState failState = TranslationFailState.Success;

        private bool IsLoading = true;

        // Static event to signal when the language initialization is complete.
        public event Action OnInitialize = delegate { };

        /// <summary>
        /// Initializes a new instance of the Translator class and starts loading translations asynchronously.
        /// </summary>
        /// <param name="ktlKey">The key for the KTL value (default: "0KTL").</param>
        /// <param name="expectedKtlValue">The expected value for the 0KTL key (default: "DO_NOT_TRANSLATE_THIS").</param>
        public Translator(string ktlKey = "0KTL", string expectedKtlValue = "DO_NOT_TRANSLATE_THIS") {
            KTLKey = ktlKey; // Assign the KTL key.
            ExpectedKTLValue = expectedKtlValue; // Assign the expected KTL value.
        }

        /// <summary>
        /// Loads translations from JSON files in the specified directory asynchronously.
        /// </summary>
        /// <param name="baseLangFolderPath">The path to the folder containing the language JSON files.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        internal async Task LoadTranslationsAsync(string baseLangFolderPath) {
            try {
                IsLoading = true;
                string[] files = Array.Empty<string>();

                try {
                    // Retrieve all JSON files from the specified directory.
                    files = Directory.GetFiles(baseLangFolderPath, "*.json");
                } catch {
                    // If there's an error reading the directory, set failure state.
                    failState = TranslationFailState.ErrorReadingDirectory;
                    return;
                }
                if(files.Length == 0) {
                    // No files found, set failure state.
                    failState = TranslationFailState.FileDoesNotExist;
                    return;
                }
                foreach(var file in files) {
                    try {
                        using var reader = new StreamReader(file);
                        var jsonString = await reader.ReadToEndAsync();
                        var jsonObject = JObject.Parse(jsonString);
                        var validTranslations = new Dictionary<string, Dictionary<string, string>>();
                        var validTranslationsArr = new Dictionary<string, Dictionary<string, string[]>>();

                        // Iterate through each property in the JSON object.
                        foreach(var property in jsonObject.Properties()) {
                            string blockName = property.Name;
                            var blockValue = property.Value;

                            // Check if the block contains the KTL key.
                            if(blockValue[KTLKey] != null) {
                                string ktValue = blockValue[KTLKey].ToString();
                                // Verify if the KTL value matches the expected value.
                                if(ktValue == ExpectedKTLValue) {
                                    // Convert block to dictionary for strings and arrays separately.
                                    var dict = new Dictionary<string, string>();
                                    var dictArr = new Dictionary<string, string[]>();

                                    foreach(var kv in (JObject)blockValue) {
                                        if(kv.Key == KTLKey)
                                            continue; // Skip validation key

                                        if(kv.Value is JArray arr)
                                            dictArr[kv.Key] = arr.Select(v => v.ToString()).ToArray();
                                        else
                                            dict[kv.Key] = kv.Value?.ToString() ?? "";
                                    }

                                    if(dict.Count > 0)
                                        validTranslations[blockName] = dict;
                                    if(dictArr.Count > 0)
                                        validTranslationsArr[blockName] = dictArr;
                                }
                            }
                        }

                        // Merge valid translations into main dictionaries.
                        foreach(var vt in validTranslations)
                            translations[vt.Key] = vt.Value;

                        foreach(var vta in validTranslationsArr)
                            translationsArr[vta.Key] = vta.Value;
                    } catch {
                        // If there's an error loading the file, set failure state.
                        failState = TranslationFailState.ErrorLoadingFile;
                    }
                }

                // Determine overall state after processing.
                if(translations.Count == 0 && translationsArr.Count == 0)
                    failState = TranslationFailState.NoValidTranslationFound;
                else
                    failState = TranslationFailState.Success;
            } catch {
                // Reset translations on unknown failure.
                translations = new Dictionary<string, Dictionary<string, string>>();
                translationsArr = new Dictionary<string, Dictionary<string, string[]>>();
                failState = TranslationFailState.UnknownCause;
            } finally {
                IsLoading = false;
                OnInitialize?.Invoke();
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
        public bool GetFail() => failState != TranslationFailState.Success;

        /// <summary>
        /// Determines if the default language should be used.
        /// </summary>
        /// <returns>True if default language should be used; otherwise, false.</returns>
        public bool GetWillDefault() => failState != TranslationFailState.Success || IsLoading || CurrentLanguage == "Default";

        /// <summary>
        /// Retrieves the current failure state code as an integer.
        /// </summary>
        /// <returns>The integer value of the current failure state.</returns>
        public int GetFailAdvence() => (int)failState;

        /// <summary>
        /// Retrieves the list of available languages for translation.
        /// </summary>
        /// <returns>An array of language codes.</returns>
        public string[] GetLanguages() {
            var languages = translations.Keys.ToList();
            // If no languages are found, or in failure/loading state, add "Default".
            if(languages.Count <= 0 || GetWillDefault()) {
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
        public string Get(string key, string defaultValue) {
            // If loading is in progress or there's a failure, return the default value.
            if(GetWillDefault()) {
                return defaultValue;
            }

            // Check if the translations contain the current language.
            if(translations.TryGetValue(CurrentLanguage, out var languageTranslations)) {
                // Attempt to retrieve the translated value using the provided key.
                if(languageTranslations.TryGetValue(key, out var translatedValue)) {
                    return translatedValue; // Return the translated value if found.
                }
            }
            return defaultValue; // Return the default value if translation is not found.
        }

        /// <summary>
        /// Retrieves a specific element from a translation array for the current language.
        /// </summary>
        /// <param name="key">The key for the translation.</param>
        /// <param name="index">The index of the element to retrieve.</param>
        /// <param name="defaultValue">The default value to return if translation is not found.</param>
        /// <returns>The translated value or the default value if not found.</returns>
        public string GetArr(string key, int index, string defaultValue) {
            if(GetWillDefault()) {
                return defaultValue;
            }

            // Try to get the array dictionary for the current language
            if(translationsArr.TryGetValue(CurrentLanguage, out var lang)) {
                // Try to get the string array for the given key
                if(lang.TryGetValue(key, out var values)) {
                    // Return the requested element if index is valid
                    if(index >= 0 && index < values.Length) {
                        return values[index];
                    }
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Retrieves the number of elements in a translation array for a given key in the current language.
        /// </summary>
        /// <param name="key">The key for the translation.</param>
        /// <returns>The count of elements for the key, or 0 if not found or translations are not ready.</returns>
        public int GetArrCount(string key) {
            if(GetWillDefault()) {
                return 0;
            }

            // Try to get the array dictionary for the current language
            if(translationsArr.TryGetValue(CurrentLanguage, out var lang)) {
                // Return the length of the array if key exists
                if(lang.TryGetValue(key, out var values)) {
                    return values.Length;
                }
            }
            return 0;
        }
    }
}

