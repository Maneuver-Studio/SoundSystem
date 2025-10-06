using UnityEngine;
using UnityEngine.Audio;

namespace Maneuver.SoundSystem
{
    [CreateAssetMenu(fileName = "AudioCategory", menuName = "Sound/Audio Category")]
    public class AudioCategory : ScriptableObject
    {
        [SerializeField] private AudioMixerGroup _mixerGroup;
        public AudioMixerGroup MixerGroup => _mixerGroup;

        [Min(1)][SerializeField] private int _maxVoices = 1; // Music costuma ser 1
        public int MaxVoices => _maxVoices;

        [SerializeField] private bool _useCrossfade = true; // pra Music geralmente true
        public bool UseCrossfade => _useCrossfade;

        [SerializeField, Min(0f)] private float _crossfadeTime = 1.25f;
        public float CrossfadeTime => _crossfadeTime;

        [Range(0,256)][SerializeField] private int _priority = 128;
        public int Priority => _priority;
    }
}