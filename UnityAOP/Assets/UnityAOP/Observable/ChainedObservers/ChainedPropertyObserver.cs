using System;
using Assets.UnityAOP.Observable.CodeObjectModel;
using Assets.UnityAOP.Observable.Core;

namespace Assets.UnityAOP.Observable.ChainedObservers {
    public class ChainedPropertyObserver<T> : BaseChainedObserver {
        private GetterDelegate<T> valueGetter;
        private SetterDelegate<T> valueSetter;
        private GetterDelegate<IObservable> observableGetter;
        private SetterDelegate<IObservable> observableSetter;
        private Action callback;
        private T lastValue;

        public ChainedPropertyObserver(IObservable root, PropertyMetadata[] propertyPath, Action onValueChanged) : base(root, propertyPath) {
            callback = onValueChanged;
            Bind(0);
        }
    
        public T GetValue() {
            if (valueGetter != null) {
                return valueGetter();
            }
            if (observableGetter != null) {
                return (T)observableGetter();
            }
            return default(T);
        }
    
        public void SetValue(T value) {
            if (valueSetter != null) {
                valueSetter(value);
            }  
            else if (observableSetter != null) {
                observableSetter((IObservable)value);
            }
        }

        protected override void BindTarget(IObservable parent, PropertyMetadata targetMeta) {
            if (!targetMeta.IsObservable) {
                valueGetter = (GetterDelegate<T>) parent.GetGetterDelegate(targetMeta.Code);
                valueSetter = (SetterDelegate<T>)parent.GetSetterDelegate(targetMeta.Code);
            } else {
                observableGetter = (GetterDelegate<IObservable>)parent.GetGetterDelegate(targetMeta.Code);
                observableSetter = (SetterDelegate<IObservable>)parent.GetSetterDelegate(targetMeta.Code);
            }
        }

        protected override void UnbindTarget() {
            valueGetter = null;
            valueSetter = null;
            observableGetter = null;
            observableSetter = null;
        }

        protected override void OnParentNodeChanged() {
            T newValue = GetValue();
            if (!Equals(newValue, lastValue)) {
                callback();
                lastValue = newValue;
            }
        }
    
        public override void Dispose() {
            base.Dispose();
            callback = null;
        }

        private static bool Equals(T obj1, T obj2) {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null) return false;
            if (obj2 == null) return false;
            return obj1.Equals(obj2);
        }
    }
}
