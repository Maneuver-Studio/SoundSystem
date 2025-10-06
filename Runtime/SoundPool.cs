using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Maneuver.SoundSystem
{
    public class SoundPool : MonoBehaviour
    {
        public enum PoolType { Stack, LinkedList }

        [SerializeField] private PoolType _poolType;
        [SerializeField] private bool _collectionChecks = true;
        [SerializeField] private int _defaultCapacity = 10;
        [SerializeField] private int _maxPoolSize = 1000;

        private const string PREFIX_NAME = "_AudioSourcePooled";

        // "Ativos" = em uso (pegos do pool)
        private readonly List<AudioSource> _activeAudioSources = new();
        public List<AudioSource> ActivedAudioSource => _activeAudioSources;

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
            var go = new GameObject(PREFIX_NAME);
            go.transform.SetParent(transform, false);

            var src = go.AddComponent<AudioSource>();

            // Defaults "estáticos" (seta 1x na criação)
            src.playOnAwake = false;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            src.dopplerLevel = 0f;

            // Garanta um estado neutro inicial
            ResetSourceRuntimeState(src);

            // IMPORTANTE: NÃO adicionar em _activeAudioSources aqui.
            // Recurso ainda NÃO está em uso; só será em OnTakeFromPool.
            go.SetActive(false);
            return src;
        }

        private void OnTakeFromPool(AudioSource src)
        {
            // Volta a ser filho do pool (útil se alguém reparentou temporariamente)
            if (src.transform.parent == null) src.transform.SetParent(transform, false);

            src.gameObject.name = PREFIX_NAME; // opcional
            src.gameObject.SetActive(true);

            // Garante estado "limpo" antes de uso
            ResetSourceRuntimeState(src);

            _activeAudioSources.Add(src);
        }

        private void OnReturnedToPool(AudioSource src)
        {
            // Para e reseta para não "grudar" volume=0 ou clip antigo
            SafeStopAndReset(src);

            // Deixa guardado no pool como filho do root do pool
            src.transform.SetParent(transform, false);
            src.gameObject.SetActive(false);

            _activeAudioSources.Remove(src);
        }

        private void OnDestroyPoolObject(AudioSource src)
        {
            _activeAudioSources.Remove(src);
            if (src) Destroy(src.gameObject);
        }

        // ----------------- Helpers -----------------

        private static void ResetSourceRuntimeState(AudioSource s)
        {
            // Estes estados podem ter sido alterados por fades/crossfades/reprodução anterior
            s.volume = 1f;      // <- crítico para teu bug
            s.pitch = 1f;
            s.loop = false;
            s.clip = null;
            s.time = 0f;
            s.timeSamples = 0;

            // 2D por padrão (ajusta ao tocar se for 3D)
            s.spatialBlend = 0f;

            // Roteamento/mixer é dinâmico, então zera aqui.
            s.outputAudioMixerGroup = null;

            // Pan e spread neutros
            s.panStereo = 0f;
            s.spread = 0f;
        }

        private static void SafeStopAndReset(AudioSource s)
        {
            if (!s) return;

            // Se estiver tocando/fazendo fade, interrompe
            s.Stop();

            // Reset runtime para próxima reutilização
            ResetSourceRuntimeState(s);
        }
    }
}
