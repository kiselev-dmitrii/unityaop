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
        var arguments = new List<string> {
            "-debug",
            "-target:library",
            "-nowarn:0169",
            "-out:" + PrepareFileName(_island._output)
        };
        foreach (var reference in _island._references) {
            arguments.Add("-r:" + PrepareFileName(reference));
        }

        foreach (var define in _island._defines.Distinct()) {
            arguments.Add("-define:" + define);
        }

        foreach (var file in _island._files) {
            arguments.Add(PrepareFileName(file));
        }

        var additionalReferences = GetAdditionalReferences();
        foreach (var path in additionalReferences) {
            var text = Path.Combine(GetProfileDirectory(), path);
            if (File.Exists(text)) {
                arguments.Add("-r:" + PrepareFileName(text));
            }
        }

        CompilerEvents.NotifyCompileStarted(arguments);
        Program result = StartCompiler(_island._target, GetCompilerPath(arguments), arguments);

        while (!Poll()) {
            Thread.Sleep(10);
        }
        CompilerEvents.NotifyCompileFinished(arguments);

        return result;
    }

    private string GetCompilerPath(List<string> arguments) {
        // calling private base method via reflection
        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        var methodInfo = GetType().BaseType.GetMethod("GetCompilerPath", bindingFlags);
        var result = (string) methodInfo.Invoke(this, new object[] {
            arguments
        });
        return result;
    }

    private string[] GetAdditionalReferences() {
        // calling private base method via reflection
        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        var methodInfo = GetType().BaseType.GetMethod("GetAdditionalReferences", bindingFlags);
        var result = (string[]) methodInfo.Invoke(this, null);
        return result;
    }
}
}