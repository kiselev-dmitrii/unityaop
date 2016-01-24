using System;
using System.Collections.Generic;
using System.Linq;
using Assets.UnityAOP.Aspect.MethodAdvice;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Editor.Injectors.MethodAdvice {
    public class MethodAdviceInjector {
        private readonly ModuleDefinition module;

        public MethodAdviceInjector(AssemblyDefinition assembly) {
            module = assembly.MainModule;
        }

        public void Inject() {
            TypeTree typeTree = new TypeTree(module);
            List<AdviceInfo> adviceMethods = FindAdvices(module);

            foreach (var advice in adviceMethods) {
                var targetNode = typeTree.GetNode(advice.TargetType);

                if (targetNode == null) {
                    throw new InjectionException(advice + ": cannot find type for injection");
                }
                var targetMethod = FindMethodOrConstructor(targetNode, advice);
                if (targetMethod == null) {
                    throw new InjectionException(advice + ": cannot find method for injection");
                }

                InjectHierarchy(targetNode, targetMethod, advice);
            }
        }

        private static List<AdviceInfo> FindAdvices(ModuleDefinition module) {
            var result = new List<AdviceInfo>();

            foreach (var type in module.Types) {
                foreach (var method in type.Methods) {
                    var attribute = method.FindAttribute<MethodAdviceAttribute>();
                    if (attribute == null) continue;

                    var advice = new AdviceInfo(type, method, attribute);

                    String conflicts = advice.Check();
                    if (conflicts != null) {
                        throw new InjectionException(advice + ": " + conflicts);
                    }

                    result.Add(advice);
                }
            }

            return result;
        }

        private void InjectHierarchy(TypeNode node, MethodDefinition method, AdviceInfo advice) {
            if (!node.Type.IsInterface && !method.IsAbstract) {
                InjectCode(method, advice);
            }

            if (node.Type.IsInterface || method.IsAbstract || method.IsVirtual || method.IsReuseSlot) {
                foreach (var derivedNode in node.Derived) {
                    var targetMethod = FindMethodOrConstructor(derivedNode, advice);
                    if (targetMethod == null) continue;
                    InjectHierarchy(derivedNode, targetMethod, advice);
                }
            }
        }

        private void InjectCode(MethodDefinition method, AdviceInfo advice) {
            var body = method.Body;
            var proc = body.GetILProcessor();

            if (advice.Phase == MethodAdvicePhase.OnEnter) {
                InjectOnEnter(method, proc, advice);
            } else {
                InjectOnSuccess(method, proc, advice);
            }
        }

        private void InjectOnEnter(MethodDefinition method, ILProcessor proc, AdviceInfo advice) {
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

        private void InjectOnSuccess(MethodDefinition method, ILProcessor proc, AdviceInfo advice) {
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

        private static MethodDefinition FindMethodOrConstructor(TypeNode node, AdviceInfo advice) {
            if (advice.IsConstructor) {
                if (advice.IsStatic) {
                    return node.Type.GetStaticConstructor();
                } else {
                    return node.Type.FindConstructor(advice.TargetParameters);
                }

            } else {
                return node.Type.FindMethod(advice.TargetMethodName, advice.TargetParameters, advice.IsStatic);
            } 
        }
    }
}
