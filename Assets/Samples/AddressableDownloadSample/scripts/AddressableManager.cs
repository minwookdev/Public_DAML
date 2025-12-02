namespace DonutMonsterDev {
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;
    using UniRx.Triggers;
    using TMPro;
    using CoffeeCat;

    public class AddressableManager : MonoBehaviour {
        enum AddressName {
            img_prefab_1,
            img_prefab_2,
            img_prefab_3,
            img_game_clear,
            img_game_over,
            Remote,
        }

        [Header("Addressable Management")]
        public Transform SpawnedObjectParent = null;
        [SerializeField] AssetReference assetRef = null;
        [SerializeField] AsyncOperationHandle handle;
        [SerializeField] System.Collections.Generic.List<GameObject> spawnedList = new System.Collections.Generic.List<GameObject>();
        [SerializeField] GameObject loadedAsset = null;
        [SerializeField] bool isConvertFormat = true;
        [SerializeField] Toggle toggleConvert = null;

        [Header("Asset Download Information")]
        [SerializeField] TextMeshProUGUI tmpDownloadTime = null;
        [SerializeField] TextMeshProUGUI tmpDownloadSize = null;
        [SerializeField] Slider sliderDownloadProgress = null;
        [SerializeField] TextMeshProUGUI tmpDownloadProgress = null;

        [Header("Local Asset Load 1")]
        [SerializeField] Button btnLocalAssetLoad = null;
        [SerializeField] Button btnLocalAssetRelease = null;

        [Header("Local Asset Load 2")]
        [SerializeField] Button btn2LocalAssetLoad = null;
        [SerializeField] Button btn2LocalAssetRelease = null;

        [Header("Remote Asset Load 1")]
        [SerializeField] Button btnRemoteAssetLoad = null;
        [SerializeField] Button btnRemoteAssetRelease = null;

        [Header("Clear Download Cache")]
        [SerializeField] Button btnClearDownloadCache = null;

        private void Start()
        {
            // BUTTON - 1
            btnLocalAssetLoad.onClick.AddListener(() =>
            {
                Addressables.InstantiateAsync(assetRef, SpawnedObjectParent).Completed += (AsyncOperationHandle<GameObject> handle) =>
                {
                    spawnedList.Add(handle.Result);
                };
            });

            btnLocalAssetRelease.onClick.AddListener(() =>
            {
                for (int i = spawnedList.Count - 1; i >= 0; i--)
                {
                    Addressables.ReleaseInstance(spawnedList[i]);
                    spawnedList.RemoveAt(i);
                }
            });

            // BUTTON - 2
            GameObject spawnedGameObject = null;
            System.Action spawnAction = () =>
            {
                spawnedGameObject = Instantiate<GameObject>(loadedAsset, SpawnedObjectParent);
            };
            btn2LocalAssetLoad.onClick.AddListener(() =>
            {
                if (spawnedGameObject)
                {
                    return;
                }

                if (!loadedAsset)
                {
                    Addressables.LoadAssetAsync<GameObject>(AddressName.img_prefab_2.ToString()).Completed += (AsyncOperationHandle<GameObject> handle) =>
                    {
                        loadedAsset = handle.Result;
                        if (!loadedAsset)
                        {
                            CatLog.WLog("Asset Load Failed.");
                            return;
                        }

                        spawnAction();
                    };
                }
                else
                {
                    spawnAction();
                }
            });

            btn2LocalAssetRelease.onClick.AddListener(() =>
            {
                if (spawnedGameObject)
                {
                    Destroy(spawnedGameObject);
                    spawnedGameObject = null;
                }
            });

            // Init Remote Asset Load Buttons
            InitRemoteAssetLoadButton1();
            InitRemoteAssetLoadButton2();
            InitRemoteAssetLoadButton3();

            // Init Download Cache Clear Button
            InitClearCacheButton();

            // Init Download Size Toggle
            InitToggle();

            void InitRemoteAssetLoadButton1()
            {
                GameObject loadedRemoteAsset = null;
                GameObject spawnedRemoteAsset = null;
                System.Action spawnRemoteAsset = () =>
                {
                    spawnedRemoteAsset = Instantiate<GameObject>(loadedRemoteAsset, SpawnedObjectParent);
                    Debug.Log("Spawn Remote Asset !");
                };
                btnRemoteAssetLoad.onClick.AddListener(() =>
                {
                    if (spawnedRemoteAsset)
                    {
                        return;
                    }

                    if (!loadedRemoteAsset)
                    {
                        string addressableNameStr = AddressName.img_prefab_3.ToString();
                        UpdateDownlaodSize(addressableNameStr, () =>
                        {
                            // Update Download Timer
                            float downloadTime = 0f;
                            this.UpdateAsObservable()
                                .Skip(0)
                                .TakeWhile(_ => loadedRemoteAsset == null)
                                .TakeUntilDestroy(this)
                                .Subscribe(_ =>
                                {
                                    downloadTime += Time.deltaTime;
                                    tmpDownloadTime.text = downloadTime.ToString("#.00 sec");
                                })
                                .AddTo(this);

                            var downloadHandle = Addressables.LoadAssetAsync<GameObject>(addressableNameStr);
                            downloadHandle.Completed += (AsyncOperationHandle<GameObject> handle) =>
                            {
                                GameObject downloadedAsset = handle.Result;
                                if (!downloadedAsset)
                                {
                                    Debug.Log("Asset Download Failed !");
                                    return;
                                }

                                loadedRemoteAsset = downloadedAsset;
                                Debug.Log("Asset Download Completed !");

                                spawnRemoteAsset();
                            };

                            // Update Download Progress Slider
                            float loadProgress = 0f; // Value Range is 0f ~ 1f
                            this.UpdateAsObservable()
                                .Skip(0)
                                .TakeWhile(_ => !downloadHandle.IsDone)
                                .TakeUntilDestroy(this)
                                .DoOnCompleted(() =>
                                {
                                    sliderDownloadProgress.value = (downloadHandle.Status == AsyncOperationStatus.Succeeded) ? 1f : 0f;
                                    tmpDownloadProgress.text = (downloadHandle.Status == AsyncOperationStatus.Succeeded) ? "100 %" : "0 %";
                                })
                                .Subscribe(_ =>
                                {
                                    loadProgress = downloadHandle.PercentComplete;
                                    sliderDownloadProgress.value = loadProgress;
                                    tmpDownloadProgress.text = loadProgress.ToString("#0 %");
                                })
                                .AddTo(this);
                        });
                    }
                    else
                    {
                        spawnRemoteAsset();
                    }
                });

                btnRemoteAssetRelease.onClick.AddListener(() =>
                {
                    if (spawnedRemoteAsset)
                    {
                        Destroy(spawnedRemoteAsset);
                        spawnedRemoteAsset = null;
                    }
                });
            }

            void InitRemoteAssetLoadButton2()
            {

            }

            void InitRemoteAssetLoadButton3()
            {

            }

            void UpdateDownlaodSize(string AddressableLabelName, System.Action OnComplete = null)
            {
                Addressables.GetDownloadSizeAsync(AddressableLabelName).Completed += (AsyncOperationHandle<long> downloadSizeHandle) =>
                {
                    tmpDownloadSize.text = string.Format("{0} byte", SizeSuffix(downloadSizeHandle.Result));
                    Addressables.Release(downloadSizeHandle);
                    Debug.Log($"Get Asset Download Size Completed ! Size: {downloadSizeHandle.Result}");
                    OnComplete?.Invoke();
                };
            }

            void InitClearCacheButton()
            {
                btnClearDownloadCache.onClick.AddListener(() =>
                {
                    Addressables.ClearDependencyCacheAsync(AddressName.img_prefab_3.ToString());
                });
            }

            void InitToggle()
            {
                this.ObserveEveryValueChanged(_ => toggleConvert.isOn)
                    .TakeUntilDestroy(this)
                    .DoOnSubscribe(() =>
                    {
                        isConvertFormat = toggleConvert.isOn;
                    })
                    .Subscribe(isOnToggle =>
                    {
                        isConvertFormat = isOnToggle;
                    })
                    .AddTo(this);
            }
        }

        System.Collections.IEnumerator UpdateDownloadTimeAsync(GameObject downloadingAsset)
        {
            float downloadTime = 0f;
            while (downloadingAsset == null)
            {
                downloadTime += Time.deltaTime;
                tmpDownloadTime.text = downloadTime.ToString("#.00 sec");
                yield return null;
            }
        }

        string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        private string SizeSuffix(System.Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
            {
                throw new System.ArgumentOutOfRangeException("decimalPlaces");
            }

            if (value < 0)
            {
                return "-" + SizeSuffix(-value, decimalPlaces);
            }

            if (value == 0)
            {
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)System.Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (System.Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}
