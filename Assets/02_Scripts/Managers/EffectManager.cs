using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UniRx;
using CoffeeCat.Utils;
using CoffeeCat.Utils.SerializedDictionaries;
using System.Collections.Generic;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat.FrameWork {
    // 
    // !동일한 씬에서 Effect Release 후 다시 Regist 했을 때 문제가 없는지 체크
    // !Play 메서드와 PlayAsync메서드 결합
    // !한 이펙트를 여러 사용자가 갖게될 경우 관리 방안 마련
    // !ResourceManager와 분리하기

    public class EffectManager : DynamicSingleton<EffectManager> {
        [SerializeField, ReadOnly] StringEffectInformationDictionary effectInfoDictionary = null;

        [Serializable]
        public class EffectInfo {
            [ShowInInspector] public string SpawnKey { get; private set; } = string.Empty;
            [ShowInInspector] public Effector Effector { get; private set; } = null;
            [ShowInInspector] public bool IsReady { get; private set; } = false;
            [ShowInInspector] public int UsersCount { get; private set; } = 0;

            public static EffectInfo New(string spawnKey, Effector effector) {
                return new EffectInfo() {
                    SpawnKey = spawnKey,
                    Effector = effector,
                    UsersCount = 1
                };
            }

            public void Release() {
                //Destroy(Effector);
                Effector = null;
            }

            public void SetIsReady(bool isReady) => this.IsReady = isReady;

            public void AddUserCount() => UsersCount++;
        }

        protected override void Initialize() {
            effectInfoDictionary = new StringEffectInformationDictionary();
        }

        private void Start() {
            // Scene Change Event Listen
            SceneManager.Inst.ChangeBeforeEvent += ChangeBeforeEvent;
            SceneManager.Inst.ChangeAfterEvent += ChangeAfterEvent;

            SubscribeCheckDuplicatedEffectGameObjectInDictionaryObservable();
        }

        #region ASSIGNMENTS

        public void RegistAddressablesAsync(string addressablesName) {
            string key = addressablesName;
            if (IsAlreadyRegisted(key)) {
                return;
            }

            ResourceManager.Inst.AddressablesAsyncLoad<GameObject>(key, false, (loadedGameObject) => {
                if (!loadedGameObject) {
                    CatLog.ELog($"EffectObject Load Failed. name: {key}");
                    return;
                }
                
                if (!loadedGameObject.TryGetComponent(out Effector loadedEffector)) {
                    CatLog.ELog($"this GameObject is Not Exist Effector Component. name: {loadedGameObject.name}");
                    return;
                }
                Regist(loadedEffector, key);
            });
        }

        public void RegistAddressablesSync(string addressablesName) {
            /*string key = addressablesName;
            if (IsAlreadyRegisted(key)) {
                return;
            }

            var effectGameObject = ResourceManager.Instance.AddressablesSyncLoad<GameObject>(key, false);
            if (effectGameObject.TryGetComponent(out Effector effector)) {
                CatLog.ELog($"this GameObject is Not Exist Effector Component. name: {effectGameObject.name}");
                return;
            }
            Regist(effector, key);*/
        }

        public string Regist(Effector effector) {
            string key = effector.gameObject.name;
            if (IsAlreadyRegisted(key)) {
                return string.Empty;
            }
            this.Regist(effector, key);
            return key;
        }

        private void Regist(Effector effector, string key) {
            var newEffectInformation = EffectInfo.New(effector.gameObject.name, effector);
            effectInfoDictionary.Add(key, newEffectInformation);
            ObjectPoolManager.Inst.AddToPool(PoolInfo.Create(effector.gameObject));
            newEffectInformation.SetIsReady(true);
        }

        private bool IsAlreadyRegisted(string key) {
            if (effectInfoDictionary.ContainsKey(key)) {
                CatLog.Log($"This Effect is Already Registed. key: {key}. Add User's Count");
                effectInfoDictionary[key].AddUserCount();
                return true;
            }

            return false;
        }

        #endregion

        #region RELEASE

        public void ReleaseEffect(string key, bool isForceStopEffects = false) {
            if (!effectInfoDictionary.ContainsKey(key))
                return;

            // Is Force Stop This Effects
            if (isForceStopEffects) {
                StopAll(key);
            }

            effectInfoDictionary[key].Release();
            effectInfoDictionary.Remove(key);
        }

        #endregion

        #region PLAY

        /// <summary>
        /// Play Effect (Change Parent)
        /// </summary>
        /// <param name="key"> key is Effector GameObject's Name </param>
        public void Play(string key, Vector3 position, Quaternion rotation, Transform parent, EffectPlayOptions playOptions = default(EffectPlayOptions)) {
            if (!IsPlayable(key)) {
                return;
            }
            EffectSpawnWithPlay(key, position, rotation, parent, playOptions);
        }

        /// <summary>
        /// Play Effect (Not Change Parent)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void Play(string key, Vector3 position, Quaternion rotation, EffectPlayOptions playOptions = default(EffectPlayOptions)) {
            if (!IsPlayable(key)) {
                return;
            }
            EffectSpawnWithPlay(key, position, rotation, playOptions);
        }

        /// <summary>
        /// Play Effect After Load Completely (Change Parent) (Confirmed). 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <param name="playOptions"></param>
        public void PlayAsync(string key, Vector3 position, Quaternion rotation, Transform parent, EffectPlayOptions playOptions = default(EffectPlayOptions)) {
            StartCoroutine(WaitPlayableAsync(key, () => {
                this.EffectSpawnWithPlay(key, position, rotation, parent, playOptions);
            }));
        }

        /// <summary>
        /// Play Effect After Load Completely (Not Change Parent) (Confirmed). 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="playOptions"></param>
        public void PlayAsync(string key, Vector3 position, Quaternion rotation, EffectPlayOptions playOptions = default(EffectPlayOptions)) {
            StartCoroutine(WaitPlayableAsync(key, () => {
                this.EffectSpawnWithPlay(key, position, rotation, playOptions);
            }));
        }

        /// <summary>
        /// Play Effect With Return Effector Component. (Change Parent)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <param name="playOptions"></param>
        /// <returns></returns>
        public Effector PlayWithGetter(string key, Vector3 position, Quaternion rotation, Transform parent, EffectPlayOptions playOptions = default(EffectPlayOptions)) {
            if (!IsPlayable(key)) {
                return null;
            }
            return EffectSpawnWithPlayGetter(key, position, rotation, parent, playOptions);
        }

        /// <summary>
        /// Play Effect With Return Effector Component. (Not Change Parent)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="playOptions"></param>
        /// <returns></returns>
        public Effector PlayWithGetter(string key, Vector3 position, Quaternion rotation, EffectPlayOptions playOptions = default(EffectPlayOptions)) {
            if (!IsPlayable(key)) {
                return null;
            }
            return EffectSpawnWithPlayGetter(key, position, rotation, playOptions);
        }

        #endregion

        #region PLAY ABOUT FUNCTIONS

        private bool IsPlayable(string key) => effectInfoDictionary.ContainsKey(key) && effectInfoDictionary[key].IsReady;

        private Effector EffectSpawnWithPlayGetter(string key, Vector3 position, Quaternion rotation, Transform parent, EffectPlayOptions playOptions) {
            var effector = ObjectPoolManager.Inst.Spawn<Effector>(effectInfoDictionary[key].SpawnKey, position, rotation, parent);
            effector?.Play(playOptions);
            return effector;
        }

        private Effector EffectSpawnWithPlayGetter(string key, Vector3 position, Quaternion rotation, EffectPlayOptions playOptions) {
            var effector = ObjectPoolManager.Inst.Spawn<Effector>(effectInfoDictionary[key].SpawnKey, position, rotation);
            effector?.Play(playOptions);
            return effector;
        }

        private void EffectSpawnWithPlay(string key, Vector3 position, Quaternion rotation, Transform parent, EffectPlayOptions playOptions) {
            var effector = ObjectPoolManager.Inst.Spawn<Effector>(effectInfoDictionary[key].SpawnKey, position, rotation, parent);
            effector?.Play(playOptions);
        }

        private void EffectSpawnWithPlay(string key, Vector3 position, Quaternion rotation, EffectPlayOptions playOptions) {
            var effector = ObjectPoolManager.Inst.Spawn<Effector>(effectInfoDictionary[key].SpawnKey, position, rotation);
            effector?.Play(playOptions);
        }

        System.Collections.IEnumerator WaitPlayableAsync(string key, Action onCompleted) {
            float waitSeconds = 1f;

            while (waitSeconds > 0f) {
                if (this.IsPlayable(key)) {
                    onCompleted.Invoke();
                    yield break;
                }

                waitSeconds -= Time.deltaTime;
                yield return null;
            }

            yield return null;
        }

        #endregion

        #region STOP

        public void StopAll(string key) {
            var activatedEffects = ObjectPoolManager.Inst.GetActiveObjectsOrEmptyFromKey(key);
            if (activatedEffects == null) {
                return;
            }

            for (int i = 0; i < activatedEffects.Length; i++) {
                if (activatedEffects[i].TryGetComponent(out Effector effector)) {
                    effector.Stop();
                }
            }
        }

        public void StopAll() {
            foreach (var keyValuePair in effectInfoDictionary) {
                var activatedEffects = ObjectPoolManager.Inst.GetActiveObjectsOrEmptyFromKey(keyValuePair.Value.SpawnKey);
                if (activatedEffects == null) {
                    continue;
                }

                for (int i = 0; i < activatedEffects.Length; i++) {
                    if (activatedEffects[i].TryGetComponent(out Effector effector)) {
                        effector.Stop();
                    }
                }
            }
        }

        #endregion

        private void ChangeBeforeEvent(SceneName currentScene) {
            this.BlockUsingAllEffects();
        }

        private void ChangeAfterEvent(SceneName nextScene) {
            if (nextScene.Equals(SceneName.LoadingScene))
                return;

            this.AddToPoolAllEffects();
        }

        private void BlockUsingAllEffects() {
            foreach (var keyValuePair in effectInfoDictionary) {
                keyValuePair.Value.SetIsReady(false);
            }
        }

        private void AddToPoolAllEffects() {
            foreach (var keyValuePair in effectInfoDictionary) {
                var effectInfo = keyValuePair.Value;
                ObjectPoolManager.Inst.AddToPool(PoolInfo.Create(effectInfo.Effector.gameObject));
                effectInfo.SetIsReady(true);
            }
        }

        private void SubscribeCheckDuplicatedEffectGameObjectInDictionaryObservable() {
            int lastCheckedCount = 0;
            List<string> tempStrList = new List<string>();
            this.ObserveEveryValueChanged(_ => effectInfoDictionary.Count)
                .Skip(TimeSpan.Zero)
                .TakeUntilDestroy(this)
                .Subscribe(dictionaryCount => {
                    // Dictionary Added
                    if (lastCheckedCount < dictionaryCount) {
                        foreach (var value in effectInfoDictionary.Values) {
                            string effectName = value.Effector.gameObject.name;
                            for (int i = 0; i < tempStrList.Count; i++) {
                                if (tempStrList[i] == effectName) {
                                    CatLog.ELog($"EffectManager: Duplicated Effect GameObject added in Dictionary. name: {effectName}");
                                }
                            }
                            tempStrList.Add(effectName);
                        }

                        tempStrList.Clear();
                    }

                    lastCheckedCount = dictionaryCount;
                })
                .AddTo(this);
        }
    }
}
