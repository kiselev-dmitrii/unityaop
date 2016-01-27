using System;
using System.Collections.Generic;
using System.Text;
using Assets.UnityAOP.Utils;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Observable.CodeObjectModel {
    public class TypeMetadata {
        public String Name { get; private set; }
        public Type Type { get; private set; }
        public Dictionary<String, PropertyMetadata> Properties { get; private set; }
        public Dictionary<String, MethodMetadata> Methods { get; private set; } 
        public Dictionary<String, MemberMetadata> Members { get; private set; } 

        public TypeMetadata(Type type) {
            Assert.IsTrue(type.HasAttribute<ObservableAttribute>(), "Тип должен иметь аттрибут Observable ");

            Name = type.Name;
            Type = type;
            Properties = new Dictionary<string, PropertyMetadata>();
            Members = new Dictionary<string, MemberMetadata>();
            Members = new Dictionary<string, MemberMetadata>();

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

                var properties = t.GetProperties();
                foreach (var p in properties) {
                    var meta = new PropertyMetadata(p);
                    Properties[meta.Name] = meta;
                    Members[meta.Name] = meta;
                }

                var methods = t.GetMethods();
                foreach (var m in methods) {
                    var meta = new MethodMetadata(m);
                    Methods[meta.Name] = meta;
                    Members[meta.Name] = meta;
                }
            }
        }

        public TypeMetadata(Type type, Dictionary<String, PropertyMetadata> properties) {
            Name = type.Name;
            Type = type;
            Properties = properties;
        }
    
        public PropertyMetadata GetPropertyMetadata(String name) {
            return Properties.Get(name);
        }
    
        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            builder.Append(Name + '\n');
            foreach (var property in Properties.Values) {
                builder.Append(property.ToString() + '\n');
            }
            return builder.ToString();
        }
    }
}