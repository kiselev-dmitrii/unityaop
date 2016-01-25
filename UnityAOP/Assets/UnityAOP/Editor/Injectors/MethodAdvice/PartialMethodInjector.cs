using System;
using System.Collections.Generic;
using System.Linq;
using Assets.UnityAOP.Aspect.MethodAdvice;
using Assets.UnityAOP.Aspect.PartialAdvice;
using Assets.UnityAOP.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Editor.Injectors.MethodAdvice {
    public enum InjectionPlace {
        Begin,
        End
    }

    public class InjectionInfo {
        public TypeDefinition Type { get; private set; }
        public MethodDefinition InjectableMethod { get; private set; }
        public InjectionPlace InjectionPlace { get; private set; }

        public bool IsConstructor { get; private set; }
        public bool IsStatic { get; private set; }
        public String MethodName { get; private set; }
        public List<TypeReference> ArgTypes { get; private set; }
        public TypeReference ReturnType { get; private set; }
        public bool InterceptReturnValue { get; private set; }

        public InjectionInfo(TypeDefinition typeDef, MethodDefinition injectableMethod, CustomAttribute attribute) {
            Type = typeDef;
            InjectableMethod = injectableMethod;

            InjectionPlace = attribute.AttributeType.Name.StartsWith("Before") ? InjectionPlace.Begin : InjectionPlace.End;
            IsConstructor = attribute.AttributeType.Name.Contains("Constructor");
            IsStatic = injectableMethod.IsStatic;

            if (!IsConstructor) {
                MethodName = (String) attribute.ConstructorArguments[0].Value;
            } else {
                MethodName = "Constructor";
            }

            ArgTypes = new List<TypeReference>();
            ReturnType = Type.Module.TypeSystem.Void;
            InterceptReturnValue = false;

            var args = injectableMethod.Parameters;
            if (args.Count != 0) {
                ParameterDefinition lastArg = args.Last();
                if (lastArg.Name != "returnValue") {
                    ArgTypes = args.Select(x => x.ParameterType).ToList();
                } else {
                    ArgTypes = args.Take(args.Count - 1).Select(x => x.ParameterType).ToList();
                    ReturnType = lastArg.ParameterType;
                    InterceptReturnValue = true;
                }
            }
        }

        public String Check() {
            if (Type.IsInterface) {
                return "You can't inject to an interface method";
            }
            if (IsConstructor && IsStatic && ArgTypes.Count > 0) {
                return "Static constructors cannot have arguments";
            }
            if (InjectionPlace != InjectionPlace.End && InterceptReturnValue) {
                return "Trying to intercept a return value in begin of the method";
            }

            return null;
        }

        public MethodDefinition FindTargetMethod() {
            if (IsConstructor) {
                if (IsStatic) {
                    return Type.GetStaticConstructor();
                } else {
                    return Type.FindConstructor(ArgTypes);
                }
            } else {
                return Type.FindMethod(MethodName, ArgTypes, IsStatic);
            }
        }

        public override string ToString() {
            return String.Format("{0} {1}::{2}({3})", 
                ReturnType.Name, 
                Type.Name, 
                InjectableMethod.Name, 
                InjectableMethod.Parameters.Select(x => x.ParameterType.Name).ToString(", ")
                );
        }
    }

    public class PartialMethodInjector {
        private readonly ModuleDefinition module;

        public PartialMethodInjector(AssemblyDefinition assembly) {
            module = assembly.MainModule;
        }

        public void Inject() {
            var injectableMethods = FindInjectableMethods(module);

            foreach (var injectionInfo in injectableMethods) {
                MethodDefinition method = injectionInfo.FindTargetMethod();
                if (method == null) {
                    throw new InjectionException("{0}: Error: Method not found", injectionInfo);
                }
                if (method.IsAbstract) {
                    throw new InjectionException("{0}: Error: Cannot inject to abstract method", injectionInfo);
                }
                if (method.ReturnType == method.Module.TypeSystem.Void && injectionInfo.InterceptReturnValue) {
                    throw new InjectionException("{0}: Error: Trying to intercept return value, but method returns void", injectionInfo);
                }
                if (method.ReturnType != method.Module.TypeSystem.Void && !injectionInfo.InterceptReturnValue && injectionInfo.InjectionPlace == InjectionPlace.End) {
                    throw new InjectionException("{0}: Error: Injectable method doesn't intercept returnValue, but must do it", injectionInfo);
                }
                InjectCode(method, injectionInfo);
            }
        }

        private static List<InjectionInfo> FindInjectableMethods(ModuleDefinition module) {
            HashSet<String> matchNames = new HashSet<string>() {
                typeof (BeforeMethodAttribute).Name,
                typeof (AfterMethodAttribute).Name,
                typeof (BeforeConstructorAttribute).Name,
                typeof (AfterConstructorAttribute).Name
            };

            var result = new List<InjectionInfo>();
            foreach (var type in module.Types) {
                foreach (var method in type.Methods) {
                    CustomAttribute attribute = method.CustomAttributes.FirstOrDefault(x => matchNames.Contains(x.AttributeType.Name));
                    if (attribute == null) continue;

                    var injectionInfo = new InjectionInfo(type, method, attribute);
                    var error = injectionInfo.Check();

                    if (error != null) throw new InjectionException("{0}: Error {1}", injectionInfo, error);
                    result.Add(injectionInfo);
                }
            }

            return result;
        }

        private void InjectCode(MethodDefinition method, InjectionInfo info) {
            var body = method.Body;
            var proc = body.GetILProcessor();

            if (info.InjectionPlace == InjectionPlace.Begin) {
                InjectOnEnter(method, proc, info);
            } else {
                InjectOnSuccess(method, proc, info);
            }
        }

        private void InjectOnEnter(MethodDefinition method, ILProcessor proc, InjectionInfo info) {
            Instruction target = null; 

            //Если это конструктор, то мы должны пропустить вызов базового конструктора
            if (method.IsConstructor) {
                var baseConstructorCall = proc.Body.Instructions.First(x => x.OpCode == OpCodes.Call);
                target = baseConstructorCall.Next;
            } else {
                target = proc.Body.Instructions[0];
            }

            //Инжектим
            if (!method.IsStatic) {
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg, method.Body.ThisParameter));
            }
            foreach (var param in method.Parameters) {
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg, param));
            }
            MethodReference injectableMethodRef = method.Module.ImportReference(info.InjectableMethod);
            proc.InsertBefore(target, Instruction.Create(OpCodes.Call, injectableMethodRef));
        }

        private void InjectOnSuccess(MethodDefinition method, ILProcessor proc, InjectionInfo advice) {
            var typeSystem = method.Module.TypeSystem;

            //Подменяем return на goto, и получаем место перед return
            Instruction target = null;
            VariableDefinition returnVariable = null;
            target = method.ReturnType == typeSystem.Void ? ReplaceVoidReturns(method) : ReplaceValueReturns(method, out returnVariable);

            // Загружаем this, параметры метода и возвращаемое значение
            if (!method.IsStatic) {
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg, method.Body.ThisParameter));
            }
            foreach (var param in method.Parameters) {
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg, param));
            }
            if (method.ReturnType != typeSystem.Void) {
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldloc, returnVariable));
            }

            //Вызываем метод
            MethodReference injectableMethodRef = method.Module.ImportReference(advice.InjectableMethod);
            proc.InsertBefore(target, Instruction.Create(OpCodes.Call, injectableMethodRef));
        }

        private Instruction ReplaceVoidReturns(MethodDefinition method) {
            var body = method.Body;
            var instructions = body.Instructions;

            // Добавляем вконце return, а остальные return заменяем на goto к последнему return'у
            var ret = Instruction.Create(OpCodes.Ret);
            var nop = Instruction.Create(OpCodes.Nop);
            instructions.Add(nop);
            instructions.Add(ret);

            for (var i = 0; i < instructions.Count - 2; i++) {
                var instruction = instructions[i];
                if (instruction.OpCode == OpCodes.Ret) {
                    instructions[i] = Instruction.Create(OpCodes.Br, nop);
                }
            }
            return ret;
        }

        private Instruction ReplaceValueReturns(MethodDefinition method, out VariableDefinition returnVariable) {
            var body = method.Body;
            var instructions = body.Instructions;

            // Добавляем новую переменную в которую поместим результат, все return заменим на присваивание переменной и goto к последнему return'у переменной
            var returnValue = new VariableDefinition("returnValue", method.ReturnType);
            body.Variables.Add(returnValue);

            var ldLock = Instruction.Create(OpCodes.Ldloc, returnValue);
            var nop = Instruction.Create(OpCodes.Nop);
            instructions.Add(nop);
            instructions.Add(ldLock);
            instructions.Add(Instruction.Create(OpCodes.Ret));

            for (var i = 0; i < instructions.Count - 3; i++) {
                var instruction = instructions[i];
                if (instruction.OpCode == OpCodes.Ret) {
                    instructions[i] = Instruction.Create(OpCodes.Br, nop);
                    instructions.Insert(i, Instruction.Create(OpCodes.Stloc, returnValue));
                    i++;
                }
            }

            returnVariable = returnValue;
            return ldLock;
        }


    }
}
