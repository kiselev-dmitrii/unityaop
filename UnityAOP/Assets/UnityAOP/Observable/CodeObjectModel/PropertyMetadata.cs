using System;
using System.Reflection;

namespace Assets.UnityAOP.Observable.CodeObjectModel {
    public class PropertyMetadata : MemberMetadata {
        public bool IsObservable;
        public bool IsCollection;
        public Type ItemType;

        public PropertyMetadata(PropertyInfo propertyInfo)
            : base(propertyInfo.Name, propertyInfo.PropertyType, MemberType.Property) {
            IsObservable = Type.GetInterface("IObservable") != null;
            IsCollection = Type.GetInterface("IObservableCollection`1") != null;
            if (IsCollection) {
                ItemType = Type.GetGenericArguments()[0];
            }
        }
    
        public override string ToString() {
            return String.Format("{0} {1} { get; }", Type.Name, Name);
        }
    }
}