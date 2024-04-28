using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Overlayer.WebAPI.Core.TUF;
using JSON;
using Overlayer.WebAPI.Core;
using Overlayer.WebAPI.Core.TUF.Types;
using Overlayer.WebAPI.Core.Utils;
using System.Net;

namespace Overlayer.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TUFController : ControllerBase
    {
        public static long RequestCount = 0;
        public const string API = "https://be.tuforums.com";
        public const string HEADER_LEVELS = "/levels";
        [HttpGet("difficulty")]
        public async Task<double> GetDifficulty([FromQuery] string artist, [FromQuery] string title, [FromQuery] string author, [FromQuery] int tiles, [FromQuery] int bpm)
        {
            Console.WriteLine($"[TUFController] Received artist:{artist},title:{title},author:{author},tiles:{tiles},bpm:{bpm}");
            try
            {
                var level = await GetLevel(artist, title, author, tiles, bpm);
                if (level == null)
                    Console.WriteLine($"[TUFController] Cannot Find Level!! (artist:{artist},title:{title},author:{author},tiles:{tiles},bpm:{bpm})");
                else Console.WriteLine($"[TUFController] Found Level! ({level.id})");
                return await Task.FromResult(level?.pguDiffNum ?? -404);
            }
            catch
            {
                Console.WriteLine($"TUFRequest Error!! (artist:{artist},title:{title},author:{author},tiles:{tiles},bpm:{bpm})");
                return await Task.FromResult(-500);
            }
        }
        [HttpGet("requestCount")]
        public async Task<long> GetTUFRequestCount() => await Task.FromResult(RequestCount);
        [HttpGet("difficulties")]
        public async Task<string> GetDifficulties([FromQuery] string artist, [FromQuery] string title, [FromQuery] string author, [FromQuery] int tiles, [FromQuery] int bpm)
        {
            Console.WriteLine($"[TUFController] Received artist:{artist},title:{title},author:{author},tiles:{tiles},bpm:{bpm}");
            var node = JsonNode.Empty;
            try
            {
                var level = await GetLevel(artist, title, author, tiles, bpm);
                if (level == null)
                    Console.WriteLine($"[TUFController] Cannot Find Level!! (artist:{artist},title:{title},author:{author},tiles:{tiles},bpm:{bpm})");
                else Console.WriteLine($"[TUFController] Found Level! ({level.id})");
                if (level != null)
                {
                    node["status"] = (int)HttpStatusCode.OK;
                    node[nameof(level.diff)] = level.diff;
                    node[nameof(level.pdnDiff)] = level.pdnDiff;
                    node[nameof(level.pguDiff)] = level.pguDiff;
                    node[nameof(level.pguDiffNum)] = level.pguDiffNum;
                }
                else node["status"] = (int)HttpStatusCode.NotFound;
                return await Task.FromResult(node.ToString());
            }
            catch
            {
                node["status"] = (int)HttpStatusCode.InternalServerError;
                Console.WriteLine($"TUFRequest Error!! (artist:{artist},title:{title},author:{author},tiles:{tiles},bpm:{bpm})");
                return await Task.FromResult(node.ToString());
            }
        }
        public static async Task<Level?> GetLevel(string artist, string title, string author, int tiles, int bpm, params Parameter[] ifFailedWith)
        {
            Response<Level> result = await TUFRequest<Level>(HEADER_LEVELS, ActualParams(-1));
            for (int i = 0; result.count <= 0 && i <= 2; i++)
                result = await TUFRequest<Level>(HEADER_LEVELS, ActualParams(i));
            if (result.count <= 0) return null;
            if (result.count > 1)
                Console.WriteLine($"[TUFController] Found {result.count}'s Levels!! Use First Level..");
            return result.results[0];
            Parameters ActualParams(int level = -1, params Parameter[] with)
            {
                List<Parameter> parameters = new List<Parameter>(with.Concat(ifFailedWith));
                string nartist = artist.TrimEx();
                string ntitle = title.TrimEx();
                string nauthor = author.TrimEx();
                if (level != -1)
                {
                    if (level == 0)
                        nartist = nartist.Replace(" ", "");
                    if (level == 1)
                        nauthor = nauthor.Replace(" ", "");
                    if (level == 2)
                    {
                        nartist = nartist.Replace(" ", "");
                        nauthor = nauthor.Replace(" ", "");
                    }
                    AdofaiggController.EscapeIfRequire(ref nartist);
                    AdofaiggController.EscapeIfRequire(ref ntitle);
                    AdofaiggController.EscapeIfRequire(ref nauthor);
                }
                if (!string.IsNullOrWhiteSpace(nartist) && !AdofaiggController.IsSameWithDefault("editor.artist", nartist))
                    parameters.Add(Level.ArtistQuery(nartist));
                if (!string.IsNullOrWhiteSpace(ntitle) && !AdofaiggController.IsSameWithDefault("editor.song", ntitle))
                    parameters.Add(Level.SongQuery(ntitle));
                if (!string.IsNullOrWhiteSpace(nauthor) && !AdofaiggController.IsSameWithDefault("editor.author", nauthor))
                    parameters.Add(Level.CreatorQuery(nauthor));
                return new Parameters(parameters);
            }
        }
        public static async Task<Response<T>> TUFRequest<T>(string header, Parameters parameters)
        {
            string reqUrl = $"{API}{header}{parameters}";
            string json = await Main.HttpClient.GetStringAsync(reqUrl);
            RequestCount++;
            Response<T> r = JsonConvert.DeserializeObject<Response<T>>(json) ?? new Response<T>();
            r.json = json;
            return r;
        }
    }
}
