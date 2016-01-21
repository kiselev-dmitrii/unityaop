using UnityEngine.Assertions;

namespace Assets.UnityAOP.Observable.Binding {
    public abstract class RootNode : Node {
        public IObservable Root { get; private set; }

        protected void Awake() {
            Initialize();
        }

        public abstract void Initialize();

        public void SetRoot(object root) {
            Assert.IsTrue(root.GetType() == Type.Type, "Type mismatch");
            Root = (IObservable)root;
        }

        public override void UpdateParentNode() { }

        public override string GetFullPath() {
            return "";
        }

        public override RootNode GetRootNode() {
            return this;
        }
    }
}
