using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Overlayer.Models;

public class FileAttempt : IModel, ICopyable<FileAttempt> {
    private int Attempts = 0;
    private List<int[]> TileAttempts = [];

    public int GetAttempts() => Attempts;

    public int GetTileAttempts(int tile) {
        foreach(var t in TileAttempts) {
            if(t[0] == tile) {
                return t[1];
            }
        }

        return 0;
    }

    public void IncreaseAttempts() => Attempts++;
    public void IncreaseTileAttempts(int tile) {
        foreach(var t in TileAttempts) {
            if(t[0] == tile) {
                t[1]++;
                return;
            }
        }
        TileAttempts.Add([tile, 1]);
    }

    public JToken Serialize() {
        return new JObject {
            [nameof(Attempts)] = Attempts,
            [nameof(TileAttempts)] = JArray.FromObject(TileAttempts)
        };
    }

    public void Deserialize(JToken node) {
        Attempts = node[nameof(Attempts)]?.Value<int>() ?? 0;
        TileAttempts = [];
        if(node[nameof(TileAttempts)] is not JArray arr) {
            return;
        }
        foreach(var item in arr) {
            if(item is not JArray pair) {
                continue;
            }
            if(pair.Count != 2) {
                continue;
            }
            if(pair[0]?.Type != JTokenType.Integer || pair[1]?.Type != JTokenType.Integer) {
                continue;
            }
            TileAttempts.Add([
                pair[0]!.Value<int>(),
                pair[1]!.Value<int>()
            ]);
        }
    }

    public FileAttempt Copy() {
        var copy = new FileAttempt {
            Attempts = Attempts
        };
        foreach(var t in TileAttempts) {
            copy.TileAttempts.Add([t[0], t[1]]);
        }
        return copy;
    }

    const string FileAttemptsFileName = "Overlayer_Attempts.json";

    public bool Save() {
        var path = GetPath();
        if(path == null) {
            return false;
        }

        var json = Serialize().ToString(Newtonsoft.Json.Formatting.None);
        File.WriteAllText(path, json);
        return true;
    }

    public bool Load() {
        var path = GetPath();
        if(path == null) {
            return false;
        }

        if(File.Exists(path)) {
            var json = File.ReadAllText(path);
            var node = JToken.Parse(json);
            Deserialize(node);
        }
        return true;
    }

    private static string GetPath() {
        if(scnGame.instance == null) {
            return null;
        }

        string level = scnGame.instance.levelPath;
        if(string.IsNullOrEmpty(level)) {
            return null;
        }

        var dir = Path.GetDirectoryName(level);
        return string.IsNullOrEmpty(dir) ? null : Path.Combine(dir, FileAttemptsFileName);
    }
}