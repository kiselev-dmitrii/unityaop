using Assets.UnityAOP.Editor.InspectorWidgets;
using Assets.UnityAOP.Observable.Binding;
using UnityEditor;

namespace Assets.UnityAOP.Editor.Inspector {
    [CustomEditor(typeof(BindingNode), true)]
    public class BindingNodeInspector : UnityEditor.Editor {
        private BindingNode node;
        private XAutocompleteField pathField;
        private XButton rebindButton;

        protected void OnEnable() {
            node = (BindingNode)target;
            node.UpdateParentNode();

            pathField = new XAutocompleteField("Path", node.Path);
            rebindButton = new XButton("Rebind", OnRebindButtonClick);
        }

        private void OnRebindButtonClick() {
            node.Rebind();
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            pathField.SetOrigin(node.GetParentType());
            
            pathField.Draw();
            rebindButton.Draw();

            node.Path = pathField.Value;
            node.Type = pathField.IsValid ? pathField.ResolvedType : null;
        }
    }
}