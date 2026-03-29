using UnityEngine;

namespace Overlayer.Core.Scripting;

public class Sound {
    public Sound() { }
    public string sound;
    public float offset = 0;
    public float volume = 1;
    public float pitch = 1;
    internal AudioClip clip;
    internal Sound SetClip(AudioClip clip) {
        this.clip = clip;
        return this;
    }
    public Sound Copy() {
        Sound newSound = new() {
            sound = sound,
            offset = offset,
            volume = volume,
            pitch = pitch
        };
        return newSound;
    }
}
