# Sound System

`com.maneuver.soundsystem` is a Unity package for centralized audio playback with category-based routing, pooling, crossfade rules, and Zenject integration.

The package is built around a simple idea:

- `AudioFileObject` describes what should be played.
- `AudioCategory` describes how that audio behaves.
- `AudioManager` owns playback, voice limits, category control, and source reuse through `SoundPool`.

With version `1.0.3`, the package also supports runtime audio loading from URL while preserving the same playback pipeline used by regular `AudioFileObject` assets.

## Features

- Reusable `AudioSource` pool to reduce allocation churn.
- Playback grouped by `AudioCategory`.
- Category voice limits and optional crossfade for single-voice categories.
- Category volume control through `AudioMixerGroup`.
- `Zenject` installer for `IAudioManager`.
- Optional helper for UI Toolkit click sounds.
- Runtime playback from CDN or remote URLs.
- Separate runtime resolver contract for download and cache concerns.

## Package Structure

- `AudioManager`: central playback service.
- `IAudioManager`: public playback contract.
- `AudioFileObject`: clip, loop flag, and category.
- `AudioCategory`: mixer group, priority, max voices, and crossfade settings.
- `SoundPool`: pooled `AudioSource` lifecycle.
- `IRuntimeAudioResolver`: runtime clip resolver abstraction.
- `UnityWebRequestAudioResolver`: default runtime resolver implementation.
- `RuntimeAudioFileFactory`: creates temporary `AudioFileObject` instances for runtime-loaded clips.
- `AudioManagerInstaller`: binds `IAudioManager` with Zenject.
- `UIToolkitClickSoundEmitter`: optional helper for UI Toolkit click SFX.

## How It Works

The package separates playback rules from audio asset loading:

- `AudioManager` decides when and how audio should play or stop.
- `SoundPool` reuses `AudioSource` instances.
- `AudioCategory` controls mixer routing, priority, max concurrent voices, and crossfade behavior.
- `IRuntimeAudioResolver` is responsible for downloading, caching, and releasing runtime clips.

This keeps CDN or streaming logic outside the playback core while still reusing the existing mixer, category, and pool behavior.

## Requirements

- Unity `2022.3`
- `Cysharp.Threading.Tasks`
- `Zenject`

The assembly definition already references the dependencies used by the package.

## Basic Setup

1. Create one or more `AudioCategory` assets.
2. Assign an `AudioMixerGroup` to each category when mixer control is needed.
3. Create `AudioFileObject` assets for local clips.
4. Add an `AudioManager` prefab or scene object.
5. Ensure the object also has `SoundPool`.
6. Bind it through `AudioManagerInstaller` if you use Zenject.

## Concepts

### AudioCategory

`AudioCategory` defines runtime behavior for a group of sounds:

- `MixerGroup`: target mixer route.
- `MaxVoices`: max simultaneous sources for the category.
- `UseCrossfade`: useful for music or other single-voice tracks.
- `CrossfadeTime`: fade duration when swapping tracks.
- `Priority`: forwarded to `AudioSource.priority`.

### AudioFileObject

`AudioFileObject` stores:

- `AudioClip`
- loop flag
- `AudioCategory`

This is the default asset-driven path for local project audio.

### Runtime URL Playback

Runtime playback follows this flow:

1. Resolve an `AudioClip` from URL through `IRuntimeAudioResolver`.
2. Create a temporary `AudioFileObject` in memory.
3. Call the same internal `AudioManager` playback path used by local assets.

This means URL-based audio still benefits from:

- source pooling
- category mixer routing
- voice limiting
- crossfade rules

## Examples

### Play a local sound

```csharp
using Maneuver.SoundSystem;
using UnityEngine;
using Zenject;

public class LocalSfxExample : MonoBehaviour
{
    [Inject] private IAudioManager _audioManager;
    [SerializeField] private AudioFileObject _clickSound;

    public void PlayClick()
    {
        _audioManager.Play(_clickSound);
    }
}
```

### Stop a local sound

```csharp
using Cysharp.Threading.Tasks;
using Maneuver.SoundSystem;
using UnityEngine;
using Zenject;

public class StopExample : MonoBehaviour
{
    [Inject] private IAudioManager _audioManager;
    [SerializeField] private AudioFileObject _musicTrack;

    public async UniTask StopMusic()
    {
        await _audioManager.Stop(_musicTrack, fadeOut: 0.5f);
    }
}
```

### Change category volume

```csharp
using Maneuver.SoundSystem;
using UnityEngine;
using Zenject;

public class VolumeExample : MonoBehaviour
{
    [Inject] private IAudioManager _audioManager;
    [SerializeField] private AudioCategory _musicCategory;

    public void SetMusicVolume(float value)
    {
        _audioManager.SetCategoryVolume(_musicCategory, value);
    }
}
```

### Play audio from URL

```csharp
using Cysharp.Threading.Tasks;
using Maneuver.SoundSystem;
using UnityEngine;
using Zenject;

public class RuntimeAudioExample : MonoBehaviour
{
    [Inject] private IAudioManager _audioManager;
    [SerializeField] private AudioCategory _voiceCategory;

    public async UniTask PlayNarration(string url)
    {
        await _audioManager.PreloadFromUrl(url);
        await _audioManager.PlayFromUrl(url, _voiceCategory, loop: false);
    }
}
```

### Query and release runtime cache

```csharp
using Maneuver.SoundSystem;
using UnityEngine;
using Zenject;

public class RuntimeCacheExample : MonoBehaviour
{
    [Inject] private IAudioManager _audioManager;

    public void ReleaseIfLoaded(string url)
    {
        if (_audioManager.IsLoaded(url))
        {
            _audioManager.Release(url);
        }
    }
}
```

### Provide a custom runtime resolver

Use a custom resolver if you need:

- authenticated requests
- signed URLs
- custom retry policies
- local disk caching
- alternative streaming rules

```csharp
using Maneuver.SoundSystem;
using UnityEngine;

public class CustomResolverInstaller : MonoBehaviour
{
    [SerializeField] private AudioManager _audioManager;

    private void Awake()
    {
        _audioManager.SetRuntimeAudioResolver(new UnityWebRequestAudioResolver());
    }
}
```

You can replace `UnityWebRequestAudioResolver` with your own `IRuntimeAudioResolver` implementation.

## UI Toolkit Helper

`UIToolkitClickSoundEmitter` can be attached to a GameObject with `UIDocument`.

It listens for click events and:

- plays a configured `AudioFileObject`
- can optionally restrict playback to `Button` clicks only
- falls back to `AudioSource.PlayClipAtPoint` if `IAudioManager` is not bound

## Zenject Integration

`AudioManagerInstaller` binds `IAudioManager` from a configured `AudioManager` prefab:

```csharp
Container.Bind<IAudioManager>()
    .FromComponentInNewPrefab(_audioManager)
    .AsSingle()
    .NonLazy();
```

## Runtime Audio Notes

- Prefer preload and memory cache for short SFX.
- Prefer streaming-oriented strategies for narration and music.
- Handle timeout, retry, and fallback in custom resolvers when needed.
- Release clips that are no longer needed with `Release(url)`.

## Version `1.0.3`

`1.0.3` adds runtime URL playback support while keeping backward compatibility with the original asset-based API.

See [CHANGELOG.md](./CHANGELOG.md) for the release summary.
