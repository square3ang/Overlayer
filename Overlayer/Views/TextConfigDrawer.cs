using Overlayer.Core;
using Overlayer.Models;
using TKTC = Overlayer.Core.Translation.TranslationKeys.TextConfig;

namespace Overlayer.Views
{
    public class TextConfigDrawer : ModelDrawable<TextConfig>
    {
        public TextConfigDrawer(TextConfig config) : base(config, L(TKTC.Prefix, config.Name)) { }
        public override void Draw()
        {
            
        }
    }
}
