namespace Assets.UnityAOP.Observable.Core {
    public interface IObserver {
        void OnNodeChanged(IObservable parent, int propertyCode);
    }

    public interface IListObserver<T> {
        void OnListCleared();
        void OnItemInserted(int index, T item);
        void OnItemRemoved(int index, T item);
    }
}
