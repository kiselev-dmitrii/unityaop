using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Assets.UnityAOP.Utils;
using Mono.Cecil;
using UnityEngine;

namespace Assets.UnityAOP.Editor.Injectors {
    public class AssemblyInjector {
        private readonly AssemblyDefinition assembly;
    
        public AssemblyInjector(AssemblyDefinition assembly) {
            this.assembly = assembly;
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

        private bool Process() {
            if (assembly.HasAttributeOfType<AssemblyProcessedAttribute>()) {
                Debug.Log("Assembly already processed");
                return false;
            }

            try {
                var injector = new GenericInstanceInjector(assembly);
                injector.Inject();
            } catch (InjectionException ex) {
                Debug.LogError(ex.Message);
                return false;
            }

            assembly.AddAttribute<AssemblyProcessedAttribute>();

            Debug.Log("assembly processed");
            return true;
        }


    }
}
