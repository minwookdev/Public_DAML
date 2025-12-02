using System;
using UnityEngine;
using UnityEngine.Audio;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;
using Random = UnityEngine.Random;

namespace CoffeeCat.FrameWork { 
    public class SoundManager : DynamicSingleton<SoundManager> {
        [Title("AUDIO GROUP", TitleAlignment = TitleAlignments.Centered)]
        [ShowInInspector, ReadOnly] public Transform AudioCameraTr { get; private set; } = null;
        [ShowInInspector, ReadOnly] public Transform AudioGroupTr { get; private set; } = null;
        [ShowInInspector, ReadOnly] public AudioListener Listener { get; private set; } = null;
        [ShowInInspector, ReadOnly] public AudioSource ChannelBgm { get; private set; } = null;
        [ShowInInspector, ReadOnly] public AudioSource ChannelSe { get; private set; } = null;
        [ShowInInspector, ReadOnly] public AudioSource ChannelAmbient { get; private set; } = null;
        [ShowInInspector, ReadOnly] public AudioMixer MainAudioMixer { get; private set; } = null;
        public Transform Tr { get; private set; } = null;

        [Title("CLIPS")]
        [SerializeField, ReadOnly] private StringAudioClipDictionary audioClipDictionary = null;
        [SerializeField, ReadOnly] private StringAudioClipDictionary globalAudioClipDict = null;
        private readonly object registClipLocker = new();

        #region EXPERIMENTAL (CUSTOM CHANNEL)

        [Title("CUSTOM CHANNEL")]
        [SerializeField, ReadOnly] private StringAudioSourceDictionary customChannelDictionary = null;
        [SerializeField, ReadOnly] private GameObject customChannelOrigin = null;

        public void RegistCustomChannel(string key, AudioClip audioClip, float volume = 1f) {
            if (customChannelDictionary.ContainsKey(key))
                return;

            if (customChannelOrigin == null) {
                customChannelOrigin = ResourceManager.Inst.ResourcesLoad<GameObject>("Audio/AudioChannel_Custom", false);
            }

            var spawnedCustomChannel = Instantiate(customChannelOrigin, Vector3.zero, Quaternion.identity, AudioGroupTr).GetComponent<AudioSource>();
            spawnedCustomChannel.transform.localPosition = Vector3.zero;
            spawnedCustomChannel.volume = volume;
            spawnedCustomChannel.clip = audioClip;
            
            customChannelDictionary.Add(key, spawnedCustomChannel);
        }

        public void PlayCustomChannel(string key) {
            if (!customChannelDictionary.TryGetValue(key, out AudioSource audioSource))
                return;
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            audioSource.Play();
        }

        public void StopCustomChannel(string key) {
            if (!customChannelDictionary.TryGetValue(key, out AudioSource audioSource))
                return;
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
        }

        public void StopAllCustomChannel() {
            foreach (var keyValuePair in customChannelDictionary) {
                var audioSource = keyValuePair.Value;
                if (audioSource.isPlaying) {
                    keyValuePair.Value.Stop();
                }
            }
        }

        public void ReleaseCustomChannel(string key) {
            if (customChannelDictionary.Remove(key, out AudioSource audioSource)) {
                audioSource.transform.SetParent(null);
                audioSource.gameObject.SetActive(false);
            }
        }

        public void ReleaseAllCustomChannel() {
            foreach (var keyValuePair in customChannelDictionary) {
                if (customChannelDictionary.Remove(keyValuePair.Key, out AudioSource audioSource)) {
                    audioSource.transform.SetParent(null);
                    audioSource.gameObject.SetActive(false);
                }
            }
        }

        #endregion

