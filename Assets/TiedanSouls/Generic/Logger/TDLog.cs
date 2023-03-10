using UnityEngine;

namespace TiedanSouls {

    public static class TDLog {

        public static void Log(object message) {
            Debug.Log(message);
        }

        public static void Warning(object message) {
            Debug.LogWarning(message);
        }

        public static void Error(object message) {
            Debug.LogError(message);
        }

        public static void Assert(bool condition) {
            Debug.Assert(condition);
        }

    }
}