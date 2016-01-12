using UnityEditor;
using UnityEditor.Scripting;
using UnityEditor.Scripting.Compilers;

namespace CompilerEvents {
internal class CustomCSharpLanguage : CSharpLanguage {
    public override ScriptCompilerBase CreateCompiler(MonoIsland island, bool buildingForEditor, BuildTarget targetPlatform, bool runUpdater) {
        return new CustomCSharpCompiler(island, runUpdater);
    }
}
}