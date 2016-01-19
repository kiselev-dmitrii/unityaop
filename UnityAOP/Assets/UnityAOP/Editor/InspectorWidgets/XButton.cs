using System;
using UnityEngine;

namespace Assets.UnityAOP.Editor.InspectorWidgets {
    public class XButton : XWidget {
        public String Name { get; private set; }
        private Action onClick;

        public XButton(String name, Action onClick) {
            Name = name;
            this.onClick = onClick;
        }

        public void SetName(String name) {
            Name = name;
        }

        public override void Draw() {
            if (GUILayout.Button(Name)) {
                onClick();
            }
        }
    }
}
