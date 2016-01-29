using System;
using System.Collections.Generic;
using System.Linq;
using Assets.UnityAOP.Observable.Core;
using Assets.UnityAOP.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using UnityEngine.Assertions;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace Assets.UnityAOP.Editor.Injectors.Observable {
    public class ObservableInjector {
        private readonly ModuleDefinition module;
    
        #region References
        private readonly TypeReference observableImplementationTypeRef;
        private readonly MethodReference observableImplementationCtorRef;
        private readonly MethodReference notifyPropertyChangedRef;
        private readonly MethodReference addObserverRef;
        private readonly MethodReference removeObserverRef;
    
        private readonly TypeReference iObservableTypeRef;

        private readonly TypeReference dictTypeRef;
        private readonly MethodReference dictCtorRef;
        private readonly MethodReference dictSetItemRef;
        private readonly MethodReference dictGetItemRef;
        #endregion
    
        #region Definitions
        private readonly TypeDefinition getterDelegateGenericTypeDef;
        private readonly TypeDefinition setterDelegateGenericTypeDef;
        #endregion
    
        public ObservableInjector(AssemblyDefinition assembly) {
            module = assembly.MainModule;
    
            TypeDefinition typeDef;
    
            typeDef = module.FindTypeDefinition(typeof(ObservableImplementation));
            observableImplementationTypeRef = module.ImportReference(typeDef);
            observableImplementationCtorRef = module.ImportReference(typeDef.GetConstructors().FirstOrDefault());
            notifyPropertyChangedRef = module.ImportReference(typeDef.FindMethodDefinition("NotifyPropertyChanged"));
            addObserverRef = module.ImportReference(typeDef.FindMethodDefinition("AddObserver"));
            removeObserverRef = module.ImportReference(typeDef.FindMethodDefinition("RemoveObserver"));
    
            typeDef = module.FindTypeDefinition(typeof (IObservable));
            iObservableTypeRef = module.ImportReference(typeDef);

            var dictType = Type.GetType("System.Collections.Generic.Dictionary`2[System.Int32, System.Object]");
            dictTypeRef = module.ImportReference(dictType);
            dictCtorRef = module.ImportReference(dictType.GetConstructor(Type.EmptyTypes));
            dictSetItemRef = module.ImportReference(dictType.GetMethod("set_Item"));
            dictGetItemRef = module.ImportReference(dictType.GetMethod("get_Item"));
    
            getterDelegateGenericTypeDef = module.FindTypeDefinition("GetterDelegate`1");
            setterDelegateGenericTypeDef = module.FindTypeDefinition("SetterDelegate`1");
        }
    
        public void Inject() {
            List<TypeDefinition> taggedTypes = module.Types.Where(x => x.HasAttributeOfType<ObservableAttribute>()).ToList();

            HashSet<TypeDefinition> baseTypes = new HashSet<TypeDefinition>();
            HashSet<TypeDefinition> derivedTypes = new HashSet<TypeDefinition>();
            foreach (var type in taggedTypes) {
                bool isDerived = type.BaseType.Resolve().HasAttributeOfType<ObservableAttribute>();
                if (isDerived) {
                    derivedTypes.Add(type);
                } else {
                    baseTypes.Add(type);
                }
            }


            foreach (var baseType in baseTypes) {
                ProcessBaseType(baseType);
            }

            foreach (var derivedType in derivedTypes) {
                ProcessDerivedType(derivedType);
            }
        }

        public void ProcessBaseType(TypeDefinition targetTypeDef) {
            // Добавляем интерфей
            var interfaceDef = targetTypeDef.AddInterface<IObservable>();

            // Создаем и добавляем новые поля
            FieldDefinition observableImplFieldDef = new FieldDefinition("ObservableImpl", FieldAttributes.Public, observableImplementationTypeRef);
            FieldDefinition gettersFieldDef = new FieldDefinition("Getters", FieldAttributes.Public, dictTypeRef);
            FieldDefinition settersFieldDef = new FieldDefinition("Setters", FieldAttributes.Public, dictTypeRef);
            FieldDefinition methodsFieldDef = new FieldDefinition("Methods", FieldAttributes.Public, dictTypeRef);
            targetTypeDef.Fields.Add(observableImplFieldDef);
            targetTypeDef.Fields.Add(gettersFieldDef);
            targetTypeDef.Fields.Add(settersFieldDef);
            targetTypeDef.Fields.Add(methodsFieldDef);

            // Инжектим инициализацию полей в конструктор
            foreach (var constructor in targetTypeDef.GetConstructors()) {
                if (constructor.IsStatic) continue;
                InjectBaseFieldInitialization(targetTypeDef, constructor, observableImplFieldDef, gettersFieldDef, settersFieldDef, methodsFieldDef);
            }

            MethodDefinition addMemberObserverDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "AddMemberObserver");
            ImplementAddMemberObserverMethod(addMemberObserverDef, observableImplFieldDef);

            MethodDefinition removeMemberObserverDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "RemoveMemberObserver");
            ImplementRemoveMemberObserverMethod(removeMemberObserverDef, observableImplFieldDef);

            MethodDefinition notifyMemberChangedDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "NotifyMemberChanged");
            ImplementNotifyMemberChangedMethod(notifyMemberChangedDef, observableImplFieldDef);

            MethodDefinition getGetterDelegateDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "GetGetterDelegate");
            ImplementGetGetterDelegatedMethod(getGetterDelegateDef, gettersFieldDef);

            MethodDefinition getSetterDelegateDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "GetSetterDelegate");
            ImplementGetSetterDelegatedMethod(getSetterDelegateDef, settersFieldDef);

            MethodDefinition getMethodDelegateDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "GetMethodDelegate");
            ImplementGetMethodDelegatedMethod(getMethodDelegateDef, methodsFieldDef);

            //Инжектим нотификацию в сеттеры
            var notifyMemberChangedRef = module.ImportReference(notifyMemberChangedDef);
            foreach (var property in targetTypeDef.Properties) {
                InjectPropertySetter(property, notifyMemberChangedRef);
            }
        }

        public void ProcessDerivedType(TypeDefinition typeDef) {
            //Выясняем базовый тип, у которого и находятся все поля
            TypeDefinition baseType = typeDef.Parents().Where(x => x.HasAttributeOfType<ObservableAttribute>()).Last();

            //Находим нужные нам поля и методы
            FieldDefinition gettersFieldDef = baseType.FindField("Getters");
            FieldDefinition settersFieldDef = baseType.FindField("Setters");
            FieldDefinition methodsFieldDef = baseType.FindField("Methods");
            MethodDefinition notifyMemberChangedDef = baseType.FindMethodDefinition("NotifyMemberChanged");

            // Инжектим инициализацию полей в конструктор
            foreach (var constructor in typeDef.GetConstructors()) {
                InjectDerivedFieldInitialization(typeDef, constructor, gettersFieldDef, settersFieldDef, methodsFieldDef);
            }

            //Инжектим нотификацию в сеттеры
            var notifyMemberChangedRef = module.ImportReference(notifyMemberChangedDef);
            foreach (var property in typeDef.Properties) {
                InjectPropertySetter(property, notifyMemberChangedRef);
            }
        }

        private void InjectBaseFieldInitialization(TypeDefinition targetTypeDef, MethodDefinition constructor,
            FieldDefinition observableImplFieldDef, FieldDefinition gettersFieldDef, FieldDefinition settersFieldDef, FieldDefinition methodsFieldDef) {
            var body = constructor.Body;
            body.SimplifyMacros();
            var proc = body.GetILProcessor();
    
            //Ищем вызов базового констурктора и берем следующую инструкцию
            int baseConstructorCallIdx = proc.Body.Instructions.IndexOf(x => x.OpCode == OpCodes.Call);
            Assert.IsTrue(baseConstructorCallIdx >= 0, "Не найден вызов базового конструктора");
            var target = proc.Body.Instructions[baseConstructorCallIdx + 1];
    
            //////// Вставляем перед target все инструкции //////
            // ObservableImpl = new ObservableImplementation();
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, observableImplementationCtorRef));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Stfld, observableImplFieldDef));

            //Getters = new Dictionary<int, Object>();
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, dictCtorRef));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Stfld, gettersFieldDef));

            //Setters = new Dictionary<int, Object>();
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, dictCtorRef));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Stfld, settersFieldDef));

            //Methods = new Dictionary<int, Object>();
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, dictCtorRef));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Stfld, methodsFieldDef));
    
            //Устанавливаем сеттеры, геттеры и методы
            InjectGettersSettersInitialization(targetTypeDef.Properties, proc, target, gettersFieldDef, settersFieldDef);
            InjectMethodsInitialization(targetTypeDef, proc, target, methodsFieldDef);
            //////////////////////////////////
    
            body.OptimizeMacros();
        }

        private void InjectDerivedFieldInitialization(TypeDefinition targetTypeDef, MethodDefinition constructor,
                                                      FieldDefinition gettersFieldDef, FieldDefinition settersFieldDef, FieldDefinition methodsFieldDef) {
            var body = constructor.Body;
            body.SimplifyMacros();
            var proc = body.GetILProcessor();

            //Ищем вызов базового констурктора и берем следующую инструкцию
            int baseConstructorCallIdx = proc.Body.Instructions.IndexOf(x => x.OpCode == OpCodes.Call);
            Assert.IsTrue(baseConstructorCallIdx >= 0, "Не найден вызов базового конструктора");
            var target = proc.Body.Instructions[baseConstructorCallIdx + 1];

            //Устанавливаем сеттеры, геттеры и методы
            InjectGettersSettersInitialization(targetTypeDef.Properties, proc, target, gettersFieldDef, settersFieldDef);
            InjectMethodsInitialization(targetTypeDef, proc, target, methodsFieldDef);

            body.OptimizeMacros();
        }

        private void InjectGettersSettersInitialization(Collection<PropertyDefinition> properties, ILProcessor proc, Instruction target,
                                                        FieldDefinition gettersFieldDef, FieldDefinition settersFieldDef) {
            //Добавляем сеттеры и геттеры
            foreach (var property in properties) {
                MethodDefinition getterMethodDef = property.GetMethod;
                MethodDefinition setterMethodDef = property.SetMethod;
                TypeDefinition propertyTypeDef = property.PropertyType.Resolve();

                TypeReference genericArg = null;
                if (propertyTypeDef.HasAttributeOfType<ObservableAttribute>() || propertyTypeDef.HasInterface<IObservable>()) {
                    genericArg = iObservableTypeRef;
                } else {
                    genericArg = property.PropertyType;
                }

                // Добавляем геттер на преперти ////////////////////////
                /// Getters["Id".GetHashCode()] = new GetterDelegate<Int32/IObservable>(delegate { return Id; });
                GenericInstanceType getterDelegateTypeRef = getterDelegateGenericTypeDef.MakeGenericInstanceType(genericArg);
                MethodReference getterDelegateCtorRef = getterDelegateTypeRef.Resolve().GetConstructors().First().MakeHostInstanceGeneric(genericArg);

                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldfld, gettersFieldDef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldc_I4, GetMemberCode(property.Name)));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldftn, getterMethodDef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, getterDelegateCtorRef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Callvirt, dictSetItemRef));
                ////////////////////////

                // Добавляем сеттер на проперти /////////////////////
                // Setters["Id".GetHashCode()] = new SetterDelegate<Int32/IObservable>(delegate(Int32 value) { Id = value; });
                GenericInstanceType setterDelegateTypeRef = setterDelegateGenericTypeDef.MakeGenericInstanceType(genericArg);
                MethodReference setterDelegateCtorRef = setterDelegateTypeRef.Resolve().GetConstructors().First().MakeHostInstanceGeneric(genericArg);

                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldfld, settersFieldDef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldc_I4, GetMemberCode(property.Name)));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldftn, setterMethodDef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, setterDelegateCtorRef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Callvirt, dictSetItemRef));
            }
        }

        private void InjectMethodsInitialization(TypeDefinition type, ILProcessor proc, Instruction target, FieldDefinition methodsFieldDef) {
            var methods = type
                .Methods
                .Where(x => !x.IsConstructor && !x.IsGetter && !x.IsSetter && !x.IsAbstract && x.HasBody)
                .ToArray();
            
            foreach (var methodDef in methods) {
                ModuleDefinition methodModule = methodDef.Module;

                MethodReference delegateCtorRef = null;
                if (methodDef.ReturnType == methodModule.TypeSystem.Void) {
                    CecilUtils.GetActionDelegateType(
                        methodModule, 
                        methodDef.Parameters.Select(x => x.ParameterType).ToArray(), 
                        out delegateCtorRef);
                } else {
                    CecilUtils.GetFuncDelegateType(methodModule, 
                        methodDef.ReturnType, 
                        methodDef.Parameters.Select(x => x.ParameterType).ToArray(), 
                        out delegateCtorRef);
                }

                // Добавляем делегат на на метод
                // Methods["IncreaseRating".GetHashCode()] = new Action/Func(IncreaseRating);
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldfld, methodsFieldDef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldc_I4, GetMemberCode(methodDef.Name)));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldftn, methodDef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, delegateCtorRef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Callvirt, dictSetItemRef));
            }
        }

        private void ImplementAddMemberObserverMethod(MethodDefinition method, FieldDefinition observableImplFieldDef) {
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

        private void ImplementRemoveMemberObserverMethod(MethodDefinition method, FieldDefinition observableImplFieldDef) {
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
    
        private void ImplementNotifyMemberChangedMethod(MethodDefinition method, FieldDefinition observableImplFieldDef) {
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

        private void ImplementGetGetterDelegatedMethod(MethodDefinition method, FieldDefinition gettersFieldDef) {
            var body = method.Body;
    
            ///////////////////
            body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, gettersFieldDef));
            body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, dictGetItemRef));
            body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            ////////////////////
        }
    
        private void ImplementGetSetterDelegatedMethod(MethodDefinition method, FieldDefinition settersFieldDef) {
            var body = method.Body;
    
            ///////////////////
            body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, settersFieldDef));
            body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, dictGetItemRef));
            body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            ////////////////////
        }

        private void ImplementGetMethodDelegatedMethod(MethodDefinition method, FieldDefinition methodsFieldDef) {
            var body = method.Body;

            ///////////////////
            body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, methodsFieldDef));
            body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, dictGetItemRef));
            body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            ////////////////////
        }
    
        private void InjectPropertySetter(PropertyDefinition property, MethodReference notifyMemberChangedRef) {
            var body = property.SetMethod.Body;
            body.SimplifyMacros();
            var proc = body.GetILProcessor();
    
            //Вставлять будем до последнего ret
            var target = proc.Body.Instructions.Last();
    
            //////// Вставляем перед target все инструкции //////
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldc_I4, GetMemberCode(property.Name)));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Callvirt, notifyMemberChangedRef));
            //////////////////////////////////
    
            body.OptimizeMacros();
        }

        private static Int32 GetMemberCode(String memberName) {
            return memberName.GetHashCode();
        }
    }
}
