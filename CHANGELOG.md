# Changelog

## 1.0.3

- Added runtime audio playback support from URL through `IAudioManager`.
- Added preload, cache inspection, and release operations for runtime-loaded audio clips.
- Added `IRuntimeAudioResolver` and a default `UnityWebRequest`-based resolver to separate playback from download/cache responsibilities.
- Added `RuntimeAudioFileFactory` to create temporary `AudioFileObject` instances for runtime playback while preserving the existing pool, mixer, and category flow.
