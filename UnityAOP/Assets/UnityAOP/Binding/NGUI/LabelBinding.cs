using System;
using System.Collections.Generic;
using Assets.UnityAOP.Binding.Core;
using Assets.UnityAOP.Observable.ChainedObservers;

namespace Assets.UnityAOP.Binding.NGUI {
    public class LabelBinding : BindingNode {
        public BindingPath Path;
        private UntypedPropertyObserver observer;
        private UILabel label;

        protected void Awake() {
            label = GetComponent<UILabel>();
        }

        public override void Bind() {
            var root = Context.Model;
            observer = root.ObserveProperty(Path.ToArray(), OnValueChanged);
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
