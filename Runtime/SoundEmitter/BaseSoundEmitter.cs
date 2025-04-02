using UnityEngine;
using Zenject;

namespace Maneuver.SoundSystem
{
    public abstract class BaseSoundEmitter : MonoBehaviour
    {
        [Inject] protected IAudioManager _audioManager;
        [SerializeField] protected AudioFileObject _audioFileObject;

        public virtual void Play()
        {
            _audioManager.Play(_audioFileObject);
        }

        public virtual void Stop()
        {
            _audioManager.Stop(_audioFileObject);
        }
    }
}