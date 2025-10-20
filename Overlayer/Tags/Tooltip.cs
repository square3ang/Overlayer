using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Overlayer.Tags;

public class Tooltip
{
    public static Dictionary<string, string> tooltip = new();
    public static string GetTooltip(string key) {
        if(string.IsNullOrEmpty(key)) {
            return null;
        }

        string langKey = "TOOLTIP_" + key.ToUpperInvariant();
        string localized = Main.Lang.Get(langKey, null);
        if(!string.IsNullOrEmpty(localized)) {
            return localized;
        }

        if(tooltip.TryGetValue(key.ToLowerInvariant(), out var staticTip)) {
            return staticTip;
        }

        return null;
    }
}