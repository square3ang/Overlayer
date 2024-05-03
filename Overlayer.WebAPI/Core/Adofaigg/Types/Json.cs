using Newtonsoft.Json;

namespace Overlayer.WebAPI.Core.Adofaigg.Types
{
    public class Json
    {
        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
