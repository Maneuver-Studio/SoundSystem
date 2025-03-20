using UnityEngine;

namespace Maneuver.SoundSystem
{
    public class AudioSourceBuilder
    {
        Transform _parent;
        private AudioFileObject _audio;

        private readonly SoundPool _soundPool;

        public AudioSourceBuilder(SoundPool soundPool) 
        {
            this._soundPool = soundPool;
        }

        public AudioSourceBuilder SetParent(Transform parent) 
        {
            this._parent = parent;
            return this;
        }

        public AudioSourceBuilder SetAudio(ref AudioFileObject audio) 
        {
            this._audio = audio;
            return this;
        }
        
        public AudioSource Builder() 
        {
            var audioSource = _soundPool.Pool.Get();
            
            audioSource.loop = this._audio.IsLoop;
            audioSource.clip = this._audio.Clip;

            audioSource.transform.SetParent(_parent);

            return audioSource;
        }
    }
}
