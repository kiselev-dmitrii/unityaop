using System;
using System.Linq;
using Assets.UnityAOP.Binding;
using UnityEditor;
using UnityEngine;

namespace Assets.UnityAOP.Editor.Inspector {
    [CustomEditor(typeof(BindingNode))]
    public class BindingNodeInspector : UnityEditor.Editor {
        private bool isInited;
        private BindingNode node;
        private GUIStyle style;
        private PathAnylazer pathAnylazer;

        public override void OnInspectorGUI() {
            if (!isInited) {
                node = (BindingNode) target;
                style = new GUIStyle(GUI.skin.textArea);
                style.richText = true;
                pathAnylazer = new PathAnylazer();

                isInited = true;
            }

            DrawDefaultInspector();
            node.Path = DrawAutocompleteField("Path", node.Path);
        }

        private String DrawAutocompleteField(String name, String input) {
            String result = null;

            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(name);
                GUI.SetNextControlName("textField");
                String text = GUILayout.TextField(input);
                Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUILayout.EndHorizontal();

            if (GUI.GetNameOfFocusedControl() == "textField") {
                pathAnylazer.SetType(node.Type);
                pathAnylazer.Anylize(text);

                Color rectColor = pathAnylazer.IsValid ? new Color(0, 1, 0, 0.1f) : new Color(1, 0, 0, 0.1f);
                EditorGUI.DrawRect(rect, rectColor);

                int n = pathAnylazer.NumVariants;
                if (n > 0) {
                    int lastDepth = GUI.depth;
                    GUI.depth += 100;

                    String unresolved = pathAnylazer.Unresolved;
                    String variants = String.Join("\n", pathAnylazer.Variants.Select(x => {
                        if (!String.IsNullOrEmpty(unresolved)) {
                            return x.Name.Replace(unresolved, "<color=#ff0000>" + unresolved + "</color>");
                        } else {
                            return x.Name;
                        }
                        
                    }).ToArray());

                    var variantsRect = new Rect(rect.x, rect.y - n * rect.height, rect.width, n * rect.height);
                    EditorGUI.DrawRect(variantsRect, Color.blue);
                    GUI.TextField(variantsRect, variants, style);

                    GUI.depth = lastDepth;
                }
            }

            return text;
        }

    }
}