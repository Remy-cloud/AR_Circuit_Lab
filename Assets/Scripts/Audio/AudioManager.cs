using System.Collections.Generic;
using UnityEngine;
using ARCircuitsLab.AssetLoading;
using ARCircuitsLab.Events;

namespace ARCircuitsLab.Audio
{
    /// <summary>
    /// Audio Manager: plays sounds in response to game events.
    /// Clips are loaded through Addressables by the Asset Manager.
    /// Volume is kept in PlayerPrefs between launches.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        private const string VolumeKey = "volume";

        [SerializeField] private AssetManager assetManager;

        private AudioSource source;
        private readonly Dictionary<SoundId, AudioClip> clips = new Dictionary<SoundId, AudioClip>();

        public float Volume
        {
            get => source.volume;
            set
            {
                source.volume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(VolumeKey, source.volume);
            }
        }

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
        }

        private void OnEnable()
        {
            GameEvents.CoreAssetsLoaded += CacheClips;
            GameEvents.ButtonClicked += OnButton;
            GameEvents.BenchPlaced += OnBenchPlaced;
            GameEvents.ComponentPlaced += OnComponentPlaced;
            GameEvents.WireConnected += OnWireConnected;
            GameEvents.SwitchToggled += OnSwitchToggled;
            GameEvents.ShortCircuit += OnShortCircuit;
            GameEvents.TaskCompleted += OnTaskCompleted;
            GameEvents.SessionEnded += OnSessionEnded;
        }

        private void OnDisable()
        {
            GameEvents.CoreAssetsLoaded -= CacheClips;
            GameEvents.ButtonClicked -= OnButton;
            GameEvents.BenchPlaced -= OnBenchPlaced;
            GameEvents.ComponentPlaced -= OnComponentPlaced;
            GameEvents.WireConnected -= OnWireConnected;
            GameEvents.SwitchToggled -= OnSwitchToggled;
            GameEvents.ShortCircuit -= OnShortCircuit;
            GameEvents.TaskCompleted -= OnTaskCompleted;
            GameEvents.SessionEnded -= OnSessionEnded;
        }

        public void Play(SoundId id)
        {
            if (clips.TryGetValue(id, out AudioClip clip) && clip != null)
                source.PlayOneShot(clip);
        }

        private void CacheClips()
        {
            clips.Clear();
            foreach (SoundId id in System.Enum.GetValues(typeof(SoundId)))
                clips[id] = assetManager.GetClip(id.ToString());
        }

        // Each event maps to the sound that best explains what just happened.
        private void OnButton() => Play(SoundId.ui_click);
        private void OnBenchPlaced(Transform _) => Play(SoundId.place);
        private void OnComponentPlaced() => Play(SoundId.place);
        private void OnWireConnected() => Play(SoundId.connect);
        private void OnSwitchToggled(bool _) => Play(SoundId.@switch);
        private void OnShortCircuit() => Play(SoundId.warning);
        private void OnTaskCompleted(int _) => Play(SoundId.task_complete);
        private void OnSessionEnded(ARCircuitsLab.Data.SessionRecord _) => Play(SoundId.session_end);
    }
}
