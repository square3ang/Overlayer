using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Overlayer.Core.Translation;

/***********************************************************************
 * TRANSLATION SYSTEM BY KKITUT                                        *
 * ------------------------------------------------------------------- *
 * This class is designed for clarity and ease of use.                 *
 * Helpful comments are included throughout the code for reference.    *
 * Feel free to study or modify it as needed.                          *
 * Happy coding :>                                                     *
 ***********************************************************************/

/// <summary>
/// Enumeration representing the various failure states of the translation system.
/// </summary>
public enum TranslationFailState {
    /// <summary>No errors; translations loaded successfully.</summary>
    Success,
    /// <summary>An unknown error occurred.</summary>
    UnknownCause,
    /// <summary>Unknown failure. Not used</summary>
    SomeFailure,
    /// <summary>Error reading the directory containing translation files.</summary>
    ErrorReadingDirectory,
    /// <summary>No translation files were found.</summary>
    FileDoesNotExist,
    /// <summary>No valid translations were found in the files.</summary>
    NoValidTranslationFound,
}

/// <summary>
/// Translator class for managing translations.
/// Validates translation files using a specific key-value pair in each JSON file,
/// and enforces a fallback string when a translation is missing.
/// Provides detailed failure states and logging for easier debugging and troubleshooting.
/// </summary>
public class Translator {
    // Key and expected value for KTL validation.
    private readonly string KTLKey;
    private readonly string ExpectedKTLValue;

    // Dictionaries to hold translations.
    private Dictionary<string, Dictionary<string, string>> translations = new();
    private Dictionary<string, Dictionary<string, string[]>> translationsArr = new();

    /// <summary>
    /// Constant representing the fallback language code.
    /// </summary>
    public const string FALLBACK_LANGUAGE = "DEFAULT";

    /// <summary>
    /// Gets or sets the current language for translations.
    /// </summary>
    public string Language = FALLBACK_LANGUAGE;

    /// <summary>
    /// Gets the failure state of the translator.
    /// </summary>
    public TranslationFailState FailState { get; private set; } = TranslationFailState.Success;

    // Flag to indicate if logging is enabled.
    private bool useLogging;

    // List to hold log messages.
    private List<string> logStacks;

    // Initializes the logging system if logging is enabled.
    private void InitLog() {
        if(!useLogging) {
            return;
        }
        logStacks = new List<string>();
    }

    // Logs a message if logging is enabled.
    private void Log(string message) {
        if(!useLogging) {
            return;
        }
        logStacks.Add(message);
    }

    // Log prefixes for standard and exception messages.
    public const string LOG_PREFIX = "[Translator] ";
    public const string LOG_PREFIX_WARNING = "[Translator Warning] ";
    public const string LOG_PREFIX_ERROR = "[Translator Error] ";
    public const string LOG_PREFIX_EXCEPTION = "[Translator Exception] ";

    /// <summary>
    /// Gets the loading state of the translator.
    /// </summary>
    /// <returns>True if loading is in progress; otherwise, false.</returns>
    public bool IsLoading { get; private set; } = false;

    /// <summary>
    /// Checks if there was any failure during translation loading.
    /// </summary>
    /// <returns>True if there was a failure; otherwise, false.</returns>
    public bool IsFail => FailState != TranslationFailState.Success;

    /// <summary>
    /// Checks if there was a partial failure during translation loading.
    /// </summary>
    /// <returns>True if there was some failure; otherwise, false.</returns>
    public bool IsSomeFail => FailState == TranslationFailState.SomeFailure;

    /// <summary>
    /// Retrieves the log messages generated during translation loading.
    /// </summary>
    /// <returns>An array of log messages.</returns>
    public string[] Logs => logStacks.ToArray();

    /// <summary>
    /// Determines if the default language should be used.
    /// </summary>
    /// <returns>True if default language should be used; otherwise, false.</returns>
    public bool IsDefault => (IsFail && FailState != TranslationFailState.SomeFailure) || IsLoading || Language == FALLBACK_LANGUAGE;

    /// <summary>
    /// Event triggered when the translator has finished initialization.
    /// </summary>
    public event Action OnInitialize = delegate { };

    // Default KTL key and expected value constants.
    public const string DEFAULT_KTL_KEY = "0KTL";
    public const string DEFAULT_EXPECTED_KTL_VALUE = "DO_NOT_TRANSLATE_THIS_KEY!";

    /// <summary>
    /// Initializes a new instance of the Translator class and starts loading translations asynchronously.
    /// </summary>
    /// <param name="ktlKey">The key for the KTL value.</param>
    /// <param name="expectedKtlValue">The expected value for the 0KTL key.</param>
    /// <param name="useLogging">Indicates whether to enable logging.</param>
    public Translator(string ktlKey = DEFAULT_KTL_KEY, string expectedKtlValue = DEFAULT_EXPECTED_KTL_VALUE, bool useLogging = true) {
        KTLKey = ktlKey;
        ExpectedKTLValue = expectedKtlValue;
        this.useLogging = useLogging;
    }

