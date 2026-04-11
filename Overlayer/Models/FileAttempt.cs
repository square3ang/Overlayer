using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Overlayer.Models;

public class FileAttempt : IModel, ICopyable<FileAttempt> {
    private int Attempts = 0;
    private Dictionary<int, int> TileAttempts = [];

    public int GetAttempts() => Attempts;

    public int GetTileAttempts(int tile) {
        if(TileAttempts.TryGetValue(tile, out var count)) {
            return count;
        }
        return 0;
    }

    public void IncreaseAttempts() => Attempts++;

    public void IncreaseTileAttempts(int tile) {
        if(TileAttempts.TryGetValue(tile, out var count)) {
            TileAttempts[tile] = count + 1;
        } else {
            TileAttempts[tile] = 1;
        }
    }

    public JToken Serialize() {
        return new JObject {
            [nameof(Attempts)] = Attempts,
            [nameof(TileAttempts)] = JToken.FromObject(TileAttempts)
        };
    }

    public void Deserialize(JToken node) {
        Attempts = node[nameof(Attempts)]?.Value<int>() ?? 0;
        TileAttempts = node[nameof(TileAttempts)]?.ToObject<Dictionary<int, int>>() ?? new();
    }

    public FileAttempt Copy() {
        return new FileAttempt {
            Attempts = Attempts,
            TileAttempts = new Dictionary<int, int>(TileAttempts)
        };
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