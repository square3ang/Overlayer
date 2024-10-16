﻿using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Overlayer.Tags;

public class Tooltip
{
    /*static Tooltip()
    {
        var dict = new Dictionary<string, string>();
        foreach (var itm in tooltip)
        {
            dict.Add("TOOLTIP_" + itm.Key.ToUpper(), itm.Value);
        }
        Main.Logger.Log(JObject.FromObject(dict).ToString());
    }*/
    public static Dictionary<string, string> tooltip = new()
    {
        ["CTE"] = "Current Too Early",
        ["CVE"] = "Current Very Early",
        ["CEP"] = "Current Early Perfect",
        ["CP"] = "Current Perfect",
        ["CLP"] = "Current Late Perfect",
        ["CVL"] = "Current Very Late",
        ["CTL"] = "Current Too Late",
        ["MissCount"] = "Number of Misses",
        ["Overloads"] = "Number of Overloads",
        ["Multipress"] = "Number of Multipresses",

        ["LTE"] = "Lenient Too Early",
        ["LVE"] = "Lenient Very Early",
        ["LEP"] = "Lenient Early Perfect",
        ["LP"] = "Lenient Perfect",
        ["LLP"] = "Lenient Late Perfect",
        ["LVL"] = "Lenient Very Late",
        ["LTL"] = "Lenient Too Late",
        ["NTE"] = "Normal Too Early",
        ["NVE"] = "Normal Very Early",
        ["NEP"] = "Normal Early Perfect",
        ["NP"] = "Normal Perfect",
        ["NLP"] = "Normal Late Perfect",
        ["NVL"] = "Normal Very Late",
        ["NTL"] = "Normal Too Late",
        ["STE"] = "Strict Too Early",
        ["SVE"] = "Strict Very Early",
        ["SEP"] = "Strict Early Perfect",
        ["SP"] = "Strict Perfect",
        ["SLP"] = "Strict Late Perfect",
        ["SVL"] = "Strict Very Late",
        ["STL"] = "Strict Too Late",

        ["TEHex"] = "TooEarly Hex Color",
        ["VEHex"] = "VeryEarly Hex Color",
        ["EPHex"] = "EarlyPerfect Hex Color",
        ["PHex"] = "Perfect Hex Color",
        ["LPHex"] = "LatePerfect Hex Color",
        ["VLHex"] = "VeryLate Hex Color",
        ["TLHex"] = "TooLate Hex Color",
        ["FMHex"] = "Miss Hex Color",
        ["FOHex"] = "Overload Hex Color",
        ["MPHex"] = "Multipress Hex Color",

        ["LHit"] = "Lenient Hit",
        ["NHit"] = "Normal Hit",
        ["SHit"] = "Strict Hit",
        ["CHit"] = "Current Hit",
        ["LHitRaw"] = "Lenient Hit Raw",
        ["NHitRaw"] = "Normal Hit Raw",
        ["SHitRaw"] = "Strict Hit Raw",
        ["CHitRaw"] = "Current Hit Raw",

        ["MarginScale"] = "Level's judgment range",

        ["LFast"] = "Fast judgment in Lenient difficulty",
        ["NFast"] = "Fast judgment in Normal difficulty",
        ["SFast"] = "Fast judgment in Strict difficulty",
        ["CFast"] = "Fast judgment in Current difficulty",
        ["LSlow"] = "Slow judgment in Lenient difficulty",
        ["NSlow"] = "Slow judgment in Normal difficulty",
        ["SSlow"] = "Slow judgment in Strict difficulty",
        ["CSlow"] = "Slow judgment in Current difficulty",

        ["Accuracy"] = "Accuracy(when perfect: 100+0.01%)",
        ["XAccuracy"] = "XAccuracy(when pure perfect: 100)",

        ["Combo"] = "Combo count",
        ["MaxCombo"] = "Maximum Combo",
        ["Progress"] = "Current progress",
        ["ActualProgress"] = "Current Actual Progress based on time",
        ["StartProgress"] = "Progress of start tile",
        ["BestProgress"] = "Best progress of the level",
        ["Score"] = "Current difficulty score",
        ["LScore"] = "Lenient difficulty score",
        ["NScore"] = "Normal difficulty score",
        ["SScore"] = "Strict difficulty score",
        ["CheckPointUsed"] = "Number of checkpoints used",
        ["CurCheckPoint"] = "Current checkpoint number",
        ["TotalCheckPoints"] = "Total number of checkpoints",

        ["StartTile"] = "Start tile number",
        ["CurTile"] = "Current tile number",
        ["LeftTile"] = "Number of tiles left",
        ["TotalTile"] = "Total number of tiles",

        ["Pitch"] = "Speed set in CLS (displayed as 1 when at 1x speed)",
        ["EditorPitch"] = "Pitch set in the LevelEditor (displayed as 1 when 100%)",

        ["Difficulty"] = "Current Difficulty",
        ["DifficultyRaw"] = "Fixed difficulty return value regardless of language setting",

        ["IsStarted"] = "Displays false if not started, or true if started by pressing any key",
        ["IsAutoEnabled"] = "Displays true if auto is enabled, false otherwise",
        ["IsPracticeModeEnabled"] = "Displays true if practice mode is enabled, false otherwise",
        ["IsOldAutoEnabled"] = "Displays true if Old Auto is enabled, false if disabled",
        ["IsNoFailEnabled"] = "Displays true if No-fail Mode is enabled, false if disabled",

        ["Timing"] = "Error range showing the timing of keystrokes in ms from the center of the tile",
        ["TimingAvg"] = "Average of Timing",

        ["CurMinute"] = "Current minute of song",
        ["TotalMinute"] = "Total minute of song",
        ["CurSecond"] = "Current second of song",
        ["TotalSecond"] = "Total second of song",
        ["CurMilliSecond"] = "Current millisecond of song",
        ["TotalMilliSecond"] = "Total millisecond of song",

        ["Title"] = "Title of the song",
        ["Artist"] = "Composer of the song",
        ["Author"] = "Level creator",
        ["TitleRaw"] = "Title of the song (with rich tags)",
        ["ArtistRaw"] = "Composer of the song (with rich tags)",
        ["AuthorRaw"] = "Level creator (with rich tags)",

        ["CurBpm"] = "Current BPM",
        ["TileBpm"] = "Tile BPM",
        ["RecKPS"] = "Recommended KPS",
        ["CurBpmWithoutPitch"] = "Current BPM (without pitch)",
        ["TileBpmWithoutPitch"] = "Tile BPM (without pitch)",
        ["RecKPSWithoutPitch"] = "Recommended KPS (without pitch)",

        ["Frame"] = "Frame",
        ["FrameTime"] = "Time difference between the previous frame and the current frame in ms",
        ["Fps"] = "Current FPS of ADOFAI",
        ["TargetFps"] = "Target FPS of ADOFAI",

        ["Year"] = "Year on current computer",
        ["Month"] = "Month on current computer",
        ["Day"] = "Day on current computer",
        ["Hour"] = "Hour on current computer",
        ["Minute"] = "Minutes on current computer",
        ["Second"] = "Seconds on current computer",
        ["MilliSecond"] = "Milliseconds on current computer",
        ["Days"] = "All days so far",
        ["Hours"] = "Total hours so far",
        ["Minutes"] = "Total minutes so far",
        ["Seconds"] = "Total seconds so far",
        ["MilliSeconds"] = "Total milliseconds so far",
        ["MovingMan"] = "You can animate text on criterion certain tags. Used with <size>.",
        ["ColorRange"] = "You can adjust the color from any color to any color you want based on a specific tag. Use with <color>.",
        ["EasedValue"] = "When the tag value changes, the tag value changes according to the speed.",
        ["Developer"] = "Display overlayer mod developer name Super Kawaii Suckyoubus Chan~♥︎"
    };
}