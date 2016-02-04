using System;
using System.Reflection;

namespace Assets.UnityAOP.Observable.CodeObjectModel {
    public class PropertyMetadata : MemberMetadata {
        public bool IsObservable { get; private set; }
        public bool IsCollection { get; private set; }
        public Type ItemType { get; private set; }

        public PropertyMetadata(PropertyInfo propertyInfo) : base(propertyInfo.Name, propertyInfo.PropertyType, MemberType.Property) {
            IsObservable = Type.GetInterface("IObservable") != null;
            IsCollection = Type.GetInterface("IObservableList`1") != null;
            if (IsCollection) {
                ItemType = Type.GetGenericArguments()[0];
            }
        }

        public PropertyMetadata(String name, Type type, int code) : base(name, type, code, MemberType.Property) {
            IsObservable = Type.GetInterface("IObservable") != null;
            IsCollection = Type.GetInterface("IObservableList`1") != null;
            if (IsCollection) {
                ItemType = Type.GetGenericArguments()[0];
            }
        }

        public override string ToString() {
            return String.Format("{0} {1} { get; }", Type.Name, Name);
        }
    }
}