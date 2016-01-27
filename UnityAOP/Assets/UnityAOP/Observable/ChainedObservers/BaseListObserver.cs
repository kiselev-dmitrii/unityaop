using Assets.UnityAOP.Observable.CodeObjectModel;
using Assets.UnityAOP.Observable.Core;

namespace Assets.UnityAOP.Observable.ChainedObservers {
public abstract class BaseListObserver<T> : BaseChainedObserver, IListObserver<T> {
    private ObservableList<T> observableList; 

    protected BaseListObserver(IObservable root, PropertyMetadata[] propertyPath) : base(root, propertyPath) {
        observableList = null;
        Bind(0);
    }

    protected override void BindTarget(IObservable parent, PropertyMetadata targetMeta) {
        var getter = (GetterDelegate<IObservable>)parent.GetGetterDelegate(targetMeta.Code);
        observableList = (ObservableList<T>) getter();

        if (observableList != null) {
            observableList.AddCollectionObserver(this);
        }
    }

    protected override void UnbindTarget() {
        if (observableList != null) {
            observableList.RemoveCollectionObserver(this);
            observableList = null;
        }
    }

    protected override void OnParentNodeChanged() {
        OnListCleared();
        if (observableList != null) {
            for (int i = 0; i < observableList.Count; ++i) {
                OnItemInserted(i, observableList[i]);
            }
        }
    }

    public abstract void OnListCleared();
    public abstract void OnItemInserted(int index, T item);
    public abstract void OnItemRemoved(int index, T item);
}
}
