using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public static class CompilerEventsHandler {
    static CompilerEventsHandler() {
        CompilerEvents.CompilerEvents.OnCompileFinished += OnCompileFinished;
    }

    private static void OnCompileFinished(List<String> args) {
        String relativePath = args[3].Replace("-out:", "/");
        String fullPath = Application.dataPath.Replace("/Assets", relativePath);

        Debug.Log(fullPath);
        /*
        var assemblyDefinition = AssemblyDefinition.ReadAssembly(fullPath);
        if (Injector.InjectEventDispatcher(assemblyDefinition)) {
            assemblyDefinition.Write(fullPath);
        }
         */
    }
}
