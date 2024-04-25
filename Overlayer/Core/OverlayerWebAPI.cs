using JSON;
using Overlayer.Models;
using System;
using System.Threading.Tasks;
using Overlayer.Utils;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

namespace Overlayer.Core
{
    public static class OverlayerWebAPI
    {
        public const string API = PROD_API;
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
        public static async Task<double> GetTUFDifficulty(string artist, string title, string author, int tiles, int bpm) =>
            StringConverter.ToDouble(
                await Main.HttpClient.GetStringAsync(
                    API + $"/tuf/difficulty/?{nameof(artist)}={artist}&{nameof(title)}={title}&{nameof(author)}={author}&{nameof(tiles)}={tiles}&{nameof(bpm)}={bpm}"));
        public static async Task<double> GetPredDifficulty(byte[] adofai)
        {
            Main.Logger.Log($"Requesting Predict Level..");
            var array = new ByteArrayContent(adofai);
            array.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/octet-stream");
            var response = await Main.HttpClient.PostAsync(API + $"/predictRaw", array);
            response.EnsureSuccessStatusCode();
            var predictedDifficultyRaw = await response.Content.ReadAsByteArrayAsync();
            var predictedDifficulty = Encoding.UTF8.GetString(predictedDifficultyRaw);
            var result = StringConverter.ToDouble(predictedDifficulty);
            Main.Logger.Log($"Response: {result}");
            return result;
        }
    }
}
