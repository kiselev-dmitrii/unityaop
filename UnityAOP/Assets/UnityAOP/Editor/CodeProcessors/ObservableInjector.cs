using System;
using System.Linq;
using System.Reflection;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Observable;
using Assets.UnityAOP.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UnityEngine.Assertions;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace Assets.UnityAOP.Editor.CodeProcessors {
public class ObservableInjector {
    private readonly ModuleDefinition module;

    #region References
    private readonly TypeReference observableImplTypeRef;
    private readonly MethodReference observableImplCtorRef;
    private readonly MethodReference setNumPropertiesRef;
    private readonly MethodReference notifyPropertyChangedRef;
    private readonly MethodReference addObserverRef;
    private readonly MethodReference removeObserverRef;

    private readonly TypeReference typeMetadataTypeRef;
    private readonly MethodReference getPropertyMetadataRef;

    private readonly MethodReference getTypeMetadataRef;
    #endregion

    #region Fields
    private FieldDefinition observableImplFieldDef;
    private FieldDefinition metadataFieldDef;
    #endregion

    public ObservableInjector(AssemblyDefinition assembly) {
        module = assembly.MainModule;

        TypeDefinition typeDef;

        typeDef = module.FindTypeDefinition(typeof(ObservableImpl));
        observableImplTypeRef = module.ImportReference(typeDef);
        observableImplCtorRef = module.ImportReference(typeDef.GetConstructors().FirstOrDefault());
        setNumPropertiesRef = module.ImportReference(typeDef.FindMethodDefinition("SetNumProperties"));
        notifyPropertyChangedRef = module.ImportReference(typeDef.FindMethodDefinition("NotifyPropertyChanged"));
        addObserverRef = module.ImportReference(typeDef.FindMethodDefinition("AddObserver"));
        removeObserverRef = module.ImportReference(typeDef.FindMethodDefinition("RemoveObserver"));

        typeDef = module.FindTypeDefinition(typeof(TypeMetadata));
        typeMetadataTypeRef = module.ImportReference(typeDef);
        getPropertyMetadataRef = module.ImportReference(typeDef.FindMethodDefinition("GetPropertyMetadata"));

        typeDef = module.FindTypeDefinition(typeof(ObservableMetadata));
        getTypeMetadataRef = module.ImportReference(typeDef.FindMethodDefinition("GetTypeMetadata"));
    }

    public void Inject() {
        var typeDefs = module.Types.Where(x => x.HasAttributeOfType<ObservableAttribute>());

        foreach (var targetTypeDef in typeDefs) {
            InjectToType(targetTypeDef);
        }
    }

    private void InjectToType(TypeDefinition targetTypeDef) {
        // Добавляем интерфей
        var interfaceDef = targetTypeDef.AddInterface<IObservable>();

        // Создаем и добавляем новые поля
        observableImplFieldDef = new FieldDefinition("ObservableImpl", FieldAttributes.Public, observableImplTypeRef);
        metadataFieldDef = new FieldDefinition("Metadata", FieldAttributes.Public, typeMetadataTypeRef);
        targetTypeDef.Fields.Add(observableImplFieldDef);
        targetTypeDef.Fields.Add(metadataFieldDef);

        // Инжектим инициализацию полей в конструктор
        foreach (var constructor in targetTypeDef.GetConstructors()) {
            InjectFieldInitialization(targetTypeDef, constructor);
        }

        MethodDefinition getPropertyMetadataDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "GetPropertyMetadata");
        ImplementGetPropertyMetadataMethod(getPropertyMetadataDef);

        MethodDefinition addObserverDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "AddObserver");
        ImplementAddObserverMethod(addObserverDef);

        MethodDefinition removeObserverDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "RemoveObserver");
        ImplementRemoveObserverMethod(removeObserverDef);

        MethodDefinition notifyPropertyChangedDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "NotifyPropertyChanged");
        ImplementNotifyPropertyChangedMethod(notifyPropertyChangedDef);

        var npcRef = module.ImportReference(notifyPropertyChangedDef);
        for (int i = 0; i < targetTypeDef.Properties.Count; ++i) {
            var property = targetTypeDef.Properties[i];
            InjectPropertySetter(property.SetMethod, i, npcRef);
        }
    }

    private void InjectFieldInitialization(TypeDefinition targetTypeDef, MethodDefinition constructor) {
        var body = constructor.Body;
        body.SimplifyMacros();
        var proc = body.GetILProcessor();

        //Ищем вызов базового констурктора и берем следующую инструкцию
        int baseConstructorCallIdx = proc.Body.Instructions.IndexOf(x => x.OpCode == OpCodes.Call);
        Assert.IsTrue(baseConstructorCallIdx >= 0, "Не найден вызов базового конструктора");
        var target = proc.Body.Instructions[baseConstructorCallIdx + 1];

        //////// Вставляем перед target все инструкции //////
        //ObservableImpl = new ObservableImpl();
        proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
        proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, observableImplCtorRef));
        proc.InsertBefore(target, Instruction.Create(OpCodes.Stfld, observableImplFieldDef));


        //Metadata = ObservableMetadata.GetTypeMetadata(targetTypeDef.Name);
        proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
        proc.InsertBefore(target, Instruction.Create(OpCodes.Ldstr, targetTypeDef.Name));
        proc.InsertBefore(target, Instruction.Create(OpCodes.Call, getTypeMetadataRef));
        proc.InsertBefore(target, Instruction.Create(OpCodes.Stfld, metadataFieldDef));

        //observableImpl.SetNumProperties(targetTypeDef.Properties.Count);
        proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
        proc.InsertBefore(target, Instruction.Create(OpCodes.Ldfld, observableImplFieldDef));
        proc.InsertBefore(target, Instruction.Create(OpCodes.Ldc_I4, targetTypeDef.Properties.Count));
        proc.InsertBefore(target, Instruction.Create(OpCodes.Callvirt, setNumPropertiesRef));
        //////////////////////////////////

        body.OptimizeMacros();
    }

    private void ImplementGetPropertyMetadataMethod(MethodDefinition method) {
        var body = method.Body;
        
        ///////////////////
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, metadataFieldDef));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, getPropertyMetadataRef));
        body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        ////////////////////
    }

    private void ImplementAddObserverMethod(MethodDefinition method) {
        var body = method.Body;

        ///////////////////
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, observableImplFieldDef));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
        body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, addObserverRef));
        body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        ////////////////////
    }

    private void ImplementRemoveObserverMethod(MethodDefinition method) {
        var body = method.Body;

        ///////////////////
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, observableImplFieldDef));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
        body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, removeObserverRef));
        body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        ////////////////////
    }

    private void ImplementNotifyPropertyChangedMethod(MethodDefinition method) {
        var body = method.Body;

        ///////////////////
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, observableImplFieldDef));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, notifyPropertyChangedRef));
        body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        ////////////////////
    }

    private void InjectPropertySetter(MethodDefinition setter, int propertyIndex, MethodReference notifyPropertyChanged) {
        var body = setter.Body;
        body.SimplifyMacros();
        var proc = body.GetILProcessor();

        //Вставлять будем до последнего ret
        var target = proc.Body.Instructions.Last();

        //////// Вставляем перед target все инструкции //////
        proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
        proc.InsertBefore(target, Instruction.Create(OpCodes.Ldc_I4, propertyIndex));
        proc.InsertBefore(target, Instruction.Create(OpCodes.Callvirt, notifyPropertyChanged));
        //////////////////////////////////

        body.OptimizeMacros();
    }
}
}
