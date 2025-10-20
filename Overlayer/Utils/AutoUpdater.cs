using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using static UnityModManagerNet.UnityModManager;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;

namespace Overlayer.Utils {
    public static class AutoUpdater {
        public enum VersionType {
            Unknown,
            Old,
            Stable,
            Beta,
            OldBeta,
            UnknownBeta,
        }

        public static bool isLatest = true;
        public static bool isBeta = false;
        public static string LatestUrl;
        public static string BetaUrl;
        public static Version LatestVersion;
        public static Version BetaVersion;
        public static VersionType CurrentVersionType = VersionType.Unknown;
        public static bool IsUpdating { get; private set; } = false;
        public static bool RequireRestart { get; private set; }
        public static readonly string OverlayerGithubApiLink = "https://api.github.com/repos/modlist-org/Overlayer/releases";
        private static Version newVersion = new();

        public static void Reload(ModEntry modEntry) {
            Type entryType = typeof(ModEntry);
            PropertyInfo canReloadProp = entryType.GetProperty("CanReload", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            MethodInfo setMethod = canReloadProp?.GetSetMethod(true);
            MethodInfo reloadMethod = entryType.GetMethod("Reload", BindingFlags.Instance | BindingFlags.NonPublic);
            setMethod?.Invoke(modEntry, new object[] { true });
            reloadMethod?.Invoke(modEntry, null);
            setMethod?.Invoke(modEntry, new object[] { false });
        }

        public static async Task InitAndUpdate(ModEntry modEntry, bool update = false, bool allowBeta = false, Action ok = null, Action<string> err = null) {
            await InitUpdate(modEntry.Version, async () => {
                if(update) {
                    await CheckAndUpdate(modEntry, allowBeta, ok, err);
                }
            }, err);
        }
        public static async Task InitUpdate(Version currentVersion, Action ok = null, Action<string> err = null) {
            string json = null;
            try {
                using(var client = new HttpClient()) {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Overlayer-Updater");
                    var response = await client.GetAsync(OverlayerGithubApiLink);
                    if(!response.IsSuccessStatusCode)
                        return;
                    json = await response.Content.ReadAsStringAsync();
                }
            } catch(Exception ex) {
                err?.Invoke("Failed to fetch update info: " + ex.Message);
                return;
            }

            JArray releases = null;
            try {
                releases = JArray.Parse(json);
            } catch(Exception ex) {
                err?.Invoke("Invalid JSON: " + ex.Message);
                return;
            }

            try {
                JObject latestBetaRelease = releases
                    .Where(r => r["target_commitish"]?.ToString() == "v3" && r["prerelease"]?.ToObject<bool>() == true)
                    .OrderByDescending(r => new Version(r["tag_name"].ToString()))
                    .FirstOrDefault() as JObject;

                if(releases
                    .Where(r => r["target_commitish"]?.ToString() == "v3" && r["prerelease"]?.ToObject<bool>() == false)
                    .OrderByDescending(r => new Version(r["tag_name"].ToString()))
                    .FirstOrDefault() is JObject latestRelease) {
                    LatestVersion = new Version(latestRelease["tag_name"].ToString());
                    if(LatestVersion > currentVersion) {
                        isLatest = false;
                        CurrentVersionType = VersionType.Old;
                    } else if(LatestVersion < currentVersion) {
                        isBeta = true;
                        CurrentVersionType = VersionType.Beta;
                    } else {
                        CurrentVersionType = VersionType.Stable;
                    }

                    var asset = latestRelease["assets"]?.FirstOrDefault();
                    LatestUrl = asset?["browser_download_url"]?.ToString();
                    newVersion = LatestVersion;
                } else {
                    LatestUrl = null;
                }

                if(latestBetaRelease != null) {
                    BetaVersion = new Version(latestBetaRelease["tag_name"].ToString());
                    var asset = latestBetaRelease["assets"]?.FirstOrDefault();
                    BetaUrl = asset?["browser_download_url"]?.ToString();
                    newVersion = BetaVersion;
                    if(currentVersion > BetaVersion) {
                        CurrentVersionType = VersionType.UnknownBeta;
                    } else if(currentVersion < BetaVersion) {
                        CurrentVersionType = VersionType.OldBeta;
                    } else if(currentVersion == BetaVersion) {
                        CurrentVersionType = VersionType.Beta;
                    }
                } else {
                    BetaUrl = null;
                }
                ok?.Invoke();
            } catch(Exception ex) {
                err?.Invoke("Version parse or asset fetch failed: " + ex.Message);
                return;
            }
        }
        public static async Task CheckAndUpdate(ModEntry modEntry, bool allowBeta = false, Action ok = null, Action<string> err = null) {
            if(IsUpdating) {
                err?.Invoke(Main.Lang.Get("ALEADY_UPDATING", "Already Updating"));
                return;
            }

            if(isLatest && (!allowBeta || CurrentVersionType == VersionType.OldBeta)) {
                err?.Invoke(Main.Lang.Get("ALREADY_LATEST", "Already the latest version"));
                return;
            }

            string url = allowBeta ? BetaUrl : LatestUrl;

            if(string.IsNullOrEmpty(url)) {
                err?.Invoke(Main.Lang.Get("DOWNLOAD_URL_EMPTY", "Download URL is empty"));
                return;
            }

            IsUpdating = true;

            string tempDir = Path.Combine(Path.GetTempPath(), "OverlayerUpdate");
            string zipPath = Path.Combine(tempDir, "Overlayer.zip");

            try {
                if(Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                using(var client = new WebClient()) {
                    await client.DownloadFileTaskAsync(new Uri(url), zipPath);
                }

                ZipFile.ExtractToDirectory(zipPath, tempDir);

                foreach(var file in Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories)) {
                    string relativePath = file.Substring(tempDir.Length + 1);
                    string destPath = Path.Combine(modEntry.Path, relativePath);

                    string destDir = Path.GetDirectoryName(destPath);
                    if(!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    File.Copy(file, destPath, true);
                }
                FieldInfo versionField = typeof(ModEntry).GetField("Version", BindingFlags.Instance | BindingFlags.Public);
                versionField.SetValue(modEntry, newVersion);
                modEntry.Info.Version = newVersion.ToString();
                ok?.Invoke();
            } catch(Exception ex) {
                err?.Invoke(ex.Message);
            } finally {
                if(File.Exists(zipPath))
                    File.Delete(zipPath);
                IsUpdating = false;
            }
        }
    }
}