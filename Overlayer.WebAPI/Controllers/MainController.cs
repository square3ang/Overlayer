using Microsoft.AspNetCore.Mvc;

namespace Overlayer.WebAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class MainController : ControllerBase
    {
        public static readonly Version VERSION = Version.Parse(System.IO.File.ReadAllText("version.txt"));
        [HttpGet("version")]
        public Version GetVersion() => VERSION;
    }
}
