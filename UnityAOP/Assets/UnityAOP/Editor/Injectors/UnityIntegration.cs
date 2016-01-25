using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Assets.UnityAOP.Utils;
using Mono.Cecil;
using SyntaxTree.VisualStudio.Unity.Bridge;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.UnityAOP.Editor.Injectors {
    [InitializeOnLoad]
    public static class UnityIntegration {
        public const long MaxWaitTime = 2000;
    
        static UnityIntegration() {
            CompilerEvents.CompilerEvents.OnCompileFinished += OnCompileFinished;
            OnEditorInitialized();
        }
    
        private static void OnCompileFinished(string[] fils, string output, string[] references, BuildTarget target) {
            if (!BuildPipeline.isBuildingPlayer) return;

            if (output.Contains("Editor")) {
                return;
            }
            String dllPath = Application.dataPath.Replace("/Assets", "/" + output);
    
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!File.Exists(dllPath)) {
                Thread.Sleep(10);
                if (stopwatch.ElapsedMilliseconds > MaxWaitTime) {
                    Debug.LogError("Assembly file not found");
                    return;
                }
            }
    
            if (!File.Exists(dllPath)) {
                Debug.LogError("Assembly file not found");
                return;
            }
    
            String mdbPath = dllPath + ".mdb";
    
            EditorApplication.LockReloadAssemblies();
            AssemblyInjector.ProcessAssembly(dllPath, mdbPath);
            EditorApplication.UnlockReloadAssemblies();
        }

        private static void OnEditorInitialized() {
            try {
                EditorApplication.LockReloadAssemblies();

                String projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.Location.Replace('\\', '/')).ToArray();

                var projectAssemblies = assemblies.Where(x => x.StartsWith(projectPath) && x.Contains("Assembly-CSharp") && !x.Contains("Editor"));

                foreach (var assembly in projectAssemblies) {
                    var dllPath = assembly;
                    String mdbPath = dllPath + ".mdb";
                    AssemblyInjector.ProcessAssembly(dllPath, mdbPath);
                }

                EditorApplication.UnlockReloadAssemblies();
            } catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
}
