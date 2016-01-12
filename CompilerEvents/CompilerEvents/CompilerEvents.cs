using UnityEditor;
using UnityEditor.Scripting;

namespace CompilerEvents {
    public static class CompilerEvents {
        public delegate void CompileDelegate(string[] files, string output, string[] references, BuildTarget target);

        public static event CompileDelegate OnCompileStarted;
        public static event CompileDelegate OnCompileFinished;

        internal static void NotifyCompileStarted(MonoIsland island) {
            if (OnCompileStarted != null) {
                OnCompileStarted(island._files, island._output, island._references, island._target);
            }
        }

        internal static void NotifyCompileFinished(MonoIsland island) {
            if (OnCompileFinished != null) {
                OnCompileFinished(island._files, island._output, island._references, island._target);
            }
        }
    }
}