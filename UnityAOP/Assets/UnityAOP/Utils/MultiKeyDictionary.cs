using System.Collections.Generic;

namespace Assets.UnityAOP.Utils {
    public class MultiKeyDictionary<K1, K2, T> {
        private readonly Dictionary<K1, Dictionary<K2, T>> mDictionary;
    
        public MultiKeyDictionary() {
            mDictionary = new Dictionary<K1, Dictionary<K2, T>>();
        }
    
        public T Get(K1 key1, K2 key2) {
            Dictionary<K2, T> nestedDictionary = null;
            if (mDictionary.TryGetValue(key1, out nestedDictionary)) {
                T result;
                if (nestedDictionary.TryGetValue(key2, out result)) {
                    return result;
                }
            }
    
            return default(T);
        }
    
        public void Set(K1 key1, K2 key2, T value) {
            Dictionary<K2, T> nestedDictionary = null;
            if (!mDictionary.TryGetValue(key1, out nestedDictionary)) {
                nestedDictionary = new Dictionary<K2, T>();
                mDictionary.Add(key1, nestedDictionary);
            }
    
            nestedDictionary[key2] = value;
        }
    
        public Dictionary<K2, T> GetNested(K1 key1) {
            return mDictionary.Get(key1);
        }
    }
}
