using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.UnityAOP.Binding.Core;
using Assets.UnityAOP.Editor.InspectorWidgets;
using UnityEditor;

namespace Assets.UnityAOP.Editor.Inspector {
    [CustomEditor(typeof (BindingNode), true)]
    public class BindingNodeInspector : UnityEditor.Editor {
        private BindingNode node;
        private Dictionary<String, PathField> pathFields;

        private class PathField {
            public XAutocompleteField Autocomplete;
            public BindingPath Reference;
        }

        protected void OnEnable() {
            node = (BindingNode) target;
            UpdateBindingContext(node);

            pathFields = new Dictionary<String, PathField>();
            foreach (var prop in GetProperties(serializedObject)) {
                if (prop.type == typeof (BindingPath).Name) {
                    var reference = (BindingPath)GetValue(node, prop.propertyPath);
                    var autocomplete = new XAutocompleteField(prop.displayName, reference.ToString());

                    pathFields[prop.propertyPath] = new PathField {
                        Autocomplete = autocomplete,
                        Reference = reference
                    };
                }
            }
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            foreach (var prop in GetProperties(serializedObject)) {
                PathField pathField = null;
                if (pathFields.TryGetValue(prop.propertyPath, out pathField)) {
                    var autocomplete = pathField.Autocomplete;
                    autocomplete.SetOrigin(node.Context.Type);
                    autocomplete.Draw();

                    pathField.Reference.SetPath(autocomplete.Value);
                } else {
                    EditorGUILayout.PropertyField(prop, true);
                }
            }
        }

        private static void UpdateBindingContext(BindingNode node) {
            node.Context = node.transform.parent.GetComponentInParent<BindingContext>();
        }

        private static IEnumerable<SerializedProperty> GetProperties(SerializedObject serializedObject) {
            var prop = serializedObject.GetIterator();
            bool result = prop.NextVisible(true);
            if (result) {
                do {
                    yield return prop;
                } while (prop.NextVisible(false));
            }
        }

        private static object GetValue(object source, string name) {
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return f.GetValue(source);
        }
    }
}