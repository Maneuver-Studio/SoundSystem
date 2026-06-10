using UnityEngine;

namespace Maneuver.SoundSystem
{
    public struct PlaybackInfo
    {
        public float CurrentTime;
        public float Duration;
        public float Progress;

        public static PlaybackInfo FromSource(AudioSource source)
        {
            if (!source || !source.clip || !source.isPlaying)
                return default;

            float duration = source.clip.length;
            float current = source.time;

            return new PlaybackInfo
            {
                CurrentTime = current,
                Duration = duration,
                Progress = duration > 0f ? current / duration : 0f,
            };
        }
    }
}
