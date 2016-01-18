using System;
using System.Collections.Generic;
using System.Linq;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Observable;
using Assets.UnityAOP.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using UnityEngine.Assertions;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace Assets.UnityAOP.Editor.Injectors {
    public class ObservableInjector {
        private readonly ModuleDefinition module;
    
        #region References
        private readonly TypeReference observableImplTypeRef;
        private readonly MethodReference observableImplCtorRef;
        private readonly MethodReference setNumPropertiesRef;
        private readonly MethodReference notifyPropertyChangedRef;
        private readonly MethodReference addObserverRef;
        private readonly MethodReference removeObserverRef;
    
        private readonly TypeReference iObservableTypeRef;
    
        private readonly TypeReference listTypeRef;
        private readonly MethodReference listCtorRef;
        private readonly MethodReference listAddRef;
        private readonly MethodReference listGetItemRef;
        #endregion
    
        #region Definitions
        private readonly TypeDefinition getterDelegateGenericTypeDef;
        private readonly TypeDefinition setterDelegateGenericTypeDef;
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
    
            typeDef = module.FindTypeDefinition(typeof (IObservable));
            iObservableTypeRef = module.ImportReference(typeDef);
    
            var listType = Type.GetType("System.Collections.Generic.List`1[System.Object]");
            listTypeRef = module.ImportReference(listType);
            listCtorRef = module.ImportReference(listType.GetConstructor(Type.EmptyTypes));
            listAddRef = module.ImportReference(listType.GetMethod("Add"));
            listGetItemRef = module.ImportReference(listType.GetMethod("get_Item"));
    
            getterDelegateGenericTypeDef = module.FindTypeDefinition("GetterDelegate`1");
            setterDelegateGenericTypeDef = module.FindTypeDefinition("SetterDelegate`1");
        }
    
        public void Inject() {
            List<TypeDefinition> taggedTypes =
                module.Types.Where(x => x.HasAttributeOfType<ObservableAttribute>()).ToList();

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
            FieldDefinition observableImplFieldDef = new FieldDefinition("ObservableImpl", FieldAttributes.Public, observableImplTypeRef);
            FieldDefinition gettersFieldDef = new FieldDefinition("Getters", FieldAttributes.Public, listTypeRef);
            FieldDefinition settersFieldDef = new FieldDefinition("Setters", FieldAttributes.Public, listTypeRef);
            targetTypeDef.Fields.Add(observableImplFieldDef);
            targetTypeDef.Fields.Add(gettersFieldDef);
            targetTypeDef.Fields.Add(settersFieldDef);

            // Инжектим инициализацию полей в конструктор
            foreach (var constructor in targetTypeDef.GetConstructors()) {
                InjectBaseFieldInitialization(targetTypeDef, constructor, observableImplFieldDef, gettersFieldDef, settersFieldDef);
            }

            MethodDefinition addObserverDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "AddObserver");
            ImplementAddObserverMethod(addObserverDef, observableImplFieldDef);

            MethodDefinition removeObserverDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "RemoveObserver");
            ImplementRemoveObserverMethod(removeObserverDef, observableImplFieldDef);

            MethodDefinition notifyPropertyChangedDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "NotifyPropertyChanged");
            ImplementNotifyPropertyChangedMethod(notifyPropertyChangedDef, observableImplFieldDef);

            MethodDefinition getGetterDelegateDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "GetGetterDelegate");
            ImplementGetGetterDelegatedMethod(getGetterDelegateDef, gettersFieldDef);

            MethodDefinition getSetterDelegateDef = targetTypeDef.AddInterfaceMethod(interfaceDef, "GetSetterDelegate");
            ImplementGetSetterDelegatedMethod(getSetterDelegateDef, settersFieldDef);

            //Инжектим нотификацию в сеттеры
            var npcRef = module.ImportReference(notifyPropertyChangedDef);
            for (int i = 0; i < targetTypeDef.Properties.Count; ++i) {
                var property = targetTypeDef.Properties[i];
                InjectPropertySetter(property.SetMethod, i, npcRef);
            }
        }

        public void ProcessDerivedType(TypeDefinition targetTypeDef) {
            List<TypeDefinition> observableParents =
                targetTypeDef.Parents().Where(x => x.HasAttributeOfType<ObservableAttribute>()).ToList();

            //Выясняем базовый тип, у которого и находятся все поля
            TypeDefinition baseObservableType = observableParents.Last();

            //Находим нужные нам поля и методы
            FieldDefinition observableImplFieldDef = baseObservableType.FindField("ObservableImpl");
            FieldDefinition gettersFieldDef = baseObservableType.FindField("Getters");
            FieldDefinition settersFieldDef = baseObservableType.FindField("Setters");
            MethodDefinition notifyPropertyChangedDef = baseObservableType.FindMethodDefinition("NotifyPropertyChanged");

            //Считаем с какого индекса нумеровать свойства
            int numParentsProperties = observableParents.Sum(x => x.Properties.Count);
            int commonNumProperties = numParentsProperties + targetTypeDef.Properties.Count;

            // Инжектим инициализацию полей в конструктор
            foreach (var constructor in targetTypeDef.GetConstructors()) {
                InjectDerivedFieldInitialization(targetTypeDef, constructor, observableImplFieldDef, gettersFieldDef, settersFieldDef, commonNumProperties);
            }

            //Инжектим нотификацию в сеттеры
            var npcRef = module.ImportReference(notifyPropertyChangedDef);
            for (int i = 0; i < targetTypeDef.Properties.Count; ++i) {
                var property = targetTypeDef.Properties[i];
                int propertyIndex = numParentsProperties + i;
                InjectPropertySetter(property.SetMethod, propertyIndex, npcRef);
            }
        }

        private void InjectBaseFieldInitialization(TypeDefinition targetTypeDef, MethodDefinition constructor,
            FieldDefinition observableImplFieldDef, FieldDefinition gettersFieldDef, FieldDefinition settersFieldDef) {
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
    
            //Getters = new List<object>();
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, listCtorRef));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Stfld, gettersFieldDef));
    
            //Setters = new List<object>();
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, listCtorRef));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Stfld, settersFieldDef));
    
            //observableImpl.SetNumProperties(targetTypeDef.Properties.Count);
            InjectSetNumProperties(targetTypeDef.Properties.Count, proc, target, observableImplFieldDef);
    
            //Добавляем сеттеры и геттеры
            InjectGettersSettersInitialization(targetTypeDef.Properties, proc, target, gettersFieldDef, settersFieldDef);
            //////////////////////////////////
    
            body.OptimizeMacros();
        }

        private void InjectDerivedFieldInitialization(TypeDefinition targetTypeDef, MethodDefinition constructor,
                                                      FieldDefinition observableImplFieldDef,
                                                      FieldDefinition gettersFieldDef, FieldDefinition settersFieldDef, int commonNumProperties) {
            var body = constructor.Body;
            body.SimplifyMacros();
            var proc = body.GetILProcessor();

            //Ищем вызов базового констурктора и берем следующую инструкцию
            int baseConstructorCallIdx = proc.Body.Instructions.IndexOf(x => x.OpCode == OpCodes.Call);
            Assert.IsTrue(baseConstructorCallIdx >= 0, "Не найден вызов базового конструктора");
            var target = proc.Body.Instructions[baseConstructorCallIdx + 1];

            //////// Вставляем перед target все инструкции //////
            //observableImpl.SetNumProperties(targetTypeDef.Properties.Count);
            InjectSetNumProperties(commonNumProperties, proc, target, observableImplFieldDef);

            //Добавляем сеттеры и геттеры
            InjectGettersSettersInitialization(targetTypeDef.Properties, proc, target, gettersFieldDef, settersFieldDef);
            //////////////////////////////////

            body.OptimizeMacros();
        }

        private void InjectSetNumProperties(int numProperties, ILProcessor proc, Instruction target, FieldDefinition observableImplFieldDef) {
            //observableImpl.SetNumProperties(targetTypeDef.Properties.Count);
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldfld, observableImplFieldDef));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Ldc_I4, numProperties));
            proc.InsertBefore(target, Instruction.Create(OpCodes.Callvirt, setNumPropertiesRef));
        } 

        private void InjectGettersSettersInitialization(Collection<PropertyDefinition> properties, ILProcessor proc, Instruction target, 
                                                        FieldDefinition gettersFieldDef, FieldDefinition settersFieldDef) {
            //Добавляем сеттеры и геттеры
            for (int i = 0; i < properties.Count; ++i) {
                var property = properties[i];
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
                GenericInstanceType getterDelegateTypeRef = getterDelegateGenericTypeDef.MakeGenericInstanceType(genericArg);
                MethodReference getterDelegateCtorRef = getterDelegateTypeRef.Resolve().GetConstructors().First().MakeHostInstanceGeneric(genericArg);

                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldfld, gettersFieldDef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldftn, getterMethodDef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, getterDelegateCtorRef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Callvirt, listAddRef));
                ////////////////////////

                // Добавляем сеттер на проперти /////////////////////
                GenericInstanceType setterDelegateTypeRef = setterDelegateGenericTypeDef.MakeGenericInstanceType(genericArg);
                MethodReference setterDelegateCtorRef = setterDelegateTypeRef.Resolve().GetConstructors().First().MakeHostInstanceGeneric(genericArg);

                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldfld, settersFieldDef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldarg_0));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Ldftn, setterMethodDef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Newobj, setterDelegateCtorRef));
                proc.InsertBefore(target, Instruction.Create(OpCodes.Callvirt, listAddRef));
                //////////////////////////////////
            }
        }

        private void ImplementAddObserverMethod(MethodDefinition method, FieldDefinition observableImplFieldDef) {
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

        private void ImplementRemoveObserverMethod(MethodDefinition method, FieldDefinition observableImplFieldDef) {
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
    
        private void ImplementNotifyPropertyChangedMethod(MethodDefinition method, FieldDefinition observableImplFieldDef) {
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
            body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, listGetItemRef));
            body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            ////////////////////
        }
    
        private void ImplementGetSetterDelegatedMethod(MethodDefinition method, FieldDefinition settersFieldDef) {
            var body = method.Body;
    
            ///////////////////
            body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, settersFieldDef));
            body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, listGetItemRef));
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
