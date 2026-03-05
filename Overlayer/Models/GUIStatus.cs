using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;

namespace Overlayer.Models;

public class GUIStatus : IModel, ICopyable<GUIStatus> {
    public bool Expanded = false;
    public bool Enabled = true;
    public GUIStatus Copy() {
        var status = new GUIStatus {
            Expanded = Expanded,
            Enabled = Enabled
        };
        return status;
    }
    public JToken Serialize() {
        return new JObject {
            [nameof(Expanded)] = Expanded,
            [nameof(Enabled)] = Enabled
        };
    }
    public void Deserialize(JToken node) {
        var defaultSettings = new GUIStatus();

        Expanded = node?[nameof(Expanded)]?.Value<bool?>() ?? defaultSettings.Expanded;
        Enabled = node?[nameof(Enabled)]?.Value<bool?>() ?? defaultSettings.Enabled;
    }
}
