using UnityEngine;

namespace Maneuver.SoundSystem
{
    public static class RuntimeAudioFileFactory
    {
        public static AudioFileObject Create(AudioClip clip, bool isLoop, AudioCategory category)
        {
            var runtimeFile = ScriptableObject.CreateInstance<AudioFileObject>();
            runtimeFile.Initialize(clip, isLoop, category);
            return runtimeFile;
        }

        public static void Release(AudioFileObject runtimeFile)
        {
            if (runtimeFile)
            {
                Object.Destroy(runtimeFile);
            }
        }
    }
}
