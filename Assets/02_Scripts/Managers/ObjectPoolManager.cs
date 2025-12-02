using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;
using CoffeeCat.Utils;
using CoffeeCat.Utils.SerializedDictionaries;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat.FrameWork
{
    public class ObjectPoolManager : DynamicSingleton<ObjectPoolManager> 
    {
        [Space(5f), Title("Parent"), ReadOnly]
        [SerializeField] private Transform rootParentTr = null;
        
        [Space(5f), Title("POOL INFORMATION")]
        [SerializeField, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout, IsReadOnly = true)] 
        private StringPoolInformationDictionary originInformationDict = null;

        [Space(5f), Title("POOL DICTIONARY")]
        [SerializeField, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout, IsReadOnly = true)] 
        private StringGameObjectStackDictionary poolStackDict = null;

        [Space(5f), Title("ROOT PARENT DICTIONARY")]
        [SerializeField, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout, IsReadOnly = true)]
        private StringTransformDictionary rootParentDict = null;

        // Original Pool Dictionary
        private StringGameObjectStackDictionary originPoolStackDictionary = null;
        private readonly List<GameObject> resultList = new();

        #region TEMP COLLECTIONS
        private List<GameObject> tempResultList = null;
        #endregion

        private void Start() => SceneManager.Inst.ChangeBeforeEvent += OnSceneChangeBeforeEvent;

        protected override void Initialize() {
            originInformationDict = new StringPoolInformationDictionary();
            poolStackDict = new StringGameObjectStackDictionary();
            rootParentDict = new StringTransformDictionary();
            originPoolStackDictionary = new StringGameObjectStackDictionary();
            tempResultList = new List<GameObject>();
        }

        #region ADD TO POOL

        public void AddToPool(PoolInfo[] infos)
        {
            for (int i = 0; i < infos.Length; i++) {
                var info = infos[i];
                if (!info) {
                    CatLog.ELog("Failed to Add PoolObject. PoolInfo is Null.");
                    continue;
                }
                AddToPool(info);
            }
        }

        public void AddToPool(PoolInfo info) {
            if (!info) {
                CatLog.ELog("Failed to Add PoolObject. PoolInfo is Null or PoolObject is Null.");
                return;
            }

            var loadType = info.PoolObjectLoadType;
            switch (loadType)
            {
                // Resources
                case PoolInfo.LoadType.Resources:
                    // Check Path Included Extensions
                    int fileExtensionPosition = info.ResourcesPath.LastIndexOf(".", StringComparison.Ordinal);
                    if (fileExtensionPosition >= 0) { // Remove Paths Extension
                        info.ResourcesPath = info.ResourcesPath.Substring(0, fileExtensionPosition);
                    }
                    info.PoolObject = ResourceManager.Inst.ResourcesLoad<GameObject>(info.ResourcesPath);
                    break;
                // Addressables
                case PoolInfo.LoadType.Addressables:
                    ResourceManager.Inst.AddressablesAsyncLoad<GameObject>(info.AddressablesName, false, loadedObject => {
                        if (!loadedObject) {
                            CatLog.ELog($"AddToPool Failed. {info.AddressablesName} Addressables Load Failed.");
                            return;
                        }
                        // Add To Pool Dictionary   
                        info.PoolObject = loadedObject;
                        AddToDictionary(info);
                    });
                    return;
                // Caching / Custom
                case PoolInfo.LoadType.Custom:
                case PoolInfo.LoadType.Caching:
                    break;
                default: throw new NotImplementedException();
            }
            
            // Add To Pool Dictionary   
            if (!info.PoolObject) {
                CatLog.ELog($"Origin Prefab is Null.");
                return;
            }
                
            AddToDictionary(info);
        }

        private void AddToDictionary(PoolInfo info) {
            // Check PoolInfo is Valid
            if (info == null || !info.PoolObject)
            {
                CatLog.ELog("Invalid PoolInfo or PoolObject is Null.");
                return;
            }
            
            // Ignore Already Containing PoolObject
            if (poolStackDict.ContainsKey(info.PoolObject.name)) {
                // CatLog.WLog($"{info.PoolObject.name} is Already Containing in Pool Dictionary.");
                return;
            }
            
            // Spawn PoolObjects Root Parent
            if (!rootParentTr) {
                SetPoolObjectsRootParent();   
            }

            // Add Origin Information Dictionary
            originInformationDict.Add(info.PoolObject.name, info);

            // Get Parent Data
            Transform parent = null;
            if (info.HasRootParent) {
                if (!info.HasCustomRootParent) {
                    string rootParentName = info.PoolObject.name + "_Root";
                    parent = new GameObject(rootParentName).GetComponent<Transform>();
                    parent.SetParent(rootParentTr);
                }
                else {
                    parent = info.CustomRootParent;
                }

                rootParentDict.Add(info.PoolObject.name, parent);
            }

            // Add Pool Dictionary
            Stack<GameObject> poolStack = new Stack<GameObject>(info.InitSpawnCount);
            for (int i = 0; i < info.InitSpawnCount; i++) {
                var clone = Instantiate(info.PoolObject, parent);
                clone.SetActive(info.IsStayEnabling);
                clone.name = info.PoolObject.name;
                poolStack.Push(clone);
            }

            poolStackDict.Add(info.PoolObject.name, poolStack);
            originPoolStackDictionary.Add(info.PoolObject.name, new Stack<GameObject>(poolStack));
        }

        #endregion

        #region SPAWN

        private GameObject Spawn(string key, Vector3 position, Quaternion rotation, bool isParentSet, Transform parent) {
            if (poolStackDict.ContainsKey(key) == false) {
                CatLog.ELog($"ObjectSpawn Key: {key} is Not Exist in Pool Dictionary.");
                return null;
            }

            if (poolStackDict[key].Count <= 0) {
                Transform rootParent = (rootParentDict.ContainsKey(key)) ? rootParentDict[key] : null;
                var clone = Instantiate(originInformationDict[key].PoolObject, rootParent);
                clone.name = key;
                poolStackDict[key].Push(clone);
                originPoolStackDictionary[key].Push(clone);
            }

            var resultTr = poolStackDict[key].Pop().transform;
            if (isParentSet) {
                resultTr.SetParent(parent);
            }

            resultTr.position = position;
            resultTr.rotation = rotation;
            resultTr.gameObject.SetActive(true);
            return resultTr.gameObject;
        }

        private T Spawn<T>(string key, Vector3 position, Quaternion rotation, bool isParentSet, Transform parent) where T : Component {
            var spawnGameObject = Spawn(key, position, rotation, isParentSet, parent);
            if (spawnGameObject == null) {
                return null;
            }
                
            if (spawnGameObject.TryGetComponent(out T result) == false) {
                CatLog.ELog($"Spawned PoolObject {spawnGameObject.name} is Not Exist {nameof(T)} Component");
                Despawn(spawnGameObject);
            }

            return result;
        }

        #endregion

        #region DESPAWN

        public void Despawn(GameObject poolObject) {
            this.Despawn(poolObject, 0f);
        }

        public void Despawn(GameObject poolObject, float delaySeconds) {
            if (poolStackDict.TryGetValue(poolObject.name, out Stack<GameObject> objectPoolStack) == false) {
                CatLog.WLog($"this '{poolObject.name}' is Not Contains Object Pool Dictionary.");
                return;
            }

            if (delaySeconds <= 0f) {
                Execute();
                return;
            }

            Observable.Timer(TimeSpan.FromSeconds(delaySeconds))
                      .Skip(TimeSpan.Zero)
                      .TakeUntilDisable(poolObject)
                      .Subscribe(_ => {
                          Execute();
                      })
                      .AddTo(this);
            return;

            // Execute Despawn GameObject
            void Execute() {
                if (objectPoolStack.Contains(poolObject))
                {
                    CatLog.WLog($"This Object ({poolObject.name}) is Already Containing in Pool Stack.");
                    return;
                }
                
                poolObject.SetActive(false);

                // Restore Root Parent
                var poolObjectTr = poolObject.transform;
                // Is Exist Root Parent
                if (rootParentDict.ContainsKey(poolObject.name)) {
                    // If Changed Parent
                    if (!ReferenceEquals(rootParentDict[poolObject.name], poolObjectTr.parent)) {
                        poolObjectTr.SetParent(rootParentDict[poolObject.name]);
                    }
                }
                objectPoolStack.Push(poolObject);
            }
        }

        public void DespawnAll(string key) {
            foreach (var poolObject in GetActiveObjectsOrEmptyFromKey(key)) {
                Despawn(poolObject);
            }
        }

        public void DespawnAll(params string[] keys) {
            foreach (var key in keys) {
                DespawnAll(key);
            }
        }

        #endregion

        #region METHOD OVERLOAD

        public GameObject Spawn(string key, Vector3 position, Quaternion rotation, Transform parent) {
            return this.Spawn(key, position, rotation, true, parent);
        }

        public GameObject Spawn(string key, Vector3 position, Quaternion rotation) {
            return this.Spawn(key, position, rotation, false, null);
        }

        public GameObject Spawn(string key, Vector3 position) {
            return this.Spawn(key, position, Quaternion.identity, false, null);
        }

        public GameObject Spawn(string key, Transform parent) {
            return this.Spawn(key, Vector3.zero, Quaternion.identity, true, parent);
        }

        public T Spawn<T>(string key, Vector3 position, Quaternion rotation, Transform parent) where T : Component {
            return this.Spawn<T>(key, position, rotation, true, parent);
        }

        public T Spawn<T>(string key, Vector3 position, Quaternion rotation) where T : Component {
            return this.Spawn<T>(key, position, rotation, false, null);
        }

        public T Spawn<T>(string key, Vector3 position) where T : Component {
            return this.Spawn<T>(key, position, Quaternion.identity, false, null);
        }

        public T Spawn<T>(string key, Transform parent) where T : Component {
            return this.Spawn<T>(key, Vector3.zero, Quaternion.identity, true, parent);
        }

        #endregion

        #region GETTER

        /// <summary>
        /// Return Activated PoolObjects Array
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public GameObject[] GetActiveObjectsOrEmptyFromKey(string key) {
            resultList.Clear();
            if (!originPoolStackDictionary.TryGetValue(key, out Stack<GameObject> poolStack)) {
                return resultList.ToArray();
            }
            var result = poolStack.Where(poolObject => poolObject.activeSelf);
            resultList.AddRange(result);
            return resultList.ToArray();
        }

        /// <summary>
        /// Return All PoolObjects Array
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public GameObject[] GetAllObjectsOrEmptyFromKey(string key) {
            resultList.Clear();
            if (originPoolStackDictionary.TryGetValue(key, out Stack<GameObject> poolStack)) {
                resultList.AddRange(poolStack);
            }
            return resultList.ToArray();
        }

        public GameObject[] GetAllObjectsOrEmptyFromTag(string gameObjectTag)
        {
            resultList.Clear();
            foreach (var pair in originPoolStackDictionary)
            {
                var first = pair.Value.FirstOrDefault();
                if (!first || !first.CompareTag(gameObjectTag)) {
                    continue;
                }
                resultList.AddRange(pair.Value);
            }

            return resultList.ToArray();
        }
        
        public GameObject[] GetActiveObjectsOrEmptyFromTag(string gameObjectTag)
        {
            resultList.Clear();
            foreach (var pair in originPoolStackDictionary)
            {
                var first = pair.Value.FirstOrDefault();
                if (!first || !first.CompareTag(gameObjectTag)) {
                    continue;
                }

                var result = pair.Value.Where(poolObject => poolObject.activeSelf);
                resultList.AddRange(result);
            }

            return resultList.ToArray();
        }

        public bool IsExistInPool(string key) {
            return poolStackDict.ContainsKey(key);
        }

        #endregion

        private void OnSceneChangeBeforeEvent(SceneName sceneName) {
            Clear();
        }

        private void Clear() {
            originInformationDict.Clear();
            originPoolStackDictionary.Clear();
            poolStackDict.Clear();
            rootParentDict.Clear();
            if (rootParentTr) {
                Destroy(rootParentTr.gameObject);
            }
            rootParentTr = null;
            resultList.Clear();
        }

        private void SetPoolObjectsRootParent() {
            if (rootParentTr) {
                return;
            }
            
            rootParentTr = new GameObject("ObjectPool_RootParent").transform;
            rootParentTr.position = Vector3.zero;
            rootParentTr.rotation = Quaternion.identity;
            rootParentTr.localScale = Vector3.one;
        }
    }
}