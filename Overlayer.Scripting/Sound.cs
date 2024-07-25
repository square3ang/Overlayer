using UnityEngine;

namespace Overlayer.Scripting
{
    public class Sound
    {
        public Sound() { }
        public string sound;
        public float offset = 0;
        public float volume = 1;
        public float pitch = 1;
        internal AudioClip clip;
        internal Sound SetClip(AudioClip clip)
        {
            this.clip = clip;
            return this;
        }
        public Sound Copy()
        {
            Sound newSound = new Sound();
            newSound.sound = sound;
            newSound.offset = offset;
            newSound.volume = volume;
            newSound.pitch = pitch;
            return newSound;
        }
    }
}
