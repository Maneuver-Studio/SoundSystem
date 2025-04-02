using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Maneuver.SoundSystem
{
    [RequireComponent(typeof(SoundPool))]
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        private AudioSourceBuilder _audioSourceBuilder;
        private SoundPool _soundPool;

        private void Awake()
        {
            _soundPool = GetComponent<SoundPool>();
            _audioSourceBuilder = new AudioSourceBuilder(_soundPool);
        }

        public async void Play(AudioFileObject audioFile)
        {
            Debug.Log($"{audioFile.Clip.name}");

            var audioSource = _audioSourceBuilder
            .SetParent(transform)
            .SetAudio(ref audioFile)
            .Builder();
            
            audioSource.Play();   

            do
            {
                await UniTask.Delay(Mathf.CeilToInt(audioFile.Clip.length * 1000));
            } while (audioFile.IsLoop &&  audioSource.isPlaying);

            try
            {
                _soundPool.Pool.Release(audioSource);
            }
            catch (System.Exception)
            {
            }
        }

        public void Stop(AudioFileObject audioFile)
        {
            var audioSource = _soundPool.ActivedAudioSource.First(a => a.clip == audioFile.Clip);

            audioSource.Stop();
            _soundPool.Pool.Release(audioSource);
        }
    }
}
