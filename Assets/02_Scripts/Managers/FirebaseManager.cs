using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;

namespace CoffeeCat.FrameWork {
    public class FirebaseManager : DynamicSingleton<FirebaseManager> {
        public bool IsFirebaseInit { get; private set; } = false;
        private FirebaseApp firebaseApp = null;
        private FirebaseRemoteConfig remoteConfig = null;

        private void Start() {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available) {
                    firebaseApp = FirebaseApp.DefaultInstance;
                    remoteConfig = FirebaseRemoteConfig.DefaultInstance;
                    Firebase.Crashlytics.Crashlytics.ReportUncaughtExceptionsAsFatal = true;
                    IsFirebaseInit = true;

                    // Load RemoveConfig Datas
                    StartCoroutine(FetchRemoteConfig());
                    
                    // CatLog.Log("Firebase Init Completed");
                }
                else {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }

        private IEnumerator FetchRemoteConfig() {
            // Fetch RemoteConfig Task
            Task fetchTask = remoteConfig.FetchAsync(TimeSpan.Zero);
            fetchTask.ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    // Debug.Log("[Firebase]: RemoteConfig Fetch Canceled.");
                }
                else if (task.IsFaulted) {
                    // Debug.Log("[Firebase]: RemoteConfig Fetch Faulted.");
                }
                else {
                    // Debug.Log("[Firebase]: RemoteConfig Fetch Success.");    
                }
            });

            // Wait Task Ended (Completed or Faulted or Canceled)
            yield return new WaitUntil(() => fetchTask.IsCompleted);

            var info = remoteConfig.Info;
            if (info.LastFetchStatus != LastFetchStatus.Success) {
                // Fetch Failed
            }
            else {
                // Fetch Completed
                StartCoroutine(LoadRemoteConfigData());
            }
        }

        private IEnumerator LoadRemoteConfigData() {
            Task activateTask = remoteConfig.ActivateAsync();
            activateTask.ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    // Debug.Log("[Firebase]: Firebase RemoteConfig Active Canceled.");
                }
                else if (task.IsFaulted) {
                    // Debug.Log("[Firebase]: Firebase RemoteConfig Active Faulted.");
                }
                else {
                    // Debug.Log("[Firebase]: Firebase RemoteConfig Active Success.");
                }
            });

            // Wait RemoteConfig ActivatedTask
            yield return new WaitUntil(() => activateTask.IsCompleted);
            if (activateTask.IsCompletedSuccessfully == false) {
                yield break;
            }

            // Is RemoteConfig FetchAndActivateAsync Complete Successfully
            var info = remoteConfig.Info;

            #region APP_VERSION_CHECK

            string newVersionStringValue = remoteConfig.GetValue("android_version").StringValue;
            if (string.IsNullOrEmpty(newVersionStringValue) == false) {
                if (int.TryParse(newVersionStringValue, out int newVersionIntValue)) {
                    // CatLog.Log("[Firebase]: NewVersionStringValue is Parse Success" + newVersionIntValue);
                }
                else {
                    // Debug.Log("[Firebase]: NewVersionStringValue is Parse Failed");
                }
            }
            else {
                // Debug.Log("[Firebase]: NewVersionStringValue is Null Or Empty");
            }

            #endregion
        }
    }
}