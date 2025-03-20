using UnityEngine;

namespace Maneuver.SoundSystem
{
    [CreateAssetMenu(fileName = "AudioFileObject", menuName = "Scriptable Objects/AudioFileObject")]
    public class AudioFileObject : ScriptableObject
    {
        [SerializeField] private AudioClip _clip;
        public AudioClip Clip => _clip;

        [SerializeField] private bool _isLoop;
        public bool IsLoop => _isLoop; 
    }
}