﻿using JSON;
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
        public static async Task<TUFDifficulties> GetTUFDifficulties(string artist, string title, string author, int tiles, int bpm)
        {
            var json = await Main.HttpClient.GetStringAsync(
                    API + $"/tuf/difficulties/?{nameof(artist)}={artist}&{nameof(title)}={title}&{nameof(author)}={author}&{nameof(tiles)}={tiles}&{nameof(bpm)}={bpm}");
            var node = JsonNode.Parse(json);
            return new TUFDifficulties()
            {
                status = EnumHelper<HttpStatusCode>.Parse(node["status"]),
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
