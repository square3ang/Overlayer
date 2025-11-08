using Overlayer.Core.Patches;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overlayer.Tags.Patches {
    public class P_ffxSetDefaultText : PatchBase<P_ffxSetDefaultText> {
        [LazyPatch("Tags.P_ffxSetDefaultText.Level_LevelNameText__StartEffect", "ffxSetDefaultText", "StartEffect", Triggers = new string[] {
            nameof(Level.LevelNameText)
        })]
        public static class Level_LevelNameText__StartEffect {
            public static void Postfix() {
                Level.UpdateLevelNameText();
            }
        }

        [LazyPatch("Tags.P_ffxSetDefaultText.Level_LevelNameTextRaw__StartEffect", "ffxSetDefaultText", "StartEffect", Triggers = new string[] {
            nameof(Level.LevelNameTextRaw)
        })]
        public static class Level_LevelNameTextRaw__StartEffect {
            public static void Postfix() {
                Level.UpdateLevelNameTextRaw();
            }
        }
    }
}
