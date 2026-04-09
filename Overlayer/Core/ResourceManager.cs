using System.IO;
using System.Reflection;

namespace Overlayer.Core;

public static class ResourceManager
{
    public static byte[] GetResourceBytes(string resourceName) {
        Assembly assembly = typeof(ResourceManager).Assembly;
        string fullName = $"Overlayer.MiscFiles.res.{resourceName}";

        using Stream stream = assembly.GetManifestResourceStream(fullName);
        if(stream == null) {
            return null;
        }

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public static byte[] GetImageBytes(string path) {
        return GetResourceBytes($"images.{path}");
    }

}