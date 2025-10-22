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
                    await CheckAndPrepareUpdate(modEntry, allowBeta, ok, err);
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
                err?.Invoke(Main.Lang.Get("UPDATER_FAILED_FEATCH_INFO", "Failed to fetch update info") + ": " + ex.Message);
                return;
            }

            JArray releases = null;
            try {
                releases = JArray.Parse(json);
            } catch(Exception ex) {
                err?.Invoke(Main.Lang.Get("UPDATER_INVALID_JSON", "Invalid JSON") + ": " + ex.Message);
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
                } else {
                    LatestUrl = null;
                }

                if(latestBetaRelease != null) {
                    BetaVersion = new Version(latestBetaRelease["tag_name"].ToString());
                    if(BetaVersion <= LatestVersion) {
                        BetaUrl = null;
                    } else {
                        var asset = latestBetaRelease["assets"]?.FirstOrDefault();
                        BetaUrl = asset?["browser_download_url"]?.ToString();
                    }

                    if(CurrentVersionType == VersionType.Beta) {
                        if(currentVersion > BetaVersion) {
                            CurrentVersionType = VersionType.UnknownBeta;
                        } else if(currentVersion < BetaVersion) {
                            CurrentVersionType = VersionType.OldBeta;
                        }
                    }
                } else {
                    BetaUrl = null;
                }
                ok?.Invoke();
            } catch(Exception ex) {
                err?.Invoke(Main.Lang.Get("UPDATER_VERSION_CHECK_ERROR", "Version parse or asset fetch failed") + ": " + ex.Message);
                return;
            }
        }
        public static async Task CheckAndPrepareUpdate(ModEntry modEntry, bool allowBeta = false, Action ok = null, Action<string> err = null) {
            if(IsUpdating) {
                err?.Invoke(Main.Lang.Get("UPDATER_LEADY_UPDATING", "Already Updating"));
                return;
            }

            if(isLatest && (!allowBeta || CurrentVersionType == VersionType.OldBeta)) {
                err?.Invoke(Main.Lang.Get("UPDATER_ALREADY_LATEST", "Already the latest version"));
                return;
            }

            string url;
            if(allowBeta && !string.IsNullOrEmpty(BetaUrl) && BetaVersion > LatestVersion) {
                url = BetaUrl;
            } else {
                url = LatestUrl;
            }

            if(string.IsNullOrEmpty(url)) {
                err?.Invoke(Main.Lang.Get("UPDATER_DOWNLOAD_URL_EMPTY", "Download URL is empty"));
                return;
            }

            IsUpdating = true;

            string tempDir = Path.Combine(modEntry.Path,"updatetemp");
            string zipPath = Path.Combine(tempDir, "Overlayer.zip");

            try {
                if(Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                using(var client = new WebClient()) {
                    await client.DownloadFileTaskAsync(new Uri(url), zipPath);
                }

                ZipFile.ExtractToDirectory(zipPath, tempDir);

                string infoPath = Path.Combine(tempDir, "info.json");
                if(File.Exists(infoPath)) {
                    try {
                        var infoJson = JObject.Parse(File.ReadAllText(infoPath));
                        string extractedVersion = infoJson["Version"]?.ToString();
                        if(!string.IsNullOrEmpty(extractedVersion) && extractedVersion == modEntry.Version.ToString()) {
                            err?.Invoke(Main.Lang.Get("UPDATER_SAME_VERSION_DETECTED", "Update package version matches the current version") + ": " + extractedVersion);
                            return;
                        }
                    } catch(Exception e) {
                        err?.Invoke(Main.Lang.Get("UPDATER_INFO_JSON_PARSE_FAILED", $"Failed to parse info.json") + ": " + e.Message);
                        return;
                    }
                } else {
                    err?.Invoke(Main.Lang.Get("UPDATER_INFO_JSON_NOT_FOUND", "info.json not found in the update package"));
                }
                RequireRestart = true;
                ok?.Invoke();
            } catch(Exception ex) {
                err?.Invoke(ex.Message);
            } finally {
                if(File.Exists(zipPath))
                    File.Delete(zipPath);
                IsUpdating = false;
            }
        }
        public static Version UpdateBeforeLoad(ModEntry modEntry) {
            string tempDir = Path.Combine(modEntry.Path, "updatetemp");

            if(!Directory.Exists(tempDir)) {
                return null;
            }

            string infoPath = Path.Combine(tempDir, "info.json");

            Version extractedVersion = null;

            try {
                var infoJson = JObject.Parse(File.ReadAllText(infoPath));
                string versionStr = infoJson["Version"]?.ToString();

                if(string.IsNullOrEmpty(versionStr)) {
                    string errPkg = "Failed to get version from info.json.";
                    Main.Logger.Log(errPkg);
                    Main.UpdateInfo = errPkg;
                    return null;
                } else {
                    extractedVersion = new Version(versionStr);
                }
            } catch(Exception e) {
                string errParse = "Failed to parse info.json: " + e.Message;
                Main.Logger.Error(errParse);
                Main.UpdateInfo = errParse;
                return null;
            }

            try {
                foreach(var file in Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories)) {
                    string relativePath = file.Substring(tempDir.Length + 1);
                    string destPath = Path.Combine(modEntry.Path, relativePath);

                    string destDir = Path.GetDirectoryName(destPath);
                    if(!Directory.Exists(destDir)) {
                        Directory.CreateDirectory(destDir);
                    }

                    File.Copy(file, destPath, true);
                }

                Directory.Delete(tempDir, true);
            } catch(Exception ex) {
                Main.Logger.Error(ex.Message);
                return null;
            }
            
            return extractedVersion;
        }
    }
}