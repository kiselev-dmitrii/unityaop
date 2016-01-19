using System;
using Assets.UnityAOP.Observable.ChainedObservers;

namespace Assets.UnityAOP.Binding.NGUI {
    public class LabelBinding : BindingNode {
        private UntypedValueObserver observer;
        private UILabel label;

        protected override void Awake() {
            label = GetComponent<UILabel>();
            base.Awake();
        }

        public override void Bind() {
            var root = GetRootNode().Root;
            var path = GetFullPath();
            observer = root.Observe(path, OnValueChanged);
            OnValueChanged();
        }

        public override void Unbind() {
            if (observer != null) {
                observer.Dispose();
                observer = null;
            } 
        }

        private void OnValueChanged() {
            label.text = observer.GetStringValue();
        }
    }
}
