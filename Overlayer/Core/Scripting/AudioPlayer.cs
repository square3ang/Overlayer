using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Overlayer.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Overlayer.Core.Scripting;

public static class AudioPlayer {
    private static readonly List<AudioSource> sources = [];
    private static readonly Dictionary<string, AudioClip> clips = [];

    public static void Play(Sound sound) {
        if (sound?.sound == null) return;

        if (clips.TryGetValue(sound.sound, out var clip)) {
            if (sound.offset > 0) {
                StaticCoroutine.Run(PlayCo(sound.SetClip(clip)));
            }
            else {
                var source = EnsureSource();
                sound.SetClip(clip);
                source.clip = sound.clip;
                source.volume = sound.volume;
                source.pitch = sound.pitch;
                source.Play();
            }
        }
        else {
            StaticCoroutine.Run(LoadClip(sound.sound, clip => StaticCoroutine.Run(PlayCo(sound.SetClip(clip)))));
        }
    }

    public static void Stop(Sound sound) {
        sources.Find(a => a.clip == sound.clip)?.Stop();
    }

    public static void StopAll() {
        sources.ForEach(a => a.Stop());
    }

    public static void LoadAudio(string path, Action<AudioClip> callback) {
        StaticCoroutine.Run(LoadClip(path, callback));
    }

    private static IEnumerator PlayCo(Sound sound) {
        var counted = 0f;
        while (counted < sound.offset) {
            counted += Time.deltaTime * 1000f;
            yield return null;
        }

        var source = EnsureSource();
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.Play();
    }

    private static IEnumerator LoadClip(string sound, Action<AudioClip> callback) {
        if (callback == null) yield break;

        if (clips.TryGetValue(sound, out var c)) {
            callback(c);
            yield break;
        }

        if (!File.Exists(sound)) {
            Main.Logger.Log($"{sound} Is Not Exist!!");
            yield break;
        }

        Uri.TryCreate(sound, UriKind.RelativeOrAbsolute, out var uri);
        if (uri == null) yield break;

        var at = Path.GetExtension(sound) switch {
            ".ogg" => AudioType.OGGVORBIS,
            ".mp3" => AudioType.MPEG,
            ".aiff" => AudioType.AIFF,
            ".wav" => AudioType.WAV,
            _ => AudioType.UNKNOWN
        };
        if (at == AudioType.UNKNOWN) yield break;

        var clipReq = UnityWebRequestMultimedia.GetAudioClip(uri, at);
        yield return clipReq.SendWebRequest();
        var clip = DownloadHandlerAudioClip.GetContent(clipReq);
        Object.DontDestroyOnLoad(clip);
        callback(clips[sound] = clip);
    }

    private static AudioSource EnsureSource() {
        var source = sources.FirstOrDefault(a => !a.isPlaying);
        if (source != null) return source;

        GameObject sourceObject = new();
        source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.ignoreListenerPause = true;
        source.ignoreListenerVolume = true;
        Object.DontDestroyOnLoad(sourceObject);
        sources.Add(source);
        return source;
    }
}