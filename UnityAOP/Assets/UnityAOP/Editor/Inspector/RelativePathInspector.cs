using System;
using System.Linq;
using Assets.UnityAOP.Editor.InspectorWidgets;
using Assets.UnityAOP.Observable.Binding;
using UnityEditor;
using UnityEngine;

namespace Assets.UnityAOP.Editor.Inspector {
    [CustomEditor(typeof(RelativePath))]
    public class RelativePathInspector : UnityEditor.Editor {
        private RelativePath node;
        private XAutocompleteField pathField;

        protected void OnEnable() {
            node = (RelativePath)target;
            node.UpdateParentNode();

            pathField = new XAutocompleteField("Path", node.Path);
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            pathField.SetOrigin(node.GetParentType());
            pathField.Draw();

            node.Path = pathField.Value;
            node.Type = pathField.IsValid ? pathField.ResolvedType : null;
        }
    }
}