    /* 
     * What is the 'KTL' key?
     * The 'KTL' key is a special validation entry included in every translation JSON file.
     * It ensures that the file is a valid translation file by containing a fixed, non-translatable value
     * (for example, "DO_NOT_TRANSLATE_THIS_KEY!").
     * This mechanism allows the Translator class to verify file authenticity before loading translations.
     * 
     * And... what does KTL mean?
     * KTL stands for "Kkitut Translation Language".
     * I just made it up, lol.
     */

    /// <summary>
    /// Loads translations from JSON files in the specified directory asynchronously.
    /// </summary>
    /// <param name="baseLangFolderPath">The path to the folder containing the language JSON files.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    internal async Task Load(string baseLangFolderPath) {
        if(IsLoading) {
            return;
        }

        IsLoading = true;

        // Initialize logging
        InitLog();

        // Log the start of the loading process.
        Log($"{LOG_PREFIX}Starting to load translations from: {baseLangFolderPath}");

        // Reset translations before loading.
        translations = new Dictionary<string, Dictionary<string, string>>();
        translationsArr = new Dictionary<string, Dictionary<string, string[]>>();

        // Array to hold file paths.
        string[] files = Array.Empty<string>();

        // Log the reading of translation files.
        Log($"{LOG_PREFIX}Reading translation files...");

        try {
            // Retrieve all JSON files from the specified directory.
            files = Directory.GetFiles(baseLangFolderPath, "*.json");
        } catch(Exception e) {
            // If there's an error reading the directory, set failure state.
            FailState = TranslationFailState.ErrorReadingDirectory;
            Log($"{LOG_PREFIX_ERROR}Error reading directory: {baseLangFolderPath}");
            Log($"[Translator Exception] {e.GetType().Name}: {e.Message}");
            IsLoading = false;
            OnInitialize.Invoke();
            return;
        }

        // Log the number of files found.
        Log($"{LOG_PREFIX}Found {files.Length} translation files.");

        if(files.Length == 0) {
            // No files found, set failure state.
            FailState = TranslationFailState.FileDoesNotExist;
            Log($"{LOG_PREFIX_WARNING}No translation files found");
            IsLoading = false;
            OnInitialize.Invoke();
            return;
        }
        foreach(var file in files) {
            // Attempt to read each file.
            StreamReader reader;
            try {
                reader = new StreamReader(file);
            } catch(Exception e) {
                // If there's an error loading the file, set failure state and continue to next file.
                FailState = TranslationFailState.SomeFailure;
                Log($"{LOG_PREFIX_ERROR}Error loading file: {file}");
                Log($"{LOG_PREFIX_EXCEPTION}{e.GetType().Name}: {e.Message}");
                continue;
            }

            // Read the file content asynchronously.
            string jsonString = string.Empty;
            try {
                jsonString = await reader.ReadToEndAsync();
            } catch(Exception e) {
                // If there's an error reading the file, set failure state and continue to next file.
                FailState = TranslationFailState.SomeFailure;
                Log($"{LOG_PREFIX_ERROR}Error reading file: {file}");
                Log($"{LOG_PREFIX_EXCEPTION}{e.GetType().Name}: {e.Message}");
                continue;
            } finally {
                reader.Close();
            }

            // Parse the JSON content.
            JObject jsonObject;
            try {
                jsonObject = JObject.Parse(jsonString);
            } catch(Exception e) {
                // If JSON is invalid, set failure state and continue to next file.
                FailState = TranslationFailState.SomeFailure;
                Log($"{LOG_PREFIX_ERROR}Invalid JSON format in file: {file}");
                Log($"{LOG_PREFIX_EXCEPTION}{e.GetType().Name}: {e.Message}");
                continue;
            }

            // Iterate through each property in the JSON object.
            foreach(var property in jsonObject.Properties()) {
                // Ensure the property value is a JObject.
                if(property.Value is not JObject block) {
                    FailState = TranslationFailState.SomeFailure;
                    Log($"{LOG_PREFIX_ERROR}Block is not an object in file: {file}, block: {property.Name}");
                    continue;
                }

                // Validate the presence and correctness of the KTL key.
                if(block.TryGetValue(KTLKey, out var ktToken) == false || ktToken.ToString() != ExpectedKTLValue) {
                    Log($"{LOG_PREFIX}Invalid or missing {DEFAULT_KTL_KEY} in file: {file}, block: {property.Name}, passing");
                    continue;
                }

                // Remove the KTL key from the block to avoid processing it further.
                block.Remove(KTLKey);

                // Separate string and array translations.
                var stringDict = new Dictionary<string, string>();
                var arrayDict = new Dictionary<string, string[]>();

                // Process each key-value pair in the block.
                foreach(var kv in block) {
                    if(kv.Value is JArray arr) {
                        arrayDict[kv.Key] = arr.Select(v => v.ToString()).ToArray();
                    } else {
                        stringDict[kv.Key] = kv.Value?.ToString() ?? "";
                    }
                }

                // Store valid translations.
                if(stringDict.Count > 0) {
                    translations[property.Name] = stringDict;
                }
                if(arrayDict.Count > 0) {
                    translationsArr[property.Name] = arrayDict;
                }
            }
        }