        protected override void Initialize() {
            audioClipDictionary = new StringAudioClipDictionary();
            //customChannelDictionary = new StringAudioSourceDictionary();

            var mainCam = Camera.main;
            if (!mainCam) {
                CatLog.WLog("Main Camera was Not Found !");
                return;
            }
            
            AudioCameraTr = mainCam.GetComponent<Transform>();
            var resourcesLoadAudioGroup = ResourceManager.Inst.ResourcesLoad<GameObject>("Audio/AudioGroup", false);
            if (resourcesLoadAudioGroup == null) {
                CatLog.WLog("AudioGroup Prefab was Not Found in Resources");
                return;
            }

            // Get AudioGroup Components
            AudioGroupTr = Instantiate(resourcesLoadAudioGroup.transform, Vector3.zero, Quaternion.identity, AudioCameraTr);
            var audioGroup = AudioGroupTr.GetComponent<AudioGroup>();
            Listener   = audioGroup.Listener;
            ChannelBgm = audioGroup.ChannelBgm;
            ChannelSe  = audioGroup.ChannelSE;
            ChannelAmbient = audioGroup.ChannelAmbient;
            MainAudioMixer = audioGroup.MainAudioMixer;
            
            // init global audio clips
            InitGlobalAudioClips();
        }

        private void Start() {
            Tr = GetComponent<Transform>();
            SceneManager.Inst.ChangeBeforeEvent += ChangeBeforeEvent;
            SceneManager.Inst.ChangeAfterEvent += ChangeAfterEvent;
        }

