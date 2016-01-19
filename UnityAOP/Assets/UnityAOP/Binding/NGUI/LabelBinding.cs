using System;
using Assets.UnityAOP.Observable.ChainedObservers;

namespace Assets.UnityAOP.Binding.NGUI {
    public class LabelBinding : BindingNode {
        private ChainedPropertyObserver<String> observer;
        private UILabel label;

        protected override void Awake() {
            label = GetComponent<UILabel>();
            base.Awake();
        }

        public override void Bind() {
            var root = GetRootNode().Root;
            var path = GetFullPath();
            observer = root.Observe<String>(path, OnValueChanged);
        }

        public override void Unbind() {
            if (observer != null) {
                observer.Dispose();
                observer = null;
            } 
        }

        private void OnValueChanged() {
            label.text = observer.GetValue();
        }
    }
}
