using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.UnityAOP.Binding.Core {
    [Serializable]
    public class BindingPath {
        [SerializeField] 
        private List<String> items;

        public String[] Slice(int start, int length) {
            return items.Skip(start).Take(length).ToArray();
        }

        public String Last() {
            return items[items.Count - 1];
        }

        public int Length() {
            return items.Count;
        }

        public String[] ToArray() {
            return items.ToArray();
        }

        public void SetPath(String path) {
            items = path.Split('.').ToList();
        }

        public override String ToString() {
            if (items == null) {
                return "";
            }
            return String.Join(".", items.ToArray());
        }
    }
}
