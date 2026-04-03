using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Maneuver.SoundSystem
{
    public interface IAudioManager
    {
        void Play(AudioFileObject audioFile);

        UniTask Stop(AudioFileObject audioFile, float fadeOut = 0.1f);
        UniTask StopCategory(AudioCategory category, bool immediate = false, float fadeOut = 0.2f);
        UniTask StopAll(bool immediate = false, float fadeOut = 0.2f);

        void SetCategoryVolume(AudioCategory category, float volume);
        float GetCategoryVolume(AudioCategory category);

        bool IsPlaying(AudioFileObject audioFile);
        bool IsCategoryPlaying(AudioCategory category);
        bool IsAnythingPlaying();

        UniTask WaitUntilFinished(AudioFileObject audioFile);
        UniTask WaitUntilCategoryFinished(AudioCategory category);
        UniTask WaitUntilAllFinished();
    }
}