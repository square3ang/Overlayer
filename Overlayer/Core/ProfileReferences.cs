using Newtonsoft.Json.Linq;
using Overlayer.Models;
using Overlayer.Unity;
using System.Collections.Generic;
using System.Linq;
using static Overlayer.Core.TextConfigImporter;

namespace Overlayer.Core;

public static class ProfileReferences {

    public static JArray GetReferences(OverlayerProfile profile) {
        var justRefs = new List<Reference>();

        foreach(var text in profile.TextManager.Texts) {
            var arr = GetJustReferences(text.Config);
            justRefs.AddRange(ModelUtils.UnwrapList<Reference>(arr));
        }

        var unique = justRefs
            .Where(r => r != null)
            .GroupBy(r => (r.ReferenceType, r.Name))
            .Select(g => g.First())
            .ToList();

        var resolved = unique
            .Select(r => Reference.GetReference(r.Name, r.ReferenceType))
            .Where(r => r != null)
            .ToList();

        return ModelUtils.WrapList(resolved);
    }
}