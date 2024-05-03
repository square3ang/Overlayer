using AdofaiMapConverter;
using AdofaiMapConverter.Types;

namespace Overlayer.WebAPI
{
    public record LevelMeta(int tileCount, double twirlRatio, double setSpeedRatio,
        double minTA, double maxTA, double taAverage, double taVariance, double taStdDeviation,
        double minSA, double maxSA, double saAverage, double saVariance, double saStdDeviation,
        double minMs, double maxMs, double msAverage, double msVariance, double msStdDeviation,
        double minBpm, double maxBpm, double bpmAverage, double bpmVariance, double bpmStdDeviation)
    {
        public override string ToString() => $"{tileCount},{twirlRatio},{setSpeedRatio},"
                    + $"{minTA},{maxTA},{taAverage},{taVariance},{taStdDeviation},"
                    + $"{minSA},{maxSA},{saAverage},{saVariance},{saStdDeviation},"
                    + $"{minMs},{maxMs},{msAverage},{msVariance},{msStdDeviation},"
                    + $"{minBpm},{maxBpm},{bpmAverage},{bpmVariance},{bpmStdDeviation}";
        public bool IsValid => !double.IsNaN(twirlRatio) && !double.IsNaN(setSpeedRatio) &&
            !double.IsNaN(minTA) && !double.IsNaN(maxTA) && !double.IsNaN(taAverage) && !double.IsNaN(taVariance) && !double.IsNaN(taStdDeviation) &&
            !double.IsNaN(minSA) && !double.IsNaN(maxSA) && !double.IsNaN(saAverage) && !double.IsNaN(saVariance) && !double.IsNaN(saStdDeviation) &&
            !double.IsNaN(minMs) && !double.IsNaN(maxMs) && !double.IsNaN(msAverage) && !double.IsNaN(msVariance) && !double.IsNaN(msStdDeviation) &&
            !double.IsNaN(minBpm) && !double.IsNaN(maxBpm) && !double.IsNaN(bpmAverage) && !double.IsNaN(bpmVariance) && !double.IsNaN(bpmStdDeviation);
        public static LevelMeta GetMeta(CustomLevel level)
        {
            var tiles = level.Tiles;
            var twirlRatio = GetRatio(tiles, t => t.GetActions(LevelEventType.Twirl).Any());
            var setSpeedRatio = GetRatio(tiles, t => t.GetActions(LevelEventType.SetSpeed).Any());

#if DEBUG
            int tileNumber = 0;
            foreach (var t in tiles)
            {
                if (double.IsNaN(t.tileMeta.travelAngle))
                    Console.WriteLine($"[LevelMeta.GetMeta] [{tileNumber}] Index Tile Has Invalid Travel Angle!!\n{t.rawActions.FirstOrDefault()?.ToNode().ToString(4)}");
                if (double.IsNaN(t.tileMeta.staticAngle))
                    Console.WriteLine($"[LevelMeta.GetMeta] [{tileNumber}] Index Tile Has Invalid Travel Angle!!\n{t.rawActions.FirstOrDefault()?.ToNode().ToString(4)}");
                tileNumber++;
            }
#endif

            var allTA = tiles.Select(t => t.tileMeta.travelAngle);
            var minTA = allTA.Min();
            var maxTA = allTA.Max();
            var taAvg = allTA.Average();
            var taVariance = allTA.Select(m => Math.Pow(m - taAvg, 2)).Average();
            var taStdDev = Math.Sqrt(taVariance);

            var allSA = tiles.Select(t => t.tileMeta.staticAngle);
            var minSA = allSA.Min();
            var maxSA = allSA.Max();
            var saAvg = allSA.Average();
            var saVariance = allSA.Select(m => Math.Pow(m - saAvg, 2)).Average();
            var saStdDev = Math.Sqrt(saVariance);

            var allMs = tiles.Select(t => t.tileMeta.TravelMs);
            var minMs = allMs.Min();
            var maxMs = allMs.Max();
            var msAvg = allMs.Average();
            var msVariance = allMs.Select(m => Math.Pow(m - msAvg, 2)).Average();
            var msStdDev = Math.Sqrt(msVariance);

            var allBpm = tiles.Select(t => t.tileMeta.bpm);
            var minBpm = allBpm.Min();
            var maxBpm = allBpm.Max();
            var bpmAvg = allBpm.Average();
            var bpmVariance = allBpm.Select(b => Math.Pow(b - bpmAvg, 2)).Average();
            var bpmStdDev = Math.Sqrt(bpmVariance);

            return new LevelMeta(
                tiles.Count,
                twirlRatio, setSpeedRatio,
                minTA, maxTA,
                taAvg, taVariance, taStdDev,
                minSA, maxSA,
                saAvg, saVariance, saStdDev,
                minMs, maxMs,
                msAvg, msVariance, msStdDev,
                minBpm, maxBpm,
                bpmAvg, bpmVariance, bpmStdDev);
        }
        public static double GetRatio<T>(IEnumerable<T> enumerable, Predicate<T> predicate)
        {
            int count, selectedCount = 0;
            if (enumerable is T[] arr)
            {
                count = arr.Length;
                for (int i = 0; i < count; i++)
                    if (predicate(arr[i]))
                        selectedCount++;
            }
            else if (enumerable is ICollection<T> collection)
            {
                count = collection.Count;
                foreach (T t in collection)
                    if (predicate(t))
                        selectedCount++;
            }
            else
            {
                count = enumerable.Count();
                foreach (T t in enumerable)
                    if (predicate(t))
                        selectedCount++;
            }
            return selectedCount / (double)count * 100d;
        }
    }
}
