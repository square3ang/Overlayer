using Newtonsoft.Json.Linq;
using Overlayer.Models;
using Overlayer.Unity;
using System.Collections.Generic;
using System.Linq;

namespace Overlayer.Core;

public static class ProfileReferences {
    public static JArray GetReferences(OverlayerProfile profile) {
        var refs = new List<TextConfigImporter.Reference>();

        foreach(var text in profile.TextManager.Texts) {
            var arr = TextConfigImporter.GetReferences(text.Config);
            refs.AddRange(ModelUtils.UnwrapList<TextConfigImporter.Reference>(arr));
        }

        return ModelUtils.WrapList(
            refs.Where(r => r != null).Distinct().ToList()
        );
    }
}