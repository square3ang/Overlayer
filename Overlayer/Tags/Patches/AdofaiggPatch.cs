using ADOFAI;
using Overlayer.Core;
using Overlayer.Core.Patches;
using Overlayer.Utils;
using System;
using System.Threading.Tasks;

namespace Overlayer.Tags.Patches
{
    public class AdofaiggPatch : PatchBase<AdofaiggPatch>
    {
        [LazyPatch("Tags.Adofaigg.RatingCalculator", "scrPlanet", "MoveToNextFloor", Triggers = new string[]
        {
            nameof(Adofaigg.GGRating)
        })]
        public static class RatingCalcualtor
        {
            public static void Postfix()
            {
                if (!Adofaigg.GGRequestCompleted) return;
                var ctrl = scrController.instance;
                var rating = Adofaigg.GGRating;
                var diff = Adofaigg.GGDifficulty;
                var xacc = ctrl.mistakesManager.percentXAcc * 100d;
                rating.rank_Internal = xacc switch
                {
                    >= 100 => Adofaigg.Rank.SSS100,
                    >= 99.5 => Adofaigg.Rank.SSS,
                    >= 99.0 => Adofaigg.Rank.SS,
                    >= 98.0 => Adofaigg.Rank.S,
                    >= 96.0 => Adofaigg.Rank.A,
                    >= 93.0 => Adofaigg.Rank.B,
                    >= 90.0 => Adofaigg.Rank.C,
                    >= 80.0 => Adofaigg.Rank.D,
                    _ => Adofaigg.Rank.F
                };
                rating.rank = rating.rank_Internal == Adofaigg.Rank.SSS100 ? Adofaigg.Rank.SSS : rating.rank_Internal;
                rating.rating = rating.rank_Internal switch
                {
                    Adofaigg.Rank.SSS100 => diff + 2,
                    Adofaigg.Rank.SSS => diff + 1.5 + Rate(xacc, 99.5, 0.01, 0.01),
                    Adofaigg.Rank.SS => diff + 1 + Rate(xacc, 99.0, 0.01, 0.01),
                    Adofaigg.Rank.S => diff + Rate(xacc, 98.0, 0.01, 0.01),
                    Adofaigg.Rank.A => diff - 0.5 + Rate(xacc, 96.0, 0.05, 0.01),
                    Adofaigg.Rank.B => diff - 2 + Rate(xacc, 93.0, 0.1, 0.05),
                    Adofaigg.Rank.C => diff - 4 + Rate(xacc, 90.0, 0.1, 0.05),
                    Adofaigg.Rank.D => ((diff - 5) / 2) + Rate(xacc, 80.0, 0.1, 0.1),
                    _ => 0
                };
            }
            public static double Rate(double xacc, double defXacc, double unit, double add)
            {
                double sub = xacc - defXacc;
                double unitSub = Math.Round(sub / unit) * unit;
                double factor = unitSub / unit;
                return add * factor;
            }
        }
        [LazyPatch("Tags.Adofaigg.GGDifficultyUpdater", "ADOFAI.LevelData", "LoadLevel", Triggers = new string[]
        {
            nameof(Adofaigg.GGDifficulty)
        })]
        public static class GGDifficultyUpdater
        {
            public static void Postfix(LevelData __instance)
            {
                Adofaigg.GGRequestCompleted = false;
                new Task(async () =>
                {
                    string artist = __instance.artist.BreakRichTag(), author = __instance.author.BreakRichTag(), title = __instance.song.BreakRichTag();
                    Adofaigg.GGDifficulty = await OverlayerWebAPI.GetGGDifficulty(artist, title, author, string.IsNullOrWhiteSpace(__instance.pathData) ? __instance.angleData.Count : __instance.pathData.Length, (int)Math.Round(__instance.bpm));
                    Adofaigg.GGRequestCompleted = true;
                }).Start();
            }
        }
    }
}
