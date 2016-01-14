using System.Linq;
using System.Reflection;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Observable;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Assets.UnityAOP.Editor.CodeProcessors {
public class ObservableInjector {
    private readonly AssemblyDefinition assembly;
    private readonly ModuleDefinition mainModule;

    public ObservableInjector(AssemblyDefinition assembly) {
        this.assembly = assembly;
        mainModule = assembly.MainModule;
    }

    public void Inject() {
        var typeDefs = mainModule.Types.Where(x => x.HasAttributeOfType<ObservableAttribute>());

        foreach (var targetTypeDef in typeDefs) {
            InjectToType(targetTypeDef);
        }
    }

    public void InjectToType(TypeDefinition targetTypeDef) {
        var interfaceDef = targetTypeDef.AddInterface<IObservable>();

        MethodDefinition getPropertyMetadataDef = targetTypeDef.ImplementMethod(interfaceDef, "GetPropertyMetadata");
        CreateDebugLog(getPropertyMetadataDef);

        MethodDefinition addObserverDef = targetTypeDef.ImplementMethod(interfaceDef, "AddObserver");
        CreateDebugLog(addObserverDef);

        MethodDefinition removeObserverDef = targetTypeDef.ImplementMethod(interfaceDef, "RemoveObserver");
        CreateDebugLog(removeObserverDef);
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
}
}
