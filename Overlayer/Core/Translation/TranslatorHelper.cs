using Overlayer.Core.Translatior;

namespace Overlayer.Core.Translation {
    internal static class TranslatorHelper {
        // Array of fail messages corresponding to the TranslationFailState enum.
        private static readonly string[] failMessages =
        {
            "Load Language Pack",
            "Fail: Unknown Cause",
            "Fail: No valid translation was found",
            "Fail: Error Reading Directory",
            "Fail: Error loading file",
            "Fail: The file does not exist"
        };
        /// <summary>
        /// Returns the failure message corresponding to the current failure state of the Translator.
        /// </summary>
        /// <param name="translator">The Translator instance from which to retrieve the failure state.</param>
        public static string FailString(Translator translator) => failMessages[translator.GetFailAdvence()];
    }
}
