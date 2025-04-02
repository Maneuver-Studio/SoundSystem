using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Maneuver.SoundSystem
{
    public class SoundPool : MonoBehaviour
    {
        public enum PoolType
        {
            Stack,
            LinkedList
        }

        [SerializeField] PoolType _poolType;

        // Collection checks will throw errors if we try to release an item that is already in the pool.
        [SerializeField] private bool _collectionChecks = true;
        [SerializeField] private int _defaultCapacity = 10;
        [SerializeField] private int _maxPoolSize = 1000;
        private const string PREX_NAME = "_AudioSourcePooled";

        private List<AudioSource> _activedAudioSource = new List<AudioSource>();
        public List<AudioSource> ActivedAudioSource => _activedAudioSource;

        private IObjectPool<AudioSource> _pool;
        public IObjectPool<AudioSource> Pool
        {
            get
            {
                if (_pool == null)
                {
                    if (_poolType == PoolType.Stack)
                        _pool = new ObjectPool<AudioSource>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, _collectionChecks, _defaultCapacity, _maxPoolSize);
                    else
                        _pool = new LinkedPool<AudioSource>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, _collectionChecks, _maxPoolSize);
                }
                return _pool;
            }
        }

        private AudioSource CreatePooledItem()
        {
            var go = new GameObject(PREX_NAME);
            var audioSource = go.AddComponent<AudioSource>();

            // TODO:
            // Reset default values
            audioSource.playOnAwake = false;

            _activedAudioSource.Add(audioSource);
            // This is used to return BaseSoundEmitter to the pool when they have stopped.
            return audioSource;
        }

        private void OnTakeFromPool(AudioSource audioSource)
        {
            _activedAudioSource.Add(audioSource);
            audioSource.gameObject.SetActive(true);
        }

        private void OnReturnedToPool(AudioSource audioSource)
        {
            _activedAudioSource.Remove(audioSource);
            audioSource.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(AudioSource audioSource)
        {
            _activedAudioSource.Remove(audioSource);
            GameObject.Destroy(audioSource.gameObject);
        }
    }
}
