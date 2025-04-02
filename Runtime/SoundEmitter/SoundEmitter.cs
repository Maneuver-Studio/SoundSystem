using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace Maneuver.SoundSystem
{
    public class SoundEmitter : BaseSoundEmitter
    {
        [SerializeField] private bool _startOnEnable = false;

        private int _lengthAudio;

        private void Awake()
        {
            _lengthAudio = (int)_audioFileObject.Clip.length * 1000;
        }

        private void OnEnable()
        {
            if(!_startOnEnable)
                return;

            Play();
        }

        private void OnDisable()
        {
            
        }

        private void OnDestroyer()
        {
            Stop();
        }
    }
}
