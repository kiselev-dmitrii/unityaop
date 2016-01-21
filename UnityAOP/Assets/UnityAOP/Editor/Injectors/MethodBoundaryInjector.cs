using System;
using Assets.UnityAOP.Aspect.BoundaryAspect;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Assets.UnityAOP.Editor.Injectors {
    public class MethodBoundaryInjector {
        private AssemblyDefinition assembly;
        private ModuleDefinition mainModule;
    
        #region References
        private MethodReference getCurrentMethodRef;    //MethodBase.GetCurrentMethod
        private MethodReference getCustomAttributeRef;  //Attribute.GetCustomAttribute
        private MethodReference getTypeFromHandleRef;   //Attribute.GetTypeFromHandle
        private TypeReference methodBaseRef;            //System.Reflection.MethodBase
        
        private TypeReference attributeRef;             //MethodBoundaryAspectAttribute
        private MethodReference attributeOnEnterRef;    //MethodBoundaryAspectAttribute.OnEnter
        private MethodReference attributeOnExitRef;     //MethodBoundaryAspectAttribute.OnExit
    
        private TypeReference dictTypeRef;                  //Dictionary<String,Object>
        private MethodReference dictConstructorMethodRef;   //Dictionary<String,Object>();
        private MethodReference dictAddMethodRef;           //Dictionary<String,Object>.Add();
        #endregion
    
        public MethodBoundaryInjector(AssemblyDefinition assembly) {
            this.assembly = assembly;
            mainModule = assembly.MainModule;
    
            getCurrentMethodRef = mainModule.ImportReference(typeof(System.Reflection.MethodBase).GetMethod("GetCurrentMethod"));
            getCustomAttributeRef = mainModule.ImportReference(typeof(System.Attribute).GetMethod("GetCustomAttribute", new Type[] { typeof(System.Reflection.MethodInfo), typeof(Type) }));
            getTypeFromHandleRef = mainModule.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle"));
            methodBaseRef = mainModule.ImportReference(typeof(System.Reflection.MethodBase));
    
            var attributeTypeDef = mainModule.FindTypeDefinition<MethodBoundaryAspectAttribute>();
            var attributeOnEnterDef = attributeTypeDef.FindMethodDefinition("OnEnter");
            var attributeOnExitDef = attributeTypeDef.FindMethodDefinition("OnExit");
    
            attributeRef = mainModule.ImportReference(attributeTypeDef);
            attributeOnEnterRef = mainModule.ImportReference(attributeOnEnterDef);
            attributeOnExitRef = mainModule.ImportReference(attributeOnExitDef);
    
            var dictionaryType = Type.GetType("System.Collections.Generic.Dictionary`2[System.String,System.Object]");
            dictTypeRef = mainModule.ImportReference(dictionaryType);
            dictConstructorMethodRef = mainModule.ImportReference(dictionaryType.GetConstructor(Type.EmptyTypes));
            dictAddMethodRef = mainModule.ImportReference(dictionaryType.GetMethod("Add"));
        }
    
        public void Inject() {
            foreach (TypeDefinition type in mainModule.GetTypes()) {
                foreach (MethodDefinition method in type.Methods) {
                    var adviceAttribute = method.FindAtributeInheritedFrom<MethodBoundaryAspectAttribute>();
                    if (adviceAttribute == null) continue;
    
                    InjectToMethod(method);
                }
            }
        }
    
        private void InjectToMethod(MethodDefinition method) {
            var body = method.Body;
            body.SimplifyMacros();
            var ilProc = body.GetILProcessor();
            method.Body.InitLocals = true; // создаем локальные переменные если нужно
    
    
            //////// Вставляем в начало вызов OnEnter у аттрибута ////////////
            
            // создаем три локальных переменных для attribute, currentMethod и parameters
            var attributeVar = new VariableDefinition(attributeRef);
            var currentMethodVar = new VariableDefinition(methodBaseRef);
            var dictVar = new VariableDefinition(dictTypeRef);
            ilProc.Body.Variables.Add(attributeVar);
            ilProc.Body.Variables.Add(currentMethodVar);
            ilProc.Body.Variables.Add(dictVar);
    
            Instruction firstInstruction = ilProc.Body.Instructions[0];
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Nop));
    
            //MethodBase currentMethodVar = System.Reflection.MethodBase.GetCurrentMethod(); //получаем текущий метод
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Call, getCurrentMethodRef));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stloc, currentMethodVar));
    
            //MethodBoundaryAspectAttribute attributeVar = (MethodBoundaryAspectAttribute)Attribute.GetCustomAttribute(currentMethod, typeof(MethodBoundaryAspectAttribute));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, currentMethodVar));
            {
                //typeof(MethodBoundaryAspectAttribute)
                ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldtoken, attributeRef));
                ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Call, getTypeFromHandleRef));
            }
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Call, getCustomAttributeRef)); // теперь у нас на стеке текущий метод и тип MethodInterceptionAttribute. Вызываем Attribute.GetCustomAttribute
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Castclass, attributeRef)); // приводим результат к типу MethodInterceptionAttribute
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stloc, attributeVar)); // сохраняем в локальной переменной attributeVar
    
            // Dictionary<stirng, object> dictVar = new Dictionary<stirng, object>();
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Newobj, dictConstructorMethodRef)); // создаем новый Dictionary<stirng, object>
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stloc, dictVar)); // помещаем в parametersVariable
    
            foreach (var argument in method.Parameters) {
                //dictVar.Add(argument.Name, argiment);
                ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, dictVar)); // загружаем на стек наш Dictionary<string,object>
                ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldstr, argument.Name)); // загружаем имя аргумента
                ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldarg, argument)); // загружаем значение аргумента
                ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Call, dictAddMethodRef)); // вызываем Dictionary.Add(string key, object value)
            }
    
            // attributeVar.OnEnter(currentMethodVar, dictVar)
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, attributeVar));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, currentMethodVar));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, dictVar));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Callvirt, attributeOnEnterRef));
    
            ///////////////////////////////////////////////////////////////////////////////////
             
            ////////// Вставляем try { code } finally { OnExit } //////////////////////////////
            var returnInstruction = FixReturns(method);
    
            var nope = Instruction.Create(OpCodes.Nop);
            ilProc.InsertBefore(returnInstruction, nope);
    
            ilProc.InsertBefore(returnInstruction, Instruction.Create(OpCodes.Ldloc, attributeVar));
            ilProc.InsertBefore(returnInstruction, Instruction.Create(OpCodes.Callvirt, attributeOnExitRef));
            ilProc.InsertBefore(returnInstruction, Instruction.Create(OpCodes.Endfinally));
    
            var handler = new ExceptionHandler(ExceptionHandlerType.Finally) {
                TryStart = firstInstruction,
                TryEnd = nope,
                HandlerStart = nope,
                HandlerEnd = returnInstruction,
            };
    
            body.ExceptionHandlers.Add(handler);
            body.InitLocals = true;
            body.OptimizeMacros();
        }
    
        private Instruction FixReturns(MethodDefinition method) {
            var body = method.Body;
            var typeSystem = mainModule.TypeSystem;
    
            if (method.ReturnType == typeSystem.Void) {
                var instructions = body.Instructions;
                var lastRet = Instruction.Create(OpCodes.Ret);
                instructions.Add(lastRet);
    
                for (var index = 0; index < instructions.Count - 1; index++) {
                    var instruction = instructions[index];
                    if (instruction.OpCode == OpCodes.Ret) {
                        instructions[index] = Instruction.Create(OpCodes.Leave, lastRet);
                    }
                }
                return lastRet;
    
            } else {
                var instructions = body.Instructions;
                var returnVariable = new VariableDefinition("methodTimerReturn", method.ReturnType);
                body.Variables.Add(returnVariable);
                var lastLd = Instruction.Create(OpCodes.Ldloc, returnVariable);
                instructions.Add(lastLd);
                instructions.Add(Instruction.Create(OpCodes.Ret));
    
                for (var index = 0; index < instructions.Count - 2; index++) {
                    var instruction = instructions[index];
                    if (instruction.OpCode == OpCodes.Ret) {
                        instructions[index] = Instruction.Create(OpCodes.Leave, lastLd);
                        instructions.Insert(index, Instruction.Create(OpCodes.Stloc, returnVariable));
                        index++;
                    }
                }
                return lastLd;
            }
        }
    }
}
