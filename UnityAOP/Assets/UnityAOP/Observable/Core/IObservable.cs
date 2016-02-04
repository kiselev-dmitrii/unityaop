
namespace Assets.UnityAOP.Observable.Core {
    public delegate T GetterDelegate<out T>();
    public delegate void SetterDelegate<in T>(T value);
    
    public interface IObservable {
        void AddMemberObserver(int memberCode, IObserver observer);
        void RemoveMemberObserver(int memberCode, IObserver observer);
        void NotifyMemberChanged(int memberCode);

        object GetGetterDelegate(int propertyCode);
        object GetSetterDelegate(int propertyCode);
        object GetMethodDelegate(int methodCode);
    }

    public interface IObservableList {
        int Count { get; }
        object ItemAt(int index);

        void AddListObserver(IListObserver observer);
        void RemoveListObserver(IListObserver observer);

        void NotifyItemInserted(int index, object item);
        void NotifyItemRemoved(int index, object item);
        void NotifyListCleared();
    }

    public interface IObservableList<T> {
        void AddListObserver(IListObserver<T> observer);
        void RemoveListObserver(IListObserver<T> observer);

        void NotifyItemInserted(int index, T item);
        void NotifyItemRemoved(int index, T item);
        void NotifyListCleared();
    }
}
