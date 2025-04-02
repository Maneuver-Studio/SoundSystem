using UnityEngine;

namespace Maneuver.SoundSystem
{
    public interface IAudioManager
    {
        void Play(AudioFileObject audioFile);
        void Stop(AudioFileObject audioFile);
    }
}
