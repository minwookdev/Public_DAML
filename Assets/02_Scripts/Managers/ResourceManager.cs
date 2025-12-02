using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.SerializedDictionaries;
using CoffeeCat.Utils.Defines;
using UnityObject = UnityEngine.Object;

namespace CoffeeCat.FrameWork {
    public class ResourceManager : DynamicSingleton<ResourceManager> {
        // Loaded Resources Dictioanry
        [Title("Loaded Resources Dictionary")]
        [SerializeField] private StringResourceInformationDictionary resourcesDict = null;
        
        // Fields
        private readonly List<string> removeTargetKeys = new();
        
        protected override void Initialize() {
            resourcesDict = new StringResourceInformationDictionary();
        }

        protected void Start() {
            SceneManager.Inst.ChangeBeforeEvent += ChangeBeforeEvent;
            SceneManager.Inst.ChangeAfterEvent += ChangeAfterEvent;
        }

        #region Resources

        public T ResourcesLoad<T>(string loadPath, bool isGlobal = false) where T : UnityObject {
            // Check Already Loaded Resources
            string fileName = GetFileName(loadPath);
            if (string.IsNullOrEmpty(fileName))
            {
                CatLog.ELog("Invalid Resources Load Path.");
                return null;
            }
            if (TryGetResourceSync(fileName, out T result)) {
                return result;
            }

            // Load Resource and Add to Dictionary
            var info = ResourceInfo.Create(isGlobal);
            resourcesDict.Add(fileName, info);
            result = Resources.Load<T>(loadPath);
            if (result) {
                info.SetResource<T>(result);
                return info.GetResource<T>();
            }
            info.SetFailed();
            CatLog.ELog($"Not Exist Asset(name:{fileName}) in Resources Folder or Load Type is MissMatached.");
            return null;
        }

        public bool ResourcesLoad<T>(string loadPath, out T tResult, bool isGlobal = false) where T : UnityObject {
            tResult = ResourcesLoad<T>(loadPath, isGlobal);
            return tResult != null;
        }

        private static string GetFileName(string resourcesLoadPath) => resourcesLoadPath.Substring(resourcesLoadPath.LastIndexOf('/') + 1);
        
        #endregion
        
        #region Addressables
        
        /// <summary>
        /// Addressables AssetLoadAsync by Addressables Name
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isGlobalResource"></param>
        /// <param name="onCompleted"></param>
        /// <typeparam name="T"></typeparam>
        public void AddressablesAsyncLoad<T>(string key, bool isGlobalResource, Action<T> onCompleted) where T : UnityObject {
            if (string.IsNullOrEmpty(key)) {
                CatLog.ELog("Invalid Key. Key is Null or Empty.");
                return;
            }

            // Dictionary에 이미 로드되거나 요청된 Resource가 존재한다면
            if (TryGetResourceAsync(key, onCompleted)) {
                return;
            }

            // 에셋이 로드중인 상태를 정의하기 위해 Dictionary에 미리 추가 (비동기 로드 전 로드중인 리소스임을 정의하기 위함)
            var info = ResourceInfo.Create<T>(isGlobalResource, onCompleted);
            resourcesDict.Add(key, info);
            Addressables.LoadAssetAsync<T>(key).Completed += (AsyncOperationHandle<T> operationHandle) => {
                info.SetHandle(operationHandle);
                if (operationHandle.Status != AsyncOperationStatus.Succeeded) {
                    info.SetFailed();
                    CatLog.ELog("ResourceMgr: Failed Addressables Async Load. key: " + key);
                }
                else {
                    info.SetResource<T>(operationHandle.Result);
                }

                /*onCompleted?.Invoke(result);*/
                info.TriggerCallback();
            };
        }
        
        #endregion
        
        #region Release

        public void Release(string key) {
            if (!resourcesDict.TryGetValue(key, out ResourceInfo info)) {
                return;
            }
            info.Dispose();
            resourcesDict.Remove(key);
        }

        private void ReleaseAll(bool isRemoveGlobal = false) {
            // remove all
            if (isRemoveGlobal) {
                foreach (var keyValuePair in resourcesDict) {
                    keyValuePair.Value.Dispose();
                }
                resourcesDict.Clear();
            }
            // remove only local resources
            else {
                removeTargetKeys.Clear();
                
                foreach (var pair in resourcesDict) {
                    if (pair.Value.isGlobalResource) {
                        continue;
                    }
                    pair.Value.Dispose();
                    removeTargetKeys.Add(pair.Key);
                }

                for (int i = 0; i < removeTargetKeys.Count; i++) {
                    var key = removeTargetKeys[i];
                    resourcesDict.Remove(key);
                }
                removeTargetKeys.Clear();
            }
        }

        #endregion

        #region Find Loaded Request
        
