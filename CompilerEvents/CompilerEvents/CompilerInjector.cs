using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Scripting;
using UnityEditor.Scripting.Compilers;

namespace CompilerEvents {
[InitializeOnLoad]
internal static class CompilerInjector {
    static CompilerInjector() {
        var list = GetSupportedLanguages();
        list.RemoveAll(language => language is CSharpLanguage);
        list.Add(new CustomCSharpLanguage());
    }

    private static List<SupportedLanguage> GetSupportedLanguages() {
        var fieldInfo = typeof (ScriptCompilers).GetField("_supportedLanguages", BindingFlags.NonPublic | BindingFlags.Static);
        var languages = (List<SupportedLanguage>) fieldInfo.GetValue(null);
        return languages;
    }
}
}