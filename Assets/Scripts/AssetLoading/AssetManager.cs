using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ARCircuitsLab.Events;

namespace ARCircuitsLab.AssetLoading
{
    /// <summary>
    /// Asset Manager: the only class that loads and releases Addressable assets.
    /// - "audio" label: loaded once at start-up (sounds are needed on every screen).
    /// - "lab" label: 3D models, loaded when AR setup starts and released
    ///   when the student returns to the main menu, to save phone memory.
    /// </summary>
    public class AssetManager : MonoBehaviour
    {
        public const string AudioLabel = "audio";
        public const string LabLabel = "lab";

        private AsyncOperationHandle<IList<AudioClip>> audioHandle;
        private AsyncOperationHandle<IList<GameObject>> labHandle;

        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

        public bool CoreLoaded { get; private set; }
        public bool LabLoaded { get; private set; }

        private async void Start()
        {
            await LoadCoreAssetsAsync();
        }

        private void OnDestroy()
        {
            ReleaseLabAssets();
            if (audioHandle.IsValid()) Addressables.Release(audioHandle);
        }

        public async Task LoadCoreAssetsAsync()
        {
            if (CoreLoaded) return;
            audioHandle = Addressables.LoadAssetsAsync<AudioClip>(AudioLabel, clip => clips[clip.name] = clip);
            await audioHandle.Task;
            CoreLoaded = true;
            Debug.Log($"[AssetManager] Loaded {clips.Count} audio clips (label '{AudioLabel}')");
            GameEvents.RaiseCoreAssetsLoaded();
        }

        public async Task LoadLabAssetsAsync()
        {
            if (LabLoaded || labHandle.IsValid()) return;
            labHandle = Addressables.LoadAssetsAsync<GameObject>(LabLabel, prefab => prefabs[prefab.name] = prefab);
            await labHandle.Task;
            LabLoaded = true;
            Debug.Log($"[AssetManager] Loaded {prefabs.Count} lab prefabs (label '{LabLabel}')");
            GameEvents.RaiseLabAssetsLoaded();
        }

        public void ReleaseLabAssets()
        {
            if (!labHandle.IsValid()) return;
            Addressables.Release(labHandle);
            prefabs.Clear();
            LabLoaded = false;
            Debug.Log("[AssetManager] Released lab assets");
            GameEvents.RaiseLabAssetsReleased();
        }

        public GameObject GetPrefab(string assetName) =>
            prefabs.TryGetValue(assetName, out GameObject p) ? p : null;

        public AudioClip GetClip(string assetName) =>
            clips.TryGetValue(assetName, out AudioClip c) ? c : null;
    }
}
