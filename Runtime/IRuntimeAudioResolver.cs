using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Maneuver.SoundSystem
{
    public interface IRuntimeAudioResolver
    {
        UniTask<AudioClip> GetClip(string url, AudioType audioType, CancellationToken ct = default);
        bool IsLoaded(string url);
        void Release(string url);
    }
}
