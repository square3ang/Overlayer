using Newtonsoft.Json.Linq;

namespace Overlayer.Core.Interfaces;

public interface IModel {
    JToken Serialize();
    void Deserialize(JToken node);
}