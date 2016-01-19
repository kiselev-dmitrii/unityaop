using Assets.UnityAOP.Observable;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Binding {
    public class RootNode : Node {
        public IObservable Root { get; private set; }

        public void SetRoot(IObservable root) {
            Assert.IsTrue(root.GetType() == Type.Type, "Type mismatch");
            Root = root;
        }

        public override void UpdateParentNode() { }
    }
}
