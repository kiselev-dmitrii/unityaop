using System;
using Assets.UnityAOP.Observable.ChainedObservers;
using Assets.UnityAOP.Observable.Core;
using UnityEngine;

namespace Assets.UnityAOP.Observable.Binding.NGUI {
    public class ButtonBinding : BindingNode {
        private ChainedPropertyObserver<IObservable> observer;
        private String methodName;

        public override void Bind() {
            var root = GetRootNode().Root;
            var path = GetFullPath();

            int lastPoint = path.LastIndexOf('.');
            String propertyPath = path.Substring(0, lastPoint);
            Debug.Log(propertyPath);
            methodName = path.Substring(lastPoint + 1, path.Length - lastPoint - 1);
            Debug.Log(methodName);

            observer = root.Observe<IObservable>(propertyPath, () => { });
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
