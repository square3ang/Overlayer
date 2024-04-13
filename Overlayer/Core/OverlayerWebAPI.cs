using JSON;
using Overlayer.Models;
using System;
using System.Threading.Tasks;
using Overlayer.Utils;

namespace Overlayer.Core
{
    public static class OverlayerWebAPI
    {
        public const string API = DEV_API;
        public const string PROD_API = "https://overlayer.c3nb.net";
        public const string DEV_API = "http://localhost:7777";
        public static async Task<string> Handshake() => await Main.HttpClient.GetStringAsync(API + "/handshake");
        public static async Task<Version> GetVersion() => Version.Parse(JsonNode.Parse(await Main.HttpClient.GetStringAsync(API + "/version")).Value);
        public static async Task<string> GetDiscordLink() => await Main.HttpClient.GetStringAsync(API + "/discord");
        public static async Task<string> GetDownloadLink() => await Main.HttpClient.GetStringAsync(API + "/download");
        public static async Task<string> GetLanguageJson(OverlayerLanguage lang) => await Main.HttpClient.GetStringAsync(API + "/language/" + lang);
        public static async Task<double> GetGGDifficulty(string artist, string title, string author, int tiles, int bpm) => 
            StringConverter.ToDouble(
                await Main.HttpClient.GetStringAsync( 
                    API + $"/adofaigg/difficulty/?{nameof(artist)}={artist}&{nameof(title)}={title}&{nameof(author)}={author}&{nameof(tiles)}={tiles}&{nameof(bpm)}={bpm}"));
    }
}
