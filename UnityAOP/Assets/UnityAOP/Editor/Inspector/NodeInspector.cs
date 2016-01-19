using System;
using System.Linq;
using Assets.UnityAOP.Binding;
using UnityEditor;
using UnityEngine;

namespace Assets.UnityAOP.Editor.Inspector {
    [CustomEditor(typeof(Node))]
    public class NodeInspector : UnityEditor.Editor {
        private Node node;
        
        protected void OnEnable() {
            node = (Node) target;
            node.UpdateParentNode();
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
        }
    }
}