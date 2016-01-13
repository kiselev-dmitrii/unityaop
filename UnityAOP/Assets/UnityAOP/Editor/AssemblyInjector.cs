﻿using System;
using Assets.UnityAOP.Editor.CodeProcessors;
using Mono.Cecil;
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
            var methodBoundaryInjector = new MethodBoundaryInjector(assembly);
            methodBoundaryInjector.Inject();
        } catch (Exception ex) {
            Debug.Log("assembly processing failed: " + ex);
            return false;
        }

        Debug.Log("assembly processed");
        return true;
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
