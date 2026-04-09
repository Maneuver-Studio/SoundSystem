using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Maneuver.SoundSystem
{
    public class UnityWebRequestAudioResolver : IRuntimeAudioResolver
    {
        private readonly Dictionary<string, AudioClip> _cache = new();
        private readonly Dictionary<string, UniTaskCompletionSource<AudioClip>> _inFlight = new();

        public bool IsLoaded(string url)
        {
            return !string.IsNullOrWhiteSpace(url) &&
                   _cache.TryGetValue(url, out var clip) &&
                   clip;
        }

        public async UniTask<AudioClip> GetClip(string url, AudioType audioType, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Audio url cannot be null or empty.", nameof(url));

            if (IsLoaded(url))
                return _cache[url];

            if (_inFlight.TryGetValue(url, out var inFlightTask))
                return await inFlightTask.Task.AttachExternalCancellation(ct);

            var completionSource = new UniTaskCompletionSource<AudioClip>();
            _inFlight[url] = completionSource;

            try
            {
                var clip = await LoadClip(url, audioType, ct);
                completionSource.TrySetResult(clip);
                return clip;
            }
            catch (OperationCanceledException ex)
            {
                completionSource.TrySetCanceled(ex.CancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                completionSource.TrySetException(ex);
                throw;
            }
            finally
            {
                _inFlight.Remove(url);
            }
        }

        public void Release(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            if (!_cache.TryGetValue(url, out var clip))
                return;

            _cache.Remove(url);

            if (clip)
            {
                Resources.UnloadAsset(clip);
            }
        }

        private static string GetClipName(string url)
        {
            try
            {
                return System.IO.Path.GetFileNameWithoutExtension(new Uri(url).AbsolutePath);
            }
            catch
            {
                return "RuntimeAudioClip";
            }
        }

        private async UniTask<AudioClip> LoadClip(string url, AudioType audioType, CancellationToken ct)
        {
            using var request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
            await request.SendWebRequest().WithCancellation(ct);

            if (request.result != UnityWebRequest.Result.Success)
                throw new InvalidOperationException($"Failed to load audio from '{url}': {request.error}");

            var clip = DownloadHandlerAudioClip.GetContent(request);
            if (!clip)
                throw new InvalidOperationException($"No AudioClip was produced for '{url}'.");

            clip.name = GetClipName(url);
            _cache[url] = clip;
            return clip;
        }
    }
}
