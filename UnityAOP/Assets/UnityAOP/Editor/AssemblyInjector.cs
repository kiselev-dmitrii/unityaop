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

    private void InjectClassMetadatas(MethodDefinition targetMethodDef, TypeDefinition[] typeDefs) {
        var module = targetMethodDef.Module;

        TypeReference typeMetadataTypeRef = module.ImportReference(typeof (TypeMetadata));
        MethodReference typeMetadataCtorRef = module.ImportReference(typeof (TypeMetadata).GetConstructor(new[] {
            typeof (String), typeof (PropertyMetadata[])
        }));

        TypeReference propertyMetadataTypeRef = module.ImportReference(typeof (PropertyMetadata));
        MethodReference propertyMetadataCtorRef = module.ImportReference(typeof(PropertyMetadata).GetConstructor(new[] {
            typeof (String), typeof (int), typeof(Type)
        }));

        MethodReference getTypeMethodRef = mainModule.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle"));

        var body = targetMethodDef.Body;
        var ilProc = body.GetILProcessor();

        var ret = ilProc.Body.Instructions[ilProc.Body.Instructions.Count-1];

        foreach (var typeDef in typeDefs) {
            var properties = typeDef.Properties;
            int numProperties = properties.Count;

            /////////////////////////////
            ilProc.InsertBefore(ret, Instruction.Create(OpCodes.Ldstr, typeDef.Name));

            // new PropertyMetadata[numProperties]
            ilProc.InsertBefore(ret, Instruction.Create(OpCodes.Ldc_I4, numProperties));
            ilProc.InsertBefore(ret, Instruction.Create(OpCodes.Newarr, propertyMetadataTypeRef));

            for (int i = 0; i < numProperties; ++i) {
                var property = properties[i];
                var propertyTypeRef = mainModule.ImportReference(property.PropertyType);

                ilProc.InsertBefore(ret, Instruction.Create(OpCodes.Dup));
                ilProc.InsertBefore(ret, Instruction.Create(OpCodes.Ldc_I4, i));

                ilProc.InsertBefore(ret, Instruction.Create(OpCodes.Ldstr, property.Name));  //PropertyName
                ilProc.InsertBefore(ret, Instruction.Create(OpCodes.Ldc_I4, i));             //PropertyIndex
                ilProc.InsertBefore(ret, Instruction.Create(OpCodes.Ldtoken, propertyTypeRef));
                ilProc.InsertBefore(ret, Instruction.Create(OpCodes.Call, getTypeMethodRef));  //typeof(property.PropertType)

                    
            }
        }
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
