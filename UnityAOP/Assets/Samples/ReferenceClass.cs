using System;
using System.Collections.Generic;

namespace Assets.Samples {
    public class ReferenceClass : BaseClass {
        public String Field;
        public List<String> Array;
        public int IntValue;
        public Dictionary<String, String> Dictionary;
        public Dictionary<String, Dictionary<int, String>> NestedDictionary;

        public ReferenceClass(string typeName) : base(typeName) { }
    }
}
