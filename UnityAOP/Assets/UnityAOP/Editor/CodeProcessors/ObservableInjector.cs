using System.Linq;
using System.Reflection;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Observable;
using Mono.Cecil;
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
        var observableType = mainModule.Types.FirstOrDefault(x => x.HasAttributeOfType<ObservableAttribute>());
        if (observableType == null) {
            return;
        }

        var iObservableTypeDef = mainModule.FindTypeDefinition<IObservable>();
        var iObservableTypeRef = mainModule.ImportReference(iObservableTypeDef);
        observableType.Interfaces.Add(iObservableTypeRef);
    }
}
}
