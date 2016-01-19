using System;
using Assets.UnityAOP.Observable.ChainedObservers;

namespace Assets.UnityAOP.Binding.NGUI {
    public class InputBinding : BindingNode {
        private ChainedPropertyObserver<String> observer;
        private UIInput input;
        private EventDelegate eventDelegate;

        protected override void Awake() {
            input = GetComponent<UIInput>();
            eventDelegate = new EventDelegate(OnInputChanged);
            base.Awake();
        }

        public override void Bind() {
            var root = GetRootNode().Root;
            var path = GetFullPath();
            observer = root.Observe<String>(path, OnValueChanged);

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

        private void OnValueChanged() {
        }

        private void OnInputChanged() {
            if (observer != null) {
                observer.SetValue(input.value);
            }
        }
    }
}
