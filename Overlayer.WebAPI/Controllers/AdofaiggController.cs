using Microsoft.AspNetCore.Mvc;
using Overlayer.WebAPI.Core.Adofaigg.Types;
using Overlayer.WebAPI.Core.Adofaigg;
using Overlayer.WebAPI.Core.Utils;
using Newtonsoft.Json;

namespace Overlayer.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdofaiggController : ControllerBase
    {
        public const string API = "https://adofai.gg/api/v1";
        public const string HEADER_LEVELS = "/levels";
        public const string HEADER_PLAY_LOGS = "/playLogs";
        public const string HEADER_RANKING = "/ranking";
        public static bool EscapeParameter { get; set; } = false;
        [HttpGet("difficulty")]
        public async Task<double> GetDifficulty([FromQuery] string artist, [FromQuery] string title, [FromQuery] string author, [FromQuery] int tiles, [FromQuery] int bpm)
        {
            Console.WriteLine($"Received artist:{artist},title:{title},author:{author},tiles:{tiles},bpm:{bpm}");
            var level = await GetLevel(artist, title, author, tiles, bpm);
            if (level == null)
            {
                EscapeParameter = true;
                level = await GetLevel(artist, title, author, tiles, bpm);
                EscapeParameter = false;
            }
            if (level == null)
                Console.WriteLine($"Cannot Find Level!! (artist:{artist},title:{title},author:{author},tiles:{tiles},bpm:{bpm})");
            else Console.WriteLine($"Found Level! ({level.id})");
            return await Task.FromResult(level?.difficulty ?? -404);
        }
        private async Task<Level?> GetLevel(string artist, string title, string author, int tiles, int bpm, params Parameter[] ifFailedWith)
        {
            var result = await GGRequest<Level>(HEADER_LEVELS, ActualParams(-1));
            if (result.count <= 0)
            {
                result = await GGRequest<Level>(HEADER_LEVELS, ActualParams(0));
                if (result.count <= 0)
                {
                    result = await GGRequest<Level>(HEADER_LEVELS, ActualParams(1));
                    if (result.count <= 0)
                    {
                        result = await GGRequest<Level>(HEADER_LEVELS, ActualParams(2));
                        if (result.count <= 0)
                            return await Fail();
                        return result.results[0];
                    }
                    return result.results[0];
                }
                return result.results[0];
            }
            if (result.count > 1)
                result = await GGRequest<Level>(HEADER_LEVELS, ActualParams(-1, Level.MinBpm(bpm - 1), Level.MaxBpm(bpm + 1)));
            if (result.count <= 0) return await Fail();
            if (result.count > 1)
                result = await GGRequest<Level>(HEADER_LEVELS, ActualParams(-1, Level.MinTiles(tiles - 1), Level.MaxTiles(tiles + 1)));
            if (result.count <= 0) return await Fail();
            if (result.count > 1)
                result = await GGRequest<Level>(HEADER_LEVELS, ActualParams(-1, Level.MinBpm(bpm - 1), Level.MaxBpm(bpm + 1), Level.MinTiles(tiles - 1), Level.MaxTiles(tiles + 1)));
            if (result.count <= 0) return await Fail();
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
                }
                if (!string.IsNullOrWhiteSpace(nartist) && !IsSameWithDefault("editor.artist", nartist))
                    parameters.Add(Level.QueryArtist(nartist));
                if (!string.IsNullOrWhiteSpace(ntitle) && !IsSameWithDefault("editor.song", ntitle))
                    parameters.Add(Level.QueryTitle(ntitle));
                if (!string.IsNullOrWhiteSpace(nauthor) && !IsSameWithDefault("editor.author", nauthor))
                    parameters.Add(Level.QueryCreator(nauthor));
                return new Parameters(parameters);
            }
            async Task<Level?> Fail()
            {
                if (ifFailedWith.Length < 1)
                    return await GetLevel(artist, title, author, tiles, bpm, Level.ShowCensored(true));
                return null;
            }
        }
        private async Task<Response<T>> GGRequest<T>(string header, Parameters parameters) where T : Json
        {
            string reqUrl = $"{API}{header}{parameters}";
            Console.WriteLine($"GGRequest:{reqUrl}");
            string json = await Main.HttpClient.GetStringAsync(reqUrl);
            Response<T> r = JsonConvert.DeserializeObject<Response<T>>(json) ?? new Response<T>();
            r.json = json;
            return r;
        }
        private static bool IsSameWithDefault(string key, string value)
        {
            return DefaultStrings[key].Contains(value);
        }
        private static readonly Dictionary<string, string[]> DefaultStrings = new Dictionary<string, string[]>()
        {
            ["editor.artist"] = new string[]
            {
                "작곡가",
                "Artist",
                "アーティスト",
                "Artista",
                "Artista",
                "Artiste",
                "Artysta:",
                "Artist",
                "Исполнитель",
                "Nghệ sĩ",
                "Skladatel",
                "Autor",
                "音乐作者",
                "音樂作者",
            },
            ["editor.song"] = new string[]
            {
                "곡명",
                "Song",
                "曲名",
                "Canción",
                "Música",
                "Chanson",
                "Muzyka",
                "Melodie",
                "Песня",
                "Nhạc",
                "Skladba",
                "Song",
                "歌曲",
                "歌曲",
            },
            ["editor.author"] = new string[]
            {
                "만든이",
                "Author",
                "作成者",
                "Autor",
                "Autor",
                "Auteur",
                "Autor",
                "Autor",
                "Автор",
                "Tác Giả",
                "Author",
                "Levelersteller",
                "关卡作者",
                "關卡作者",
            }
        };
    }
}
