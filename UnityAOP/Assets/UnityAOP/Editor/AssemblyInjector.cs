using System;
using System.Linq;
using Assets.ObservableTest;
using Assets.UnityAOP.Editor.CodeProcessors;
using Assets.UnityAOP.Observable;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using UnityEngine;

namespace Assets.UnityAOP.Editor {
public class AssemblyInjector {
    private AssemblyDefinition assembly;
    private ModuleDefinition mainModule;

    public AssemblyInjector(AssemblyDefinition assembly) {
        this.assembly = assembly;
        mainModule = assembly.MainModule;
    }

    public bool Process() {
        try {
            var targetTypeDef = mainModule.FindTypeDefinition<Player>();
            var interfaceDef = targetTypeDef.AddInterface<IObservable>();

            MethodDefinition getPropertyMetadataDef = targetTypeDef.ImplementMethod(interfaceDef, "GetPropertyMetadata");
            CreateDebugLog(getPropertyMetadataDef);

            MethodDefinition addObserverDef = targetTypeDef.ImplementMethod(interfaceDef, "AddObserver");
            CreateDebugLog(addObserverDef);

            MethodDefinition removeObserverDef = targetTypeDef.ImplementMethod(interfaceDef, "RemoveObserver");
            CreateDebugLog(removeObserverDef);

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

    public void CreateDebugLog(MethodDefinition method) {
        var module = method.Module;
        MethodReference debugLogRef = module.ImportReference(typeof(UnityEngine.Debug).GetMethod("Log", new[] { typeof(object) }));

        var body = method.Body;
        body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, method.Name));
        body.Instructions.Add(Instruction.Create(OpCodes.Call, debugLogRef));
        if (method.ReturnType != module.TypeSystem.Void) {
            body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
        }
        body.Instructions.Add(Instruction.Create(OpCodes.Ret));
    }

    /*
    private class Advice {
        public TypeDefinition AdviceType;
        public MethodDefinition AdviceMethod;
        public TypeDefinition TargetType;
        public String TargetMethod;
        public AdvicePhase Phase;

        public Advice(TypeDefinition type, MethodDefinition method, CustomAttribute attribute) {
            AdviceType = type;
            AdviceMethod = method;
            TargetType = (TypeDefinition) attribute.ConstructorArguments[0].Value;
            TargetMethod = (String) attribute.ConstructorArguments[1].Value;
            Phase = (AdvicePhase) attribute.ConstructorArguments[2].Value;
        }
    }

    private void ProcessAdviceMethods() {
        var advices = new List<Advice>();

        foreach (TypeDefinition type in mainModule.GetTypes()) {
            foreach (MethodDefinition method in type.Methods) {
                var adviceAttribute = method.FindAttribute<AdviceAttribute>();
                if (adviceAttribute == null) continue;
                
                advices.Add(new Advice(type, method, adviceAttribute));
            }
        }

        var lookup = new MultiKeyDictionary<TypeDefinition, String, Advice>();
        foreach (var advice in advices) {
            lookup.Set(advice.TargetType, advice.TargetMethod, advice);
        }

        foreach (TypeDefinition type in mainModule.GetTypes()) {
            var advicedMethods = lookup.GetNested(type);
            if (advicedMethods == null) {
                continue;
            }

            foreach (MethodDefinition method in type.Methods) {
                var advicedMethod = advicedMethods.Get(method.Name);
                if (advicedMethod == null) {
                    continue;
                }

                MethodReference adviceMethodReference = advicedMethod.AdviceType.Module.Import(advicedMethod.AdviceMethod);

                ILProcessor ilProcessor = method.Body.GetILProcessor();

                if (advicedMethod.Phase == AdvicePhase.Begin) {
                    Instruction first = method.Body.Instructions[0];
                    ilProcessor.InsertBefore(first, Instruction.Create(OpCodes.Call, adviceMethodReference));
                } else {
                    Instruction last = method.Body.Instructions[method.Body.Instructions.Count - 1];
                    ilProcessor.InsertBefore(last, Instruction.Create(OpCodes.Call, adviceMethodReference));
                }
            }
        }
    }
    */
}
}
