using System;
using System.Collections.Generic;
using System.Text;
using Assets.UnityAOP.Observable.Core;
using Assets.UnityAOP.Utils;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Observable.CodeObjectModel {
    public class TypeMetadata {
        public String Name { get; private set; }
        public Type Type { get; private set; }
        private readonly Dictionary<String, PropertyMetadata> properties;
        private readonly Dictionary<String, MethodMetadata> methods;
        private readonly Dictionary<String, MemberMetadata> members;

        public TypeMetadata(Type type) {
            Assert.IsTrue(type.HasAttribute<ObservableAttribute>(), "Тип должен иметь аттрибут Observable ");

            Name = type.Name;
            Type = type;
            properties = new Dictionary<string, PropertyMetadata>();
            methods = new Dictionary<string, MethodMetadata>();
            members = new Dictionary<string, MemberMetadata>();

            // Записываем всю иерархию наблюдаемых типов
            Stack<Type> hierarchy = new Stack<Type>();
            hierarchy.Push(type);
            var baseType = hierarchy.Peek().BaseType;
            while (baseType.HasAttribute<ObservableAttribute>()) {
                hierarchy.Push(baseType);
                baseType = hierarchy.Peek().BaseType;
            }
    
            // Спускаемся с базового типа до текущего и записываем информацию о свойствах и методах
            // Алгоритм таков, чтобы перезаписать свойство базового класса, в случае если оно будет переопределено или иметь такое же имя
            while (hierarchy.Count > 0) {
                var t = hierarchy.Pop();

                var typeProperties = t.GetProperties();
                foreach (var p in typeProperties) {
                    var meta = new PropertyMetadata(p);
                    properties[meta.Name] = meta;
                    members[meta.Name] = meta;
                }

                var typeMethods = t.GetMethods();
                foreach (var m in typeMethods) {
                    if (m.IsConstructor || m.IsSpecialName || m.IsAbstract) continue;

                    var meta = new MethodMetadata(m);
                    methods[meta.Name] = meta;
                    members[meta.Name] = meta;
                }
            }
        }
    
        public PropertyMetadata GetProperty(String name) {
            return properties.Get(name);
        }

        public MethodMetadata GetMethod(String name) {
            return methods.Get(name);
        }

        public MemberMetadata GetMember(String name) {
            return members.Get(name);
        }

        public IEnumerable<MemberMetadata> Members {
            get { return members.Values; }
        } 

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            builder.Append(Name + '\n');
            foreach (var pair in members) {
                builder.Append('\t' + pair.Value.ToString() + '\n');
            }
            return builder.ToString();
        }
    }
}