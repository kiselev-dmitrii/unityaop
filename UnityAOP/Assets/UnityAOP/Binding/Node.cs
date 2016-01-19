using System;
using Assets.UnityAOP.Attributes;
using Assets.UnityAOP.Utils;
using UnityEngine;

namespace Assets.UnityAOP.Binding {
    public class Node : MonoBehaviour {
        [ClassHasAttribute(typeof(ObservableAttribute))]
        public SerializableType Type;
        public Node ParentNode;

        public virtual void UpdateParentNode() {
            ParentNode = transform.parent.GetComponentInParent<Node>();
        }

        public Type GetParentType() {
            return ParentNode.Type;
        }
    }
}