        public void RegistAudioClips(StringAudioClipDictionary sceneExistAudioClipDictionary) {
            foreach (var keyValuePair in sceneExistAudioClipDictionary) {
                audioClipDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public void RegistAudioClips(Span<SoundKey> keys) {
            for (int i = 0; i < keys.Length; i++) {
                RegistAudioClip(keys[i]);
            }
        }
        
        public void RegistAudioClip(SoundKey key) {
            string keyString = key.ToKey();
            RegistAudioClip(keyString);
        }
        
        public void RegistAudioClip(string key) {
            if (key == string.Empty || audioClipDictionary.ContainsKey(key)) {
                return;
            }
            
            SafeLoader.Load<AudioClip>(key, false, (clip) => {
                if (!clip) {
                    CatLog.WLog($"AudioClip Not Found in Addressables. key : {key}");
                    return;
                }
                RegistAudioClip(key, clip);
            });
        }
        
        private void RegistAudioClip(string key, AudioClip clip) {
            lock (registClipLocker) {
                var successed = audioClipDictionary.TryAdd(key, clip);
                // if (successed) {
                //     CatLog.Log($"load completed: {key}");
                // }
            }
        }

        #region SE

        public void PlayCurrencyUsedSE() => PlayGlobalSE(SoundKey.Currency_Used);

        public void PlayDefaultItemSE() => PlayGlobalSE(SoundKey.Item_Equipment);

        public void PlayConsumeItemSE() => PlayGlobalSE(SoundKey.Item_Consumable);

        public void PlayExpRandomSE() {
            int randomValue = Random.Range(0, 3);
            SoundKey key = randomValue switch {
                0 => SoundKey.EXP_0,
                1 => SoundKey.EXP_1,
                2 => SoundKey.EXP_2,
                _ => SoundKey.EXP_0
            };
            PlayGlobalSE(key, 0.6f);
        }

        public void PlayCurrencyRandomSE() {
            int randomValue = Random.Range(0, 3);
            SoundKey key = randomValue switch {
                0 => SoundKey.Currency_0,
                1 => SoundKey.Currency_1,
                2 => SoundKey.Currency_2,
                _ => SoundKey.Currency_0
            };
            PlayGlobalSE(key, 0.8f);
        }
        
        public void PlayButtonSE(bool isPositiveType) {
            var key = isPositiveType ? SoundKey.Button_0 : SoundKey.Button_1;
            PlayGlobalSE(key);
        }
        
        private void PlayGlobalSE(SoundKey key, float volume = 1f) {
            if (!globalAudioClipDict.TryGetValue(key.ToKey(), out AudioClip clip)) {
                return;
            }

            PlaySE(clip, volume);
        }

        public void PlaySE(string key, float volume = 1f) {
            if (!audioClipDictionary.TryGetValue(key, out AudioClip clip)) {
                CatLog.WLog($"AudioClip Not Exist in Dictioanry. key : {key}");
                return;
            }

            PlaySE(clip, volume);
        }

        public void PlaySE(AudioClip clip, float volume = 1f) {
            ChannelSe.PlayOneShot(clip, volume);
        }

        #endregion

        #region BGM

        public void PlayBgm(string key, float volume = 1f, bool isLoop = true) {
            if (!audioClipDictionary.TryGetValue(key, out AudioClip clip)) {
                CatLog.ELog($"AudioClip Not Exist in Dictioanry. key : {key}");
                return;
            }

            PlayBgm(clip, volume);
        }

        public void PlayBgm(AudioClip clip, float volume = 1f, bool isLoop = true) {
            if (ChannelBgm.isPlaying) {
                ChannelBgm.Stop();
            }

            ChannelBgm.volume = volume;
            ChannelBgm.loop = isLoop;
            ChannelBgm.clip = clip;
            ChannelBgm.Play();
        }

        public void StopBgm(bool isReleaseClip = true) {
            ChannelBgm.Stop();
            if (isReleaseClip) {
                ChannelBgm.clip = null;
            }
        }

        public void FadeInBgm(bool isStop) {

        }

        public void FadeOutBgm() {

        }

        #endregion

        #region AMBIENT

        public void PlayAmbient(string key, float volume = 1f, bool isLoop = true) {
            if (!audioClipDictionary.TryGetValue(key, out AudioClip clip)) {
                CatLog.ELog($"AudioClip Not Exist in Dictioanry. key : {key}");
                return;
            }

            PlayAmbient(clip, volume);
        }

        public void PlayAmbient(AudioClip clip, float volume = 1f, bool isLoop = true) {
            if (ChannelAmbient.isPlaying) {
                ChannelAmbient.Stop();
            }
            
            ChannelAmbient.volume = volume;
            ChannelAmbient.loop = isLoop;
            ChannelAmbient.clip = clip;
            ChannelAmbient.Play();
        }

        public void StopAmbient(bool isReleaseClip = true) {
            ChannelAmbient.Stop();
            if (isReleaseClip) {
                ChannelAmbient.clip = null;
            }
        }

        #endregion

        private void ChangeBeforeEvent(SceneName sceneName) {
            ClearAudioClipDictionary();
            ReleaseParentAudioGroup();
        }

        private void ChangeAfterEvent(SceneName sceneName) {
            AttatchAudioGroupToMainCamera();
        }

        private void ClearAudioClipDictionary() => audioClipDictionary.Clear();

        private void ReleaseParentAudioGroup() {
            AudioGroupTr.SetParent(Tr);
            AudioCameraTr = null;
        }

        private void AttatchAudioGroupToMainCamera() {
            var mainCam = Camera.main;
            if (!mainCam) {
                CatLog.WLog("Main Camera was Not Found !");
                return;
            }
            
            AudioCameraTr = mainCam.transform;
            AudioGroupTr.SetParent(AudioCameraTr);
            AudioGroupTr.localPosition = Vector3.zero;
            AudioGroupTr.localRotation = Quaternion.identity;
        }

        private void InitGlobalAudioClips() {
            globalAudioClipDict = new();
            LoadClip(SoundKey.Button_0.ToKey());
            LoadClip(SoundKey.Button_1.ToKey());
            LoadClip(SoundKey.Currency_0.ToKey());
            LoadClip(SoundKey.Currency_1.ToKey());
            LoadClip(SoundKey.Currency_2.ToKey());
            LoadClip(SoundKey.EXP_0.ToKey());
            LoadClip(SoundKey.EXP_1.ToKey());
            LoadClip(SoundKey.EXP_2.ToKey());
            LoadClip(SoundKey.Item_Consumable.ToKey());
            LoadClip(SoundKey.Item_Equipment.ToKey());
        }

        private void LoadClip(string key) {
            SafeLoader.Load<AudioClip>(key, true, (loadedClip) => {
                if (globalAudioClipDict.TryAdd(loadedClip.name, loadedClip)) {
                    return;
                }
                CatLog.WLog("Already Exist AudioClip in Global Dictionary " + loadedClip.name);
            });
        }

        public void PlayBGM(object town)
        {
            throw new NotImplementedException();
        }
    }
}
