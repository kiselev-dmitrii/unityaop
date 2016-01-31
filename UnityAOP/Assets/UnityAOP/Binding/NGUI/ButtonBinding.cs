using System;
using System.Collections.Generic;
using System.Linq;
using Assets.UnityAOP.Binding.Core;
using Assets.UnityAOP.Observable.ChainedObservers;
using Assets.UnityAOP.Observable.Core;

namespace Assets.UnityAOP.Binding.NGUI {
    public class ButtonBinding : BindingNode {
        public BindingPath Path; 
        private ChainedPropertyObserver<IObservable> observer;
        private String methodName;

        public override void Bind() {
            var root = Context.Model;
            String[] propertyPath = Path.Slice(0, Path.Length()- 1);
            methodName = Path.Last();

            observer = root.Observe<IObservable>(propertyPath);
        }

        public override void Unbind() {
            if (observer != null) {
                observer.Dispose();
                observer = null;
            } 
        }

        public void OnClick() {
            if (observer != null) {
                observer.CallMethod(methodName);
            }
        }
    }
}
