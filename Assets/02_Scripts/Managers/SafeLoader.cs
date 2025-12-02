using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UnityObject = UnityEngine.Object;

namespace CoffeeCat.FrameWork {
    public static class SafeLoader {
        private static readonly Queue<Action> processQueue = new();
        private static bool IsProcessing = false;
        private const int maxProcessPerFrame = 5;
        
        /// <summary>
        /// Use Only SceneBase Component
        /// </summary>
        /// <param name="key"></param>
        public static void StartProcess(GameObject bindingObject) {
            if (IsProcessing) {
                return;
            }
            // Main Process Observable Start
            int processPerFrame = 0;
            IsProcessing = true;
            Observable.EveryUpdate()
                      .TakeUntilDestroy(bindingObject)
                      .Skip(TimeSpan.Zero)
                      .TakeWhile(_ => IsProcessing)
                      .Select(_ => processQueue)
                      .Where(queue => queue.Count > 0)
                      .Subscribe(queue => {
                          // Clear Processed Count
                          processPerFrame = 0;
                          while (queue.Count > 0 && processPerFrame < maxProcessPerFrame)
                          {
                              var process = queue.Dequeue();
                              process.Invoke();
                              processPerFrame++;
                          }
                      })
                      .AddTo(bindingObject);
        }

        /// <summary>
        /// Use Only SceneBase Component
        /// </summary>
        /// <param name="key"></param>
        public static void StopProcess() {
            IsProcessing = false;
            int processLeft = processQueue.Count;
            if (processLeft > 0) {
                CatLog.WLog($"Count Of UnProcessed: {processLeft.ToString()}");
            }

            processQueue.Clear();
        }

        public static void Load<T>(string key, bool isGlobalResource = false, Action<T> onCompleted = null) where T: UnityObject {
            processQueue.Enqueue(() => RequestLoad(key, isGlobalResource, onCompleted));
        }

        public static void Regist(string key, Action<bool> onCompleted = null, int spawnCount = PoolInfo.DEFAULT_COUNT) {
            processQueue.Enqueue(() => RequestLoadWithRegist(key, spawnCount, onCompleted));
        }
        
        public static void LoadAll(Req[] requests, Action<bool> onAllCompleted) {
            if (requests == null || requests.Length == 0) {
                onAllCompleted?.Invoke(false);
                return;
            }
            
            bool isSended = false; // OnAllCompleted Callback Is Sended
            object lockObject = new object();
            
            processQueue.Enqueue(() => {
                foreach (var req in requests) {
                    RequestLoad<UnityObject>(req.Key, req.IsGlobalResource, (onCompleted) => {
                        req.IsCompleted = true;
                        req.IsRequestSuccessed = onCompleted;
                        
                        lock (lockObject) {
                            if (isSended) {
                                CatLog.ELog("There is a callback that completes before any request is executed.");
                                return;
                            }

                            // If Request Is Failed, Send OnCompleted(Failed) Callback
                            if (!req.IsRequestSuccessed) {
                                onAllCompleted?.Invoke(false);
                                isSended = true;
                                return;
                            }

                            // Wait Other Request Completed
                            if (!requests.All(r => r.IsCompleted)) {
                                return;
                            }
                        
                            // Send OnCompleted Callback
                            onAllCompleted?.Invoke(true);
                            isSended = true;
                        }
                    });
                }
            });
        }
        
        public static void RegistAll(Req[] requests, Action<bool> onAllCompleted) {
            if (requests == null || requests.Length == 0) {
                onAllCompleted?.Invoke(false);
                return;
            }
            
            bool isSended = false; // OnAllCompleted Callback Is Sended
            object lockObject = new object();
            
            processQueue.Enqueue(() => {
                foreach (var req in requests) {
                    RequestLoadWithRegist(req.Key, req.SpawnCount, (onCompleted) => {
                        req.IsCompleted = true;
                        req.IsRequestSuccessed = onCompleted;

                        lock (lockObject) {
                            if (isSended) {
                                CatLog.ELog("There is a callback that completes before any request is executed.");
                                return;
                            }

                            // If Request Is Failed, Send OnCompleted(Failed) Callback
                            if (!req.IsRequestSuccessed) {
                                onAllCompleted?.Invoke(false);
                                isSended = true;
                                return;
                            }

                            // Wait Other Request Completed
                            if (!requests.All(r => r.IsCompleted)) {
                                return;
                            }
                        
                            // Send OnCompleted Callback
                            onAllCompleted?.Invoke(true);
                            isSended = true;
                        }
                    });
                }
            });
        }
        
        private static void RequestLoad<T>(string key, bool isGlobalResource = false, Action<T> onCompleted = null) where T : UnityObject {
            ResourceManager.Inst.AddressablesAsyncLoad<T>(key, isGlobalResource, (loadedResource) => {
                onCompleted?.Invoke(loadedResource);
            });
        }
        
        private static void RequestLoadWithRegist(string key, int spawnCount, Action<bool> onCompleted = null) {
            if (ObjectPoolManager.Inst.IsExistInPool(key)) {
                // CatLog.WLog($"{key} is Already Containing in Pool Dictionary.");
                onCompleted?.Invoke(true);
                return;
            }

            ResourceManager.Inst.AddressablesAsyncLoad<GameObject>(key, false, (loadedGameObject) => {
                if (!loadedGameObject) {
                    onCompleted?.Invoke(false);
                    return;
                }

                ObjectPoolManager.Inst.AddToPool(PoolInfo.Create(loadedGameObject, spawnCount: spawnCount));
                onCompleted?.Invoke(true);
            });
        }
        
        public class Req {
            public string Key = string.Empty;
            public int SpawnCount = PoolInfo.DEFAULT_COUNT;
            public bool IsCompleted = false;
            public bool IsRequestSuccessed = false;
            public bool IsGlobalResource = false;
        }
    }
}