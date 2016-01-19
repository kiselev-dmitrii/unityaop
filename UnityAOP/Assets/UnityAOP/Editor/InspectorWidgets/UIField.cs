using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace Assets.UnityAOP.Editor.InspectorWidgets {
    public abstract class UIField<T> : UIWidget {
        public String Name { get; private set; }
        public T Value { get; protected set; }
        public bool ReadOnly = false;

        public UIField(String name, T value) {
            Name = name;
            Value = value;
        }

        public void SetValue(T value) {
            Value = value;
        }
    }

    public class UIIntField : UIField<int> {
        public UIIntField(String name, int value) : base(name, value) { }

        public override void Draw() {
            if (ReadOnly) {
                EditorGUILayout.LabelField(Name, Value.ToString());
            } else {
                Value = EditorGUILayout.IntField(Name, Value);
            }
        }
    }

    public class UIFloatField : UIField<float> {
        public UIFloatField(String name, float value) : base(name, value) { }

        public override void Draw() {
            if (ReadOnly) {
                EditorGUILayout.LabelField(Name, Value.ToString());
            } else {
                Value = EditorGUILayout.FloatField(Name, Value);
            }

        }
    }

    public class UIStringField : UIField<String> {
        public UIStringField(String name, String value) : base(name, value) { }

        public override void Draw() {
            if (ReadOnly) {
                EditorGUILayout.LabelField(Name, Value.ToString());
            } else {
                Value = EditorGUILayout.TextField(Name, Value);
            }
        }
    }
}
