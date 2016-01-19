using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Editor.Inspector;
using Assets.UnityAOP.Observable;
using UnityEditor;
using UnityEngine;

namespace Assets.UnityAOP.Editor.InspectorWidgets {
    public class UIAutocompleteField : UIField<String> {
        private static GUIStyle style;
        private readonly PathAnylazer pathAnylazer;

        private const String ControlName = "TextField";
        private static readonly Color ValidColor = new Color(0, 1, 0, 0.1f);
        private static readonly Color InvalidColor = new Color(1, 0, 0, 0.1f);
        private static readonly Color PopupBackgroundColor = new Color(0.82f, 0.82f, 0.82f);
        private static readonly int Z = 100;

        public bool IsValid {
            get { return pathAnylazer.IsValid; }
        }

        public Type ResolvedType {
            get { return pathAnylazer.ResolvedType; }
        }

        public UIAutocompleteField(string name, string value) : base(name, value) {
            pathAnylazer = new PathAnylazer();
            if (Value == null) {
                Value = "";
            }
        }

        public void SetOrigin(Type type) {
            pathAnylazer.SetType(type);
        }

        public override void Draw() {
            if (style == null) {
                style = new GUIStyle(GUI.skin.textArea);
                style.richText = true;
            }

            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(Name);
                GUI.SetNextControlName(ControlName);
                Value = GUILayout.TextField(Value);
                Rect textFieldRect = GUILayoutUtility.GetLastRect();
            EditorGUILayout.EndHorizontal();

            pathAnylazer.Anylize(Value);
            var rectColor = pathAnylazer.IsValid ? ValidColor : InvalidColor;
            EditorGUI.DrawRect(textFieldRect, rectColor);

            if (GUI.GetNameOfFocusedControl() == ControlName) {
                DrawPopup(textFieldRect);
            }
        }

        private void DrawPopup(Rect textFieldRect) {
            var n = pathAnylazer.NumVariants;
            if (n > 0) {
                GUI.depth += Z;

                var variants = CalculateVariants(pathAnylazer);
                var popupRect = new Rect(textFieldRect.x, textFieldRect.y - n*textFieldRect.height, textFieldRect.width, n*textFieldRect.height);
                EditorGUI.DrawRect(popupRect, PopupBackgroundColor);
                GUI.TextField(popupRect, variants, style);

                GUI.depth -= Z;
            }
        }

        private static String CalculateVariants(PathAnylazer pathAnylazer) {
            var unresolved = pathAnylazer.Unresolved;
            var variants = pathAnylazer.Variants;

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < variants.Count; i++) {
                var variant = variants[i];
                String str = VariantToString(unresolved, variant);
                builder.Append(str);

                if (i != variants.Count - 1) {
                    builder.Append("\n");
                }
            }

            return builder.ToString();
        }

        private static String VariantToString(String unresolved, PropertyMetadata property) {
            const String typeColor = "#7e7e7e";
            const String nameColor = "#000000";
            const String selectionColor = "#ff0000";

            String name = String.Format("<color={0}>{1}</color>", nameColor, property.Name);
            if (!String.IsNullOrEmpty(unresolved)) {
                String replacement = String.Format("<color={0}>{1}</color>", selectionColor, unresolved);
                name = name.Replace(unresolved, replacement);
            }

            String type = String.Format("<color={0}>{1}{2}</color>", typeColor, property.Type.Name, property.IsCollection ? "[]" : "");

            return String.Format("<b>{0} {1}</b>", name, type);
        }
    }
}
