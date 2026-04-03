using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Maneuver.SoundSystem
{
    [RequireComponent(typeof(SoundPool))]
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        private AudioSourceBuilder _audioSourceBuilder;
        private SoundPool _soundPool;

        // categoria -> fontes ativas (em uso)
        private readonly Dictionary<AudioCategory, List<AudioSource>> _activeByCategory = new();

        private void Awake()
        {
            _soundPool = GetComponent<SoundPool>();
            _audioSourceBuilder = new AudioSourceBuilder(_soundPool);
        }

        public async void Play(AudioFileObject audioFile)
        {
            if (!audioFile || !audioFile.Clip) return;

            var cat = audioFile.Category;
            if (!cat)
            {
                Debug.LogWarning($"AudioFileObject '{audioFile.name}' está sem categoria.");
                return;
            }

            var list = GetList(cat);

            // Regras por categoria
            if (cat.MaxVoices <= 1)
            {
                if (cat.UseCrossfade && list.Count > 0)
                {
                    await PlayWithCrossfade(audioFile, list, cat.CrossfadeTime);
                    return;
                }
                else
                {
                    await StopCategory(cat, immediate: true, fadeOut: 0f);
                }
            }
            else
            {
                // Limita vozes (voice stealing simples: para os mais antigos)
                TrimToMax(list, cat.MaxVoices - 1); // deixa 1 slot pro novo
            }

            // Construção e play
            var src = BuildSource(audioFile, transform);
            list.Add(src);

            try
            {
                // Começa no volume alvo (pool já garante src.volume = 1f)
                src.Play();

                // Espera terminar (loop só sai se alguém parar)
                await UniTask.WaitUntil(() => !src || !src.isPlaying);
            }
            finally
            {
                if (src)
                {
                    list.Remove(src);
                    SafeRelease(src); // pool reseta tudo (inclui volume=1f)
                }
            }
        }
        public bool IsPlaying(AudioFileObject audioFile)
        {
            if (!audioFile || !audioFile.Clip) return false;

            var cat = audioFile.Category;
            var list = GetList(cat);

            return list.Any(a => a && a.isPlaying && a.clip == audioFile.Clip);
        }

        public async UniTask Stop(AudioFileObject audioFile, float fadeOut = 0.1f)
        {
            if (!audioFile || !audioFile.Clip) return;
            var cat = audioFile.Category;
            var list = GetList(cat);

            var src = list.FirstOrDefault(a => a && a.clip == audioFile.Clip);
            if (!src) return;

            if (fadeOut > 0f) await FadeOutAndStop(src, fadeOut);
            else src.Stop();

            list.Remove(src);
            SafeRelease(src);
        }
        public bool IsAnythingPlaying()
        {
            return _activeByCategory.Values
                .Any(list => list.Any(s => s && s.isPlaying));
        }
        public UniTask WaitUntilFinished(AudioFileObject audioFile)
        {
            if (!audioFile || !audioFile.Clip) return UniTask.CompletedTask;

            var cat = audioFile.Category;
            var list = GetList(cat);

            return UniTask.WaitUntil(() =>
                !list.Any(a => a && a.isPlaying && a.clip == audioFile.Clip));
        }

        public UniTask WaitUntilAllFinished()
        {
            return UniTask.WaitUntil(() =>
                !_activeByCategory.Values
                    .Any(list => list.Any(s => s && s.isPlaying)));
        }
        // --------- Categoria ---------

        public UniTask WaitUntilCategoryFinished(AudioCategory category)
        {
            if (!category) return UniTask.CompletedTask;

            var list = GetList(category);

            return UniTask.WaitUntil(() =>
                !list.Any(s => s && s.isPlaying));
        }

        public bool IsCategoryPlaying(AudioCategory category)
        {
            if (!category) return false;

            var list = GetList(category);
            return list.Any(s => s && s.isPlaying);
        }

        public async UniTask StopCategory(AudioCategory category, bool immediate = false, float fadeOut = 0.2f)
        {
            if (!category) return;
            var list = GetList(category);
            if (list.Count == 0) return;

            var copy = list.ToList();
            list.Clear();

            foreach (var s in copy)
            {
                if (!s) continue;
                if (immediate || fadeOut <= 0f) s.Stop();
                else await FadeOutAndStop(s, fadeOut);
                SafeRelease(s);
            }
        }

        public async UniTask StopAll(bool immediate = false, float fadeOut = 0.2f)
        {
            var cats = _activeByCategory.Keys.ToList();
            foreach (var cat in cats)
                await StopCategory(cat, immediate, fadeOut);
        }

        // Controle via AudioMixer (volume por categoria)
        public void SetCategoryVolume(AudioCategory category, float volume01)
        {
            if (!category || !category.MixerGroup) return;
            var mixer = category.MixerGroup.audioMixer;
            float dB = Mathf.Log10(Mathf.Clamp(volume01, 0.001f, 1f)) * 20f; // 0..1 -> -60..0 dB
            mixer.SetFloat($"{category.name}Volume", dB);
        }

        public float GetCategoryVolume(AudioCategory category)
        {
            if (!category || !category.MixerGroup) return 1f;
            if (category.MixerGroup.audioMixer.GetFloat($"{category.name}Volume", out var dB))
                return Mathf.Pow(10f, dB / 20f);
            return 1f;
        }

        // --------- Internals ---------

        private List<AudioSource> GetList(AudioCategory cat)
        {
            if (!_activeByCategory.TryGetValue(cat, out var list))
            {
                list = new List<AudioSource>();
                _activeByCategory.Add(cat, list);
            }
            return list;
        }

        private void TrimToMax(List<AudioSource> list, int maxAlive)
        {
            var playing = list.Where(s => s && s.isPlaying).ToList();
            int excess = playing.Count - maxAlive;
            for (int i = 0; i < excess; i++)
            {
                var s = playing[i];
                if (!s) continue;
                s.Stop();
                list.Remove(s);
                SafeRelease(s);
            }
        }

        private async UniTask PlayWithCrossfade(AudioFileObject file, List<AudioSource> list, float time)
        {
            var next = BuildSource(file, transform);
            list.Add(next);

            // alvo de volume da nova trilha (geralmente 1f; o ganho por categoria vem do Mixer)
            float targetVolume = 1f;

            // começa em 0 e vai até o alvo
            next.volume = 0f;
            next.Play();

            // quem estiver tocando na mesma categoria recebe fade out
            var fadingOut = list.Where(s => s && s != next && s.isPlaying).ToList();

            var fadeInTask = FadeTo(next, targetVolume, time);
            var fadeOutTasks = fadingOut.Select(s => FadeOutAndStop(s, time));

            await UniTask.WhenAll(fadeInTask, UniTask.WhenAll(fadeOutTasks));

            // libera os antigos
            foreach (var s in fadingOut)
            {
                list.Remove(s);
                SafeRelease(s);
            }

            try
            {
                await UniTask.WaitUntil(() => !next || !next.isPlaying);
            }
            finally
            {
                if (next)
                {
                    list.Remove(next);
                    SafeRelease(next);
                }
            }
        }

        private AudioSource BuildSource(AudioFileObject audioFile, Transform parent)
        {
            var src = _audioSourceBuilder
                .SetParent(parent)
                .SetAudio(ref audioFile)
                .Builder();

            var cat = audioFile.Category;

            // Roteamento e prioridade por categoria
            if (cat && cat.MixerGroup) src.outputAudioMixerGroup = cat.MixerGroup;
            src.priority = cat ? cat.Priority : 128;

            // Deixa volume em 1f (o nível “musical” vem do Mixer e/ou do próprio clip)
            // O pool garante volume=1f no OnTake, mas reafirmamos por segurança.
            src.volume = 1f;

            return src;
        }

        private async UniTask FadeTo(AudioSource s, float target, float time)
        {
            if (!s) return;
            float start = s.volume;
            float t = 0f;
            while (t < time && s)
            {
                t += Time.unscaledDeltaTime;
                s.volume = Mathf.Lerp(start, target, t / time);
                await UniTask.Yield();
            }
            if (s) s.volume = target;
        }

        private async UniTask FadeOutAndStop(AudioSource s, float time)
        {
            if (!s) return;
            float start = s.volume;
            float t = 0f;
            while (t < time && s && s.isPlaying)
            {
                t += Time.unscaledDeltaTime;
                s.volume = Mathf.Lerp(start, 0f, t / time);
                await UniTask.Yield();
            }
            if (s) s.Stop();
            // Não precisa restaurar volume aqui: o POOL zera/normaliza no OnReturnedToPool
        }

        private void SafeRelease(AudioSource s)
        {
            try { _soundPool.Pool.Release(s); }
            catch { /* Pool pode ter sido destruído no teardown */ }
        }
    }
}
