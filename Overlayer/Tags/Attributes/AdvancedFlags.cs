using System;

namespace Overlayer.Tags.Attributes
{
    [Flags]
    public enum AdvancedFlags
    {
        None = 0,
        Round = 1 << 0,
    }
}
