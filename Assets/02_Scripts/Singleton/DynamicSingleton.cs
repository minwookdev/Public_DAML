using UnityEngine;

namespace CoffeeCat.FrameWork {
    public class DynamicSingleton<T> : MonoBehaviour where T : MonoBehaviour {
        // Destroy 여부 확인용
        private static bool shuttingDown = false;
        private static object _lock = new object();
        protected static T inst;

        /// <summary>
        /// Check Singleton Instance Exist 
        /// </summary>
        public static bool IsExist => inst != null && !shuttingDown;

        public static T Inst {
            get {
                // 게임 종료 시 Object 보다 싱글톤의 OnDestroy 가 먼저 실행 될 수도 있다. 
                // 해당 싱글톤을 gameObject.Ondestory() 에서는 사용하지 않거나 사용한다면 null 체크를 해주자
                if (shuttingDown) {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T).Name + "' already destroyed. Returning null.");
                    return null;
                }

                // Thread Safe
                lock (_lock)    
                {
                    if (inst) {
                        return inst;
                    }
                    // 인스턴스 존재 여부 확인
                    inst = (T)FindObjectOfType(typeof(T));

                    // 아직 생성되지 않았다면 인스턴스 생성
                    if (inst) {
                        return inst;
                    }
                    
                    // 새로운 게임오브젝트를 만들어서 싱글톤 Attach
                    var singletonObject = new GameObject();
                    inst = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T).Name + " (Singleton)";

                    // Make instance persistent.
                    DontDestroyOnLoad(singletonObject);

                    // Singleton Initialize Call. 
                    inst.SendMessage(nameof(DynamicSingleton<T>.Initialize)); // Type 1. SendMessage
                    //_instance.GetComponent<GenericSingleton<T>>().Initialize();  // Type 2. GetComponent
                    // CatLog.Log($"Initialized Singleton {typeof(T).Name}");
                    return inst;
                }
            }
        }

        // 비 싱글턴 생성자 사용 방지
        protected DynamicSingleton() { }
        
        public static void Create() {
            if (IsExist) {
                return;
            }

            var create = Inst;
            if (!create) {
                CatLog.ELog("Dynamic Singleton Create Failed");
            }
        }
        
        /// <summary>
        /// 대형 로직 작성 금지
        /// </summary>
        protected virtual void Initialize() { }
        
        private void OnDestroy() {
            InvokeOnDestroy();
            shuttingDown = true;
        }
        
        private void OnApplicationQuit() {
            InvokeOnApplicationQuit();
            shuttingDown = true;
        }

        public virtual void ReleaseSingleton() {
            inst = null;
            Destroy(this);
        }

        protected virtual void InvokeOnDestroy() { }

        protected virtual void InvokeOnApplicationQuit() { }
        
        protected static bool IsExistWithLog(bool printLog = true) {
            if (IsExist) {
                return true;
            }
            if (printLog) {
                CatLog.WLog($"Dynamic Manager {nameof(T)} is not exist.");
            }
            return false;
        }
    }
}
