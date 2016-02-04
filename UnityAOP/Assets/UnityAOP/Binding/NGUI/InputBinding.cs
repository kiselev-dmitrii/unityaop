using System;
using System.Collections.Generic;
using Assets.UnityAOP.Binding.Core;
using Assets.UnityAOP.Observable.ChainedObservers;

namespace Assets.UnityAOP.Binding.NGUI {
    public class InputBinding : BindingNode {
        public BindingPath Path; 
        private UntypedPropertyObserver observer;
        private UIInput input;
        private EventDelegate eventDelegate;

        protected void Awake() {
            input = GetComponent<UIInput>();
            eventDelegate = new EventDelegate(OnInputChanged);
        }

        public override void Bind() {
            var root = Context.Model;

            observer = root.ObserveProperty(Path.ToArray());
            OnInputChanged();

            if (input != null) {
                input.onChange.Add(eventDelegate);
            }
        }

        public override void Unbind() {
            if (observer != null) {
                observer.Dispose();
                observer = null;
            }

            if (input != null) {
                input.onChange.Remove(eventDelegate);
            }
        }

        private void OnInputChanged() {
            if (observer != null) {
                observer.SetStringValue(input.value);
            }
        }
    }
}
