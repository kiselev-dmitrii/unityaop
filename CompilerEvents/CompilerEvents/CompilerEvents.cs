using System;
using System.Collections.Generic;

namespace CompilerEvents {
    public static class CompilerEvents {
        public static event Action<List<string>> OnCompileStarted;
        public static event Action<List<string>> OnCompileFinished;

        internal static void NotifyCompileStarted(List<string> args) {
            if (OnCompileStarted != null) {
                OnCompileStarted(args);
            }
        }

        internal static void NotifyCompileFinished(List<string> args) {
            if (OnCompileFinished != null) {
                OnCompileFinished(args);
            }
        }
    }
}