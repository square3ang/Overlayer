using JSON;
using Microsoft.AspNetCore.Mvc;

namespace Overlayer.WebAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class MainController : ControllerBase
    {
        public static readonly JsonNode Info = JsonNode.Parse(System.IO.File.ReadAllText("Info.json"));
        [HttpGet("version")]
        public Version GetVersion() => Version.Parse(Info["version"].Value);
        [HttpGet("discord")]
        public string GetDiscordLink() => Info["discord"].Value;
        [HttpGet("download")]
        public string GetDownloadLink() => Info["download"].Value;
    }
}
