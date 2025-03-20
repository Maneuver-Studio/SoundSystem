using System;
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

        IObjectPool<AudioSource> _pool;
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

            // This is used to return BaseSoundEmitter to the pool when they have stopped.
            return audioSource;
        }

        private void OnTakeFromPool(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(true);
        }

        private void OnReturnedToPool(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(AudioSource audioSource)
        {
            GameObject.Destroy(audioSource.gameObject);
        }
    }
}
