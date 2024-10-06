using System.Collections.Generic;

namespace Overlayer.Tags;

public class Tooltip
{
    public static Dictionary<string, string> tooltip = new()
    {
        ["Accuracy"] = "Accuracy(when perfect: 100+0.01%)",
        ["ActiveScene"] = "Active Scene",
        ["ActualProgress"] = "Current (Actual) Progress based on time"
    };
}