        private bool TryGetResourceSync<T>(string key, out T result) where T : UnityObject {
            var isExist = resourcesDict.TryGetValue(key, out ResourceInfo info);
            if (!isExist) {
                result = null;
                return false;
            }
            
            result = info.GetResource<T>();
            return true;
        }

        /// <summary>
        /// Find Resource in Dictionary and Return Result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private bool TryGetResourceAsync<T>(string key, Action<T> onCompleted) where T : UnityObject {
            var result = resourcesDict.TryGetValue(key, out ResourceInfo info);
            if (!result) { // Target Resource is Not Requested
                return false;
            }
            
            switch (info.Status) {
                case ResourceInfo.ASSETLOADSTATUS.SUCCESS:
                    onCompleted?.Invoke(info.GetResource<T>());
                    break;
                case ResourceInfo.ASSETLOADSTATUS.FAILED:
                    onCompleted?.Invoke(null);
                    CatLog.ELog("This Resource is Load Failed.");
                    break;
                case ResourceInfo.ASSETLOADSTATUS.LOADING:
                    info.AddCallback(onCompleted);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return true;
        }
        
        #endregion
        
        #region Events

        private void ChangeBeforeEvent(SceneName sceneName) => ReleaseAll();

        private void ChangeAfterEvent(SceneName sceneName) => Resources.UnloadUnusedAssets();

        #endregion

        #region Inner Class
        
        [Serializable]
        public class ResourceInfo {
            public enum ASSETLOADSTATUS {
                LOADING,
                FAILED,
                SUCCESS
            }

            [ShowInInspector, ReadOnly] public UnityObject Resource { get; private set; } = null;
            [ShowInInspector, ReadOnly] public bool isGlobalResource { get; private set; } = false;
            [ShowInInspector, ReadOnly] public ASSETLOADSTATUS Status { get; private set; } = ASSETLOADSTATUS.LOADING;
            private Action onCompleted = null;
            private bool isAddressablesAsset = false;
            private AsyncOperationHandle handle;
            public string ResourceName => Resource ? Resource.name : "NOT_LOADED";

            public static ResourceInfo Create(bool isGlobal) {
                var info = new ResourceInfo {
                    isGlobalResource = isGlobal,
                    isAddressablesAsset = false
                };
                return info;
            }

            public static ResourceInfo Create<T>(bool isGlobal, Action<T> onComplete = null) where T : UnityObject {
                var info = new ResourceInfo {
                    isGlobalResource = isGlobal,
                    isAddressablesAsset = true
                };
                info.onCompleted = Wrapped;
                return info;

                void Wrapped() {
                    onComplete?.Invoke(info.GetResource<T>());
                }
            }

            public void Dispose() {
                if (!isAddressablesAsset) {
                    // avoid UnloadAsset Error
                    if (Resource is GameObject or Component) {
                        return;
                    }
                    Resources.UnloadAsset(Resource);
                }
                else {
                    // release addressables loaded resource
                    if (Resource) {
                        Addressables.Release(Resource);
                    }
                    // release addressables handle
                    if (handle.IsValid()) {
                        Addressables.Release(handle);
                    }
                }

                Resource = null;
            }

            public void SetFailed() {
                if (Status != ASSETLOADSTATUS.LOADING || Resource) {
                    return;
                }

                Status = ASSETLOADSTATUS.FAILED;
            }

            public void SetResource<T>(T resource) where T : UnityObject {
                if (Status != ASSETLOADSTATUS.LOADING || Resource) {
                    CatLog.ELog("Status has already yielded results.");
                    return;
                }

                if (!resource) {
                    CatLog.ELog("Failed To Set Resource. Resource is Null.");
                    return;
                }

                Resource = resource;
                Status = ASSETLOADSTATUS.SUCCESS;
            }

            public T GetResource<T>() where T : UnityObject {
                if (Status != ASSETLOADSTATUS.SUCCESS || !Resource) {
                    CatLog.ELog("Reosurce Get Failed. Resource is Null or Status is Not Success.");
                    return null;
                }

                var casting = Resource as T;
                if (casting) {
                    return casting;
                }

                CatLog.ELog($"Resource Casting Failed. Target: {ResourceName}, Type: {nameof(T)}");
                return null;
            }

            public void SetHandle(AsyncOperationHandle asyncOperationHandle) {
                if (!isAddressablesAsset) {
                    CatLog.ELog("Invalid Operation: This Information is Not Allowed Setting The Handle.");
                    return;
                }

                handle = asyncOperationHandle;
            }

            public void AddCallback<T>(Action<T> callback) where T : UnityObject {
                onCompleted += Wrapped;
                return;

                void Wrapped() {
                    callback?.Invoke(GetResource<T>());
                }
            }

            public void TriggerCallback() {
                onCompleted?.Invoke();
                onCompleted = null;
            }
        }
        
        #endregion
    }
}