        // Determine overall state after processing.
        if(translations.Count == 0 && translationsArr.Count == 0) {
            FailState = TranslationFailState.NoValidTranslationFound;
            Log($"{LOG_PREFIX_WARNING}No valid translations were found in any files.");
        } else if(FailState != TranslationFailState.SomeFailure) {
            FailState = TranslationFailState.Success;
        }

        // Sort translations by language code.
        translations = translations
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
        translationsArr = translationsArr
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        // Log the completion of the loading process.
        Log($"{LOG_PREFIX}Finished loading translations.");

        // Invoke the OnInitialize event safely.
        try {
            OnInitialize.Invoke();
        } catch(Exception e) {
            // Log any exceptions that occur during the event invocation.
            Log($"{LOG_PREFIX_EXCEPTION}Exception during OnInitialize event: {e.GetType().Name}: {e.Message}");
        } finally {
            // Set loading state to false.
            IsLoading = false;
        }
    }

    /// <summary>
    /// Retrieves the translation for a specified key in the current language.
    /// </summary>
    /// <param name="key">The key for the translation.</param>
    /// <param name="defaultValue">The default value to return if translation is not found.</param>
    /// <returns>The translated value or the default value if not found.</returns>
    public string Get(string key, string defaultValue) {
        if(IsDefault) {
            return defaultValue;
        }

        // Check if the translations contain the current language.
        if(translations.TryGetValue(Language, out var langDict)) {
            // Attempt to retrieve the translated value using the provided key.
            if(langDict.TryGetValue(key, out var val)) {
                return val;
            }
        }

        // Return the default value if translation is not found.
        return defaultValue;
    }

    /// <summary>
    /// Retrieves the translation for a specified key in a given language.
    /// </summary>
    /// <param name="key">The key for the translation.</param>
    /// <param name="language">The language code.</param>
    /// <param name="defaultValue">The default value to return if translation is not found.</param>
    /// <returns>The translated value or the default value if not found.</returns>
    public string GetForLanguage(string key, string language, string defaultValue) {
        // If the specified language is null, empty, or the fallback language, return the default value.
        if(string.IsNullOrEmpty(language) || language == FALLBACK_LANGUAGE) {
            return defaultValue;
        }

        // Check if the translations contain the current language.
        if(translations.TryGetValue(language, out var langDict)) {
            // Attempt to retrieve the translated value using the provided key.
            if(langDict.TryGetValue(key, out var val)) {
                return val;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Retrieves the list of available languages for translation.
    /// </summary>
    /// <returns>An array of language codes.</returns>
    public string[] GetLanguages() {
        // Initialize a list to hold the language codes.
        List<string> languages = new();

        // If there was a failure, add the fallback language to the first position.
        if(IsFail) {
            languages.Add(FALLBACK_LANGUAGE);
        }

        // Get the language codes from the translations dictionary.
        languages.AddRange(translations.Keys);

        // Return the list of languages as an array.
        return languages.ToArray();
    }

    /// <summary>
    /// Retrieves the native names of available languages for translation.
    /// </summary>
    /// <returns>An array of native language names.</returns>
    public string[] GetLanguageNativeNames() {
        // Initialize a list to hold the native names.
        List<string> names = new();

        // If there was a failure, add the fallback language to the first position.
        if(IsFail) {
            names.Add(FALLBACK_LANGUAGE);
        }

        // Get the native names of languages from the translations dictionary.
        names.AddRange(translations.Keys

            // Order the languages alphabetically (case-insensitive) and select their native names.
            .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)

            // Map each language code to its native name using the "0NATIVELANG" key.
            .Select(lang => GetForLanguage("0NATIVELANG", lang, lang))

            // Convert the result to a list.
            .ToList()
        );

        // Return the list of native names as an array.
        return names.ToArray();
    }

    /// <summary>
    /// Retrieves a specific element from a translation array for the current language.
    /// </summary>
    /// <param name="key">The key for the translation.</param>
    /// <param name="index">The index of the element to retrieve.</param>
    /// <param name="defaultValue">The default value to return if translation is not found.</param>
    /// <returns>The translated value or the default value if not found.</returns>
    public string GetArr(string key, int index, string defaultValue) {
        if(IsDefault) {
            return defaultValue;
        }

        // Try to get the array dictionary for the current language
        if(translationsArr.TryGetValue(Language, out var lang)) {
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
        if(IsDefault) {
            return 0;
        }

        // Try to get the array dictionary for the current language
        if(translationsArr.TryGetValue(Language, out var lang)) {
            // Return the length of the array if key exists
            if(lang.TryGetValue(key, out var values)) {
                return values.Length;
            }
        }
        return 0;
    }

    /// <summary>
    /// Releases resources used by the Translator.
    /// </summary>
    public void Release() {
        translations.Clear();
        translationsArr.Clear();
        logStacks?.Clear();
        OnInitialize = delegate { };
    }
}

