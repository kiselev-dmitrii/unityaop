using System;
using System.Collections.Generic;

namespace Assets.TestTask {
    public class BaseClass {
        public String TypeName;

        public BaseClass(String typeName) {
            TypeName = typeName;
        }

        public String GetTypeName() {
            return TypeName;
        }
    }

    public class DerivedClass<T> : BaseClass {
        public T Field;
        public List<T> Array;
        public int IntValue;
        public Dictionary<T, T> Dictionary;
        public Dictionary<T, Dictionary<int, T>> NestedDictionary; 
         
        public DerivedClass(string typeName) : base(typeName) {}
    }
}
