using UnityEngine;
using Zenject;

namespace Maneuver.SoundSystem
{
    public class AudioManagerInstaller : MonoInstaller
    {
        [SerializeField] private AudioManager _audioManager;

        public override void InstallBindings()
        {
            Container.Bind<IAudioManager>().FromComponentInNewPrefab(_audioManager).AsSingle().NonLazy();
        }
    }
}
