using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Assets.UnityAOP.Editor.Injectors.MethodAdvice;
using Assets.UnityAOP.Editor.Injectors.Observable;
using Assets.UnityAOP.Utils;
using Mono.Cecil;
using UnityEngine;

namespace Assets.UnityAOP.Editor.Injectors {
    public class AssemblyInjector {
        private AssemblyDefinition assembly;
    
        public AssemblyInjector(AssemblyDefinition assembly) {
            this.assembly = assembly;
        }
    
        public bool Process() {
            if (assembly.HasAttributeOfType<AssemblyProcessedAttribute>()) {
                Debug.Log("Assembly already processed");
                return false;
            }

            try {
                var injector = new ObservableInjector(assembly);
                injector.Inject();
                //var targetTypeDef = mainModule.FindTypeDefinition(typeof (ObservableMetadata));
                //MethodDefinition targetMethodDef = targetTypeDef.FindMethodDefinition("InitMetadata");
    
                //var targetTypeDef = mainModule.FindTypeDefinition<EmptyClass>();
                //targetTypeDef.OverrideMethod("BaseMethod");
                //var interfaceInjector = new InterfaceInjector(assembly);
                //interfaceInjector.Inject();
            } catch (InjectionException ex) {
                Debug.LogError(ex.Message);
                return false;
            }

            assembly.AddAttribute<AssemblyProcessedAttribute>();

            Debug.Log("assembly processed");
            return true;
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
