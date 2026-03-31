using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using System.IO;

namespace Overlayer.Models;

public class FileAttempt : IModel, ICopyable<FileAttempt>  {
    public int Attempts { get; private set; } = 0;
    public int TileAttempts { get; private set; } = 0;

    public void IncreaseAttempts() => Attempts++;

    public JToken Serialize() {
        return new JObject {
            [nameof(Attempts)] = Attempts,
            [nameof(TileAttempts)] = TileAttempts
        };
    }

    public void Deserialize(JToken node) {
        Attempts = node[nameof(Attempts)]?.Value<int>() ?? default;
        TileAttempts = node[nameof(TileAttempts)]?.Value<int>() ?? default;
    }

    public FileAttempt Copy() {
        return new FileAttempt {
            Attempts = Attempts,
            TileAttempts = TileAttempts
        };
    }

    const string FileAttemptsFileName = "Overlayer_Attempts.json";

    public bool Save() {
        if(scnGame.instance == null) {
            return false;
        }

        var path = Path.Combine(scnGame.instance.levelPath, FileAttemptsFileName);
        var json = Serialize().ToString();
        File.WriteAllText(path, json);
        return true;
    }

    public bool Load() {
        if(scnGame.instance == null) {
            return false;
        }

        var path = Path.Combine(scnGame.instance.levelPath, FileAttemptsFileName);
        if(File.Exists(path)) {
            var json = File.ReadAllText(path);
            var node = JToken.Parse(json);
            Deserialize(node);
        }
        return true;
    }
}