using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Assets.UnityAOP.Editor.CodeProcessors {
public class TryFinallyInjector {
    public ModuleDefinition Module { get; private set; }
    public MethodDefinition Method { get; private set; }

    public TryFinallyInjector(ModuleDefinition module, MethodDefinition method) {
        Module = module;
        Method = method;
    }

    public void Inject() {
        var body = Method.Body;
        body.SimplifyMacros();
        
        var ilProcessor = body.GetILProcessor();
        var returnInstruction = FixReturns();
        var firstInstruction = FirstInstructionSkipCtor();

        var beforeReturn = Instruction.Create(OpCodes.Endfinally);
        ilProcessor.InsertBefore(returnInstruction, beforeReturn);

        //InjectIlForFinaly(returnInstruction);

        var handler = new ExceptionHandler(ExceptionHandlerType.Finally) {
            TryStart = firstInstruction,
            TryEnd = beforeReturn,
            HandlerStart = beforeReturn,
            HandlerEnd = returnInstruction,
        };

        body.ExceptionHandlers.Add(handler);
        body.InitLocals = true;
        body.OptimizeMacros();
    }

    private Instruction FixReturns() {
        var body = Method.Body;
        var typeSystem = Module.TypeSystem;

        if (Method.ReturnType == typeSystem.Void) {
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
            var returnVariable = new VariableDefinition("methodTimerReturn", Method.ReturnType);
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

    private Instruction FirstInstructionSkipCtor() {
        var body = Method.Body;

        if (Method.IsConstructor && !Method.IsStatic) {
            return body.Instructions.Skip(2).First();
        }
        return body.Instructions.First();
    }

}
}
