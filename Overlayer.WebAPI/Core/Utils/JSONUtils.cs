using JSON;

namespace Overlayer.WebAPI.Core.Utils
{
    public static class JSONUtils
    {
        public static string? ToStringN(this JsonNode node)
        {
            if (node is JsonLazyCreator) return null;
            return node.Value;
        }
    }
}
