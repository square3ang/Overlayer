using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;

namespace Overlayer.Models;

public abstract class ObjectConfig : IModel {
    public bool Active = true;
    public string Name;

    protected void CopyBase(ObjectConfig target) {
        target.Active = Active;
        target.Name = Name;
    }

    protected JObject SerializeBase() {
        return new JObject {
            [nameof(Active)] = Active,
            [nameof(Name)] = Name
        };
    }

    protected void DeserializeBase(JToken node) {
        Active = node[nameof(Active)]?.Value<bool>() ?? true;
        Name = node[nameof(Name)]?.Value<string>();
    }

    public abstract ObjectConfig Copy();
    public abstract JToken Serialize();
    public abstract void Deserialize(JToken node);
}