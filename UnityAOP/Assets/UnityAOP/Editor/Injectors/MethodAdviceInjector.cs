using System;
using System.Collections.Generic;
using System.Linq;
using Assets.UnityAOP.Aspect.MethodAdvice;
using Assets.UnityAOP.Observable;
using Assets.UnityAOP.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using UnityEngine.Assertions;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace Assets.UnityAOP.Editor.Injectors {
    public class AdviceMethod {
        public TypeDefinition TypeDef;
        public MethodDefinition MethodDef;

        public TypeDefinition TargetType;
        public String TargetMethodName;
        public List<TypeReference> TargetParameters;
        public bool IsStaticTarget;

        public MethodAdvicePhase Phase;
        public bool EnableForChild;

        public AdviceMethod(TypeDefinition type, MethodDefinition method, CustomAttribute attribute) {
            TypeDef = type;
            MethodDef = method;

            TargetType = (TypeDefinition)attribute.ConstructorArguments[0].Value;
            TargetMethodName = (String)attribute.ConstructorArguments[1].Value;
            Phase = (MethodAdvicePhase)attribute.ConstructorArguments[2].Value;
            EnableForChild = (bool)attribute.ConstructorArguments[3].Value;
            IsStaticTarget = false;
            TargetParameters = new List<TypeReference>();


            var parameters = MethodDef.Parameters;
            if (parameters.Count == 0) {
                IsStaticTarget = true;
                return;
            }

            ParameterDefinition firstParam = parameters[0];
            ParameterDefinition lastParam = parameters[parameters.Count - 1];
            IsStaticTarget = firstParam.ParameterType.Resolve() != TargetType;
            
            int skipFirst = IsStaticTarget ? 0 : 1;
            int skipLast = lastParam.Name == "returnValue" ? 1 : 0;
            for (int i = skipFirst; i < parameters.Count - skipLast; ++i) {
                TargetParameters.Add(parameters[i].ParameterType);
            }
        }

        public override String ToString() {
            return String.Format("{0}::{1}", TypeDef, MethodDef);
        }
    }

    public class MethodAdviceInjector {
        private readonly ModuleDefinition module;

        #region References
        #endregion

        public MethodAdviceInjector(AssemblyDefinition assembly) {
            module = assembly.MainModule;

            TypeDefinition typeDef;
        }

        public void Inject() {
            // Собираем по всему проекту помеченные статические методы
            List<AdviceMethod> adviceMethods = new List<AdviceMethod>();
            foreach (var type in module.Types) {
                foreach (var method in type.Methods) {
                    if (!method.IsStatic) continue;

                    var attribute = method.FindAttribute<MethodAdviceAttribute>();
                    if (attribute == null) continue;
                    
                    adviceMethods.Add(new AdviceMethod(type, method, attribute));
                }
            }

            TypeTree typeTree = new TypeTree(module);

            foreach (var advice in adviceMethods) {
                var targetNode = typeTree.GetNode(advice.TargetType);
                if (targetNode == null) {
                    throw new InjectionException("Не найден тип для инъекции " + advice);
                }
                
                var targetMethod = targetNode.Type.FindMethod(advice.TargetMethodName, advice.TargetParameters, advice.IsStaticTarget);
                if (targetMethod == null) {
                    throw new InjectionException("Не найден метод для инъекции " + advice);
                }

                if (targetNode.Type.IsInterface || targetMethod.IsAbstract) {
                    advice.EnableForChild = true;
                }

                InjectHierarchy(targetNode, targetMethod, advice);
            }
        }

        public void InjectHierarchy(TypeNode node, MethodDefinition method, AdviceMethod advice) {
            if (!node.Type.IsInterface && !method.IsAbstract) {
                InjectCode(method, advice);
            }

            if (advice.EnableForChild) {
                if (node.Type.IsInterface || method.IsAbstract || method.IsVirtual || method.IsReuseSlot) {
                    foreach (var derivedNode in node.Derived) {
                        var targetMethod = derivedNode.Type.FindMethod(advice.TargetMethodName, advice.TargetParameters, advice.IsStaticTarget);
                        if (targetMethod == null) continue;
                        InjectHierarchy(derivedNode, targetMethod, advice);
                    }
                }
            }
        }

        public void InjectCode(MethodDefinition method, AdviceMethod advice) {
            var body = method.Body;
            body.SimplifyMacros();
            var proc = body.GetILProcessor();

            if (advice.Phase == MethodAdvicePhase.OnEnter) {
                InjectOnEnter(method, proc, advice);
            } else {
                InjectOnSuccess(method, proc, advice);
            }

            body.OptimizeMacros();
        }

        private void InjectOnEnter(MethodDefinition method, ILProcessor proc, AdviceMethod advice) {
            Instruction target = null; 

            //Если это конструктор, то мы должны пропустить вызов базового конструктора
            if (method.IsConstructor) {
                var baseConstructorCall = proc.Body.Instructions.First(x => x.OpCode == OpCodes.Call);
                target = baseConstructorCall.Next;
            } else {
                target = proc.Body.Instructions[0];
            }

            //Вызываем advice
            MethodReference onEnterMethodRef = method.Module.ImportReference(advice.MethodDef);

            if (!method.IsStatic) {
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg, method.Body.ThisParameter));
            }
            foreach (var param in method.Parameters) {
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg, param));
            }
            proc.InsertBefore(target, Instruction.Create(OpCodes.Call, onEnterMethodRef));
        }

        private void InjectOnSuccess(MethodDefinition method, ILProcessor proc, AdviceMethod advice) {
            var typeSystem = method.Module.TypeSystem;
            MethodReference onSuccessMethodRef = method.Module.ImportReference(advice.MethodDef);

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
            proc.InsertBefore(target, Instruction.Create(OpCodes.Call, onSuccessMethodRef));
        }

        private Instruction ReplaceVoidReturns(MethodDefinition method) {
            var body = method.Body;
            var instructions = body.Instructions;

            // Добавляем вконце return, а остальные return заменяем на goto к последнему return'у
            var ret = Instruction.Create(OpCodes.Ret);
            var nop = Instruction.Create(OpCodes.Nop);
            instructions.Add(nop);
            instructions.Add(ret);

            for (var i = 0; i < instructions.Count - 1; i++) {
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
