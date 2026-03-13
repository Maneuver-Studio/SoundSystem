using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Maneuver.SoundSystem
{
    /// <summary>
    /// Optional helper that hooks UI Toolkit click events and plays a configured AudioFileObject.
    /// Attach to a GameObject with a UIDocument; assign a sound; done.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class UIToolkitClickSoundEmitter : MonoBehaviour
    {
        [SerializeField] private AudioFileObject _clickSound;
        [SerializeField] private bool _onlyButtons = true;

        [Inject(Optional = true)] private IAudioManager _audioManager;

        private UIDocument _document;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            var root = _document != null ? _document.rootVisualElement : null;
            if (root == null || _clickSound == null)
            {
                return;
            }

            // Listen to all bubbling click events under this document.
            root.RegisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
        }

        private void OnDestroy()
        {
            if (_document?.rootVisualElement != null)
            {
                _document.rootVisualElement.UnregisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
            }
        }

        private void OnClick(ClickEvent evt)
        {
            if (_clickSound == null)
            {
                return;
            }

            if (_onlyButtons && evt.target is not Button)
            {
                return;
            }

            if (_audioManager != null)
            {
                _audioManager.Play(_clickSound);
                return;
            }

            // Fallback when AudioManager is not bound.
            var clip = _clickSound.Clip;
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, Vector3.zero);
            }
        }
    }
}
