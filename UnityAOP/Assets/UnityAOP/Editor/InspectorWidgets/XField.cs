using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace Assets.UnityAOP.Editor.InspectorWidgets {
    public abstract class XField<T> : XWidget {
        public String Name { get; private set; }
        public T Value { get; protected set; }
        public bool ReadOnly = false;

        public XField(String name, T value) {
            Name = name;
            Value = value;
        }

        public void SetValue(T value) {
            Value = value;
        }
    }

    public class XIntField : XField<int> {
        public XIntField(String name, int value) : base(name, value) { }

        public override void Draw() {
            if (ReadOnly) {
                EditorGUILayout.LabelField(Name, Value.ToString());
            } else {
                Value = EditorGUILayout.IntField(Name, Value);
            }
        }
    }

    public class XFloatField : XField<float> {
        public XFloatField(String name, float value) : base(name, value) { }

        public override void Draw() {
            if (ReadOnly) {
                EditorGUILayout.LabelField(Name, Value.ToString());
            } else {
                Value = EditorGUILayout.FloatField(Name, Value);
            }

        }
    }

    public class XStringField : XField<String> {
        public XStringField(String name, String value) : base(name, value) { }

        public override void Draw() {
            if (ReadOnly) {
                EditorGUILayout.LabelField(Name, Value.ToString());
            } else {
                Value = EditorGUILayout.TextField(Name, Value);
            }
        }
    }
}
