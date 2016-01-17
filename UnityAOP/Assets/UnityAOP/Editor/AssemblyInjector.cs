using System;
using System.Linq;
using Assets.UnityAOP.Editor.CodeProcessors;
using Mono.Cecil;
using UnityEngine;

namespace Assets.UnityAOP.Editor {
public class AssemblyInjector {
    private AssemblyDefinition assembly;

    public AssemblyInjector(AssemblyDefinition assembly) {
        this.assembly = assembly;
    }

    public bool Process() {
        try {
            var injector = new ObservableInjector(assembly);
            injector.Inject();
            //var targetTypeDef = mainModule.FindTypeDefinition(typeof (ObservableMetadata));
            //MethodDefinition targetMethodDef = targetTypeDef.FindMethodDefinition("InitMetadata");

            //var targetTypeDef = mainModule.FindTypeDefinition<EmptyClass>();
            //targetTypeDef.OverrideMethod("BaseMethod");
            //var interfaceInjector = new InterfaceInjector(assembly);
            //interfaceInjector.Inject();
        } catch (Exception ex) {
            Debug.Log("assembly processing failed: " + ex);
            return false;
        }

        Debug.Log("assembly processed");
        return true;
    }
}
}
