using JSON;
using Overlayer.Models;
using System;
using System.Threading.Tasks;

namespace Overlayer.Core
{
    public static class OverlayerWebAPI
    {
        public const string PROD_API = "https://overlayer.c3nb.net";
        public const string DEV_API = "https://overlayerdev.c3nb.net";
        public static async Task<Version> GetVersion() => Version.Parse(JsonNode.Parse(await Main.HttpClient.GetStringAsync(DEV_API + "/version")).Value);
        public static async Task<string> GetDiscordLink() => await Main.HttpClient.GetStringAsync(DEV_API + "/discord");
        public static async Task<string> GetDownloadLink() => await Main.HttpClient.GetStringAsync(DEV_API + "/download");
        public static async Task<string> GetLanguageJson(OverlayerLanguage lang) => await Main.HttpClient.GetStringAsync(DEV_API + "/language/" + lang);
    }
}
