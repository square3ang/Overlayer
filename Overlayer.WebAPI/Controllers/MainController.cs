using AdofaiMapConverter;
using DifficultyPredictor;
using JSON;
using Microsoft.AspNetCore.Mvc;
using Overlayer.WebAPI.Core.Utils;
using System;
using System.Text;
using static AdofaiMapConverter.Helpers.AngleHelper;

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
        [HttpGet("handshake")]
        public string Handshake()
        {
            var ip = HttpContext.GetIpAddress() ?? "Anonymous";
            System.IO.File.AppendAllText("handshakes.txt", ip + "\n");
            Console.WriteLine(ip);
            return ip;
        }
        [HttpGet("predict")]
        public double Predict([FromQuery] int tileCount, [FromQuery] float twirlRatio, [FromQuery] float setSpeedRatio,
        [FromQuery] float minTA, [FromQuery] float maxTA, [FromQuery] float taAverage, [FromQuery] float taVariance, [FromQuery] float taStdDeviation,
        [FromQuery] float minSA, [FromQuery] float maxSA, [FromQuery] float saAverage, [FromQuery] float saVariance, [FromQuery] float saStdDeviation,
        [FromQuery] float minMs, [FromQuery] float maxMs, [FromQuery] float msAverage, [FromQuery] float msVariance, [FromQuery] float msStdDeviation,
        [FromQuery] float minBpm, [FromQuery] float maxBpm, [FromQuery] float bpmAverage, [FromQuery] float bpmVariance, [FromQuery] float bpmStdDeviation)
        {
            var input = new DP_V2.ModelInput()
            {
                TileCount = tileCount,
                TwirlRatio = twirlRatio,
                SetSpeedRatio = setSpeedRatio,

                MinTA = minTA,
                MaxTA = maxTA,
                TaAverage = taAverage,
                TaVariance = taVariance,
                TaStdDeviation = taStdDeviation,

                MinSA = minSA,
                MaxSA = maxSA,
                SaAverage = saAverage,
                SaVariance = saVariance,
                SaStdDeviation = saStdDeviation,

                MinMs = minMs,
                MaxMS = maxMs,
                MsAverage = msAverage,
                MsVariance = msVariance,
                MsStdDeviation = msStdDeviation,

                MinBpm = minBpm,
                MaxBpm = maxBpm,
                BpmAverage = bpmAverage,
                BpmVariance = bpmVariance,
                BpmStdDeviation = bpmStdDeviation,
            };
            var predicted = DP_V2.Predict(input).Score;
            if (predicted > 25) return 21;
            if (predicted <= 20)
                if (predicted > 18)
                    return Math.Round(predicted / .5) * .5;
                else return Math.Round(predicted);
            else return Math.Round(20d + (predicted % 20 / 5), 1);
        }
        [HttpPost("predictRaw")]
        public async Task PredictRaw([FromBody] byte[] adofai)
        {
            var difficulty = -500.0;
            try
            {
                var adofaiJson = Encoding.UTF8.GetString(adofai);
                var level = CustomLevel.Read(JsonNode.Parse(adofaiJson));
                var setting = level.Setting;
                Console.WriteLine($"[PredictRaw] Received artist:{setting.artist},title:{setting.song},author:{setting.author},tiles:{level.Tiles.Count},bpm:{setting.bpm}");
                var meta = LevelMeta.GetMeta(level);
                if (meta.IsValid)
                {
                    var input = new DP_V2.ModelInput()
                    {
                        TileCount = meta.tileCount,
                        TwirlRatio = (float)meta.twirlRatio,
                        SetSpeedRatio = (float)meta.setSpeedRatio,

                        MinTA = (float)meta.minTA,
                        MaxTA = (float)meta.maxTA,
                        TaAverage = (float)meta.taAverage,
                        TaVariance = (float)meta.taVariance,
                        TaStdDeviation = (float)meta.taStdDeviation,

                        MinSA = (float)meta.minSA,
                        MaxSA = (float)meta.maxSA,
                        SaAverage = (float)meta.saAverage,
                        SaVariance = (float)meta.saVariance,
                        SaStdDeviation = (float)meta.saStdDeviation,

                        MinMs = (float)meta.minMs,
                        MaxMS = (float)meta.maxMs,
                        MsAverage = (float)meta.msAverage,
                        MsVariance = (float)meta.msVariance,
                        MsStdDeviation = (float)meta.msStdDeviation,

                        MinBpm = (float)meta.minBpm,
                        MaxBpm = (float)meta.maxBpm,
                        BpmAverage = (float)meta.bpmAverage,
                        BpmVariance = (float)meta.bpmVariance,
                        BpmStdDeviation = (float)meta.bpmStdDeviation,
                    };
                    var predicted = DP_V2.Predict(input).Score;
                    if (predicted > 25) difficulty = 21;
                    if (predicted <= 20)
                        if (predicted > 18)
                            difficulty = Math.Round(predicted / .5) * .5;
                        else difficulty = Math.Round(predicted);
                    else difficulty = Math.Round(20d + (predicted % 20 / 5), 1);
                    Console.WriteLine($"[PredictRaw] Predicted Difficulty! (artist:{setting.artist},title:{setting.song},author:{setting.author},tiles:{level.Tiles.Count},bpm:{setting.bpm})");
                }
                else Console.WriteLine($"[PredictRaw] Cannot Predict Difficulty! (artist:{setting.artist},title:{setting.song},author:{setting.author},tiles:{level.Tiles.Count},bpm:{setting.bpm})");
            }
            catch { Console.WriteLine($"[PredictRaw] Cannot Predict Difficulty! (Exception Occured!!)"); }
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(difficulty.ToString()));
        }
    }
}
