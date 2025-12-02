using UnityEngine;

namespace CoffeeCat.FrameWork {
    public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour {
        private static object _lock = new object();
        private static T inst = null;

        public static bool IsExist => inst != null;

        public static T Inst {
            get {

                lock (_lock)
                {
                    if (inst) {
                        return inst;
                    }
                    CatLog.WLog($"Prelocated Singleton: {nameof(T)} Is Not Exist !");
                    return null;
                }
            }
        }

        protected virtual void Initialize() { }

        protected void Awake() {
            //_instance = (T)FindObjectOfType(typeof(T));
            inst = this as T;
            Initialize();
        }

        protected void OnDestroy() => inst = null;

        protected void OnApplicationQuit() => inst = null;
        
        protected static bool IsExistWithLog() {
            if (IsExist) {
                return true;
            }
            CatLog.WLog($"Dynamic Manager {nameof(T)} is not exist.");
            return false;
        }
    }
}
