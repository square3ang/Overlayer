using JSON;
using Microsoft.AspNetCore.Mvc;
using Overlayer.WebAPI.Core;

namespace Overlayer.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LanguageController : ControllerBase
    {
        public enum Lang
        {
            Korean = 0,
            English = 920194841
        }
        public static SpreadSheet OTS = new SpreadSheet("1vg9U2jje6tdJvuyCuH18-LOBbfTeyIoJB9PIge2AeLQ");
        public static Dictionary<Lang, Dictionary<string, string>> sheets = new Dictionary<Lang, Dictionary<string, string>>();
        public static Dictionary<Lang, string> sheetsJson = new Dictionary<Lang, string>();
        [HttpGet("{lang}")]
        public async Task<string> Get(Lang lang)
        {
            if (!sheetsJson.TryGetValue(lang, out var json))
            {
                var dict = await OTS.Download((int)lang);
                JsonNode node = JsonNode.Empty;
                foreach (var item in dict)
                    node[item.Key] = item.Value;
                sheetsJson[lang] = json = node.ToString();
            }
            return json;
        }
    }
}