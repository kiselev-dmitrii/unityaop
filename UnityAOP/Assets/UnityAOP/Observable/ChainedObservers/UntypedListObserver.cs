using System;
using Assets.UnityAOP.Observable.CodeObjectModel;
using Assets.UnityAOP.Observable.Core;

namespace Assets.UnityAOP.Observable.ChainedObservers {
    public class UntypedListObserver : BaseChainedObserver, IListObserver {
        private IObservableList observableList;
        
        public event Action<int, object> OnInserted;
        public event Action<int, object> OnRemoved;
        public event Action OnCleared;

        public UntypedListObserver(IObservable root, PropertyMetadata[] propertyPath) : base(root, propertyPath) {
            observableList = null;
            Bind(0);
        }

        protected override void BindTarget(IObservable parent, PropertyMetadata targetMeta) {
            var getter = (GetterDelegate<IObservable>)parent.GetGetterDelegate(targetMeta.Code);
            observableList = (IObservableList)getter();

            if (observableList != null) {
                observableList.AddListObserver(this);
            }
        }

        protected override void UnbindTarget() {
            if (observableList != null) {
                observableList.RemoveListObserver(this);
                observableList = null;
            }
        }

        protected override void OnParentNodeChanged() {
            OnListCleared();
            if (observableList != null) {
                for (int i = 0; i < observableList.Count; ++i) {
                    OnItemInserted(i, observableList.ItemAt(i));
                }
            }
        }

        public void OnItemInserted(int index, object item) {
            if (OnInserted != null) {
                OnInserted(index, item);
            }
        }

        public void OnItemRemoved(int index, object item) {
            if (OnRemoved != null) {
                OnRemoved(index, item);
            }
        }

        public void OnListCleared() {
            if (OnCleared != null) {
                OnCleared();
            }
        }

        public override void Dispose() {
            base.Dispose();
            OnInserted = null;
            OnRemoved = null;
            OnCleared = null;
        }
    }
}