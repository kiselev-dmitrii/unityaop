using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Mono.Cecil;
using SyntaxTree.VisualStudio.Unity.Bridge;
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

        ProcessAssembly(dllPath, mdbPath);
    }

    public static void ProcessAssembly(String dllPath, String mdbPath) {
        String directory = dllPath.Substring(0, dllPath.LastIndexOf('/'));

        HashSet<string> searchDirs = new HashSet<string>();
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            searchDirs.Add(Path.GetDirectoryName(assembly.Location));
        }
        searchDirs.Add(directory);

        // Assembly resolver
        DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
        foreach (String dir in searchDirs) {
            assemblyResolver.AddSearchDirectory(dir);
        }

        // Read-Write params
        ReaderParameters readerParameters = new ReaderParameters() {
            AssemblyResolver = assemblyResolver
        };
        WriterParameters writerParameters = new WriterParameters();

        readerParameters.ReadSymbols = true;
        readerParameters.SymbolReaderProvider = new Mono.Cecil.Mdb.MdbReaderProvider();
        writerParameters.WriteSymbols = true;
        writerParameters.SymbolWriterProvider = new Mono.Cecil.Mdb.MdbWriterProvider();

        var assemblyDefinition = AssemblyDefinition.ReadAssembly(dllPath, readerParameters);
        var assemblyInjector = new AssemblyInjector(assemblyDefinition);
        if (assemblyInjector.Process()) {
            assemblyDefinition.Write(dllPath, writerParameters);
        }
    }
}
}
