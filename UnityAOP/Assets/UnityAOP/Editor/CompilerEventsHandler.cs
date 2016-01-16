using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Mono.Cecil;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.UnityAOP.Editor {
[InitializeOnLoad]
public static class CompilerEventsHandler {
    public const long MaxWaitTime = 2000;

    static CompilerEventsHandler() {
        CompilerEvents.CompilerEvents.OnCompileFinished += OnCompileFinished;
    }

    private static void OnCompileFinished(string[] fils, string output, string[] references, BuildTarget target) {
        if (output.Contains("Editor")) {
            return;
        }
        String fullPath = Application.dataPath.Replace("/Assets", "/" + output);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        while (!File.Exists(fullPath)) {
            Thread.Sleep(10);
            if (stopwatch.ElapsedMilliseconds > MaxWaitTime) {
                Debug.LogError("Assembly file not found");
                return;
            }
        }

        if (!File.Exists(fullPath)) {
            Debug.LogError("Assembly file not found");
            return;
        }

        /*
        var assemblyDefinition = AssemblyDefinition.ReadAssembly(fullPath);
        var assemblyInjector = new AssemblyInjector(assemblyDefinition);
        if (assemblyInjector.Process()) {
            assemblyDefinition.Write(fullPath);
        }
         */
    }
}
}
