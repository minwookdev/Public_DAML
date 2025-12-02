using System.Diagnostics;

namespace CoffeeCat {
    public static class CatLog {
        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("ENABLE_LOG")]
        public static void Log(string message) {
#if ENABLE_LOG
            UnityEngine.Debug.Log(message);
#endif
        }

        [Conditional("ENABLE_LOG")]
        public static void Log(params string[] messages) {
#if ENABLE_LOG
            string msgString = string.Empty;
            for (int i = 0; i < messages.Length; i++) {
                msgString += messages[i];
            }
            UnityEngine.Debug.Log(msgString);
#endif
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("ENABLE_LOG")]
        public static void WLog(string message) {
#if ENABLE_LOG
            UnityEngine.Debug.LogWarning(message);
#endif
        }

        [Conditional("ENABLE_LOG")]
        public static void WLog(params string[] messages) {
#if ENABLE_LOG
            string msgString = string.Empty;
            for (int i = 0; i < messages.Length; i++) {
                msgString = messages[i];
            }
            UnityEngine.Debug.LogWarning(msgString);
#endif
        }

        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("ENABLE_LOG")]
        public static void ELog(string message) {
#if ENABLE_LOG
            UnityEngine.Debug.LogError(message);
#endif
        }

        [Conditional("ENABLE_LOG")]
        public static void ELog(params string[] messages) {
#if ENABLE_LOG
            string msgString = string.Empty;
            for (int i = 0; i < messages.Length; i++) {
                msgString += messages[i];
            }
            UnityEngine.Debug.LogError(msgString);
#endif
        }
    }
}
