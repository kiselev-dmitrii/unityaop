using System;
using UnityEngine;

namespace Assets.UnityAOP.Binding {
    public abstract class BindingNode : Node {
        [HideInInspector]
        public String Path;
        private String fullPath;

        protected virtual void Awake() {
            Bind();
        }

        protected virtual void OnDestroy() {
            Unbind();
        }

        public override string GetFullPath() {
            if (fullPath == null) {
                if (ParentNode == GetRootNode()) {
                    return Path;
                } else {
                    fullPath = ParentNode.GetFullPath() + "." + Path;
                }
            }
            return fullPath;
        }

        public void Rebind() {
            Unbind();
            Rebind();
        }

        public abstract void Bind();
        public abstract void Unbind();
    
        
    }
}
