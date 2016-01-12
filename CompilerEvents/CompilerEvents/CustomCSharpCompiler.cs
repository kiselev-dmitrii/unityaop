using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEditor.Scripting;
using UnityEditor.Scripting.Compilers;
using UnityEditor.Utils;

namespace CompilerEvents {
internal class CustomCSharpCompiler : MonoCSharpCompiler {
    public CustomCSharpCompiler(MonoIsland island, bool runUpdater) : base(island, runUpdater) {
    }

    protected override Program StartCompiler() {
        CompilerEvents.NotifyCompileStarted(_island);
        Program result = base.StartCompiler();
        CompilerEvents.NotifyCompileFinished(_island);

        return result;
    }
}
}