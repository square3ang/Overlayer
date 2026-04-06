using System.IO;

namespace Overlayer.Core;

public static class ResourceImageManager {
    public static byte[] GetResourceBytes(string path) {
        var assembly = typeof(ResourceImageManager).Assembly;

        var fullName = $"Overlayer.MiscFiles.images.{path}";

        using (var stream = assembly.GetManifestResourceStream(fullName)) {
            if (stream == null) return null;

            using (MemoryStream ms = new()) {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}