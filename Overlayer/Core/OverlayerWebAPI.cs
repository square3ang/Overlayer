using JSON;
using Overlayer.Models;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Overlayer.Core
{
    public static class OverlayerWebAPI
    {
        public const string API =
#if DEBUG
            DEV_API;
#else
            PROD_API;
#endif
        public const string PROD_API = "https://overlayer.i-v.cc";
        public const string DEV_API = "http://localhost:7777";
        public static async Task Play() => await Main.HttpClient.GetStringAsync(API + "/play");
        public static async Task<string> Handshake() => await Main.HttpClient.GetStringAsync(API + "/handshake");
        public static async Task<Version> GetVersion() => Version.Parse(JsonNode.Parse(await Main.HttpClient.GetStringAsync(API + "/version")).Value);
        public static async Task<string> GetDiscordLink() => await Main.HttpClient.GetStringAsync(API + "/discord");
        public static async Task<string> GetDownloadLink() => await Main.HttpClient.GetStringAsync(API + "/download");
        public static async Task<long> GetHandshakeCount() => StringConverter.ToInt64(await Main.HttpClient.GetStringAsync(API + "/handshakeCount"));
        public static async Task<long> GetPlayCount() => StringConverter.ToInt64(await Main.HttpClient.GetStringAsync(API + "/playCount"));
        public static async Task<long> GetGGRequestCount() => StringConverter.ToInt64(await Main.HttpClient.GetStringAsync(API + "/adofaigg/requestCount"));
        public static async Task<long> GetTUFRequestCount() => StringConverter.ToInt64(await Main.HttpClient.GetStringAsync(API + "/tuf/requestCount"));
        public static async Task<long> GetGetGGRequestCount() => StringConverter.ToInt64(await Main.HttpClient.GetStringAsync(API + "/adofaigg/getRequestCount"));
        public static async Task<long> GetGetTUFRequestCount() => StringConverter.ToInt64(await Main.HttpClient.GetStringAsync(API + "/tuf/getRequestCount"));
        public static async Task<string> GetLanguageJson(OverlayerLanguage lang) => await Main.HttpClient.GetStringAsync(API + "/language/" + lang);
        public static async Task<double> GetGGDifficulty(string artist, string title, string author, int tiles, int bpm) =>
            StringConverter.ToDouble(
                await Main.HttpClient.GetStringAsync(
                    API + $"/adofaigg/difficulty_/?{nameof(artist)}={artist}&{nameof(title)}={title}&{nameof(author)}={author}&{nameof(tiles)}={tiles}&{nameof(bpm)}={bpm}"));
        public static async Task<TUFDifficulties> GetTUFDifficulties(string artist, string title, string author, int tiles, int bpm)
        {
            var json = await Main.HttpClient.GetStringAsync(
                    API + $"/tuf/difficulties_/?{nameof(artist)}={artist}&{nameof(title)}={title}&{nameof(author)}={author}&{nameof(tiles)}={tiles}&{nameof(bpm)}={bpm}");
            var node = JsonNode.Parse(json);
            Main.Logger.Log(json);
            return new TUFDifficulties()
            {
                status = (HttpStatusCode)node["status"].AsInt,
                diff = node["diff"].IfNotExist(-999),
                pdnDiff = node["pdnDiff"].IfNotExist(-999),
                pguDiff = node["pguDiff"].IfNotExist(-999),
                pguDiffNum = node["pguDiffNum"].IfNotExist(-999)
            };
        }
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
        [IgnoreCase]
        public class TUFDifficulties
        {
            public HttpStatusCode status = (HttpStatusCode)(-1);
            public double diff;
            public double pdnDiff;
            public string pguDiff;
            public double pguDiffNum = -999;
        }
    }
}
