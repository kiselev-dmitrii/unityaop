using System;
using Assets.UnityAOP.Utils;
using UnityEngine;

namespace Assets.UnityAOP.Observable.Binding {
    public class Node : MonoBehaviour {
        [ClassHasAttribute(typeof(ObservableAttribute))]
        public SerializableType Type;
        public Node ParentNode;
        public RootNode RootNode;

        public virtual void UpdateParentNode() {
            ParentNode = transform.parent.GetComponentInParent<Node>();
        }

        public virtual String GetFullPath() {
            return ParentNode.GetFullPath();
        }

        public virtual RootNode GetRootNode() {
            if (RootNode == null) {
                RootNode = ParentNode.GetRootNode();
            }
            return RootNode;
        }

        public Type GetParentType() {
            return ParentNode.Type;
        }
    }
}
