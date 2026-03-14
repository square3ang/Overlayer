using System.IO;
using System.Reflection;

namespace Overlayer.Core;

public static class ImageManager {
    public static byte[] GetResourceBytes(string path) {
        Assembly assembly = typeof(ImageManager).Assembly;

        string fullName = $"Overlayer.MiscFiles.images.{path}";

        using(Stream stream = assembly.GetManifestResourceStream(fullName)) {
            if(stream == null) {
                return null;
            }

            using(MemoryStream ms = new()) {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}